// wwwroot/js/room.js
//
// TIMER NOTE:
// session-timer.js owns all timer logic via `timerManager`.
// It scans for id="timer-{sessionId}" elements on load and after smartRestart().

// ============================================
// i18n helper — falls back to key if missing
// Translations are injected by the Razor view via window.RoomI18n
// ============================================
function t(key) {
    return (window.RoomI18n && window.RoomI18n[key]) ? window.RoomI18n[key] : key;
}

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
// ============================================

function openQuickBookModal(roomId, roomName, sessionType, hourlyRate, maxGuests) {
    document.getElementById('qb-room-id').value = roomId;
    document.getElementById('qb-session-type').value = sessionType;
    document.getElementById('qb-hourly-rate').value = hourlyRate;
    document.getElementById('qb-max-guests').value = maxGuests;

    const isMulti = sessionType === 'Multi';
    const iconWrap = document.getElementById('qb-icon-wrap');
    const typeLabel = document.getElementById('qb-type-label');
    const roomLabel = document.getElementById('qb-room-label');
    const ratePreview = document.getElementById('qb-rate-preview');
    const guestInput = document.getElementById('qb-guest-count');
    const guestMaxNote = document.getElementById('qb-guest-max');

    iconWrap.className = `w-10 h-10 rounded-xl flex items-center justify-center ${isMulti ? 'bg-purple-100' : 'bg-green-100'}`;
    iconWrap.innerHTML = `<i class="fas ${isMulti ? 'fa-users text-purple-600' : 'fa-user text-green-600'} text-lg"></i>`;

    typeLabel.textContent = isMulti ? t('MultiSession') : t('SingleSession');
    typeLabel.className = `text-xl font-bold ${isMulti ? 'text-purple-700' : 'text-gray-900'}`;
    roomLabel.textContent = roomName;

    ratePreview.textContent = formatEGP(parseFloat(hourlyRate)) + '/hr';
    ratePreview.className = `font-bold text-lg ${isMulti ? 'text-purple-700' : 'text-blue-700'}`;

    const previewBox = document.getElementById('qb-rate-box');
    previewBox.className = `rounded-xl p-4 border transition-colors duration-200 ${isMulti ? 'bg-purple-50 border-purple-200' : 'bg-blue-50 border-blue-200'}`;

    guestInput.max = maxGuests;
    guestInput.value = 1;
    guestMaxNote.textContent = t('MaxPersons').replace('{0}', maxGuests);

    const submitBtn = document.getElementById('qb-submit-btn');
    submitBtn.className = `btn ${isMulti ? 'btn-multi' : 'btn-success'}`;
    submitBtn.innerHTML = `<i class="fas fa-play mr-2"></i>${isMulti ? t('StartMultiSession') : t('StartSingleSession')}`;

    openModal('quick-book-modal');
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
            showToast('error', t('GuestCountError').replace('{max}', maxGuests).replace('{type}', sessionType));
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
                await refreshRoomCard(roomId);
                showToast('success',
                    t('SessionStartedMsg').replace('{client}', clientName).replace('{type}', data.sessionType),
                    t('SessionStartedTitle')
                );
            } else {
                const err = await res.json().catch(() => ({}));
                showToast('error', err.message || t('FailedStartSession'));
            }
        } catch {
            showToast('error', t('NetworkError'));
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
    const local = new Date(dt.getTime() - dt.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
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
                body: JSON.stringify({ roomId, clientName, phone, reservationTime: new Date(dt).toISOString(), notes })
            });

            if (res.ok) {
                closeModal('reserve-room-modal');
                await refreshRoomCard(roomId);
                showToast('success',
                    t('RoomReservedMsg').replace('{client}', clientName),
                    t('RoomReservedTitle')
                );
            } else {
                const err = await res.json().catch(() => ({}));
                showToast('error', err.message || t('FailedReserve'));
            }
        } catch {
            showToast('error', t('NetworkError'));
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
                showToast('success', t('SessionEndedMsg'), t('SessionEndedTitle'));
            }
        } else {
            const err = await res.json().catch(() => ({}));
            showToast('error', err.message || t('FailedEndSession'));
        }
    } catch {
        showToast('error', t('NetworkError'));
    }
}

// ============================================
// RESERVATION ACTIONS
// ============================================

function cancelReservation(roomId, roomName) {
    showDeleteConfirmation(
        t('CancelReservationAction').replace('{room}', roomName),
        t('RoomWillBeAvailableImmediately'),
        async () => {
            const res = await fetch(`/Room/CancelReservation?roomId=${roomId}`, { method: 'POST' });
            if (res.ok) {
                await refreshRoomCard(roomId);
                showToast('success',
                    t('ReservationCancelledMsg').replace('{room}', roomName),
                    t('ReservationCancelledTitle')
                );
            } else {
                showToast('error', t('FailedCancelReservation'));
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
            showToast('success',
                t('CheckedInMsg').replace('{client}', data.clientName).replace('{room}', roomName),
                t('CheckedInTitle')
            );
        } else {
            const err = await res.json().catch(() => ({}));
            showToast('error', err.message || t('FailedCheckIn'));
        }
    } catch {
        showToast('error', t('NetworkError'));
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
        showToast('error', t('FailedLoadRoom'));
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
                showToast('success', t('RoomUpdatedMsg'), t('RoomUpdatedTitle'));
            } else {
                showToast('error', t('FailedUpdateRoom'));
            }
        } catch {
            showToast('error', t('NetworkError'));
        }
    });
}

// ============================================
// DELETE ROOM
// ============================================

function deleteRoom(roomId, roomName) {
    showDeleteConfirmation(
        t('DeleteRoomAction').replace('{room}', roomName),
        t('ActionCannotBeUndone'),
        async () => {
            const res = await fetch(`/Room/Delete/${roomId}`, { method: 'DELETE' });
            if (res.ok) {
                const card = document.querySelector(`[data-room-id="${roomId}"]`);
                if (card) {
                    card.style.transition = 'opacity 0.2s ease, transform 0.2s ease';
                    card.style.opacity = '0';
                    card.style.transform = 'scale(0.95)';
                    setTimeout(() => {
                        card.remove();
                        closeDeletingOverlay(); // ← FIX: was missing from this branch
                    }, 200);
                } else {
                    closeDeletingOverlay(); // ← FIX: was never called when card not found
                }
                showToast('success',
                    t('RoomDeletedMsg').replace('{room}', roomName),
                    t('RoomDeletedTitle')
                );
            } else {
                closeDeletingOverlay(); // ← FIX: always close overlay on error too
                const err = await res.json().catch(() => ({}));
                showToast('error', err.message || t('FailedDeleteRoom'));
            }
        }
    );
}

// ============================================
// ADD ROOM (HTMX callback)
// ============================================

function handleRoomAdded() {
    closeModal('add-room-modal');
    if (window.timerManager) setTimeout(() => window.timerManager.smartRestart(), 50);
    showToast('success', t('RoomAddedMsg'), t('RoomAddedTitle'));
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
                <p class="text-gray-600 font-medium">${t('NoRoomsFound')}</p>
                <p class="text-gray-400 text-sm mt-2">${t('TryAdjustingFilters')}</p>`;
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
            </div>
            <h3 class="delete-confirmation-title">${t('AreYouSure')}</h3>
            <p class="delete-confirmation-message">${t('YouAreAboutTo')} <strong>${actionLabel}</strong>.</p>
            <p class="delete-confirmation-warning">
                <i class="fas fa-info-circle mr-1"></i>${warningText}
            </p>
            <div class="delete-confirmation-actions">
                <button onclick="closeDeleteConfirmation()" class="delete-confirmation-btn-cancel">
                    <i class="fas fa-times mr-2"></i>${t('Cancel')}
                </button>
                <button id="delete-confirm-btn" class="delete-confirmation-btn-delete">
                    <i class="fas fa-check mr-2"></i>${t('Confirm')}
                </button>
            </div>
        </div>`;

    document.body.appendChild(modal);

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
    if (m) {
        m.classList.add('hide');
        setTimeout(() => m.remove(), 200);
    }
}

function showDeletingOverlay() {
    const el = document.createElement('div');
    el.id = 'deleting-overlay';
    el.className = 'deleting-overlay';
    el.innerHTML = `<div class="deleting-spinner-wrapper">
        <div class="deleting-spinner"></div>
        <p class="deleting-text">${t('Processing')}</p>
    </div>`;
    document.body.appendChild(el);
}

function closeDeletingOverlay() {
    const el = document.getElementById('deleting-overlay');
    if (el) {
        el.classList.add('hide');
        setTimeout(() => el.remove(), 200);
    }
}

// ============================================
// CENTERED NOTIFICATIONS
// showToast(type, message, title?)
// type: 'success' | 'error' | 'warning' | 'info'
// ============================================

let _notifTimer = null;

function showToast(type, message, title) {
    // Remove any existing notification
    document.getElementById('room-notification')?.remove();
    clearTimeout(_notifTimer);

    const cfg = {
        success: { icon: 'fa-check-circle', circleCss: 'notif-circle-success', iconCss: 'notif-icon-success', btnCss: 'notif-btn-success' },
        error: { icon: 'fa-times-circle', circleCss: 'notif-circle-error', iconCss: 'notif-icon-error', btnCss: 'notif-btn-error' },
        warning: { icon: 'fa-exclamation-circle', circleCss: 'notif-circle-warning', iconCss: 'notif-icon-warning', btnCss: 'notif-btn-warning' },
        info: { icon: 'fa-info-circle', circleCss: 'notif-circle-info', iconCss: 'notif-icon-info', btnCss: 'notif-btn-info' },
    };
    const c = cfg[type] || cfg.info;

    const el = document.createElement('div');
    el.id = 'room-notification';
    el.className = 'room-notif-overlay';
    el.innerHTML = `
        <div class="room-notif-card">
            <div class="room-notif-circle ${c.circleCss}">
                <i class="fas ${c.icon} ${c.iconCss}"></i>
            </div>
            ${title ? `<h3 class="room-notif-title">${title}</h3>` : ''}
            <p class="room-notif-message">${message}</p>
            <div class="room-notif-progress"><div class="room-notif-bar ${c.btnCss}-bar"></div></div>
            <button class="room-notif-btn ${c.btnCss}" onclick="closeNotification()">
                <i class="fas fa-check mr-2"></i>${t('Awesome') || 'OK'}
            </button>
        </div>`;

    document.body.appendChild(el);

    // Click backdrop to close
    el.addEventListener('click', e => { if (e.target === el) closeNotification(); });

    // Auto-close after 5s
    _notifTimer = setTimeout(closeNotification, 5000);
}

function closeNotification() {
    clearTimeout(_notifTimer);
    const el = document.getElementById('room-notification');
    if (el) {
        el.classList.add('hide');
        setTimeout(() => el.remove(), 220);
    }
}

// Legacy wrappers — keep these so nothing else breaks
function showSuccessNotification(title, message) { showToast('success', message, title); }
function closeSuccessNotification() { closeNotification(); }
function showErrorNotification(msg) { showToast('error', msg); }
function closeErrorNotification() { closeNotification(); }

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
            closeNotification();
            closeDeleteConfirmation();
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
});