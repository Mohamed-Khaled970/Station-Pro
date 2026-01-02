// wwwroot/js/device.js

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
                showToast('Device updated successfully!', 'success');
                closeModal('edit-device-modal');

                // Reload page after short delay
                setTimeout(() => location.reload(), 1000);
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
            showToast('Device deleted successfully!', 'success');

            // Remove device card with animation
            const deviceCard = document.querySelector(`[data-device-id="${deviceId}"]`);
            if (deviceCard) {
                deviceCard.style.opacity = '0';
                deviceCard.style.transform = 'scale(0.9)';

                setTimeout(() => {
                    deviceCard.remove();
                    updateDeviceStats();
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
// HANDLE DEVICE ADDED
// ============================================

function handleDeviceAdded() {
    closeModal('add-device-modal');
    showToast('Device added successfully!', 'success');

    const form = document.getElementById('add-device-form');
    if (form) form.reset();

    // Reload page to update stats
    setTimeout(() => location.reload(), 1000);
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

        // Number keys 1-4: Quick filter
        if (!e.ctrlKey && !e.metaKey && !e.altKey) {
            if (e.key === '1') filterByStatus('all');
            if (e.key === '2') filterByStatus('available');
            if (e.key === '3') filterByStatus('in-use');
            if (e.key === '4') filterByStatus('maintenance');
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

// ============================================
// EXPORT DEVICE LIST
// ============================================

function exportDeviceList() {
    const devices = [];
    document.querySelectorAll('.device-card:not(.hidden)').forEach(card => {
        const name = card.querySelector('h3')?.textContent;
        const type = card.querySelector('.text-sm.text-gray-500')?.textContent;
        const rate = card.querySelector('.text-blue-600')?.textContent;
        const status = card.querySelector('.status-badg