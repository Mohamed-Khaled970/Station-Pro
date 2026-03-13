// wwwroot/js/dashboard.js

let pendingSessionId = null;
let selectedPaymentMethod = 1;

// ============================================
// UTILITY FUNCTIONS
// ============================================

function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) { console.warn('Toast container not found'); return; }
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} mr-2"></i><span>${message}</span>`;
    toastContainer.appendChild(toast);
    setTimeout(() => toast.remove(), 5000);
}

function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) { modal.classList.remove('hidden'); document.body.style.overflow = 'hidden'; }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) { modal.classList.add('hidden'); document.body.style.overflow = 'auto'; }
}

function refreshDashboard() {
    const refreshIcon = document.getElementById('refresh-icon');
    if (refreshIcon) refreshIcon.classList.add('fa-spin');
    const statsContainer = document.getElementById('stats-container');
    if (statsContainer) htmx.trigger(statsContainer, 'refresh');
    const sessionsContainer = document.getElementById('active-sessions-container');
    if (sessionsContainer) htmx.trigger(sessionsContainer, 'refresh');
    setTimeout(() => {
        if (refreshIcon) refreshIcon.classList.remove('fa-spin');
        showToast('Dashboard refreshed!', 'success');
    }, 1000);
}

// ============================================
// DASHBOARD INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Dashboard initialized');
    initializeDashboard();
});

function initializeDashboard() {
    setupStatsRefresh();
}

// ============================================
// START SESSION MODAL
// ============================================

async function openStartSessionModal() {
    const select = document.querySelector('#start-session-modal select[name="DeviceId"]');
    if (select) { select.innerHTML = '<option value="">Loading devices...</option>'; select.disabled = true; }
    openModal('start-session-modal');
    try {
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(dashboard|Dashboard).*/, '');
        const res = await fetch(`${basePath}/Dashboard/DeviceOptions`);
        if (res.ok) {
            const devices = await res.json();
            if (select) {
                if (devices.length === 0) {
                    select.innerHTML = '<option value="">No available devices</option>';
                } else {
                    select.innerHTML = '<option value="">Choose a device...</option>';
                    devices.forEach(d => {
                        const option = document.createElement('option');
                        option.value = d.id;
                        option.textContent = d.name;
                        select.appendChild(option);
                    });
                }
            }
        } else if (select) {
            select.innerHTML = '<option value="">Failed to load devices</option>';
        }
    } catch (err) {
        console.error('Error loading devices for modal:', err);
        if (select) select.innerHTML = '<option value="">Failed to load devices</option>';
    } finally {
        if (select) select.disabled = false;
    }
}

// ============================================
// END SESSION
// FIX: endSessionWithReceipt was reading device name, duration and cost
// from the wrong DOM elements.
//
// The _ActiveSessions partial renders:
//   - id="timer-{id}"          → HIDDEN span (source of truth, textContent is empty)
//   - .timer-display-{id}      → VISIBLE timer text (updated by session-timer.js)
//   - .cost-display-{id}       → VISIBLE cost text  (updated by session-timer.js)
//   - h4 inside the card       → device name
//
// The old code did:
//   timerElement.textContent   → always "" because the hidden span has no text
//   costElement.textContent    → same problem (was getElementById("cost-{id}") which doesn't exist)
//   .querySelector('h4')       → wrong ancestor, the hidden span's closest('.rounded-lg')
//                                 may not contain the h4 in the new partial structure
//
// FIX: read from the VISIBLE class-based display elements instead.
// ============================================

function endSessionWithReceipt(sessionId) {
    pendingSessionId = sessionId;

    // ✅ Read the VISIBLE timer and cost elements (class-based, updated by session-timer.js)
    const timerDisplay = document.querySelector(`.timer-display-${sessionId}`);
    const costDisplay = document.querySelector(`.cost-display-${sessionId}`);

    // ✅ Get device name from the card's h4, walking up from the hidden timer element
    const hiddenTimerEl = document.getElementById(`timer-${sessionId}`);
    let deviceName = 'Unknown Device';
    if (hiddenTimerEl) {
        // Walk up to the session card container and find the h4
        const card = hiddenTimerEl.closest('[data-session-id]') ||
            hiddenTimerEl.closest('.bg-gradient-to-r') ||
            hiddenTimerEl.parentElement;
        if (card) {
            const h4 = card.querySelector('h4');
            if (h4) deviceName = h4.textContent.trim();
        }
    }

    document.getElementById('confirm-device-name').textContent = deviceName;
    document.getElementById('confirm-duration').textContent =
        timerDisplay ? timerDisplay.textContent.trim() : '00:00:00';
    document.getElementById('confirm-cost').textContent =
        costDisplay ? costDisplay.textContent.trim() : 'EGP 0.00';

    selectedPaymentMethod = 1;
    document.getElementById('payment-cash')?.classList.add('active');
    document.getElementById('payment-card')?.classList.remove('active');

    openModal('end-session-confirm-modal');
}

function selectPaymentMethod(method) {
    selectedPaymentMethod = method;
    const cashBtn = document.getElementById('payment-cash');
    const cardBtn = document.getElementById('payment-card');
    if (method === 1) { cashBtn?.classList.add('active'); cardBtn?.classList.remove('active'); }
    else { cardBtn?.classList.add('active'); cashBtn?.classList.remove('active'); }
}

function closeEndSessionModal() { closeModal('end-session-confirm-modal'); }

async function confirmEndSession() {
    if (!pendingSessionId) return;
    try {
        closeEndSessionModal();
        if (window.timerManager) timerManager.removeTimer(String(pendingSessionId));

        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(dashboard|Dashboard).*/, '');

        const response = await fetch(
            `${basePath}/Dashboard/End?sessionId=${pendingSessionId}&paymentMethod=${selectedPaymentMethod}`,
            { method: 'POST', headers: { 'Content-Type': 'application/json' } }
        );

        if (response.ok) {
            const receiptHtml = await response.text();
            const receiptContent = document.getElementById('receipt-content');
            const receiptModal = document.getElementById('receipt-modal');
            if (receiptContent && receiptModal) {
                receiptContent.innerHTML = receiptHtml;
                receiptModal.classList.remove('hidden');
                document.body.style.overflow = 'hidden';
            }
            if (window.timerManager) timerManager.removeTimer(String(pendingSessionId));
            setTimeout(() => {
                const sessionsContainer = document.getElementById('active-sessions-container');
                if (sessionsContainer && typeof htmx !== 'undefined') htmx.trigger(sessionsContainer, 'load');
            }, 300);
            showToast('Session ended successfully!', 'success');
        } else {
            showToast('Failed to end session', 'error');
        }
    } catch (error) {
        console.error('Error ending session:', error);
        showToast('An error occurred while ending the session', 'error');
    }
}

function closeReceiptModal() {
    const modal = document.getElementById('receipt-modal');
    if (modal) { modal.classList.add('hidden'); document.body.style.overflow = 'auto'; }
}

function printReceipt(sessionId) {
    const receiptContent = document.getElementById('receipt-content');
    if (!receiptContent) return;
    const printWindow = window.open('', '_blank');
    if (!printWindow) { showToast('Please allow popups to print receipt', 'error'); return; }
    printWindow.document.write(`<!DOCTYPE html><html><head><title>Receipt - Session #${sessionId}</title>
        <script src="https://cdn.tailwindcss.com"><\/script>
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
        <style>@media print { body { margin:0; padding:20px; } .no-print { display:none !important; } }</style>
        </head><body>${receiptContent.innerHTML}
        <script>window.onload=function(){setTimeout(()=>{window.print();window.close();},500);};<\/script>
        </body></html>`);
    printWindow.document.close();
    showToast('Printing receipt...', 'info');
}

// ============================================
// STATS REFRESH
// ============================================

function setupStatsRefresh() {
    document.addEventListener('htmx:afterSwap', (event) => {
        if (event.detail.target.id === 'stats-container') {
            event.detail.target.querySelectorAll('.stat-card').forEach((card, index) => {
                card.style.animation = `slideInRight 0.3s ease-out ${index * 0.1}s`;
            });
        }
    });
}

// ============================================
// SESSION START SUCCESS NOTIFICATION
// ============================================

document.body.addEventListener('sessionStarted', function (event) {
    const { deviceName, sessionType, rate } = event.detail;
    showSessionStartedNotification(deviceName, sessionType, rate);
});

function showSessionStartedNotification(deviceName, sessionType, rate) {
    const messages = getLocalizedMessages();
    const sessionTypeText = sessionType === 'multi' ? messages.multiSession : messages.singleSession;
    const overlay = document.createElement('div');
    overlay.className = 'success-notification-overlay';
    overlay.id = 'session-success-notification';
    overlay.innerHTML = `
        <div class="success-notification-content">
            <div class="success-notification-icon-wrapper">
                <div class="success-notification-icon-circle">
                    <i class="fas fa-check success-notification-icon"></i>
                </div>
            </div>
            <h2 class="success-notification-title">${messages.sessionStarted}</h2>
            <p class="success-notification-message">${deviceName} - ${sessionTypeText} (${formatCurrency(rate)}/${messages.hour})</p>
            <div class="success-notification-actions">
                <button onclick="closeSessionNotification()" class="success-notification-btn-primary">
                    <i class="fas fa-check mr-2"></i>${messages.ok}
                </button>
            </div>
        </div>`;
    document.body.appendChild(overlay);
    setTimeout(() => overlay.classList.add('show'), 10);
}

function closeSessionNotification() {
    const overlay = document.getElementById('session-success-notification');
    if (!overlay) return;
    overlay.classList.remove('show');
    setTimeout(() => { overlay.remove(); window.location.reload(); }, 300);
}

function getLocalizedMessages() {
    const el = document.getElementById('localized-messages');
    if (el) { try { return JSON.parse(el.dataset.messages); } catch (e) { /* fallback */ } }
    return { sessionStarted: 'Session Started Successfully!', singleSession: 'Single Session', multiSession: 'Multi Session', hour: 'hr', ok: 'OK' };
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-EG', { style: 'currency', currency: 'EGP' }).format(amount);
}

document.addEventListener('DOMContentLoaded', function () {
    console.log('Dashboard notification handler ready');
});

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

document.addEventListener('keydown', (e) => {
    if (e.altKey && e.key === 'n') { e.preventDefault(); openStartSessionModal(); }
    if (e.altKey && e.key === 'd') { e.preventDefault(); window.location.href = '/device'; }
    if (e.altKey && e.key === 'r') { e.preventDefault(); window.location.href = '/report'; }
    if (e.key === 'Escape') {
        closeModal('end-session-confirm-modal');
        closeModal('receipt-modal');
        closeModal('start-session-modal');
    }
});

function exportDashboardData() {
    const data = { date: new Date().toISOString(), stats: dashboardMetrics?.metrics ?? {} };
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `dashboard-${new Date().toISOString().split('T')[0]}.json`; a.click();
    URL.revokeObjectURL(url);
}