// FIXED VERSION - Timers survive HTMX swaps

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-EG', {
        style: 'currency',
        currency: 'EGP'
    }).format(amount);
}

class SessionTimer {
    constructor(elementId, startTime) {
        this.elementId = elementId;
        this.startTime = new Date(startTime).getTime();
        this.element = document.getElementById(elementId);

        if (this.element) {
            this.hourlyRate = parseFloat(this.element.dataset.hourlyRate) || 0;
            this.costElementId = elementId.replace('timer-', 'cost-');
            this.costElement = document.getElementById(this.costElementId);
            console.log(`✅ Timer created: ${elementId}, rate: ${this.hourlyRate}`);
        } else {
            console.error(`❌ Timer element not found: ${elementId}`);
        }

        this.lastRenderedTime = '';
        this.lastRenderedCost = '';
        this.updateCount = 0;
    }

    // Refresh element references (critical after HTMX swap)
    refreshElements() {
        this.element = document.getElementById(this.elementId);
        this.costElement = document.getElementById(this.costElementId);

        if (this.element) {
            // Re-read hourly rate in case it changed
            this.hourlyRate = parseFloat(this.element.dataset.hourlyRate) || this.hourlyRate;
        }
    }

    update(currentTime) {
        // Refresh element if it's missing (HTMX might have replaced it)
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

        // Update time
        // to enhance the performace , we check if the lastRender time and cost are still the same or not , if they ? don't update the DOM
        if (this.lastRenderedTime !== formatted) {
            this.element.textContent = formatted;
            this.lastRenderedTime = formatted;
        }

        // Update cost
        const elapsedHours = elapsed / 3600;
        const currentCost = elapsedHours * this.hourlyRate;
        const formattedCost = formatCurrency(currentCost);

        if (this.costElement && this.lastRenderedCost !== formattedCost) {
            this.costElement.textContent = formattedCost;
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

        // Update all timers and collect invalid ones
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
        this.timers.delete(sessionId);
        console.log(`delete timer ${sessionId}`);
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

    // CRITICAL FIX: After HTMX swap, refresh all timer element references
    refreshAllTimerElements() {
        console.log('🔄 Refreshing all timer element references after HTMX swap');
        let refreshed = 0;

        this.timers.forEach(timer => {
            timer.refreshElements();
            if (timer.element) refreshed++;
        });

        console.log(`✅ Refreshed ${refreshed}/${this.timers.size} timer elements`);
    }

    // Smart restart - sync with DOM but preserve timer state
    smartRestart() {
        console.log('🔄 SMART RESTART: Syncing timers with DOM');

        const timerElements = document.querySelectorAll('[id^="timer-"]');
        const currentSessions = new Set();

        timerElements.forEach(timerElement => {
            const sessionId = timerElement.id.replace('timer-', '');
            currentSessions.add(sessionId);

            if (!this.timers.has(sessionId)) {
                // New session - create timer
                const startTime = timerElement.dataset.startTime;
                if (startTime) {
                    const timer = new SessionTimer(timerElement.id, startTime);
                    this.addTimer(sessionId, timer);
                    console.log(`➕ Added new timer: ${sessionId}`);
                }
            }
        });

        // Remove timers for sessions that no longer exist
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

        // Refresh element references for existing timers
        this.refreshAllTimerElements();

        console.log(`✅ Smart restart complete: ${this.timers.size} active timers`);
    }
}

const timerManager = new TimerManager();

function startAllTimers() {
    console.log('🚀 Initializing all timers');

    timerManager.clear();

    const timerElements = document.querySelectorAll('[id^="timer-"]');

    timerElements.forEach(timerElement => {
        const sessionId = timerElement.id.replace('timer-', '');
        const startTime = timerElement.dataset.startTime;

        if (startTime) {
            const timer = new SessionTimer(timerElement.id, startTime);
            timerManager.addTimer(sessionId, timer);
        }
    });

    console.log(`✅ Initialized ${timerManager.getTimerCount()} timers`);
}

// Event Handlers
document.addEventListener('DOMContentLoaded', () => {
    console.log('🎯 DOM Ready - Starting timers');
    startAllTimers();
});

// CRITICAL FIX: Handle HTMX swaps properly
document.body.addEventListener('htmx:afterSwap', (event) => {
    if (event.target.id === 'active-sessions-container') {
        console.log('🔄 HTMX swapped active-sessions-container');

        // Use smartRestart to sync with new DOM while preserving timer state
        setTimeout(() => {
            timerManager.smartRestart();
        }, 50);
    }
});

// Pause timers when tab is hidden (save CPU)
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

// Export for debugging
window.SessionTimer = SessionTimer;
window.startAllTimers = startAllTimers;
window.timerManager = timerManager;