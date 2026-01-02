// wwwroot/js/dashboard.js

// ============================================
// UTILITY FUNCTIONS
// ============================================

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        console.warn('Toast container not found');
        return;
    }

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} mr-2"></i>
        <span>${message}</span>
    `;

    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 5000);
}

function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
    }
}

function refreshDashboard() {
    const refreshIcon = document.getElementById('refresh-icon');
    if (refreshIcon) {
        refreshIcon.classList.add('fa-spin');
    }

    // Trigger HTMX refresh for stats
    const statsContainer = document.getElementById('stats-container');
    if (statsContainer) {
        htmx.trigger(statsContainer, 'refresh');
    }

    // Trigger HTMX refresh for active sessions
    const sessionsContainer = document.getElementById('active-sessions-container');
    if (sessionsContainer) {
        htmx.trigger(sessionsContainer, 'refresh');
    }

    setTimeout(() => {
        if (refreshIcon) {
            refreshIcon.classList.remove('fa-spin');
        }
        showToast('Dashboard refreshed!', 'success');
    }, 1000);
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
// SESSION TIMERS
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

// ============================================
// DASHBOARD INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Dashboard initialized');
    initializeDashboard();
});

// Listen for HTMX swaps and restart timers
document.body.addEventListener('htmx:afterSwap', (event) => {
    console.log('HTMX afterSwap event:', event.detail.target.id);

    // If the active sessions container was updated, restart timers
    if (event.detail.target.id === 'active-sessions-container') {
        console.log('Active sessions updated, restarting timers...');

        // Small delay to ensure DOM is fully updated
        setTimeout(() => {
            startAllTimers();
        }, 100);
    }
});

function initializeDashboard() {
    // Start session timers
    startAllTimers();

    // Load device cards
    loadDeviceCards();

    // Setup auto-refresh for stats
    setupStatsRefresh();
}

// ============================================
// LOAD DEVICE CARDS
// ============================================

async function loadDeviceCards() {
    const container = document.getElementById('devices-container');
    if (!container) return;

    try {
        // HTMX will handle this, but we can add loading state
        container.classList.add('opacity-50');
    } catch (error) {
        console.error('Error loading devices:', error);
        showToast('Failed to load devices', 'error');
    }
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
            // HTMX will auto-refresh the containers
        } else {
            showToast('Failed to start session', 'error');
        }
    } catch (error) {
        console.error('Error starting session:', error);
        showToast('An error occurred', 'error');
    }
}

// ============================================
// END SESSION WITH RECEIPT
// ============================================

async function endSessionWithReceipt(sessionId, paymentMethod = 1) {
    if (!confirm('Are you sure you want to end this session?')) {
        return;
    }

    try {
        // Stop the timer for this session
        stopTimer(sessionId);

        // Call the End endpoint
        const response = await fetch(`/Dashboard/End?sessionId=${sessionId}&paymentMethod=${paymentMethod}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const receiptHtml = await response.text();

            // Show receipt modal
            const receiptContent = document.getElementById('receipt-content');
            const receiptModal = document.getElementById('receipt-modal');

            if (receiptContent && receiptModal) {
                receiptContent.innerHTML = receiptHtml;
                receiptModal.classList.remove('hidden');
                document.body.style.overflow = 'hidden';
            }

            // Refresh active sessions to remove ended session
            const sessionsContainer = document.getElementById('active-sessions-container');
            if (sessionsContainer && typeof htmx !== 'undefined') {
                htmx.trigger(sessionsContainer, 'load');
            }

            // Show success toast
            showToast('Session ended successfully!', 'success');
        } else {
            showToast('Failed to end session', 'error');
        }
    } catch (error) {
        console.error('Error ending session:', error);
        showToast('An error occurred while ending the session', 'error');
    }
}

// Close receipt modal
function closeReceiptModal() {
    const modal = document.getElementById('receipt-modal');
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
    }
}

// Print receipt
function printReceipt(sessionId) {
    const receiptContent = document.getElementById('receipt-content');
    if (!receiptContent) return;

    // Create a new window for printing
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
        showToast('Please allow popups to print receipt', 'error');
        return;
    }

    // Write the receipt HTML with styles
    printWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Receipt - Session #${sessionId}</title>
            <script src="https://cdn.tailwindcss.com"></script>
            <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
            <style>
                @media print {
                    body { margin: 0; padding: 20px; }
                    .no-print { display: none !important; }
                }
            </style>
        </head>
        <body>
            ${receiptContent.innerHTML}
            <script>
                window.onload = function() {
                    setTimeout(() => {
                        window.print();
                        window.close();
                    }, 500);
                };
            </script>
        </body>
        </html>
    `);

    printWindow.document.close();
    showToast('Printing receipt...', 'info');
}

// ============================================
// STATS REFRESH
// ============================================

function setupStatsRefresh() {
    // Stats are auto-refreshed by HTMX, but we can add visual feedback
    document.addEventListener('htmx:afterSwap', (event) => {
        if (event.detail.target.id === 'stats-container') {
            // Add fade-in animation to new stats
            event.detail.target.querySelectorAll('.stat-card').forEach((card, index) => {
                card.style.animation = `slideInRight 0.3s ease-out ${index * 0.1}s`;
            });
        }
    });
}

// ============================================
// DEVICE STATUS UPDATES
// ============================================

function updateDeviceStatus(deviceId, status) {
    const deviceCard = document.querySelector(`[data-device-id="${deviceId}"]`);
    if (!deviceCard) return;

    // Remove all status classes
    deviceCard.classList.remove('available', 'in-use', 'maintenance', 'offline');

    // Add new status class
    deviceCard.classList.add(status.toLowerCase().replace(' ', '-'));

    // Update status badge
    const statusBadge = deviceCard.querySelector('.status-badge');
    if (statusBadge) {
        statusBadge.textContent = status;
        statusBadge.className = `status-badge status-${status.toLowerCase().replace(' ', '-')}`;
    }
}

// ============================================
// REAL-TIME NOTIFICATIONS
// ============================================

function showSessionNotification(sessionData) {
    const notification = document.createElement('div');
    notification.className = 'toast success';
    notification.innerHTML = `
        <i class="fas fa-play-circle text-green-500 text-xl"></i>
        <div class="flex-1">
            <p class="font-semibold text-gray-900">Session Started</p>
            <p class="text-sm text-gray-600">${sessionData.deviceName}</p>
        </div>
    `;

    const container = document.getElementById('toast-container');
    if (container) {
        container.appendChild(notification);

        setTimeout(() => {
            notification.remove();
        }, 5000);
    }
}

// ============================================
// DASHBOARD METRICS
// ============================================

class DashboardMetrics {
    constructor() {
        this.metrics = {
            revenue: 0,
            sessions: 0,
            activeDevices: 0
        };
    }

    update(newMetrics) {
        this.metrics = { ...this.metrics, ...newMetrics };
        this.render();
    }

    render() {
        // Update DOM elements with new metrics
        const revenueElement = document.getElementById('revenue-value');
        if (revenueElement) {
            this.animateValue(revenueElement, this.metrics.revenue);
        }
    }

    animateValue(element, targetValue) {
        const startValue = parseFloat(element.textContent.replace(/[^0-9.-]+/g, '')) || 0;
        const duration = 1000;
        const startTime = Date.now();

        const animate = () => {
            const elapsed = Date.now() - startTime;
            const progress = Math.min(elapsed / duration, 1);

            const currentValue = startValue + (targetValue - startValue) * progress;
            element.textContent = formatCurrency(currentValue);

            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };

        animate();
    }
}

const dashboardMetrics = new DashboardMetrics();

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

document.addEventListener('keydown', (e) => {
    // Alt + N: New Session
    if (e.altKey && e.key === 'n') {
        e.preventDefault();
        openModal('start-session-modal');
    }

    // Alt + D: View Devices
    if (e.altKey && e.key === 'd') {
        e.preventDefault();
        window.location.href = '/device';
    }

    // Alt + R: View Reports
    if (e.altKey && e.key === 'r') {
        e.preventDefault();
        window.location.href = '/report';
    }
});

// ============================================
// EXPORT DASHBOARD DATA
// ============================================

function exportDashboardData() {
    const data = {
        date: new Date().toISOString(),
        stats: dashboardMetrics.metrics,
        sessions: Array.from(activeTimers.keys())
    };

    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = `dashboard-${new Date().toISOString().split('T')[0]}.json`;
    a.click();

    URL.revokeObjectURL(url);
}

// ============================================
// CLEANUP ON PAGE UNLOAD
// ============================================

window.addEventListener('beforeunload', () => {
    // Stop all timers
    activeTimers.forEach(timer => timer.stop());
    activeTimers.clear();
});