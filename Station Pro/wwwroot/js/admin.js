// =============================================================================
// FILE: wwwroot/js/admin.js
// Admin dashboard — tenant management functions.
// Used by: Index.cshtml and _TenantsList.cshtml partial
// =============================================================================

'use strict';

// =============================================================================
// TENANT STATUS TOGGLE
// =============================================================================

function toggleTenantStatus(tenantId, tenantName) {
    if (!confirm(`Are you sure you want to toggle the status for "${tenantName}"?`)) return;

    fetch(`/Admin/ToggleTenantStatus?tenantId=${tenantId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                showToast('Status updated successfully.', 'success');
                htmx.trigger('#tenants-container', 'load');
            } else {
                showToast(data.message || 'Failed to update status.', 'error');
            }
        })
        .catch(() => showToast('An error occurred.', 'error'));
}

// =============================================================================
// SUBSCRIPTION PLAN DROPDOWN
// =============================================================================

function toggleSubscriptionMenu(tenantId) {
    const menu = document.getElementById(`subscription-menu-${tenantId}`);
    if (!menu) return;

    // Close all other open menus first
    document.querySelectorAll('[id^="subscription-menu-"]').forEach(m => {
        if (m.id !== `subscription-menu-${tenantId}`) m.classList.add('hidden');
    });

    menu.classList.toggle('hidden');
}

function updateSubscription(tenantId, planValue) {
    const planNames = ['Free', 'Basic', 'Pro', 'Enterprise'];
    const planName = planNames[planValue] ?? 'Unknown';

    if (!confirm(`Change subscription to ${planName}?`)) return;

    fetch(`/Admin/UpdateSubscription?tenantId=${tenantId}&plan=${planValue}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                showToast(`Subscription updated to ${planName}.`, 'success');
                document.getElementById(`subscription-menu-${tenantId}`)?.classList.add('hidden');
                htmx.trigger('#tenants-container', 'load');
                setTimeout(() => location.reload(), 1200);
            } else {
                showToast(data.message || 'Failed to update subscription.', 'error');
            }
        })
        .catch(() => showToast('An error occurred.', 'error'));
}

// =============================================================================
// TENANT DETAILS MODAL
// =============================================================================

function viewTenantDetails(tenantId) {
    fetch(`/Admin/TenantDetails?tenantId=${tenantId}`)
        .then(r => r.json())
        .then(data => {
            if (!data.success) { showToast(data.message || 'Could not load details.', 'error'); return; }
            const t = data.tenant;

            // Populate modal fields
            document.getElementById('detail-name').textContent = t.name ?? '—';
            document.getElementById('detail-email').textContent = t.email ?? '—';
            document.getElementById('detail-phone').textContent = t.phoneNumber ?? '—';
            document.getElementById('detail-plan').textContent = t.plan ?? '—';
            document.getElementById('detail-status').textContent = t.isActive ? 'Active' : 'Inactive';
            document.getElementById('detail-status').className = t.isActive
                ? 'font-semibold text-green-600'
                : 'font-semibold text-red-500';
            document.getElementById('detail-joined').textContent = t.joinedDate ?? '—';
            document.getElementById('detail-expires').textContent = t.subscriptionEndDate ?? 'N/A';
            document.getElementById('detail-devices').textContent = t.totalDevices ?? 0;
            document.getElementById('detail-sessions').textContent = t.totalSessions ?? 0;
            document.getElementById('detail-revenue').textContent = (t.totalRevenue ?? 0).toLocaleString() + ' EGP';
            document.getElementById('detail-monthly').textContent = (t.monthlyRevenue ?? 0).toLocaleString() + ' EGP';

            document.getElementById('tenantDetailsModal').classList.remove('hidden');
        })
        .catch(() => showToast('Network error loading tenant details.', 'error'));
}

function closeTenantDetailsModal() {
    document.getElementById('tenantDetailsModal').classList.add('hidden');
}

// =============================================================================
// TOAST NOTIFICATIONS
// =============================================================================

// Exposed as window.adminShowToast so PendingSubscriptions.cshtml can reuse it
window.adminShowToast = function showToast(message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'fixed top-4 right-4 z-50 flex flex-col gap-2';
        document.body.appendChild(container);
    }

    const colors = {
        success: 'bg-green-500',
        error: 'bg-red-500',
        info: 'bg-blue-500',
        warning: 'bg-yellow-500'
    };
    const icons = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        info: 'fa-info-circle',
        warning: 'fa-exclamation-triangle'
    };

    const toast = document.createElement('div');
    toast.className = `${colors[type] ?? 'bg-gray-700'} text-white px-5 py-3 rounded-xl shadow-lg flex items-center gap-3 text-sm font-medium`;
    toast.innerHTML = `<i class="fas ${icons[type] ?? 'fa-info-circle'}"></i><span>${message}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.cssText += 'opacity:0;transition:opacity 0.3s';
        setTimeout(() => toast.remove(), 300);
    }, 3500);
};

// Alias so inline calls to showToast() inside Index.cshtml also work
function showToast(message, type = 'info') {
    window.adminShowToast(message, type);
}

// =============================================================================
// GLOBAL EVENTS
// =============================================================================

// Close plan dropdown when clicking anywhere outside it
document.addEventListener('click', (e) => {
    if (!e.target.closest('[onclick^="toggleSubscriptionMenu"]')) {
        document.querySelectorAll('[id^="subscription-menu-"]').forEach(m => m.classList.add('hidden'));
    }
});

// Close tenant details modal on Escape
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeTenantDetailsModal();
});

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    console.log('Admin dashboard initialized.');
});