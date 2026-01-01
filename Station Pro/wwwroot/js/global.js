// wwwroot/js/global.js

// ============================================
// TOAST NOTIFICATIONS
// ============================================

function showToast(message, type = 'info', duration = 3000) {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type} fade-in`;

    const icon = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        info: 'fa-info-circle'
    }[type] || 'fa-info-circle';

    const iconColor = {
        success: 'text-green-500',
        error: 'text-red-500',
        info: 'text-blue-500'
    }[type] || 'text-blue-500';

    toast.innerHTML = `
        <i class="fas ${icon} ${iconColor} text-xl"></i>
        <span class="flex-1 font-medium text-gray-800">${message}</span>
        <button onclick="this.parentElement.remove()" class="text-gray-400 hover:text-gray-600">
            <i class="fas fa-times"></i>
        </button>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// ============================================
// MODAL MANAGEMENT
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
    }
}

// Close modal on overlay click
document.addEventListener('click', (e) => {
    if (e.target.classList.contains('modal-overlay')) {
        closeModal(e.target.id);
    }
});

// Close modal on ESC key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-overlay:not(.hidden)').forEach(modal => {
            closeModal(modal.id);
        });
    }
});

// ============================================
// CONFIRMATION DIALOGS
// ============================================

function confirmAction(message, onConfirm) {
    if (confirm(message)) {
        onConfirm();
    }
}

// ============================================
// FORMAT CURRENCY
// ============================================

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-EG', {
        style: 'currency',
        currency: 'EGP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    }).format(amount);
}

// ============================================
// FORMAT DURATION
// ============================================

function formatDuration(seconds) {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
}

// ============================================
// TIMER UTILITY
// ============================================

class SessionTimer {
    constructor(elementId, startTime) {
        this.element = document.getElementById(elementId);
        this.startTime = new Date(startTime);
        this.interval = null;
    }

    start() {
        this.update();
        this.interval = setInterval(() => this.update(), 1000);
    }

    stop() {
        if (this.interval) {
            clearInterval(this.interval);
        }
    }

    update() {
        const now = new Date();
        const diff = Math.floor((now - this.startTime) / 1000);
        if (this.element) {
            this.element.textContent = formatDuration(diff);
        }
    }
}

// ============================================
// FORM VALIDATION
// ============================================

function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;

    let isValid = true;
    const inputs = form.querySelectorAll('[required]');

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('error');
            isValid = false;
        } else {
            input.classList.remove('error');
        }
    });

    return isValid;
}

// Remove error class on input
document.addEventListener('input', (e) => {
    if (e.target.classList.contains('error')) {
        e.target.classList.remove('error');
    }
});

// ============================================
// HTMX EVENT LISTENERS
// ============================================

// Show loading indicator
document.addEventListener('htmx:beforeRequest', (e) => {
    const target = e.detail.target;
    if (target) {
        target.style.opacity = '0.6';
        target.style.pointerEvents = 'none';
    }
});

// Hide loading indicator
document.addEventListener('htmx:afterRequest', (e) => {
    const target = e.detail.target;
    if (target) {
        target.style.opacity = '1';
        target.style.pointerEvents = 'auto';
    }
});

// Show success toast after successful request
document.addEventListener('htmx:afterOnLoad', (e) => {
    const xhr = e.detail.xhr;
    const successMessage = xhr.getResponseHeader('X-Success-Message');
    if (successMessage) {
        showToast(successMessage, 'success');
    }
});

// Show error toast on failed request
document.addEventListener('htmx:responseError', (e) => {
    showToast('An error occurred. Please try again.', 'error');
});

// ============================================
// NUMBER FORMATTING
// ============================================

function formatNumber(num) {
    return new Intl.NumberFormat('en-US').format(num);
}

// ============================================
// DEBOUNCE UTILITY
// ============================================

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// ============================================
// LOCAL STORAGE HELPERS
// ============================================

function saveToStorage(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
    } catch (e) {
        console.error('Error saving to localStorage:', e);
    }
}

function getFromStorage(key, defaultValue = null) {
    try {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : defaultValue;
    } catch (e) {
        console.error('Error reading from localStorage:', e);
        return defaultValue;
    }
}

// ============================================
// COPY TO CLIPBOARD
// ============================================

async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        showToast('Copied to clipboard!', 'success');
    } catch (err) {
        showToast('Failed to copy', 'error');
    }
}

// ============================================
// DEVICE TYPE ICONS
// ============================================

function getDeviceIcon(deviceType) {
    const icons = {
        'PS5': 'fa-playstation',
        'PS4': 'fa-playstation',
        'PS3': 'fa-playstation',
        'Xbox': 'fa-xbox',
        'PC': 'fa-desktop',
        'PingPong': 'fa-table-tennis',
        'Pool': 'fa-circle',
        'Billiards': 'fa-circle',
        'Other': 'fa-gamepad'
    };
    return icons[deviceType] || 'fa-gamepad';
}

// ============================================
// AUTO-REFRESH HANDLER
// ============================================

class AutoRefresh {
    constructor(elementId, url, interval = 5000) {
        this.elementId = elementId;
        this.url = url;
        this.interval = interval;
        this.timer = null;
    }

    start() {
        this.refresh();
        this.timer = setInterval(() => this.refresh(), this.interval);
    }

    stop() {
        if (this.timer) {
            clearInterval(this.timer);
        }
    }

    async refresh() {
        const element = document.getElementById(this.elementId);
        if (!element) return;

        try {
            const response = await fetch(this.url);
            const html = await response.text();
            element.innerHTML = html;
        } catch (error) {
            console.error('Auto-refresh error:', error);
        }
    }
}

// ============================================
// INITIALIZE ON PAGE LOAD
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    console.log('Station Pro initialized');

    // Set active nav link
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link').forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('text-blue-600', 'bg-blue-50');
        }
    });
});