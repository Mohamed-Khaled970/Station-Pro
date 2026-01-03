function formatCurrency(amount) {
    return new Intl.NumberFormat('en-EG', {
        style: 'currency',
        currency: 'EGP'
    }).format(amount);
}

// ============================================
// SESSION TIMER CLASS
// ============================================

class SessionTimer {
    constructor(elementId, startTime) {
        this.elementId = elementId;
        this.startTime = new Date(startTime);
        this.timerInterval = null;
        this.element = document.getElementById(elementId);

        if (this.element) {
            this.hourlyRate = parseFloat(this.element.dataset.hourlyRate) || 0;
            this.costElementId = elementId.replace('timer-', 'cost-');
        }
    }

    start() {
        this.update(); // Update immediately
        this.timerInterval = setInterval(() => this.update(), 1000);
    }

    stop() {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
    }

    update() {
        const now = new Date();
        const elapsed = Math.floor((now - this.startTime) / 1000); // seconds

        const hours = Math.floor(elapsed / 3600);
        const minutes = Math.floor((elapsed % 3600) / 60);
        const seconds = elapsed % 60;

        const formatted = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

        if (this.element) {
            this.element.textContent = formatted;
        }

        // Update cost
        const elapsedHours = elapsed / 3600;
        const currentCost = elapsedHours * this.hourlyRate;
        const costElement = document.getElementById(this.costElementId);

        if (costElement) {
            costElement.textContent = formatCurrency(currentCost);
        }
    }
}

// ============================================
// GLOBAL TIMER MANAGER
// ============================================

const activeTimers = new Map();

function startAllTimers() {
    console.log('Starting all timers...');

    // Stop existing timers first to avoid duplicates
    activeTimers.forEach(timer => timer.stop());
    activeTimers.clear();

    // Find all timer elements
    const timerElements = document.querySelectorAll('[id^="timer-"]');
    console.log(`Found ${timerElements.length} timer elements`);

    timerElements.forEach(timerElement => {
        const sessionId = timerElement.id.replace('timer-', '');
        const startTime = timerElement.dataset.startTime;

        console.log(`Initializing timer for session ${sessionId}, start time: ${startTime}`);

        if (startTime) {
            const timer = new SessionTimer(timerElement.id, startTime);
            timer.start();
            activeTimers.set(sessionId, timer);
            console.log(`Timer started for session ${sessionId}`);
        }
    });

    console.log(`Total active timers: ${activeTimers.size}`);
}

function stopTimer(sessionId) {
    const timer = activeTimers.get(sessionId);
    if (timer) {
        timer.stop();
        activeTimers.delete(sessionId);
        console.log(`Timer stopped for session ${sessionId}`);
    }
}

function stopAllTimers() {
    activeTimers.forEach(timer => timer.stop());
    activeTimers.clear();
    console.log('All timers stopped');
}

// ============================================
// AUTO-INITIALIZATION
// ============================================

// Start timers when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('Session timer module initialized');
    startAllTimers();
});

// Restart timers after HTMX swaps
document.body.addEventListener('htmx:afterSwap', (event) => {
    console.log('HTMX afterSwap detected, checking for timers...');

    // Small delay to ensure DOM is fully updated
    setTimeout(() => {
        startAllTimers();
    }, 100);
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    stopAllTimers();
});

// Export for use in other scripts
if (typeof window !== 'undefined') {
    window.SessionTimer = SessionTimer;
    window.startAllTimers = startAllTimers;
    window.stopTimer = stopTimer;
    window.stopAllTimers = stopAllTimers;
}