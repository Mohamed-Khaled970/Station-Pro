// wwwroot/js/device.js - Updated with Delete Modal

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

        const form = modal.querySelector('form');
        if (form) {
            form.reset();
        }
    }
}

// ============================================
// DELETE MODAL FUNCTIONS
// ============================================

let deviceToDelete = null;

function deleteDevice(deviceId, deviceName) {
    // Store device info for deletion
    deviceToDelete = { id: deviceId, name: deviceName };

    // Update modal with device name
    document.getElementById('delete-device-name').textContent = deviceName;

    // Open delete confirmation modal
    openModal('delete-device-modal');
}

function closeDeleteModal() {
    closeModal('delete-device-modal');
    deviceToDelete = null;
}

async function confirmDeleteDevice() {
    if (!deviceToDelete) return;

    const { id, name } = deviceToDelete;

    // Disable confirm button to prevent double-clicks
    const confirmBtn = document.getElementById('confirm-delete-btn');
    const originalHtml = confirmBtn.innerHTML;
    confirmBtn.disabled = true;
    confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>جاري الحذف...';

    try {
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(device|Device).*/, '');

        const response = await fetch(`${basePath}/device/delete/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            // Close modal
            closeDeleteModal();

            // Remove device card with animation
            const deviceCard = document.querySelector(`[data-device-id="${id}"]`);
            if (deviceCard) {
                deviceCard.style.opacity = '0';
                deviceCard.style.transform = 'scale(0.9)';

                setTimeout(() => {
                    deviceCard.remove();
                    updateDeviceStats();

                    // Show success notification
                    const successMsg = document.getElementById('device-deleted-success')?.value || 'Device deleted successfully!';
                    showSuccessNotification(
                        '✅ ' + successMsg,
                        `"${name}" تم حذف الجهاز`,
                        'fa-trash-alt'
                    );
                }, 300);
            }
        } else {
            const errorData = await response.json();
            const failedMsg = document.getElementById('failed-delete-device')?.value || 'Failed to delete device';
            showToast(errorData.message || failedMsg, 'error');

            // Re-enable button
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = originalHtml;
        }
    } catch (error) {
        console.error('Error deleting device:', error);
        const errorMsg = document.getElementById('error-deleting')?.value || 'An error occurred while deleting';
        showToast(errorMsg, 'error');

        // Re-enable button
        confirmBtn.disabled = false;
        confirmBtn.innerHTML = originalHtml;
    }
}

// ============================================
// MULTI-SESSION FIELD TOGGLE
// ============================================

function toggleMultiSessionFields() {
    const deviceType = document.getElementById('device-type-select').value;
    const multiSessionContainer = document.getElementById('multi-session-container');
    const supportsMultiCheckbox = document.getElementById('supports-multi-session');

    // Device types that support multi-session: PS5(1), PS4(2), PS3(3), Xbox(4), PingPong(6), Pool(7), Billiards(8)
    const multiSessionTypes = ['1', '2', '3', '4', '6', '7', '8'];

    if (multiSessionTypes.includes(deviceType)) {
        multiSessionContainer.classList.remove('hidden');
    } else {
        multiSessionContainer.classList.add('hidden');
        supportsMultiCheckbox.checked = false;
        toggleMultiRateField();
    }
}

function toggleMultiRateField() {
    const supportsMulti = document.getElementById('supports-multi-session').checked;
    const multiRateField = document.getElementById('multi-rate-field');
    const multiRateInput = document.getElementById('multi-session-rate-input');

    if (supportsMulti) {
        multiRateField.classList.remove('hidden');
        multiRateInput.required = true;
    } else {
        multiRateField.classList.add('hidden');
        multiRateInput.required = false;
        multiRateInput.value = '';
    }
}

function toggleEditMultiRate() {
    const supportsMulti = document.getElementById('edit-supports-multi').checked;
    const multiRateField = document.getElementById('edit-multi-rate-field');
    const multiRateInput = document.getElementById('edit-multi-rate');

    if (supportsMulti) {
        multiRateField.classList.remove('hidden');
        multiRateInput.required = true;
    } else {
        multiRateField.classList.add('hidden');
        multiRateInput.required = false;
        multiRateInput.value = '';
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
    setupAddDeviceForm();
    setupEditDeviceForm();
    setupKeyboardShortcuts();

    // Setup multi-session toggle listener
    const supportsMultiCheckbox = document.getElementById('supports-multi-session');
    if (supportsMultiCheckbox) {
        supportsMultiCheckbox.addEventListener('change', toggleMultiRateField);
    }
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
        const multiSession = device.dataset.multiSession || '';
        let isMatch = false;

        if (status === 'all') {
            isMatch = true;
        } else if (status === 'multi-session') {
            isMatch = multiSession === 'multi-session';
        } else {
            isMatch = deviceStatus === status;
        }

        if (isMatch) {
            device.classList.remove('hidden');
            visibleCount++;
        } else {
            device.classList.add('hidden');
        }
    });

    updateFilterStats(status, visibleCount);
}

// ============================================
// UPDATE NO RESULTS MESSAGE
// ============================================

function updateNoResultsMessage(visibleCount) {
    let noResultsDiv = document.getElementById('no-results-message');
    const noResultsText = document.getElementById('no-results-text')?.value || 'No devices found';
    const adjustFiltersText = document.getElementById('adjust-filters-text')?.value || 'Try adjusting your search or filters';

    if (visibleCount === 0) {
        if (!noResultsDiv) {
            noResultsDiv = document.createElement('div');
            noResultsDiv.id = 'no-results-message';
            noResultsDiv.className = 'col-span-full text-center py-12';
            noResultsDiv.innerHTML = `
                <i class="fas fa-search text-gray-300 text-5xl mb-4"></i>
                <p class="text-gray-600 font-medium">${noResultsText}</p>
                <p class="text-gray-400 text-sm mt-2">${adjustFiltersText}</p>
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
    const existing = document.getElementById('success-notification');
    if (existing) existing.remove();

    const awesomeText = document.getElementById('awesome-text')?.value || 'Awesome!';

    const notification = document.createElement('div');
    notification.id = 'success-notification';
    notification.className = 'success-notification-overlay';

    notification.innerHTML = `
        <div class="success-notification-content">
            <div class="confetti-container" id="confetti-container"></div>
            
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
                <div class="sparkle sparkle-1">✨</div>
                <div class="sparkle sparkle-2">⭐</div>
                <div class="sparkle sparkle-3">✨</div>
                <div class="sparkle sparkle-4">⭐</div>
            </div>
            
            <h3 class="success-notification-title">${title}</h3>
            <p class="success-notification-message">${message}</p>
            
            <div class="success-progress-bar">
                <div class="success-progress-fill"></div>
            </div>
            
            <div class="success-notification-actions">
                <button onclick="closeSuccessNotification()" class="success-notification-btn-primary">
                    <i class="fas fa-check mr-2"></i>
                    <span>${awesomeText}</span>
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.classList.add('show');
        createConfetti();
    }, 10);

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

    // Get localized messages from hidden inputs
    const successTitle = document.getElementById('device-added-title')?.value || 'Device Added Successfully! 🎉';
    const successMessage = document.getElementById('device-added-message')?.value || 'Your new device has been created and is ready to use.';

    showSuccessNotification(
        successTitle,
        successMessage,
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
        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(device|Device).*/, '');

        const response = await fetch(`${basePath}/device/get/${deviceId}`);

        if (!response.ok) {
            throw new Error('Failed to load device');
        }

        const device = await response.json();

        // Populate form
        document.getElementById('edit-device-id').value = device.id;
        document.getElementById('edit-device-name').value = device.name;
        document.getElementById('edit-single-rate').value = device.singleSessionRate;
        document.getElementById('edit-multi-rate').value = device.multiSessionRate || '';
        document.getElementById('edit-supports-multi').checked = device.supportsMultiSession;
        document.getElementById('edit-device-status').value = device.status;
        document.getElementById('edit-device-active').checked = device.isActive;

        // Show/hide multi rate field
        toggleEditMultiRate();

        openModal('edit-device-modal');
    } catch (error) {
        console.error('Error loading device:', error);
        const errorMsg = document.getElementById('failed-load-device')?.value || 'Failed to load device details';
        showToast(errorMsg, 'error');
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

        const currentPath = window.location.pathname;
        const basePath = currentPath.replace(/\/(device|Device).*/, '');

        const data = {
            name: formData.get('Name'),
            singleSessionRate: parseFloat(formData.get('SingleSessionRate')),
            multiSessionRate: formData.get('MultiSessionRate') ? parseFloat(formData.get('MultiSessionRate')) : null,
            supportsMultiSession: formData.get('SupportsMultiSession') === 'on',
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

                const updateTitle = document.getElementById('device-updated-title')?.value || 'Device Updated! ✨';
                const updateMessage = document.getElementById('device-updated-message')?.value || 'Your device has been updated successfully.';

                showSuccessNotification(
                    updateTitle,
                    updateMessage,
                    'fa-edit'
                );
            } else {
                const errorData = await response.json();
                const failedMsg = document.getElementById('failed-update-device')?.value || 'Failed to update device';
                showToast(errorData.message || failedMsg, 'error');
            }
        } catch (error) {
            console.error('Error updating device:', error);
            const errorMsg = document.getElementById('error-updating')?.value || 'An error occurred while updating';
            showToast(errorMsg, 'error');
        }
    });
}

// ============================================
// SETUP ADD DEVICE FORM
// ============================================

function setupAddDeviceForm() {
    const form = document.getElementById('add-device-form');
    if (!form) return;

    form.addEventListener('submit', (e) => {
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
        const activeElement = document.activeElement;
        const isTyping = activeElement.tagName === 'INPUT' ||
            activeElement.tagName === 'TEXTAREA' ||
            activeElement.tagName === 'SELECT';

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            document.getElementById('search-input')?.focus();
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal('add-device-modal');
        }

        if (!e.ctrlKey && !e.metaKey && !e.altKey && !isTyping) {
            if (e.key === '1') filterByStatus('all');
            if (e.key === '2') filterByStatus('available');
            if (e.key === '3') filterByStatus('in-use');
            if (e.key === '4') filterByStatus('multi-session');
        }

        if (e.key === 'Escape') {
            closeSuccessNotification();
            closeDeleteModal();

            const modals = document.querySelectorAll('.modal-overlay:not(.hidden)');
            modals.forEach(modal => closeModal(modal.id));
        }
    });
}

// ============================================
// QUICK START SESSION FROM DEVICE CARD
// ============================================

function quickStartDeviceSession(deviceId, deviceName, isMultiSession) {
    const singleText = document.getElementById('single-player-text')?.value || 'single-player';
    const multiText = document.getElementById('multi-player-text')?.value || 'multi-player';
    const startSessionText = document.getElementById('start-session-text')?.value || 'Start a';
    const onText = document.getElementById('on-text')?.value || 'on';

    const sessionType = isMultiSession ? multiText : singleText;
    const message = `${startSessionText} ${sessionType} ${onText} "${deviceName}"?`;

    if (confirm(message)) {
        console.log(`Starting ${sessionType} session on device ${deviceId}`);
        // This will be handled by your session creation logic
        // You can pass isMultiSession parameter to determine which rate to use
    }
}

// ============================================
// TOAST NOTIFICATION (Simple)
// ============================================

function showToast(message, type = 'info') {
    console.log(`[${type.toUpperCase()}] ${message}`);
    // You can implement a proper toast notification system here
    alert(message);
}