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
    // Store the session ID
    pendingSessionId = sessionId;

    // Update confirmation modal with session details
    document.getElementById('confirm-device-name').textContent = deviceName;
    document.getElementById('confirm-duration').textContent = duration;
    document.getElementById('confirm-cost').textContent = cost;

    // Reset payment method to Cash
    selectedPaymentMethod = 1;
    const cashBtn = document.getElementById('payment-cash');
    const cardBtn = document.getElementById('payment-card');

    if (cashBtn && cardBtn) {
        cashBtn.classList.add('active');
        cardBtn.classList.remove('active');
    }

    // Show confirmation modal
    openModal('end-session-confirm-modal');
}

function selectPaymentMethod(method) {
    selectedPaymentMethod = method;

    // Update button states
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
        // Close confirmation modal
        closeEndSessionModal();

        // Get current path (handle tenant URLs)
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(session|Session).*/, '');

        // Call the End endpoint
        const response = await fetch(`${basePath}/Dashboard/End?sessionId=${pendingSessionId}&paymentMethod=${selectedPaymentMethod}`, {
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

            // Show success toast and reload page
            showToast('Session ended successfully!', 'success');

            // Reload page after a short delay
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        } else {
            showToast('Failed to end session', 'error');
        }
    } catch (error) {
        console.error('Error ending session:', error);
        showToast('An error occurred while ending the session', 'error');
    }
}

// ============================================
// SESSION DETAILS
// ============================================

async function viewSessionDetails(sessionId) {
    try {
        // Get current path (handle tenant URLs)
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(session|Session).*/, '');

        // Fetch session details
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
        // Get current path (handle tenant URLs)
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(session|Session).*/, '');

        // Fetch receipt
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

// Close receipt modal
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
    // Get current path
    const currentPath = window.location.pathname;
    const basePath = currentPath.replace(/\/(session|Session).*/, '');

    // Redirect to base sessions page without filters
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
    }, 500); // Debounce for 500ms
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

document.addEventListener('keydown', (e) => {
    // Escape: Close modals
    if (e.key === 'Escape') {
        closeModal('session-details-modal');
        closeModal('start-session-modal');
        closeModal('receipt-modal');
    }

    // Alt + N: Start new session
    if (e.altKey && e.key === 'n') {
        e.preventDefault();
        openModal('start-session-modal');
    }

    // Alt + F: Focus on search
    if (e.altKey && e.key === 'f') {
        e.preventDefault();
        const searchInput = document.querySelector('input[name="search"]');
        if (searchInput) {
            searchInput.focus();
        }
    }
});

// ============================================
// TABLE SORTING (OPTIONAL ENHANCEMENT)
// ============================================

function sortTable(column) {
    console.log(`Sorting by ${column}`);
    // This can be implemented later if needed
    showToast('Sorting feature coming soon!', 'info');
}

// ============================================
// EXPORT TO CSV (OPTIONAL)
// ============================================

function exportToCSV() {
    showToast('Exporting sessions...', 'info');

    // Get current path
    const currentPath = window.location.pathname;
    const basePath = currentPath.replace(/\/(session|Session).*/, '');

    // Build export URL with current filters
    const params = new URLSearchParams(window.location.search);
    window.location.href = `${basePath}/Session/ExportCSV?${params.toString()}`;
}

// ============================================
// PRINT MULTIPLE RECEIPTS
// ============================================

function printMultipleReceipts() {
    showToast('Print multiple receipts feature coming soon!', 'info');
}

// ============================================
// REFRESH DATA
// ============================================

function refreshSessions() {
    location.reload();
}

// ============================================
// INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Sessions page initialized');

    // Add fade-in animation to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });

    // Initialize tooltips (if using a tooltip library)
    initializeTooltips();
});

function initializeTooltips() {
    // Placeholder for tooltip initialization
    // Can be implemented with a library like Tippy.js if needed
}

// ============================================
// CLICK OUTSIDE TO CLOSE MODALS
// ============================================

document.addEventListener('click', (e) => {
    // Close session details modal when clicking outside
    const sessionDetailsModal = document.getElementById('session-details-modal');
    if (sessionDetailsModal && !sessionDetailsModal.classList.contains('hidden')) {
        if (e.target === sessionDetailsModal) {
            closeModal('session-details-modal');
        }
    }

    // Close start session modal when clicking outside
    const startSessionModal = document.getElementById('start-session-modal');
    if (startSessionModal && !startSessionModal.classList.contains('hidden')) {
        if (e.target === startSessionModal) {
            closeModal('start-session-modal');
        }
    }

    // Close receipt modal when clicking outside
    const receiptModal = document.getElementById('receipt-modal');
    if (receiptModal && !receiptModal.classList.contains('hidden')) {
        if (e.target === receiptModal) {
            closeReceiptModal();
        }
    }
});

// ============================================
// HTMX INTEGRATION
// ============================================

// Listen for HTMX events
document.body.addEventListener('htmx:afterSwap', (event) => {
    console.log('HTMX swap completed');

    // Re-initialize any components after HTMX swap
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

        if (progress < 1) {
            requestAnimationFrame(update);
        }
    }

    update();
}

// ============================================
// STATISTICS ANIMATIONS
// ============================================

function animateStatistics() {
    const statElements = document.querySelectorAll('[data-animate-stat]');

    statElements.forEach(element => {
        const targetValue = parseInt(element.dataset.animateStat);
        animateValue(element, 0, targetValue, 1000);
    });
}

// Call on page load
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

// Export functions for use in other scripts
if (typeof window !== 'undefined') {
    window.viewSessionDetails = viewSessionDetails;
    window.printReceipt = printReceipt;
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
}