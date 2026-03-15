// =============================================================================
// FILE: wwwroot/js/pendingsubscriptions.js
//
// Handles the Pending Subscriptions admin page:
//   - Status filter tabs
//   - Proof image modal
//   - Approve modal  → delegates actual fetch to approveSubscription() in admin.js
//   - Reject modal   → delegates actual fetch to rejectSubscription()  in admin.js
//   - Auto-refresh pending badge every 30 s
//
// NOTE: approveSubscription() and rejectSubscription() are defined in admin.js.
//       Do NOT redefine them here — that was causing the approve/reject conflict.
// =============================================================================

'use strict';

// =============================================================================
// FILTER
// =============================================================================

function filterStatus(status) {
    const items = document.querySelectorAll('.subscription-item');
    const buttons = document.querySelectorAll('.filter-btn');

    buttons.forEach(btn => btn.classList.remove('active', 'bg-blue-600', 'text-white'));
    event.target.classList.add('active', 'bg-blue-600', 'text-white');

    items.forEach(item => {
        const show = status === 'all' || item.dataset.status === status;
        item.style.display = show ? 'block' : 'none';
        if (show) item.classList.add('fade-in');
    });
}

// =============================================================================
// PROOF IMAGE MODAL
// =============================================================================

function openProofModal(imageUrl, tenantName) {
    document.getElementById('modalImage').src = imageUrl;
    document.getElementById('modalTitle').textContent = tenantName + ' - Payment Proof';
    document.getElementById('proofModal').style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeProofModal() {
    document.getElementById('proofModal').style.display = 'none';
    document.body.style.overflow = 'auto';
}

// =============================================================================
// APPROVE MODAL
// The actual fetch is handled by window.approveSubscription(id) in admin.js.
// =============================================================================

function showApproveModal(id, tenantName, plan) {
    document.getElementById('approveTenantName').textContent = tenantName;
    document.getElementById('approvePlan').textContent = plan;
    document.getElementById('approveModal').style.display = 'block';
    document.body.style.overflow = 'hidden';

    // Wire the confirm button to admin.js handler with the correct id
    const confirmBtn = document.getElementById('approveConfirmBtn');
    if (confirmBtn) {
        // Remove any previous listener to avoid stacking calls
        confirmBtn.replaceWith(confirmBtn.cloneNode(true));
        document.getElementById('approveConfirmBtn').addEventListener('click', () => {
            closeApproveModal();
            window.approveSubscription(id);
        });
    }
}

function closeApproveModal() {
    document.getElementById('approveModal').style.display = 'none';
    document.body.style.overflow = 'auto';
}

// =============================================================================
// REJECT MODAL
// Stores the id on window._currentRejectId so rejectSubscription() in admin.js
// can read it. The submit button in the modal calls window.rejectSubscription().
// =============================================================================

function showRejectModal(id, tenantName) {
    window._currentRejectId = id;   // read by rejectSubscription() in admin.js

    document.getElementById('rejectTenantName').textContent = tenantName;
    document.getElementById('rejectReason').value = '';
    document.getElementById('rejectModal').style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeRejectModal() {
    document.getElementById('rejectModal').style.display = 'none';
    document.body.style.overflow = 'auto';
    window._currentRejectId = null;
}

// =============================================================================
// CLOSE MODALS ON OUTSIDE CLICK
// =============================================================================

window.addEventListener('click', (event) => {
    const proofModal = document.getElementById('proofModal');
    const approveModal = document.getElementById('approveModal');
    const rejectModal = document.getElementById('rejectModal');

    if (event.target === proofModal) closeProofModal();
    if (event.target === approveModal) closeApproveModal();
    if (event.target === rejectModal) closeRejectModal();
});

// =============================================================================
// KEYBOARD SHORTCUTS
// =============================================================================

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeProofModal();
        closeApproveModal();
        closeRejectModal();
    }
});

// =============================================================================
// INIT
// =============================================================================

document.addEventListener('DOMContentLoaded', () => {
    // Activate the "All" filter button by default
    const allButton = document.querySelector('.filter-btn[data-status="all"]');
    if (allButton) allButton.classList.add('bg-blue-600', 'text-white', 'active');

    // Honour ?filter= query param
    const filter = new URLSearchParams(window.location.search).get('filter');
    if (filter) {
        const btn = document.querySelector(`.filter-btn[data-status="${filter}"]`);
        btn?.click();
    }
});

// =============================================================================
// AUTO-REFRESH PENDING BADGE (every 30 s)
// =============================================================================

setInterval(async () => {
    try {
        const data = await fetch('/Admin/GetPendingCount').then(r => r.json());
        const badge = document.querySelector('.pending-count-badge');
        if (!badge) return;
        if (data.count > 0) {
            badge.textContent = data.count;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    } catch {
        // silently ignore network errors
    }
}, 30_000);