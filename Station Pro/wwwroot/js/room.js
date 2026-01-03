// wwwroot/js/room.js

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
// INITIALIZATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Room page initialized');
    initializeRoomPage();
});

function initializeRoomPage() {
    setupAddRoomForm();
    setupEditRoomForm();
    setupKeyboardShortcuts();
}

// ============================================
// FILTER ROOMS BY SEARCH
// ============================================

function filterRooms() {
    const searchTerm = document.getElementById('search-input').value.toLowerCase().trim();
    const rooms = document.querySelectorAll('.room-card');
    let visibleCount = 0;

    rooms.forEach(room => {
        const roomName = room.dataset.roomName || '';
        const isMatch = roomName.includes(searchTerm);

        if (isMatch) {
            room.classList.remove('hidden');
            visibleCount++;
        } else {
            room.classList.add('hidden');
        }
    });

    updateNoResultsMessage(visibleCount);
}

// ============================================
// FILTER ROOMS BY STATUS
// ============================================

function filterByStatus(status) {
    const rooms = document.querySelectorAll('.room-card');
    const buttons = document.querySelectorAll('.filter-btn');
    let visibleCount = 0;

    buttons.forEach(btn => {
        btn.classList.remove('active');
        if (btn.dataset.filter === status) {
            btn.classList.add('active');
        }
    });

    rooms.forEach(room => {
        const roomStatus = room.dataset.roomStatus || '';
        const isMatch = status === 'all' || roomStatus === status;

        if (isMatch) {
            room.classList.remove('hidden');
            visibleCount++;
        } else {
            room.classList.add('hidden');
        }
    });
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
                <p class="text-gray-600 font-medium">No rooms found</p>
                <p class="text-gray-400 text-sm mt-2">Try adjusting your search or filters</p>
            `;
            document.getElementById('rooms-grid').appendChild(noResultsDiv);
        }
    } else {
        if (noResultsDiv) {
            noResultsDiv.remove();
        }
    }
}

// ============================================
// ENHANCED SUCCESS NOTIFICATION
// ============================================

function showSuccessNotification(title, message, icon = 'fa-check-circle') {
    const existing = document.getElementById('success-notification');
    if (existing) existing.remove();

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
                    <span>Awesome!</span>
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
    const shapes = ['circle', 'square'];

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
// HANDLE ROOM ADDED
// ============================================

function handleRoomAdded() {
    closeModal('add-room-modal');

    showSuccessNotification(
        'Room Added Successfully! 🎉',
        'Your new room has been created and is ready for bookings.',
        'fa-door-open'
    );

    const form = document.getElementById('add-room-form');
    if (form) form.reset();
}

// ============================================
// EDIT ROOM
// ============================================

async function editRoom(roomId) {
    try {
        const response = await fetch(`/Room/Get/${roomId}`);

        if (!response.ok) {
            throw new Error('Failed to load room');
        }

        const room = await response.json();

        document.getElementById('edit-room-id').value = room.id;
        document.getElementById('edit-room-name').value = room.name;
        document.getElementById('edit-room-rate').value = room.hourlyRate;
        document.getElementById('edit-room-capacity').value = room.capacity;
        document.getElementById('edit-room-devices').value = room.deviceCount || 0;
        document.getElementById('edit-room-ac').checked = room.hasAC;
        document.getElementById('edit-room-active').checked = room.isActive;

        openModal('edit-room-modal');
    } catch (error) {
        console.error('Error loading room:', error);
        alert('Failed to load room details');
    }
}

// ============================================
// SETUP EDIT ROOM FORM
// ============================================

function setupEditRoomForm() {
    const form = document.getElementById('edit-room-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const formData = new FormData(e.target);
        const roomId = formData.get('Id');

        const data = {
            name: formData.get('Name'),
            hourlyRate: parseFloat(formData.get('HourlyRate')),
            capacity: parseInt(formData.get('Capacity')),
            deviceCount: parseInt(formData.get('DeviceCount')) || 0,
            hasAC: formData.get('HasAC') === 'on',
            isActive: formData.get('IsActive') === 'on'
        };

        try {
            const response = await fetch(`/Room/Update/${roomId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                closeModal('edit-room-modal');
                showSuccessNotification(
                    'Room Updated! ✨',
                    'Your room has been updated successfully with the new information.',
                    'fa-edit'
                );
            } else {
                alert('Failed to update room');
            }
        } catch (error) {
            console.error('Error updating room:', error);
            alert('An error occurred while updating');
        }
    });
}

// ============================================
// DELETE ROOM - NEW ENHANCED VERSION
// ============================================

function deleteRoom(roomId, roomName) {
    // Show custom delete confirmation modal
    showDeleteConfirmation(roomId, roomName);
}

function showDeleteConfirmation(roomId, roomName) {
    const existing = document.getElementById('delete-confirmation');
    if (existing) existing.remove();

    const modal = document.createElement('div');
    modal.id = 'delete-confirmation';
    modal.className = 'delete-confirmation-overlay';

    // Escape the room name to prevent XSS and quote issues
    const escapedName = roomName.replace(/'/g, "\\'").replace(/"/g, '&quot;');

    modal.innerHTML = `
        <div class="delete-confirmation-content">
            <div class="delete-confirmation-icon-wrapper">
                <div class="delete-confirmation-icon-circle">
                    <i class="fas fa-exclamation-triangle delete-confirmation-icon"></i>
                </div>
                <div class="warning-pulse"></div>
            </div>
            
            <h3 class="delete-confirmation-title">Delete Room?</h3>
            <p class="delete-confirmation-message">
                Are you sure you want to delete <strong>"${roomName}"</strong>?
            </p>
            <p class="delete-confirmation-warning">
                <i class="fas fa-info-circle mr-1"></i>
                This action cannot be undone.
            </p>
            
            <div class="delete-confirmation-actions">
                <button onclick="closeDeleteConfirmation()" class="delete-confirmation-btn-cancel">
                    <i class="fas fa-times mr-2"></i>
                    <span>Cancel</span>
                </button>
                <button onclick="confirmDeleteRoom(${roomId}, '${escapedName}')" class="delete-confirmation-btn-delete">
                    <i class="fas fa-trash mr-2"></i>
                    <span>Delete Room</span>
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(modal);

    setTimeout(() => {
        modal.classList.add('show');
    }, 10);

    // Close on Escape key
    const handleEscape = (e) => {
        if (e.key === 'Escape') {
            closeDeleteConfirmation();
            document.removeEventListener('keydown', handleEscape);
        }
    };
    document.addEventListener('keydown', handleEscape);

    // Close on backdrop click
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            closeDeleteConfirmation();
        }
    });
}

function closeDeleteConfirmation() {
    const modal = document.getElementById('delete-confirmation');
    if (modal) {
        modal.classList.remove('show');
        modal.classList.add('hide');
        setTimeout(() => {
            modal.remove();
        }, 300);
    }
}

async function confirmDeleteRoom(roomId, roomName) {
    closeDeleteConfirmation();

    // Show deleting overlay
    showDeletingOverlay();

    try {
        const response = await fetch(`/Room/Delete/${roomId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            const roomCard = document.querySelector(`[data-room-id="${roomId}"]`);
            if (roomCard) {
                roomCard.style.opacity = '0';
                roomCard.style.transform = 'scale(0.9)';

                setTimeout(() => {
                    roomCard.remove();
                    closeDeletingOverlay();
                    showDeleteSuccessNotification(roomName);
                }, 300);
            }
        } else {
            closeDeletingOverlay();
            showDeleteErrorNotification();
        }
    } catch (error) {
        console.error('Error deleting room:', error);
        closeDeletingOverlay();
        showDeleteErrorNotification();
    }
}

function showDeletingOverlay() {
    const overlay = document.createElement('div');
    overlay.id = 'deleting-overlay';
    overlay.className = 'deleting-overlay';
    overlay.innerHTML = `
        <div class="deleting-spinner-wrapper">
            <div class="deleting-spinner"></div>
            <p class="deleting-text">Deleting room...</p>
        </div>
    `;
    document.body.appendChild(overlay);

    setTimeout(() => {
        overlay.classList.add('show');
    }, 10);
}

function closeDeletingOverlay() {
    const overlay = document.getElementById('deleting-overlay');
    if (overlay) {
        overlay.classList.remove('show');
        setTimeout(() => overlay.remove(), 300);
    }
}

function showDeleteSuccessNotification(roomName) {
    showSuccessNotification(
        'Room Deleted Successfully! 🗑️',
        `"${roomName}" has been permanently removed from your system.`,
        'fa-check-circle'
    );
}

function showDeleteErrorNotification() {
    const existing = document.getElementById('error-notification');
    if (existing) existing.remove();

    const notification = document.createElement('div');
    notification.id = 'error-notification';
    notification.className = 'error-notification-overlay';

    notification.innerHTML = `
        <div class="error-notification-content">
            <div class="error-notification-icon-wrapper">
                <div class="error-notification-icon-circle">
                    <i class="fas fa-times-circle error-notification-icon"></i>
                </div>
            </div>
            
            <h3 class="error-notification-title">Delete Failed</h3>
            <p class="error-notification-message">
                We couldn't delete the room. Please try again.
            </p>
            
            <div class="error-notification-actions">
                <button onclick="closeErrorNotification()" class="error-notification-btn">
                    <i class="fas fa-check mr-2"></i>
                    <span>Got it</span>
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.classList.add('show');
    }, 10);

    setTimeout(() => {
        closeErrorNotification();
    }, 4000);
}

function closeErrorNotification() {
    const notification = document.getElementById('error-notification');
    if (notification) {
        notification.classList.remove('show');
        notification.classList.add('hide');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }
}

// ============================================
// SETUP ADD ROOM FORM
// ============================================

function setupAddRoomForm() {
    const form = document.getElementById('add-room-form');
    if (!form) return;

    form.addEventListener('submit', (e) => {
        console.log('Adding new room...');
    });
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

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            document.getElementById('search-input')?.focus();
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal('add-room-modal');
        }

        // Only allow number key shortcuts when NOT typing in a field
        if (!e.ctrlKey && !e.metaKey && !e.altKey && !isTyping) {
            if (e.key === '1') filterByStatus('all');
            if (e.key === '2') filterByStatus('available');
            if (e.key === '3') filterByStatus('occupied');
            if (e.key === '4') filterByStatus('reserved');
        }

        if (e.key === 'Escape') {
            closeSuccessNotification();
            closeDeleteConfirmation();
            closeErrorNotification();

            // Close any open modal
            const modals = document.querySelectorAll('.modal-overlay:not(.hidden)');
            modals.forEach(modal => closeModal(modal.id));
        }
    });
}