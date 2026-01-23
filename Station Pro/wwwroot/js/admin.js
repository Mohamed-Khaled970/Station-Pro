// ============================================
// ADMIN DASHBOARD FUNCTIONS
// ============================================

function toggleTenantStatus(tenantId, tenantName) {
    if (!confirm(`Are you sure you want to toggle the status for "${tenantName}"?`)) {
        return;
    }

    fetch(`/Admin/ToggleTenantStatus?tenantId=${tenantId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast(`Status updated successfully`, 'success');
                // Refresh tenants list
                htmx.trigger('#tenants-container', 'load');
            } else {
                showToast('Failed to update status', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('An error occurred', 'error');
        });
}

function toggleSubscriptionMenu(tenantId) {
    const menu = document.getElementById(`subscription-menu-${tenantId}`);
    if (!menu) return;

    // Close all other menus
    document.querySelectorAll('[id^="subscription-menu-"]').forEach(m => {
        if (m.id !== `subscription-menu-${tenantId}`) {
            m.classList.add('hidden');
        }
    });

    // Toggle current menu
    menu.classList.toggle('hidden');
}

function updateSubscription(tenantId, planValue) {
    const planNames = ['Free', 'Basic', 'Pro', 'Enterprise'];
    const planName = planNames[planValue];

    if (!confirm(`Change subscription to ${planName}?`)) {
        return;
    }

    fetch(`/Admin/UpdateSubscription?tenantId=${tenantId}&plan=${planValue}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast(`Subscription updated to ${planName}`, 'success');
                // Close menu
                document.getElementById(`subscription-menu-${tenantId}`)?.classList.add('hidden');
                // Refresh tenants list
                htmx.trigger('#tenants-container', 'load');
                // Refresh stats
                setTimeout(() => location.reload(), 1000);
            } else {
                showToast('Failed to update subscription', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('An error occurred', 'error');
        });
}

function viewTenantDetails(tenantId) {
    // TODO: Implement tenant details modal or redirect
    showToast('Tenant details coming soon...', 'info');
}

function showToast(message, type = 'info') {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'fixed top-4 right-4 z-50 space-y-2';
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    const icons = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        info: 'fa-info-circle',
        warning: 'fa-exclamation-triangle'
    };

    const colors = {
        success: 'bg-green-500',
        error: 'bg-red-500',
        info: 'bg-blue-500',
        warning: 'bg-yellow-500'
    };

    toast.className = `${colors[type]} text-white px-6 py-3 rounded-lg shadow-lg flex items-center gap-3 animate-slide-in`;
    toast.innerHTML = `
        <i class="fas ${icons[type]}"></i>
        <span>${message}</span>
    `;

    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('animate-slide-out');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
s
// Close subscription menus when clicking outside
document.addEventListener('click', (e) => {
    if (!e.target.closest('[onclick^="toggleSubscriptionMenu"]')) {
        document.querySelectorAll('[id^="subscription-menu-"]').forEach(menu => {
            menu.classList.add('hidden');
        });
    }
});

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    console.log('Admin dashboard initialized');
});