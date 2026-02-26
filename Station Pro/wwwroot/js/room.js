// wwwroot/js/room.js
//
// TIMER NOTE:
// session-timer.js owns all timer logic via `timerManager`.
// It scans for id="timer-{sessionId}" elements on load and after smartRestart().

// ============================================
// MODAL HELPERS
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
        if (form) form.reset();
    }
}

// ============================================
// RECEIPT MODAL
// ============================================

function showReceiptModal(html) {
    const content = document.getElementById('receipt-content');
    const modal = document.getElementById('receipt-modal');
    if (content && modal) {
        content.innerHTML = html;
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

function closeReceiptModal() {
    const modal = document.getElementById('receipt-modal');
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
    }
}

// ============================================
// QUICK BOOK MODAL
// Called directly from the Single / Multi buttons on each room card.
//
// @param {number} roomId
// @param {string} roomName
// @param {string} sessionType   "Single" | "Multi"
// @param {string} hourlyRate    invariant-culture decimal string
// @param {number} maxGuests     2 for Single, 4 for Multi
// ============================================

function openQuickBookModal(roomId, roomName, sessionType, hourlyRate, maxGuests) {
    // Populate hidden fields
    document.getElementById('qb-room-id').value = roomId;
    document.getElementById('qb-session-type').value = sessionType;
    document.getElementById('qb-hourly-rate').value = hourlyRate;
    document.getElementById('qb-max-guests').value = maxGuests;

    // Header styling — blue for Single, purple for Multi
    const isMulti = sessionType === 'Multi';
    const iconWrap = document.getElementById('qb-icon-wrap');
    const typeLabel = document.getElementById('qb-type-label');
    const roomLabel = document.getElementById('qb-room-label');
    const ratePreview = document.getElementById('qb-rate-preview');
    const guestInput = document.getElementById('qb-guest-count');
    const guestMaxNote = document.getElementById('qb-guest-max');

    iconWrap.className = `w-10 h-10 rounded-xl flex items-center justify-center ${isMulti ? 'bg-purple-100' : 'bg-green-100'}`;
    iconWrap.innerHTML = `<i class="fas ${isMulti ? 'fa-users text-purple-600' : 'fa-user text-green-600'} text-lg"></i>`;
    typeLabel.textContent = `${sessionType} Session`;
    typeLabel.className = `text-xl font-bold ${isMulti ? 'text-purple-700' : 'text-gray-900'}`;
    roomLabel.textContent = roomName;

    ratePreview.textContent = formatEGP(parseFloat(hourlyRate)) + '/hr';
    ratePreview.className = `font-bold text-lg ${isMulti ? 'text-purple-700' : 'text-blue-700'}`;

    const previewBox = document.getElementById('qb-rate-box');
    previewBox.className = `rounded-xl p-4 border transition-colors duration-200 ${isMulti ? 'bg-purple-50 border-purple-200' : 'bg-blue-50 border-blue-200'}`;

    guestInput.max = maxGuests;
    guestInput.value = 1;
    guestMaxNote.textContent = `Max ${maxGuests} ${maxGuests === 1 ? 'person' : 'persons'}`;

    // Submit button colour
    const submitBtn = document.getElementById('qb-submit-btn');
    submitBtn.className = `btn ${isMulti ? 'btn-multi' : 'btn-success'}`;
    submitBtn.innerHTML = `<i class="fas fa-play mr-2"></i>Start ${sessionType} Session`;

    openModal('quick-book-modal');
    // Focus client name for fast keyboard entry
    setTimeout(() => document.getElementById('qb-client-name')?.focus(), 100);
}

function setupQuickBookForm() {
    const form = document.getElementById('quick-book-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const roomId = parseInt(document.getElementById('qb-room-id').value);
        const sessionType = document.getElementById('qb-session-type').value;
        const clientName = document.getElementById('qb-client-name').value.trim();
        const guestCount = parseInt(document.getElementById('qb-guest-count').value);
        const maxGuests = parseInt(document.getElementById('qb-max-guests').value);

        if (!clientName) return;
        if (guestCount < 1 || guestCount > maxGuests) {
            showErrorNotification(`Guest count must be between 1 and ${maxGuests} for a ${sessionType} session.`);
            return;
        }

        try {
            const res = await fetch('/Room/StartSession', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ roomId, clientName, guestCount, sessionType })
            });

            if (res.ok) {
                const data = await res.json();
                closeModal('quick-book-modal');
                // Refresh card → timerManager.smartRestart() picks up the new timer element
                await refreshRoomCard(roomId);
                showSuccessNotification(
                    'Session Started! 🎉',
                    `${clientName}'s ${data.sessionType} session is live. Timer is running.`,
                    'fa-play-circle'
                );
            } else {
                const err = await res.json().catch(() => ({}));
                showErrorNotification(err.message || 'Failed to start session.');
            }
        } catch {
            showErrorNotification('Network error. Please try again.');
        }
    });
}

// ============================================
// RESERVE ROOM MODAL
// ============================================

function openReserveModal(roomId, roomName) {
    document.getElementById('reserve-room-id').value = roomId;
    document.getElementById('reserve-room-name-label').textContent = roomName;

    const dt = new Date(Date.now() + 60 * 60 * 1000);
    const local = new Date(dt.getTime() - dt.getTimezoneOffset() * 60000)
        .toISOString().slice(0, 16);
    document.getElementById('reserve-datetime').value = local;

    openModal('reserve-room-modal');
}

function setupReserveRoomForm() {
    const form = document.getElementById('reserve-room-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const roomId = parseInt(document.getElementById('reserve-room-id').value);
        const clientName = document.getElementById('reserve-client-name').value.trim();
        const phone = document.getElementById('reserve-phone').value.trim();
        const dt = document.getElementById('reserve-datetime').value;
        const notes = document.getElementById('reserve-notes').value.trim();

        if (!clientName || !dt) return;

        try {
            const res = await fetch('/Room/Reserve', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    roomId,
                    clientName,
                    phone,
                    reservationTime: new Date(dt).toISOString(),
                    notes
                })
            });

            if (res.ok) {
                closeModal('reserve-room-modal');
                await refreshRoomCard(roomId);
                showSuccessNotification(
                    'Room Reserved! 📅',
                    `${clientName}'s reservation has been confirmed.`,
                    'fa-calendar-check'
                );
            } else {
                const err = await res.json().catch(() => ({}));
                showErrorNotification(err.message || 'Failed to create reservation.');
            }
        } catch {
            showErrorNotification('Network error. Please try again.');
        }
    });
}

// ============================================
// END SESSION MODAL
// ============================================

function openEndSessionModal(roomId, sessionId, roomName, clientName) {
    document.getElementById('end-session-id').value = sessionId;
    document.getElementById('end-session-room-id').value = roomId;
    document.getElementById('end-session-room-label').textContent = roomName;
    document.getElementById('end-client-name').textContent = clientName;

    const timerEl = document.getElementById(`timer-${sessionId}`);
    const costEl = document.getElementById(`cost-${sessionId}`);

    document.getElementById('end-session-duration').textContent =
        timerEl ? timerEl.textContent.trim() : '00:00:00';
    document.getElementById('end-session-cost').textContent =
        costEl ? costEl.textContent.trim() : formatEGP(0);

    openModal('end-session-modal');
}

async function confirmEndSession() {
    const sessionId = document.getElementById('end-session-id').value;
    const roomId = parseInt(document.getElementById('end-session-room-id').value);

    closeModal('end-session-modal');

    try {
        const res = await fetch(`/Room/EndSession?sessionId=${sessionId}`, { method: 'POST' });

        if (res.ok) {
            const data = await res.json();
            await refreshRoomCard(roomId);

            const receiptRes = await fetch(`/Room/SessionReceipt?sessionId=${data.sessionId}`);
            if (receiptRes.ok) {
                showReceiptModal(await receiptRes.text());
            } else {
                showSuccessNotification('Session Ended ✅', 'The room is now available.', 'fa-check-circle');
            }
        } else {
            const err = await res.json().catch(() => ({}));
            showErrorNotification(err.message || 'Failed to end session.');
        }
    } catch {
        showErrorNotification('Network error. Please try again.');
    }
}

// ============================================
// RESERVATION ACTIONS
// ============================================

function cancelReservation(roomId, roomName) {
    showDeleteConfirmation(
        `cancel the reservation for "${roomName}"`,
        'The room will become available immediately.',
        async () => {
            const res = await fetch(`/Room/CancelReservation?roomId=${roomId}`, { method: 'POST' });
            if (res.ok) {
                await refreshRoomCard(roomId);
                showSuccessNotification('Reservation Cancelled', `${roomName} is now available.`, 'fa-calendar-times');
            } else {
                showErrorNotification('Failed to cancel reservation.');
            }
        }
    );
}

async function activateReservation(roomId, roomName) {
    try {
        const res = await fetch(`/Room/ActivateReservation?roomId=${roomId}`, { method: 'POST' });
        if (res.ok) {
            const data = await res.json();
            await refreshRoomCard(roomId);
            showSuccessNotification(
                'Checked In! 🎉',
                `${data.clientName} is now checked in to ${roomName}. Timer started.`,
                'fa-check-circle'
            );
        } else {
            const err = await res.json().catch(() => ({}));
            showErrorNotification(err.message || 'Failed to check in.');
        }
    } catch {
        showErrorNotification('Network error. Please try again.');
    }
}

// ============================================
// REFRESH A SINGLE ROOM CARD
// ============================================

async function refreshRoomCard(roomId) {
    try {
        const res = await fetch(`/Room/CardPartial?id=${roomId}`);
        if (!res.ok) { location.reload(); return; }

        const html = await res.text();
        const card = document.getElementById(`room-card-${roomId}`);
        if (!card) { location.reload(); return; }

        card.outerHTML = html;

        if (window.timerManager) {
            setTimeout(() => window.timerManager.smartRestart(), 50);
        }
    } catch {
        location.reload();
    }
}

// ============================================
// EDIT ROOM
// ============================================

async function editRoom(roomId) {
    try {
        const res = await fetch(`/Room/Get/${roomId}`);
        if (!res.ok) throw new Error();
        const room = await res.json();

        document.getElementById('edit-room-id').value = room.id;
        document.getElementById('edit-room-name').value = room.name;
        document.getElementById('edit-room-single-rate').value = room.singleHourlyRate ?? room.hourlyRate ?? '';
        document.getElementById('edit-room-multi-rate').value = room.multiHourlyRate ?? '';
        document.getElementById('edit-room-capacity').value = room.capacity;
        document.getElementById('edit-room-devices').value = room.deviceCount || 0;
        document.getElementById('edit-room-ac').checked = room.hasAC;
        document.getElementById('edit-room-active').checked = room.isActive;
        openModal('edit-room-modal');
    } catch {
        showErrorNotification('Failed to load room details.');
    }
}

function setupEditRoomForm() {
    const form = document.getElementById('edit-room-form');
    if (!form) return;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData(e.target);
        const roomId = fd.get('Id');

        try {
            const res = await fetch(`/Room/Update/${roomId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    name: fd.get('Name'),
                    singleHourlyRate: parseFloat(fd.get('SingleHourlyRate')),
                    multiHourlyRate: parseFloat(fd.get('MultiHourlyRate')),
                    capacity: parseInt(fd.get('Capacity')),
                    deviceCount: parseInt(fd.get('DeviceCount')) || 0,
                    hasAC: fd.get('HasAC') === 'on',
                    isActive: fd.get('IsActive') === 'on'
                })
            });

            if (res.ok) {
                closeModal('edit-room-modal');
                await refreshRoomCard(parseInt(roomId));
                showSuccessNotification('Room Updated! ✨', 'Room details saved successfully.', 'fa-edit');
            } else {
                showErrorNotification('Failed to update room.');
            }
        } catch {
            showErrorNotification('Network error. Please try again.');
        }
    });
}

// ============================================
// DELETE ROOM
// ============================================

function deleteRoom(roomId, roomName) {
    showDeleteConfirmation(
        `delete "${roomName}"`,
        'This action cannot be undone.',
        async () => {
            const res = await fetch(`/Room/Delete/${roomId}`, { method: 'DELETE' });
            if (res.ok) {
                const card = document.querySelector(`[data-room-id="${roomId}"]`);
                if (card) {
                    card.style.transition = 'all .3s ease';
                    card.style.opacity = '0';
                    card.style.transform = 'scale(0.9)';
                    setTimeout(() => { card.remove(); closeDeletingOverlay(); }, 300);
                } else {
                    closeDeletingOverlay();
                }
                showSuccessNotification('Room Deleted 🗑️', `"${roomName}" has been removed.`, 'fa-check-circle');
            } else {
                closeDeletingOverlay();
                const err = await res.json().catch(() => ({}));
                showErrorNotification(err.message || 'Cannot delete this room.');
            }
        }
    );
}

// ============================================
// ADD ROOM (HTMX callback)
// ============================================

function handleRoomAdded() {
    closeModal('add-room-modal');
    if (window.timerManager) {
        setTimeout(() => window.timerManager.smartRestart(), 50);
    }
    showSuccessNotification('Room Added! 🎉', 'Your new room is ready for bookings.', 'fa-door-open');
    document.getElementById('add-room-form')?.reset();
}

// ============================================
// FILTER
// ============================================

function filterRooms() {
    const term = document.getElementById('search-input').value.toLowerCase().trim();
    let visible = 0;
    document.querySelectorAll('.room-card').forEach(card => {
        const match = (card.dataset.roomName || '').includes(term);
        card.classList.toggle('hidden', !match);
        if (match) visible++;
    });
    updateNoResultsMessage(visible);
}

function filterByStatus(status) {
    document.querySelectorAll('.filter-btn').forEach(btn =>
        btn.classList.toggle('active', btn.dataset.filter === status)
    );
    document.querySelectorAll('.room-card').forEach(card => {
        const match = status === 'all' || (card.dataset.roomStatus || '') === status;
        card.classList.toggle('hidden', !match);
    });
}

function updateNoResultsMessage(visibleCount) {
    let el = document.getElementById('no-results-message');
    if (visibleCount === 0) {
        if (!el) {
            el = document.createElement('div');
            el.id = 'no-results-message';
            el.className = 'col-span-full text-center py-12';
            el.innerHTML = `
                <i class="fas fa-search text-gray-300 text-5xl mb-4 block"></i>
                <p class="text-gray-600 font-medium">No rooms found</p>
                <p class="text-gray-400 text-sm mt-2">Try adjusting your search or filters</p>`;
            document.getElementById('rooms-grid').appendChild(el);
        }
    } else {
        el?.remove();
    }
}

// ============================================
// DELETE CONFIRMATION
// ============================================

function showDeleteConfirmation(actionLabel, warningText, onConfirm) {
    document.getElementById('delete-confirmation')?.remove();

    const modal = document.createElement('div');
    modal.id = 'delete-confirmation';
    modal.className = 'delete-confirmation-overlay';
    modal.innerHTML = `
        <div class="delete-confirmation-content">
            <div class="delete-confirmation-icon-wrapper">
                <div class="delete-confirmation-icon-circle">
                    <i class="fas fa-exclamation-triangle delete-confirmation-icon"></i>
                </div>
                <div class="warning-pulse"></div>
            </div>
            <h3 class="delete-confirmation-title">Are you sure?</h3>
            <p class="delete-confirmation-message">You are about to <strong>${actionLabel}</strong>.</p>
            <p class="delete-confirmation-warning">
                <i class="fas fa-info-circle mr-1"></i>${warningText}
            </p>
            <div class="delete-confirmation-actions">
                <button onclick="closeDeleteConfirmation()" class="delete-confirmation-btn-cancel">
                    <i class="fas fa-times mr-2"></i>Cancel
                </button>
                <button id="delete-confirm-btn" class="delete-confirmation-btn-delete">
                    <i class="fas fa-check mr-2"></i>Confirm
                </button>
            </div>
        </div>`;

    document.body.appendChild(modal);
    setTimeout(() => modal.classList.add('show'), 10);

    document.getElementById('delete-confirm-btn').addEventListener('click', async () => {
        closeDeleteConfirmation();
        showDeletingOverlay();
        await onConfirm();
    });

    modal.addEventListener('click', e => { if (e.target === modal) closeDeleteConfirmation(); });
    const esc = e => {
        if (e.key === 'Escape') { closeDeleteConfirmation(); document.removeEventListener('keydown', esc); }
    };
    document.addEventListener('keydown', esc);
}

function closeDeleteConfirmation() {
    const m = document.getElementById('delete-confirmation');
    if (m) { m.classList.remove('show'); m.classList.add('hide'); setTimeout(() => m.remove(), 300); }
}

function showDeletingOverlay() {
    const el = document.createElement('div');
    el.id = 'deleting-overlay';
    el.className = 'deleting-overlay';
    el.innerHTML = `<div class="deleting-spinner-wrapper">
        <div class="deleting-spinner"></div>
        <p class="deleting-text">Processing...</p>
    </div>`;
    document.body.appendChild(el);
    setTimeout(() => el.classList.add('show'), 10);
}

function closeDeletingOverlay() {
    const el = document.getElementById('deleting-overlay');
    if (el) { el.classList.remove('show'); setTimeout(() => el.remove(), 300); }
}

// ============================================
// SUCCESS / ERROR NOTIFICATIONS
// ============================================

let _successTimeout = null;

function showSuccessNotification(title, message, icon = 'fa-check-circle') {
    document.getElementById('success-notification')?.remove();
    clearTimeout(_successTimeout);

    const el = document.createElement('div');
    el.id = 'success-notification';
    el.className = 'success-notification-overlay';
    el.innerHTML = `
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
            <div class="success-progress-bar"><div class="success-progress-fill"></div></div>
            <div class="success-notification-actions">
                <button onclick="closeSuccessNotification()" class="success-notification-btn-primary">
                    <i class="fas fa-check mr-2"></i><span>Awesome!</span>
                </button>
            </div>
        </div>`;
    document.body.appendChild(el);
    setTimeout(() => { el.classList.add('show'); createConfetti(); }, 10);
    _successTimeout = setTimeout(closeSuccessNotification, 5000);
}

function createConfetti() {
    const container = document.getElementById('confetti-container');
    if (!container) return;
    const colors = ['#10b981', '#3b82f6', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];
    const shapes = ['circle', 'square'];
    for (let i = 0; i < 40; i++) {
        setTimeout(() => {
            const el = document.createElement('div');
            el.className = `confetti confetti-${shapes[i % 2]}`;
            el.style.left = Math.random() * 100 + '%';
            el.style.backgroundColor = colors[Math.floor(Math.random() * colors.length)];
            el.style.animationDelay = Math.random() * 0.3 + 's';
            el.style.animationDuration = (Math.random() * 2 + 2) + 's';
            container.appendChild(el);
            setTimeout(() => el.remove(), 4000);
        }, i * 30);
    }
}

function closeSuccessNotification() {
    clearTimeout(_successTimeout);
    const el = document.getElementById('success-notification');
    if (el) { el.classList.remove('show'); el.classList.add('hide'); setTimeout(() => el.remove(), 400); }
}

function showErrorNotification(msg) {
    document.getElementById('error-notification')?.remove();
    const el = document.createElement('div');
    el.id = 'error-notification';
    el.className = 'error-notification-overlay';
    el.innerHTML = `
        <div class="error-notification-content">
            <div class="error-notification-icon-wrapper">
                <div class="error-notification-icon-circle">
                    <i class="fas fa-times-circle error-notification-icon"></i>
                </div>
            </div>
            <h3 class="error-notification-title">Something went wrong</h3>
            <p class="error-notification-message">${msg}</p>
            <div class="error-notification-actions">
                <button onclick="closeErrorNotification()" class="error-notification-btn">
                    <i class="fas fa-check mr-2"></i>Got it
                </button>
            </div>
        </div>`;
    document.body.appendChild(el);
    setTimeout(() => el.classList.add('show'), 10);
    setTimeout(closeErrorNotification, 5000);
}

function closeErrorNotification() {
    const el = document.getElementById('error-notification');
    if (el) { el.classList.remove('show'); el.classList.add('hide'); setTimeout(() => el.remove(), 300); }
}

// ============================================
// UTILITIES
// ============================================

function pad(n) { return String(n).padStart(2, '0'); }

function formatEGP(amount) {
    return new Intl.NumberFormat('en-EG', { style: 'currency', currency: 'EGP' }).format(amount);
}

// ============================================
// KEYBOARD SHORTCUTS
// ============================================

function setupKeyboardShortcuts() {
    document.addEventListener('keydown', e => {
        const typing = ['INPUT', 'TEXTAREA', 'SELECT'].includes(document.activeElement.tagName);

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') { e.preventDefault(); document.getElementById('search-input')?.focus(); }
        if ((e.ctrlKey || e.metaKey) && e.key === 'n') { e.preventDefault(); openModal('add-room-modal'); }

        if (!typing && !e.ctrlKey && !e.metaKey && !e.altKey) {
            if (e.key === '1') filterByStatus('all');
            if (e.key === '2') filterByStatus('available');
            if (e.key === '3') filterByStatus('occupied');
            if (e.key === '4') filterByStatus('reserved');
        }

        if (e.key === 'Escape') {
            closeSuccessNotification();
            closeDeleteConfirmation();
            closeErrorNotification();
            closeReceiptModal();
            document.querySelectorAll('.modal-overlay:not(.hidden)').forEach(m => closeModal(m.id));
        }
    });
}

// ============================================
// INIT
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    setupQuickBookForm();
    setupEditRoomForm();
    setupReserveRoomForm();
    setupKeyboardShortcuts();
    // Timers are started by session-timer.js on DOMContentLoaded
});