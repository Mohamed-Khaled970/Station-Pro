// FIXED VERSION - Timers survive HTMX swaps
// Uses CSS classes for display elements instead of IDs — supports desktop + mobile simultaneously

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-EG', {
        style: 'currency',
        currency: 'EGP'
    }).format(amount);
}

class SessionTimer {
    constructor(elementId, startTime) {
        this.elementId = elementId;

        // Extract session ID from element ID (e.g. "timer-5" → "5")
        this.sessionId = elementId.replace('timer-', '');

        this.startTime = new Date(startTime).getTime();

        // The hidden source-of-truth element (has the data attributes)
        this.element = document.getElementById(elementId);

        if (this.element) {
            this.hourlyRate = parseFloat(this.element.dataset.hourlyRate) || 0;
            console.log(`✅ Timer created: ${elementId}, rate: ${this.hourlyRate}`);
        } else {
            console.error(`❌ Timer element not found: ${elementId}`);
        }

        this.lastRenderedTime = '';
        this.lastRenderedCost = '';
        this.updateCount = 0;
    }

    // Refresh the hidden source element reference (critical after HTMX swap)
    refreshElements() {
        this.element = document.getElementById(this.elementId);

        if (this.element) {
            this.hourlyRate = parseFloat(this.element.dataset.hourlyRate) || this.hourlyRate;
        }
    }

    // Get all visual display elements for this session (desktop + mobile + any future breakpoints)
    getTimerDisplayElements() {
        return document.querySelectorAll(`.timer-display-${this.sessionId}`);
    }

    getCostDisplayElements() {
        return document.querySelectorAll(`.cost-display-${this.sessionId}`);
    }

    update(currentTime) {
        // Refresh hidden element reference if HTMX replaced it
        if (!this.element || !document.body.contains(this.element)) {
            this.refreshElements();
        }

        if (!this.element) {
            return false; // Signal that this timer is invalid
        }

        this.updateCount++;

        const elapsed = Math.floor((currentTime - this.startTime) / 1000);
        const hours = Math.floor(elapsed / 3600);
        const minutes = Math.floor((elapsed % 3600) / 60);
        const seconds = elapsed % 60;

        const formatted = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

        // Update ALL timer display elements (desktop + mobile) at once
        if (this.lastRenderedTime !== formatted) {
            this.getTimerDisplayElements().forEach(el => el.textContent = formatted);
            this.lastRenderedTime = formatted;
        }

        // Update ALL cost display elements (desktop + mobile) at once
        const elapsedHours = elapsed / 3600;
        const currentCost = elapsedHours * this.hourlyRate;
        const formattedCost = formatCurrency(currentCost);

        if (this.lastRenderedCost !== formattedCost) {
            this.getCostDisplayElements().forEach(el => el.textContent = formattedCost);
            this.lastRenderedCost = formattedCost;
        }

        return true; // Timer is valid
    }

    isValid() {
        return this.element && document.body.contains(this.element);
    }
}

class TimerManager {
    constructor() {
        this.timers = new Map();
        this.intervalId = null;
        this.isRunning = false;
        this.totalUpdates = 0;
    }

    start() {
        if (this.isRunning) return;

        this.isRunning = true;
        console.log('▶️ TimerManager STARTED');

        this.intervalId = setInterval(() => {
            this.updateAll();
        }, 1000);

        // First update immediately
        this.updateAll();
    }

    stop() {
        this.isRunning = false;
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
            console.log('⏸️ TimerManager STOPPED');
        }
    }

    updateAll() {
        if (!this.isRunning || this.timers.size === 0) return;

        this.totalUpdates++;
        const currentTime = Date.now();

        const invalidTimers = [];

        this.timers.forEach((timer, sessionId) => {
            const isValid = timer.update(currentTime);
            if (!isValid && !timer.isValid()) {
                invalidTimers.push(sessionId);
            }
        });

        // Remove invalid timers
        if (invalidTimers.length > 0) {
            console.warn(`🗑️ Removing ${invalidTimers.length} invalid timers`);
            invalidTimers.forEach(id => this.timers.delete(id));

            if (this.timers.size === 0) {
                this.stop();
            }
        }

        // Log status every 10 seconds
        if (this.totalUpdates % 10 === 0) {
            console.log(`📊 Active timers: ${this.timers.size}`);
        }
    }

    addTimer(sessionId, timer) {
        this.timers.set(sessionId, timer);

        if (!this.isRunning) {
            this.start();
        }
    }

    removeTimer(sessionId) {
        this.timers.delete(String(sessionId));
        console.log(`🗑️ Deleted timer ${sessionId}`);
        console.log(`📊 Active timers: ${this.timers.size}`);

        if (this.timers.size === 0) {
            this.stop();
        }
    }

    clear() {
        this.timers.clear();
        this.stop();
    }

    getTimerCount() {
        return this.timers.size;
    }

    // After HTMX swap, refresh all hidden source element references
    refreshAllTimerElements() {
        console.log('🔄 Refreshing all timer element references after HTMX swap');
        let refreshed = 0;

        this.timers.forEach(timer => {
            timer.refreshElements();
            if (timer.element) refreshed++;
        });

        console.log(`✅ Refreshed ${refreshed}/${this.timers.size} timer elements`);
    }

    // Smart restart — sync JS timers with DOM without resetting elapsed time
    smartRestart() {
        console.log('🔄 SMART RESTART: Syncing timers with DOM');

        // Find all hidden source-of-truth timer elements
        const timerElements = document.querySelectorAll('[id^="timer-"]');
        const currentSessions = new Set();

        timerElements.forEach(timerElement => {
            const sessionId = timerElement.id.replace('timer-', '');
            currentSessions.add(sessionId);

            if (!this.timers.has(sessionId)) {
                // New session appeared — create timer for it
                const startTime = timerElement.dataset.startTime;
                if (startTime) {
                    const timer = new SessionTimer(timerElement.id, startTime);
                    this.addTimer(sessionId, timer);
                    console.log(`➕ Added new timer: ${sessionId}`);
                }
            }
        });

        // Remove timers for sessions that no longer exist in the DOM
        const toRemove = [];
        this.timers.forEach((timer, sessionId) => {
            if (!currentSessions.has(sessionId)) {
                toRemove.push(sessionId);
            }
        });

        toRemove.forEach(sessionId => {
            this.removeTimer(sessionId);
            console.log(`➖ Removed ended session timer: ${sessionId}`);
        });

        // Refresh hidden element references for existing timers
        this.refreshAllTimerElements();

        console.log(`✅ Smart restart complete: ${this.timers.size} active timers`);
    }
}

const timerManager = new TimerManager();

function startAllTimers() {
    console.log('🚀 Initializing all timers');

    timerManager.clear();

    // Only select the hidden source-of-truth elements (not display elements)
    const timerElements = document.querySelectorAll('[id^="timer-"]');

    console.log(`Found ${timerElements.length} timer elements`);

    timerElements.forEach(timerElement => {
        const sessionId = timerElement.id.replace('timer-', '');
        const startTime = timerElement.dataset.startTime;

        console.log(`Element: ${timerElement.id}, startTime: ${startTime}`);

        if (startTime) {
            const timer = new SessionTimer(timerElement.id, startTime);
            timerManager.addTimer(sessionId, timer);
        }
    });

    console.log(`✅ Initialized ${timerManager.getTimerCount()} timers`);
}

// ============================================
// EVENT HANDLERS
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('🎯 DOM Ready - Starting timers');
    startAllTimers();
});

// Handle HTMX swaps — used on Dashboard active sessions container
document.body.addEventListener('htmx:afterSwap', (event) => {
    if (event.target.id === 'active-sessions-container') {
        console.log('🔄 HTMX swapped active-sessions-container');

        setTimeout(() => {
            timerManager.smartRestart();
        }, 50);
    }
});

// Pause timers when tab is hidden to save CPU
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        console.log('👁️ Tab hidden - pausing updates');
        timerManager.stop();
    } else {
        console.log('👁️ Tab visible - resuming updates');
        if (timerManager.getTimerCount() > 0) {
            timerManager.start();
        }
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    timerManager.clear();
});

// Export for debugging in browser console
window.SessionTimer = SessionTimer;
window.startAllTimers = startAllTimers;
window.timerManager = timerManager;