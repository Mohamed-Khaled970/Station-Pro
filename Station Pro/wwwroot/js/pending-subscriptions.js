// ============================================
// PENDING SUBSCRIPTIONS PAGE FUNCTIONS
// ============================================

let currentSubscriptionId = null;

// Filter Status
function filterStatus(status) {
    const items = document.querySelectorAll('.subscription-item');
    const buttons = document.querySelectorAll('.filter-btn');

    // Update button states
    buttons.forEach(btn => {
        btn.classList.remove('active', 'bg-blue-600', 'text-white');
    });
    event.target.classList.add('active', 'bg-blue-600', 'text-white');

    // Filter items
    items.forEach(item => {
        if (status === 'all' || item.dataset.status === status) {
            item.style.display = 'block';
            item.classList.add('fade-in');
        } else {
            item.style.display = 'none';
        }
    });
}

// Modal Functions
function openProofModal(imageUrl, tenantName) {
    const modal = document.getElementById('proofModal');
    const modalImage = document.getElementById('modalImage');
    const modalTitle = document.getElementById('modalTitle');

    modalImage.src = imageUrl;
    modalTitle.textContent = tenantName + ' - Payment Proof';
    modal.style.display = 'block';
    document.body.style.overflow = 'hidden'; // Prevent background scroll
}

function closeProofModal() {
    const modal = document.getElementById('proofModal');
    modal.style.display = 'none';
    document.body.style.overflow = 'auto'; // Restore scroll
}

function showApproveModal(id, tenantName, plan) {
    currentSubscriptionId = id;
    document.getElementById('approveTenantName').textContent = tenantName;
    document.getElementById('approvePlan').textContent = plan;
    document.getElementById('approveModal').style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeApproveModal() {
    document.getElementById('approveModal').style.display = 'none';
    document.body.style.overflow = 'auto';
}

function showRejectModal(id, tenantName) {
    currentSubscriptionId = id;
    document.getElementById('rejectTenantName').textContent = tenantName;
    document.getElementById('rejectReason').value = '';
    document.getElementById('rejectModal').style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeRejectModal() {
    document.getElementById('rejectModal').style.display = 'none';
    document.body.style.overflow = 'auto';
}

// Approve Subscription
async function approveSubscription() {
    if (!currentSubscriptionId) {
        showToast('Error: No subscription selected', 'error');
        return;
    }

    try {
        const response = await fetch(`/Admin/ApproveSubscription/${currentSubscriptionId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (response.ok) {
            showToast('Subscription approved successfully!', 'success');
            closeApproveModal();

            // Reload page after short delay to show updated status
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showToast(data.message || 'Failed to approve subscription', 'error');
        }
    } catch (error) {
        console.error('Error approving subscription:', error);
        showToast('An error occurred. Please try again.', 'error');
    }
}

// Reject Subscription
async function rejectSubscription() {
    const reason = document.getElementById('rejectReason').value.trim();

    if (!reason) {
        showToast('Please provide a reason for rejection', 'warning');
        document.getElementById('rejectReason').focus();
        return;
    }

    if (!currentSubscriptionId) {
        showToast('Error: No subscription selected', 'error');
        return;
    }

    try {
        const response = await fetch(`/Admin/RejectSubscription/${currentSubscriptionId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ reason: reason })
        });

        const data = await response.json();

        if (response.ok) {
            showToast('Subscription rejected', 'success');
            closeRejectModal();

            // Reload page after short delay
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            showToast(data.message || 'Failed to reject subscription', 'error');
        }
    } catch (error) {
        console.error('Error rejecting subscription:', error);
        showToast('An error occurred. Please try again.', 'error');
    }
}

// Toast Notification
function showToast(message, type = 'info') {
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

// Close modals on outside click
window.onclick = function (event) {
    const proofModal = document.getElementById('proofModal');
    const approveModal = document.getElementById('approveModal');
    const rejectModal = document.getElementById('rejectModal');

    if (event.target == proofModal) {
        closeProofModal();
    }
    if (event.target == approveModal) {
        closeApproveModal();
    }
    if (event.target == rejectModal) {
        closeRejectModal();
    }
}

// Keyboard shortcuts
document.addEventListener('keydown', function (e) {
    // ESC key closes modals
    if (e.key === 'Escape') {
        closeProofModal();
        closeApproveModal();
        closeRejectModal();
    }
});

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    console.log('Pending subscriptions page initialized');

    // Set active filter on page load
    const allButton = document.querySelector('.filter-btn[data-status="all"]');
    if (allButton) {
        allButton.classList.add('bg-blue-600', 'text-white', 'active');
    }

    // Check URL parameters for filter
    const urlParams = new URLSearchParams(window.location.search);
    const filter = urlParams.get('filter');
    if (filter) {
        const filterButton = document.querySelector(`.filter-btn[data-status="${filter}"]`);
        if (filterButton) {
            filterButton.click();
        }
    }
});

// Auto-refresh pending count every 30 seconds
setInterval(async () => {
    try {
        const response = await fetch('/Admin/GetPendingCount');
        const data = await response.json();

        // Update badge if exists
        const badge = document.querySelector('.pending-count-badge');
        if (badge && data.count > 0) {
            badge.textContent = data.count;
            badge.style.display = 'flex';
        } else if (badge && data.count === 0) {
            badge.style.display = 'none';
        }
    } catch (error) {
        console.error('Error fetching pending count:', error);
    }
}, 30000); // 30 seconds