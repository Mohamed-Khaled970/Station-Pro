// =============================================================================
// FILE: wwwroot/js/admin.js
// Admin dashboard — tenant management + subscription actions.
// =============================================================================

'use strict';

// =============================================================================
// CUSTOM CONFIRM MODAL
// =============================================================================

function _ensureConfirmModal() {
    if (document.getElementById('admin-confirm-modal')) return;

    const modal = document.createElement('div');
    modal.id = 'admin-confirm-modal';
    modal.style.cssText = 'display:none; position:fixed; inset:0; background:rgba(0,0,0,.5); backdrop-filter:blur(4px); z-index:9999; align-items:center; justify-content:center; padding:1rem;';
    modal.innerHTML = `
        <div id="admin-confirm-box"
             style="background:#fff; border-radius:1.25rem; box-shadow:0 25px 60px rgba(0,0,0,.2); width:100%; max-width:22rem; transform:scale(.95); opacity:0; transition:transform .2s ease, opacity .2s ease;"
             onclick="event.stopPropagation()">
            <div style="padding:2rem 1.75rem 1.25rem; text-align:center;">
                <div id="acm-icon-wrap" style="width:4.5rem; height:4.5rem; border-radius:1rem; display:flex; align-items:center; justify-content:center; margin:0 auto 1.25rem;"></div>
                <h3 id="acm-title" style="font-size:1.05rem; font-weight:700; color:#111827; margin:0 0 .5rem;"></h3>
                <p  id="acm-message" style="font-size:.875rem; color:#6b7280; line-height:1.55; margin:0;"></p>
            </div>
            <div style="display:flex; gap:.75rem; padding:0 1.5rem 1.5rem;">
                <button id="acm-cancel"
                        style="flex:1; padding:.7rem; border:2px solid #e5e7eb; border-radius:.75rem; font-size:.875rem; font-weight:600; color:#374151; background:#fff; cursor:pointer; transition:background .15s;"
                        onmouseover="this.style.background='#f9fafb'" onmouseout="this.style.background='#fff'"
                        onclick="closeAdminConfirm()">
                    Cancel
                </button>
                <button id="acm-ok"
                        style="flex:1; padding:.7rem; border:none; border-radius:.75rem; font-size:.875rem; font-weight:600; color:#fff; cursor:pointer; transition:filter .15s;"
                        onmouseover="this.style.filter='brightness(1.1)'" onmouseout="this.style.filter='brightness(1)'">
                </button>
            </div>
        </div>`;
    document.body.appendChild(modal);
    modal.addEventListener('click', closeAdminConfirm);
}

const _acmPalette = {
    blue: { wrap: '#dbeafe', icon: '#2563eb', btn: '#2563eb' },
    green: { wrap: '#dcfce7', icon: '#16a34a', btn: '#16a34a' },
    red: { wrap: '#fee2e2', icon: '#dc2626', btn: '#dc2626' },
    yellow: { wrap: '#fef3c7', icon: '#d97706', btn: '#d97706' },
    purple: { wrap: '#f3e8ff', icon: '#9333ea', btn: '#9333ea' },
};

function showAdminConfirm({ title, message, confirmText = 'Confirm', color = 'blue', icon = 'fa-question-circle' }, onConfirm) {
    _ensureConfirmModal();
    const p = _acmPalette[color] ?? _acmPalette.blue;

    const wrap = document.getElementById('acm-icon-wrap');
    wrap.style.background = p.wrap;
    wrap.innerHTML = `<i class="fas ${icon}" style="font-size:1.75rem; color:${p.icon};"></i>`;

    document.getElementById('acm-title').textContent = title;
    document.getElementById('acm-message').textContent = message;

    const ok = document.getElementById('acm-ok');
    ok.style.background = p.btn;
    ok.textContent = confirmText;
    ok.onclick = () => { closeAdminConfirm(); onConfirm(); };

    const modal = document.getElementById('admin-confirm-modal');
    const box = document.getElementById('admin-confirm-box');
    modal.style.display = 'flex';
    requestAnimationFrame(() => requestAnimationFrame(() => {
        box.style.transform = 'scale(1)';
        box.style.opacity = '1';
    }));
}

function closeAdminConfirm() {
    const modal = document.getElementById('admin-confirm-modal');
    const box = document.getElementById('admin-confirm-box');
    if (!modal) return;
    box.style.transform = 'scale(.95)';
    box.style.opacity = '0';
    setTimeout(() => { modal.style.display = 'none'; }, 200);
}

// =============================================================================
// TENANT STATUS TOGGLE
// =============================================================================

function toggleTenantStatus(tenantId, tenantName, isActive) {
    const deactivating = isActive !== false;

    showAdminConfirm({
        title: deactivating ? `Deactivate "${tenantName}"?` : `Activate "${tenantName}"?`,
        message: deactivating
            ? 'Their dashboard access will be blocked immediately. You can re-activate them at any time.'
            : 'This will restore full dashboard access for this tenant right away.',
        confirmText: deactivating ? 'Yes, Deactivate' : 'Yes, Activate',
        color: deactivating ? 'red' : 'green',
        icon: deactivating ? 'fa-ban' : 'fa-check-circle',
    }, () => {
        fetch(`/Admin/ToggleTenantStatus?tenantId=${tenantId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    showToast(
                        deactivating
                            ? `"${tenantName}" has been deactivated.`
                            : `"${tenantName}" is active again! 🎉`,
                        deactivating ? 'warning' : 'success'
                    );
                    if (typeof htmx !== 'undefined') htmx.trigger('#tenants-container', 'load');
                } else {
                    showToast(data.message || 'Failed to update status.', 'error');
                }
            })
            .catch(() => showToast('A network error occurred. Please try again.', 'error'));
    });
}

// =============================================================================
// SUBSCRIPTION PLAN CHANGE
// =============================================================================

function toggleSubscriptionMenu(tenantId) {
    const menu = document.getElementById(`subscription-menu-${tenantId}`);
    if (!menu) return;
    document.querySelectorAll('[id^="subscription-menu-"]').forEach(m => {
        if (m.id !== `subscription-menu-${tenantId}`) m.classList.add('hidden');
    });
    menu.classList.toggle('hidden');
}

function updateSubscription(tenantId, planValue) {
    const plans = [
        { name: 'Free', color: 'blue', icon: 'fa-gift' },
        { name: 'Basic', color: 'blue', icon: 'fa-star' },
        { name: 'Pro', color: 'purple', icon: 'fa-gem' },
        { name: 'Enterprise', color: 'yellow', icon: 'fa-crown' },
    ];
    const plan = plans[planValue] ?? plans[0];

    document.getElementById(`subscription-menu-${tenantId}`)?.classList.add('hidden');

    showAdminConfirm({
        title: `Switch to ${plan.name}?`,
        message: `The tenant's plan will be changed to ${plan.name} immediately.`,
        confirmText: `Switch to ${plan.name}`,
        color: plan.color,
        icon: plan.icon,
    }, () => {
        fetch(`/Admin/UpdateSubscription?tenantId=${tenantId}&plan=${planValue}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    showToast(`Plan changed to ${plan.name} successfully ✨`, 'success');
                    if (typeof htmx !== 'undefined') htmx.trigger('#tenants-container', 'load');
                    setTimeout(() => location.reload(), 1400);
                } else {
                    showToast(data.message || 'Failed to update plan.', 'error');
                }
            })
            .catch(() => showToast('A network error occurred. Please try again.', 'error'));
    });
}

// =============================================================================
// APPROVE SUBSCRIPTION  (used by PendingSubscriptions.cshtml)
// =============================================================================

window.approveSubscription = function (id) {
    showAdminConfirm({
        title: 'Approve this subscription?',
        message: 'The tenant will gain full access to their plan right away.',
        confirmText: 'Yes, Approve',
        color: 'green',
        icon: 'fa-check-circle',
    }, () => {
        fetch(`/Admin/ApproveSubscription?id=${id}`, { method: 'POST' })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    window.updateRowStatus(id, 'Approved');
                    showToast('Subscription approved! The tenant now has access 🎉', 'success');
                } else {
                    showToast(data.message || 'Failed to approve.', 'error');
                }
            })
            .catch(() => showToast('A network error occurred. Please try again.', 'error'));
    });
};

// =============================================================================
// TENANT DETAILS MODAL
// =============================================================================

function viewTenantDetails(tenantId) {
    fetch(`/Admin/TenantDetails?tenantId=${tenantId}`)
        .then(r => r.json())
        .then(data => {
            if (!data.success) { showToast(data.message || 'Could not load details.', 'error'); return; }
            const t = data.tenant;

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
    document.getElementById('tenantDetailsModal')?.classList.add('hidden');
}

// =============================================================================
// TOAST NOTIFICATIONS
// =============================================================================

window.adminShowToast = function (message, type = 'info') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = 'position:fixed; bottom:1.5rem; right:1.5rem; z-index:9998; display:flex; flex-direction:column; gap:.5rem;';
        document.body.appendChild(container);
    }

    const palette = {
        success: { bg: '#16a34a', icon: 'fa-check-circle' },
        error: { bg: '#dc2626', icon: 'fa-exclamation-circle' },
        warning: { bg: '#d97706', icon: 'fa-exclamation-triangle' },
        info: { bg: '#2563eb', icon: 'fa-info-circle' },
    };
    const s = palette[type] ?? palette.info;

    const toast = document.createElement('div');
    toast.style.cssText = `
        background:${s.bg}; color:#fff; display:flex; align-items:center; gap:.75rem;
        padding:.875rem 1rem .875rem 1.125rem; border-radius:.875rem;
        box-shadow:0 10px 30px rgba(0,0,0,.2); min-width:17rem; max-width:22rem;
        font-size:.875rem; font-weight:500; animation:toastSlideIn .3s ease-out;`;
    toast.innerHTML = `
        <i class="fas ${s.icon}" style="font-size:1.1rem; flex-shrink:0;"></i>
        <span style="flex:1; line-height:1.45;">${message}</span>
        <button onclick="this.parentElement.remove()"
                style="background:rgba(255,255,255,.15); border:none; border-radius:.5rem; color:#fff; cursor:pointer;
                       width:1.75rem; height:1.75rem; display:flex; align-items:center; justify-content:center; flex-shrink:0;">
            <i class="fas fa-times" style="font-size:.7rem;"></i>
        </button>`;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.transition = 'opacity .3s, transform .3s';
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(110%)';
        setTimeout(() => toast.remove(), 320);
    }, 4000);
};

function showToast(message, type = 'info') { window.adminShowToast(message, type); }

// =============================================================================
// GLOBAL EVENTS
// =============================================================================

document.addEventListener('click', (e) => {
    if (!e.target.closest('[onclick^="toggleSubscriptionMenu"]')) {
        document.querySelectorAll('[id^="subscription-menu-"]').forEach(m => m.classList.add('hidden'));
    }
});

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeAdminConfirm();
        closeTenantDetailsModal();
    }
});

document.addEventListener('DOMContentLoaded', () => console.log('Admin dashboard ready.'));