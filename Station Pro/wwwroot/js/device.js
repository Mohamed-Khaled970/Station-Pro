// wwwroot/js/device.js

// ============================================
// MODAL FUNCTIONS
// ============================================

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

        // Reset form if it exists
        const form = modal.querySelector('form');
        if (form) {
            form.reset();
        }
    }
}

// ============================================
// INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Device page initialized');
    initializeDevicePage();
});

function initializeDevicePage() {
    // Setup form handlers
    setupAddDeviceForm();
    setupEditDeviceForm();

    // Setup keyboard shortcuts
    setupKeyboardShortcuts();
}

// ============================================
// FILTER DEVICES BY SEARCH
// ============================================

function filterDevices() {
    const searchTerm = document.getElementById('search-input').value.toLowerCase().trim();
    const devices = document.querySelectorAll('.device-card');
    let visibleCount = 0;

    devices.forEach(device => {
        const deviceName = device.dataset.deviceName || '';
        const isMatch = deviceName.includes(searchTerm);

        if (isMatch) {
            device.classList.remove('hidden');
            visibleCount++;
        } else {
            device.classList.add('hidden');
        }
    });

    // Show no results message if needed
    updateNoResultsMessage(visibleCount);
}

// ============================================
// FILTER DEVICES BY STATUS
// ============================================

function filterByStatus(status) {
    const devices = document.querySelectorAll('.device-card');
    const buttons = document.querySelectorAll('.filter-btn');
    let visibleCount = 0;

    // Update active button
    buttons.forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.filter === status) {
            btn.classList.add('active');
        }
    });

    // Filter devices
    devices.forEach(device => {
        const deviceStatus = device.dataset.deviceStatus || '';
        const isMatch = status === 'all' || deviceStatus === status;

        if (isMatch) {
            device.classList.remove('hidden');
            visibleCount++;
        } else {
            device.classList.add('hidden');
        }
    });

    // Update stats
    updateFilterStats(status, visibleCount);
}

// ============================================
// UPDATE NO RESULTS MESSAGE
// ============================================

function updateNoResultsMessage(visibleCount) {
    let noResultsDiv = document.getElementById('no-results-message');

    if (visibleCount === 0) {
        if (!noResultsDiv) {
            noResultsDiv = document.createElement('div');
            noResultsDiv.id = 'no-results-message';
            noResultsDiv.className = 'col-span-full text-center py-12';
            noResultsDiv.innerHTML = `
                <i class="fas fa-search text-gray-300 text-5xl mb-4"></i>
                <p class="text-gray-600 font-medium">No devices found</p>
                <p class="text-gray-400 text-sm mt-2">Try adjusting your search or filters</p>
            `;
            document.getElementById('devices-grid').appendChild(noResultsDiv);
        }
    } else {
        if (noResultsDiv) {
            noResultsDiv.remove();
        }
    }
}

// ============================================
// UPDATE FILTER STATS
// ============================================

function updateFilterStats(status, count) {
    console.log(`Filtered by ${status}: ${count} devices`);
}

// ============================================
// ENHANCED SUCCESS NOTIFICATION WITH CONFETTI
// ============================================

function showSuccessNotification(title, message, icon = 'fa-check-circle') {
    // Remove any existing notification
    const existing = document.getElementById('success-notification');
    if (existing) existing.remove();

    // Create notification overlay
    const notification = document.createElement('div');
    notification.id = 'success-notification';
    notification.className = 'success-notification-overlay';

    notification.innerHTML = `
        <div class="success-notification-content">
            <!-- Confetti Container -->
            <div class="confetti-container" id="confetti-container"></div>
            
            <!-- Success Icon with animated circle -->
            <div class="success-notification-icon-wrapper">
                <div class="success-notification-icon-circle">
                    <i class="fas ${icon} success-notification-icon"></i>
                </div>
                <div class="success-notification-checkmark">
                    <svg class="checkmark" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 52 52">
                        <circle class="checkmark-circle" cx="26" cy="26" r="25" fill="none"/>
                        <path class="checkmark-check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
                    </svg>
                </div>
                <!-- Sparkle effects -->
                <div class="sparkle sparkle-1">✨</div>
                <div class="sparkle sparkle-2">⭐</div>
                <div class="sparkle sparkle-3">✨</div>
                <div class="sparkle sparkle-4">⭐</div>
            </div>
            
            <h3 class="success-notification-title">${title}</h3>
            <p class="success-notification-message">${message}</p>
            
            <!-- Progress bar -->
            <div class="success-progress-bar">
                <div class="success-progress-fill"></div>
            </div>
            
            <div class="success-notification-actions">
                <button onclick="closeSuccessNotification()" class="success-notification-btn-primary">
                    <i class="fas fa-check mr-2"></i>
                    <span>Awesome!</span>
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(notification);

    // Trigger animation
    setTimeout(() => {
        notification.classList.add('show');
        createConfetti();
    }, 10);

    // Auto close after 5 seconds
    setTimeout(() => {
        closeSuccessNotification();
    }, 5000);
}

function createConfetti() {
    const container = document.getElementById('confetti-container');
    if (!container) return;

    const colors = ['#10b981', '#3b82f6', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];
    const shapes = ['circle', 'square', 'triangle'];

    for (let i = 0; i < 50; i++) {
        setTimeout(() => {
            const confetti = document.createElement('div');
            confetti.className = `confetti confetti-${shapes[Math.floor(Math.random() * shapes.length)]}`;
            confetti.style.left = Math.random() * 100 + '%';
            confetti.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
            confetti.style.animationDelay = Math.random() * 0.3 + 's';
            confetti.style.animationDuration = (Math.random() * 2 + 2) + 's';

            container.appendChild(confetti);

            // Remove after animation
            setTimeout(() => confetti.remove(), 4000);
        }, i * 30);
    }
}

function closeSuccessNotification() {
    const notification = document.getElementById('success-notification');
    if (notification) {
        notification.classList.remove('show');
        notification.classList.add('hide');
        setTimeout(() => {
            notification.remove();
            location.reload();
        }, 400);
    }
}

// ============================================
// HANDLE DEVICE ADDED
// ============================================

function handleDeviceAdded() {
    closeModal('add-device-modal');

    // Show beautiful success notification
    showSuccessNotification(
        'Device Added Successfully! 🎉',
        'Your new device has been created and is ready to use. You can now start sessions on this device.',
        'fa-gamepad'
    );

    const form = document.getElementById('add-device-form');
    if (form) form.reset();
}

// ============================================
// EDIT DEVICE
// ============================================

async function editDevice(deviceId) {
    try {
        // Get current path (handle tenant URLs)
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(device|Device).*/, '');

        // Fetch device data
        const response = await fetch(`${basePath}/device/get/${deviceId}`);

        if (!response.ok) {
            throw new Error('Failed to load device');
        }

        const device = await response.json();

        // Populate form
        document.getElementById('edit-device-id').value = device.id;
        document.getElementById('edit-device-name').value = device.name;
        document.getElementById('edit-device-rate').value = device.hourlyRate;
        document.getElementById('edit-device-status').value = device.status;
        document.getElementById('edit-device-active').checked = device.isActive;

        // Open modal
        openModal('edit-device-modal');
    } catch (error) {
        console.error('Error loading device:', error);
        showToast('Failed to load device details', 'error');
    }
}

// ============================================
// SETUP EDIT DEVICE FORM
// ============================================

function setupEditDeviceForm() {
    const form = document.getElementById('edit-device-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = new FormData(e.target);
        const deviceId = formData.get('Id');

        // Get current path
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(device|Device).*/, '');

        const data = {
            name: formData.get('Name'),
            hourlyRate: parseFloat(formData.get('HourlyRate')),
            status: parseInt(formData.get('Status')),
            isActive: formData.get('IsActive') === 'on'
        };

        try {
            const response = await fetch(`${basePath}/device/update/${deviceId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                closeModal('edit-device-modal');
                showSuccessNotification(
                    'Device Updated! ✨',
                    'Your device has been updated successfully with the new information.',
                    'fa-edit'
                );
            } else {
                const errorData = await response.json();
                showToast(errorData.message || 'Failed to update device', 'error');
            }
        } catch (error) {
            console.error('Error updating device:', error);
            showToast('An error occurred while updating', 'error');
        }
    });
}

// ============================================
// DELETE DEVICE
// ============================================

async function deleteDevice(deviceId, deviceName) {
    // Confirm deletion
    const confirmed = confirm(
        `⚠️ Delete Device\n\nAre you sure you want to delete "${deviceName}"?\n\nThis action cannot be undone.`
    );

    if (!confirmed) return;

    try {
        // Get current path
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(device|Device).*/, '');

        const response = await fetch(`${basePath}/device/delete/${deviceId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            // Remove device card with animation
            const deviceCard = document.querySelector(`[data-device-id="${deviceId}"]`);
            if (deviceCard) {
                deviceCard.style.opacity = '0';
                deviceCard.style.transform = 'scale(0.9)';

                setTimeout(() => {
                    deviceCard.remove();
                    updateDeviceStats();
                    showToast('Device deleted successfully!', 'success');
                }, 300);
            }
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'Failed to delete device', 'error');
        }
    } catch (error) {
        console.error('Error deleting device:', error);
        showToast('An error occurred while deleting', 'error');
    }
}

// ============================================
// SETUP ADD DEVICE FORM
// ============================================

function setupAddDeviceForm() {
    const form = document.getElementById('add-device-form');
    if (!form) return;

    // You can add custom validation or handling here
    form.addEventListener('submit', (e) => {
        // HTMX will handle the actual submission
        // This is just for additional client-side logic if needed
        console.log('Adding new device...');
    });
}

// ============================================
// UPDATE DEVICE STATISTICS
// ============================================

function updateDeviceStats() {
    const devices = document.querySelectorAll('.device-card:not(.hidden)');
    const total = devices.length;
    const available = document.querySelectorAll('[data-device-status="available"]:not(.hidden)').length;
    const inUse = document.querySelectorAll('[data-device-status="in-use"]:not(.hidden)').length;
    const maintenance = document.querySelectorAll('[data-device-status="maintenance"]:not(.hidden)').length;

    console.log('Device Stats:', { total, available, inUse, maintenance });
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

function setupKeyboardShortcuts() {
    document.addEventListener('keydown', (e) => {
        // Don't trigger shortcuts if user is typing in an input, textarea, or select
        const activeElement = document.activeElement;
        const isTyping = activeElement.tagName === 'INPUT' ||
            activeElement.tagName === 'TEXTAREA' ||
            activeElement.tagName === 'SELECT';

        // Ctrl/Cmd + K: Focus search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            document.getElementById('search-input')?.focus();
        }

        // Ctrl/Cmd + N: Add new device
        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal('add-device-modal');
        }

        // Number keys 1-4: Quick filter (only when NOT typing)
        if (!e.ctrlKey && !e.metaKey && !e.altKey && !isTyping) {
            if (e.key === '1') filterByStatus('all');
            if (e.key === '2') filterByStatus('available');
            if (e.key === '3') filterByStatus('in-use');
            if (e.key === '4') filterByStatus('maintenance');
        }

        // Escape: Close success notification or modals
        if (e.key === 'Escape') {
            closeSuccessNotification();

            // Close any open modal
            const modals = document.querySelectorAll('.modal-overlay:not(.hidden)');
            modals.forEach(modal => closeModal(modal.id));
        }
    });
}

// ============================================
// QUICK START SESSION FROM DEVICE CARD
// ============================================

function quickStartSession(deviceId, deviceName) {
    if (confirm(`Start a new session on "${deviceName}"?`)) {
        // HTMX will handle this via the button
        console.log(`Starting session on device ${deviceId}`);
    }
}