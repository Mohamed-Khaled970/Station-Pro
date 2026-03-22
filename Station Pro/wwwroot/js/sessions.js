// wwwroot/js/sessions.js

// ============================================
// UTILITY FUNCTIONS
// ============================================

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

// ============================================
// END SESSION FROM LIST
// ============================================

let pendingSessionId = null;
let selectedPaymentMethod = 1; // Default: Cash

function endSessionFromList(sessionId, deviceName, duration, cost) {
    pendingSessionId = parseInt(sessionId);

    document.getElementById('confirm-device-name').textContent = deviceName;
    document.getElementById('confirm-duration').textContent = duration;
    document.getElementById('confirm-cost').textContent = cost;

    selectedPaymentMethod = 1;
    const cashBtn = document.getElementById('payment-cash');
    const cardBtn = document.getElementById('payment-card');

    if (cashBtn && cardBtn) {
        cashBtn.classList.add('active');
        cardBtn.classList.remove('active');
    }

    openModal('end-session-confirm-modal');
}

function selectPaymentMethod(method) {
    selectedPaymentMethod = method;

    const cashBtn = document.getElementById('payment-cash');
    const cardBtn = document.getElementById('payment-card');

    if (method === 1) {
        cashBtn.classList.add('active');
        cardBtn.classList.remove('active');
    } else {
        cardBtn.classList.add('active');
        cashBtn.classList.remove('active');
    }
}

function closeEndSessionModal() {
    closeModal('end-session-confirm-modal');
}

async function confirmEndSession() {
    if (!pendingSessionId) return;

    try {
        closeEndSessionModal();

        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/[Ss]ession.*/, '');

        const response = await fetch(
            `${basePath}/Session/End?sessionId=${parseInt(pendingSessionId)}&paymentMethod=${selectedPaymentMethod}`,
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

            showToast('Session ended successfully!', 'success');
            setTimeout(() => window.location.reload(), 2000);
        } else {
            showToast('Failed to end session', 'error');
        }
    } catch (error) {
        showToast('An error occurred while ending the session', 'error');
    }
}

// ============================================
// SESSION DETAILS
// ============================================

async function viewSessionDetails(sessionId) {
    try {
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(session|Session).*/, '');

        const response = await fetch(`${basePath}/Session/Details?id=${sessionId}`);

        if (response.ok) {
            const html = await response.text();
            const detailsContent = document.getElementById('session-details-content');

            if (detailsContent) {
                detailsContent.innerHTML = html;
                openModal('session-details-modal');
            }
        } else {
            showToast('Failed to load session details', 'error');
        }
    } catch (error) {
        console.error('Error loading session details:', error);
        showToast('An error occurred while loading session details', 'error');
    }
}

// ============================================
// PRINT RECEIPT
// ============================================

async function printReceipt(sessionId) {
    try {
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(session|Session).*/, '');

        const response = await fetch(`${basePath}/Session/Receipt?id=${sessionId}`);

        if (response.ok) {
            const receiptHtml = await response.text();
            const receiptContent = document.getElementById('receipt-content');
            const receiptModal = document.getElementById('receipt-modal');

            if (receiptContent && receiptModal) {
                receiptContent.innerHTML = receiptHtml;
                receiptModal.classList.remove('hidden');
                document.body.style.overflow = 'hidden';
            }
        } else {
            showToast('Failed to load receipt', 'error');
        }
    } catch (error) {
        console.error('Error loading receipt:', error);
        showToast('An error occurred while loading the receipt', 'error');
    }
}

// ============================================
// PRINT RECEIPT CONTENT — opens a print window
// FIX: was missing entirely in sessions.js,
//      only existed in dashboard.js, so the
//      Print button inside the receipt modal
//      had no handler and did nothing.
// ============================================

function printReceiptContent() {
    const dataEl = document.getElementById('receipt-data');
    if (!dataEl) { showToast('No receipt to print', 'error'); return; }

    let d;
    try {
        d = JSON.parse(dataEl.getAttribute('data-receipt'));
    } catch (e) {
        showToast('Could not read receipt data', 'error');
        return;
    }

    const printWindow = window.open('', '_blank');
    if (!printWindow) { showToast('Please allow popups to print receipt', 'error'); return; }

    printWindow.document.write(`<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
<meta charset="UTF-8">
<title>Receipt</title>
<style>
  *{box-sizing:border-box;margin:0;padding:0}
  body{font-family:'Segoe UI',Tahoma,Arial,sans-serif;background:#f3f4f6;display:flex;align-items:flex-start;justify-content:center;padding:20px;min-height:100vh}
  .card{background:#fff;border-radius:14px;overflow:hidden;width:340px;box-shadow:0 8px 30px rgba(0,0,0,.12)}
  .hdr{background:linear-gradient(135deg,#22c55e,#059669);color:#fff;padding:22px 16px 18px;text-align:center}
  .hdr .circle{width:50px;height:50px;background:rgba(255,255,255,.25);border-radius:50%;display:flex;align-items:center;justify-content:center;margin:0 auto 10px;font-size:22px}
  .hdr h2{font-size:20px;font-weight:700;margin-bottom:4px}
  .hdr p{font-size:12px;opacity:.85}
  .body{padding:16px}
  .dev{text-align:center;padding-bottom:12px;margin-bottom:14px;border-bottom:1px dashed #d1d5db}
  .dev h3{font-size:18px;font-weight:700;color:#111827}
  .dev p{font-size:13px;color:#6b7280;margin-top:3px}
  .two{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:12px}
  .bblue{background:#eff6ff;border-radius:10px;padding:10px;text-align:center}
  .bblue .lbl{font-size:11px;color:#2563eb;margin-bottom:4px}
  .bblue .val{font-size:22px;font-weight:700;color:#1d4ed8;font-family:monospace;direction:ltr;unicode-bidi:embed;display:block}
  .trow{display:grid;grid-template-columns:1fr 1fr;gap:6px;margin-top:8px}
  .tbox{background:#f9fafb;border-radius:8px;padding:7px 4px;text-align:center}
  .tbox .lbl{font-size:10px;color:#9ca3af;margin-bottom:3px}
  .tbox .val{font-size:12px;font-weight:600;color:#111827;direction:ltr;unicode-bidi:embed;display:block}
  .bgreen{background:linear-gradient(135deg,#f0fdf4,#ecfdf5);border-radius:10px;padding:10px}
  .bgreen .lbl{font-size:11px;color:#6b7280;margin-bottom:4px}
  .bgreen .amt{font-size:28px;font-weight:800;color:#16a34a;direction:ltr;unicode-bidi:embed;display:block}
  .bgreen .cur{font-size:11px;color:#15803d;margin-top:2px}
  .ibox{background:#f9fafb;border-radius:8px;padding:10px;margin-top:8px;font-size:12px}
  .irow{display:flex;justify-content:space-between;align-items:center;padding:3px 0}
  .irow .k{color:#6b7280}
  .irow .v{font-weight:600;color:#111827;direction:ltr;unicode-bidi:embed}
  .sid{text-align:center;font-size:11px;color:#9ca3af;margin:12px 0 0;direction:ltr;unicode-bidi:embed}
  .ftr{background:#f9fafb;border-top:1px solid #f3f4f6;text-align:center;padding:8px;font-size:12px;color:#9ca3af;margin-top:14px}
  @media print{body{background:#fff;padding:0}.card{box-shadow:none;border-radius:0;width:100%}}
</style>
</head>
<body>
<div class="card">
  <div class="hdr">
    <div class="circle">&#10003;</div>
    <h2>${d.labelCompleted}</h2>
    <p>${d.labelThanks}</p>
  </div>
  <div class="body">
    <div class="dev"><h3>${d.deviceName}</h3><p>${d.customerName}</p></div>
    <div class="two">
      <div>
        <div class="bblue"><div class="lbl">${d.labelDuration}</div><span class="val">${d.duration}</span></div>
        <div class="trow">
          <div class="tbox"><div class="lbl">${d.labelStart}</div><span class="val">${d.startTime}</span></div>
          <div class="tbox"><div class="lbl">${d.labelEnd}</div><span class="val">${d.endTime}</span></div>
        </div>
      </div>
      <div>
        <div class="bgreen"><div class="lbl">${d.labelTotal}</div><span class="amt">${d.totalCost}</span><div class="cur">EGP</div></div>
        <div class="ibox">
          <div class="irow"><span class="k">${d.labelRate}</span><span class="v">${d.hourlyRate} ج.م/س</span></div>
          <div class="irow"><span class="k">${d.labelPayment}</span><span class="v">${d.paymentMethod}</span></div>
        </div>
      </div>
    </div>
    <div class="sid">${d.labelSession} #${d.sessionId} &bull; ${d.sessionDate} ${d.completedAt}</div>
  </div>
  <div class="ftr">Station Pro &#9829;</div>
</div>
<script>setTimeout(()=>{window.print();window.close();},400);<\/script>
</body></html>`);

    printWindow.document.close();
    showToast('Printing receipt...', 'info');
}

// ============================================
// RECEIPT MODAL
// ============================================

function closeReceiptModal() {
    const modal = document.getElementById('receipt-modal');
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
    }
}

// ============================================
// FILTERS
// ============================================

function applyFilters() {
    document.getElementById('filters-form').submit();
}

function clearFilters() {
    const currentPath = window.location.pathname;
    const basePath = currentPath.replace(/\/(session|Session).*/, '');
    window.location.href = `${basePath}/Session/Index`;
}

// ============================================
// SEARCH FUNCTIONALITY
// ============================================

let searchTimeout;

function handleSearchInput(input) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        document.getElementById('filters-form').submit();
    }, 500);
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeModal('session-details-modal');
        closeModal('start-session-modal');
        closeModal('receipt-modal');
        closeModal('end-session-confirm-modal');
    }

    if (e.altKey && e.key === 'n') {
        e.preventDefault();
        openModal('start-session-modal');
    }

    if (e.altKey && e.key === 'f') {
        e.preventDefault();
        const searchInput = document.querySelector('input[name="search"]');
        if (searchInput) searchInput.focus();
    }
});

// ============================================
// EXPORT TO CSV
// ============================================

function exportToCSV() {
    showToast('Exporting sessions...', 'info');
    const currentPath = window.location.pathname;
    const basePath = currentPath.replace(/\/(session|Session).*/, '');
    const params = new URLSearchParams(window.location.search);
    window.location.href = `${basePath}/Session/ExportCSV?${params.toString()}`;
}

function printMultipleReceipts() {
    showToast('Print multiple receipts feature coming soon!', 'info');
}

function refreshSessions() {
    location.reload();
}

// ============================================
// INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Sessions page initialized');

    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });

    initializeTooltips();
});

function initializeTooltips() {
    // Placeholder for tooltip initialization
}

// ============================================
// CLICK OUTSIDE TO CLOSE MODALS
// ============================================

document.addEventListener('click', (e) => {
    const sessionDetailsModal = document.getElementById('session-details-modal');
    if (sessionDetailsModal && !sessionDetailsModal.classList.contains('hidden')) {
        if (e.target === sessionDetailsModal) closeModal('session-details-modal');
    }

    const startSessionModal = document.getElementById('start-session-modal');
    if (startSessionModal && !startSessionModal.classList.contains('hidden')) {
        if (e.target === startSessionModal) closeModal('start-session-modal');
    }

    const receiptModal = document.getElementById('receipt-modal');
    if (receiptModal && !receiptModal.classList.contains('hidden')) {
        if (e.target === receiptModal) closeReceiptModal();
    }
});

// ============================================
// HTMX INTEGRATION
// ============================================

document.body.addEventListener('htmx:afterSwap', (event) => {
    console.log('HTMX swap completed');
    if (event.detail.target.id === 'session-details-content') {
        console.log('Session details loaded');
    }
});

document.body.addEventListener('htmx:responseError', (event) => {
    console.error('HTMX request failed:', event.detail);
    showToast('An error occurred. Please try again.', 'error');
});

// ============================================
// ANIMATION HELPERS
// ============================================

function animateValue(element, start, end, duration) {
    const startTime = Date.now();

    function update() {
        const elapsed = Date.now() - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const currentValue = start + (end - start) * progress;
        element.textContent = Math.round(currentValue);
        if (progress < 1) requestAnimationFrame(update);
    }

    update();
}

function animateStatistics() {
    const statElements = document.querySelectorAll('[data-animate-stat]');
    statElements.forEach(element => {
        const targetValue = parseInt(element.dataset.animateStat);
        animateValue(element, 0, targetValue, 1000);
    });
}

window.addEventListener('load', () => {
    animateStatistics();
});

// ============================================
// HELPER: Format Currency
// ============================================

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-EG', {
        style: 'currency',
        currency: 'EGP'
    }).format(amount);
}

// ============================================
// HELPER: Format Duration
// ============================================

function formatDuration(seconds) {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
}

// ============================================
// SESSION TYPE SELECTION
// ============================================

let selectedSessionType = 'single';

function handleDeviceSelection(selectElement) {
    const selectedOption = selectElement.options[selectElement.selectedIndex];
    const supportsMulti = selectedOption.dataset.supportsMulti === 'true';
    const singleRate = selectedOption.dataset.singleRate;
    const multiRate = selectedOption.dataset.multiRate;
    const sessionTypeContainer = document.getElementById('session-type-container');

    if (supportsMulti) {
        sessionTypeContainer.classList.remove('hidden');
        document.getElementById('single-rate-display').textContent =
            formatCurrency(parseFloat(singleRate)) + '/hr';
        document.getElementById('multi-rate-display').textContent =
            formatCurrency(parseFloat(multiRate)) + '/hr';
        selectSessionType('single');
    } else {
        sessionTypeContainer.classList.add('hidden');
        selectSessionType('single');
    }
}

function selectSessionType(type) {
    selectedSessionType = type;

    const singleBtn = document.getElementById('session-type-single');
    const multiBtn = document.getElementById('session-type-multi');
    const input = document.getElementById('session-type-input');

    if (type === 'single') {
        singleBtn.classList.add('active');
        multiBtn.classList.remove('active');
    } else {
        multiBtn.classList.add('active');
        singleBtn.classList.remove('active');
    }

    if (input) input.value = type;
}

function handleSessionStartResponse(event) {
    const xhr = event.detail.xhr;

    if (xhr.status === 200) {
        try {
            const response = JSON.parse(xhr.responseText);
            if (response.success) {
                showToast(response.message || 'Session started successfully!', 'success');
                closeModal('start-session-modal');
                document.getElementById('start-session-form').reset();
                document.getElementById('session-type-container').classList.add('hidden');
                setTimeout(() => window.location.reload(), 1500);
            } else {
                showToast(response.message || 'Failed to start session', 'error');
            }
        } catch (e) {
            showToast('Session started! Refreshing...', 'success');
            setTimeout(() => window.location.reload(), 1500);
        }
    } else {
        showToast('Failed to start session. Please try again.', 'error');
    }
}

function openStartSessionModal() {
    const form = document.getElementById('start-session-form');
    if (form) form.reset();

    const sessionTypeContainer = document.getElementById('session-type-container');
    if (sessionTypeContainer) sessionTypeContainer.classList.add('hidden');

    selectSessionType('single');
    openModal('start-session-modal');
}

// ============================================
// TABLE SORTING
// ============================================

function sortTable(column) {
    showToast('Sorting feature coming soon!', 'info');
}

// ============================================
// EXPORTS
// ============================================

window.handleDeviceSelection = handleDeviceSelection;
window.selectSessionType = selectSessionType;
window.handleSessionStartResponse = handleSessionStartResponse;
window.openStartSessionModal = openStartSessionModal;
window.viewSessionDetails = viewSessionDetails;
window.printReceipt = printReceipt;
window.printReceiptContent = printReceiptContent;   // ← NEW: used by Print button inside receipt modal
window.closeReceiptModal = closeReceiptModal;
window.openModal = openModal;
window.closeModal = closeModal;
window.applyFilters = applyFilters;
window.clearFilters = clearFilters;
window.exportToCSV = exportToCSV;
window.refreshSessions = refreshSessions;
window.endSessionFromList = endSessionFromList;
window.selectPaymentMethod = selectPaymentMethod;
window.closeEndSessionModal = closeEndSessionModal;
window.confirmEndSession = confirmEndSession;