// ============================================
// DASHBOARD INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Dashboard initialized');
    initializeDashboard();
});

function initializeDashboard() {
    // Start session timers
    startAllTimers();

    // Setup keyboard shortcuts
    setupKeyboardShortcuts();

    // Handle HTMX events
    setupHtmxEventHandlers();
}

// ============================================
// SESSION TIMER CLASS
// ============================================

class SessionTimer {
    constructor(elementId, startTime, hourlyRate) {
        this.element = document.getElementById(elementId);
        this.costElement = document.getElementById(elementId.replace('timer-', 'cost-'));
        this.startTime = new Date(startTime);
        this.hourlyRate = parseFloat(hourlyRate) || 0;
        this.intervalId = null;
    }

    start() {
        // Update immediately
        this.update();

        // Then update every second
        this.intervalId = setInterval(() => this.update(), 1000);
    }

    update() {
        if (!this.element) {
            this.stop();
            return;
        }

        const now = new Date();
        const diffMs = now - this.startTime;

        // Calculate time components
        const hours = Math.floor(diffMs / 3600000);
        const minutes = Math.floor((diffMs % 3600000) / 60000);
        const seconds = Math.floor((diffMs % 60000) / 1000);

        // Update timer display
        this.element.textContent =
            `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        // Update cost if element exists
        if (this.costElement && this.hourlyRate > 0) {
            const totalHours = diffMs / 3600000;
            const cost = totalHours * this.hourlyRate;
            this.costElement.textContent = formatCurrency(cost);
        }
    }

    stop() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
    }
}

// ============================================
// SESSION TIMERS MANAGEMENT
// ============================================

const activeTimers = new Map();

function startAllTimers() {
    // Stop existing timers first
    activeTimers.forEach(timer => timer.stop());
    activeTimers.clear();

    // Start new timers
    document.querySelectorAll('[id^="timer-"]').forEach(timerElement => {
        const sessionId = timerElement.id.replace('timer-', '');
        const startTime = timerElement.dataset.startTime;

        if (startTime && !activeTimers.has(sessionId)) {
            // Try to get hourly rate from the page
            const rateText = timerElement.parentElement?.querySelector('.text-gray-500')?.textContent || '';
            const hourlyRate = parseFloat(rateText.replace(/[^0-9.]/g, '')) || 0;

            const timer = new SessionTimer(timerElement.id, startTime, hourlyRate);
            timer.start();
            activeTimers.set(sessionId, timer);

            console.log(`Started timer for session ${sessionId}`);
        }
    });
}

function stopTimer(sessionId) {
    const timer = activeTimers.get(sessionId);
    if (timer) {
        timer.stop();
        activeTimers.delete(sessionId);
        console.log(`Stopped timer for session ${sessionId}`);
    }
}

// ============================================
// MANUAL REFRESH DASHBOARD
// ============================================

function refreshDashboard() {
    const refreshBtn = document.getElementById('refresh-btn');
    const refreshIcon = document.getElementById('refresh-icon');

    // Add spinning animation
    refreshIcon.classList.add('fa-spin');
    refreshBtn.disabled = true;

    // Trigger HTMX refreshes
    htmx.trigger('#stats-container', 'refresh');
    htmx.trigger('#devices-container', 'refresh');

    // Show feedback
    showToast('Dashboard refreshed', 'success');

    // Remove animation after 1 second
    setTimeout(() => {
        refreshIcon.classList.remove('fa-spin');
        refreshBtn.disabled = false;
    }, 1000);
}

// ============================================
// HTMX EVENT HANDLERS
// ============================================

function setupHtmxEventHandlers() {
    // Restart timers after active sessions are updated
    document.body.addEventListener('htmx:afterSwap', (event) => {
        if (event.detail.target.id === 'active-sessions-container') {
            console.log('Active sessions updated, restarting timers...');
            startAllTimers();
        }

        // Add fade-in animation to stats cards
        if (event.detail.target.id === 'stats-container') {
            event.detail.target.querySelectorAll('.stat-card').forEach((card, index) => {
                card.style.animation = `slideInRight 0.3s ease-out ${index * 0.1}s`;
            });
        }
    });

    // Handle HTMX errors gracefully (no more console spam!)
    document.body.addEventListener('htmx:responseError', function (event) {
        console.warn('HTMX request failed:', event.detail.pathInfo.requestPath);
        showToast('Failed to update data', 'error');
    });

    // Log successful requests (optional - remove in production)
    document.body.addEventListener('htmx:afterRequest', function (event) {
        if (event.detail.successful) {
            console.log('✓ HTMX request completed:', event.detail.pathInfo.requestPath);
        }
    });
}

// ============================================
// QUICK START SESSION
// ============================================

async function quickStartSession(deviceId) {
    try {
        const response = await fetch(`/session/quick-start`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ deviceId })
        });

        if (response.ok) {
            showToast('Session started successfully!', 'success');
            // HTMX will auto-refresh the active sessions container
        } else {
            showToast('Failed to start session', 'error');
        }
    } catch (error) {
        console.error('Error starting session:', error);
        showToast('An error occurred', 'error');
    }
}

// ============================================
// END SESSION
// ============================================

async function endSession(sessionId, paymentMethod = 'Cash') {
    if (!confirm('Are you sure you want to end this session?')) {
        return;
    }

    try {
        const response = await fetch(`/session/end`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ sessionId, paymentMethod })
        });

        if (response.ok) {
            const data = await response.json();
            showToast(`Session ended. Total: ${formatCurrency(data.totalCost)}`, 'success');
            stopTimer(sessionId);
            // HTMX will auto-refresh
        } else {
            showToast('Failed to end session', 'error');
        }
    } catch (error) {
        console.error('Error ending session:', error);
        showToast('An error occurred', 'error');
    }
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

function setupKeyboardShortcuts() {
    document.addEventListener('keydown', (e) => {
        // Alt + N: New Session
        if (e.altKey && e.key === 'n') {
            e.preventDefault();
            openModal('start-session-modal');
        }

        // Alt + R: Refresh Dashboard
        if (e.altKey && e.key === 'r') {
            e.preventDefault();
            refreshDashboard();
        }

        // Alt + D: View Devices
        if (e.altKey && e.key === 'd') {
            e.preventDefault();
            window.location.href = '/device';
        }
    });
}

// ============================================
// UTILITY FUNCTIONS
// ============================================

function formatCurrency(value) {
    return new Intl.NumberFormat('ar-EG', {
        style: 'currency',
        currency: 'EGP'
    }).format(value);
}

function showToast(message, type = 'info') {
    // Implement your toast notification here
    console.log(`[${type.toUpperCase()}] ${message}`);

    // Simple alert fallback (replace with your toast library)
    if (type === 'error') {
        alert(message);
    }
}

// ============================================
// CLEANUP ON PAGE UNLOAD
// ============================================

window.addEventListener('beforeunload', () => {
    // Stop all timers
    activeTimers.forEach(timer => timer.stop());
    activeTimers.clear();
    console.log('Dashboard cleanup complete');
});

// ============================================
// EXPORT FOR DEBUGGING (Remove in production)
// ============================================

window.dashboardDebug = {
    activeTimers,
    refreshDashboard,
    startAllTimers,
    stopTimer
};