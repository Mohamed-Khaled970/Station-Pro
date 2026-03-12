// =============================================================================
// FILE: wwwroot/js/auth.js
//
// Register → plain form POST (controller returns redirect, not JSON)
// Login    → AJAX fetch (controller returns JSON { success, redirectUrl })
// =============================================================================

document.addEventListener('DOMContentLoaded', () => {

    // ── Login (AJAX) ──────────────────────────────────────────────────────────
    const loginForm = document.getElementById('login-form');
    if (loginForm) initializeLoginForm(loginForm);

    // ── Register (plain POST + client validation only) ────────────────────────
    const registerForm = document.querySelector('form:not(#login-form)');
    if (registerForm) initializeRegisterValidation(registerForm);

    // Feature card animations
    document.querySelectorAll('.feature-card').forEach(c => c.classList.add('fade-in'));
});

// =============================================================================
// REGISTER — validation only, no fetch (controller returns redirect)
// =============================================================================

function initializeRegisterValidation(form) {
    const passwordInput = document.getElementById('password');
    const confirmInput = document.getElementById('confirm-password');
    const phoneInput = form.querySelector('input[name="PhoneNumber"]');

    if (passwordInput) {
        passwordInput.addEventListener('input', debounce(() =>
            checkPasswordStrength(passwordInput.value), 200));
    }

    if (confirmInput && passwordInput) {
        confirmInput.addEventListener('input', debounce(() =>
            validatePasswordMatch(passwordInput.value, confirmInput.value), 200));
    }

    // Digits only, max 11
    if (phoneInput) {
        phoneInput.addEventListener('input', (e) => {
            let v = e.target.value.replace(/\D/g, '');
            if (v.length > 11) v = v.slice(0, 11);
            e.target.value = v;
        });
    }

    form.addEventListener('submit', (e) => {
        if (!validateRegisterForm(form)) {
            e.preventDefault();
        } else {
            const btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Creating your store...';
            }
        }
    });
}

function validateRegisterForm(form) {
    const password = form.querySelector('#password')?.value ?? '';
    const confirm = form.querySelector('#confirm-password')?.value ?? '';
    const terms = form.querySelector('#terms');

    if (password.length < 6) {
        showError('Password must be at least 6 characters.');
        return false;
    }
    if (password !== confirm) {
        showError('Passwords do not match.');
        return false;
    }
    if (terms && !terms.checked) {
        showError('Please accept the terms and conditions.');
        return false;
    }
    return true;
}

// =============================================================================
// LOGIN — AJAX
// =============================================================================

function initializeLoginForm(form) {
    form.addEventListener('submit', (e) => {
        e.preventDefault();
        if (validateLoginForm(form)) submitLoginForm(form);
    });
}

function validateLoginForm(form) {
    const email = form.querySelector('[name="Email"]')?.value ?? '';
    const password = form.querySelector('[name="Password"]')?.value ?? '';

    if (!email || !password) { showError('Please fill in all fields.'); return false; }
    if (!isValidEmail(email)) { showError('Please enter a valid email address.'); return false; }
    return true;
}

async function submitLoginForm(form) {
    const btn = form.querySelector('button[type="submit"]');
    const origHtml = btn?.innerHTML ?? '';

    if (btn) { btn.disabled = true; btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Logging in...'; }

    try {
        const response = await fetch('/Auth/Login', { method: 'POST', body: new FormData(form) });

        const text = await response.text();
        let data = {};
        try { data = JSON.parse(text); } catch { /* not JSON */ }

        if (response.ok && data.success) {
            // Replace current history entry so back button skips login
            window.location.replace(data.redirectUrl || '/Dashboard/Index');
        } else {
            showError(data.message || 'Invalid email or password.');
            if (btn) { btn.disabled = false; btn.innerHTML = origHtml; }
        }
    } catch (err) {
        console.error('Login error:', err);
        showError('An error occurred. Please try again.');
        if (btn) { btn.disabled = false; btn.innerHTML = origHtml; }
    }
}

// =============================================================================
// PASSWORD STRENGTH
// =============================================================================

function checkPasswordStrength(password) {
    let indicator = document.getElementById('password-strength');
    if (!indicator) {
        const parent = document.getElementById('password')?.parentElement;
        if (!parent) return;
        indicator = document.createElement('div');
        indicator.id = 'password-strength';
        indicator.className = 'mt-2';
        indicator.innerHTML = '<div class="password-strength-bar h-1 rounded-full transition-all duration-300"></div>';
        parent.appendChild(indicator);
    }

    const bar = indicator.querySelector('.password-strength-bar');
    if (!bar) return;

    let s = 0;
    if (password.length >= 6) s++;
    if (password.length >= 10) s++;
    if (/\d/.test(password)) s++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) s++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) s++;

    bar.className = 'password-strength-bar h-1 rounded-full transition-all duration-300 ';
    if (s <= 2) bar.className += 'bg-red-500 w-1/3';
    else if (s <= 3) bar.className += 'bg-yellow-500 w-2/3';
    else bar.className += 'bg-green-500 w-full';
}

function validatePasswordMatch(password, confirm) {
    const error = document.getElementById('password-match-error');
    const input = document.getElementById('confirm-password');
    if (confirm && password !== confirm) {
        error?.classList.remove('hidden');
        input?.classList.add('border-red-500');
        return false;
    }
    error?.classList.add('hidden');
    input?.classList.remove('border-red-500');
    return true;
}

// =============================================================================
// TOGGLE PASSWORD VISIBILITY
// =============================================================================

function togglePassword(inputId) {
    const input = document.getElementById(inputId);
    const icon = input?.nextElementSibling?.querySelector('i');
    if (!input || !icon) return;
    const hidden = input.type === 'password';
    input.type = hidden ? 'text' : 'password';
    icon.classList.toggle('fa-eye', !hidden);
    icon.classList.toggle('fa-eye-slash', hidden);
}

// =============================================================================
// TOAST NOTIFICATIONS
// =============================================================================

function showError(msg) { showToast(msg, 'error'); }
function showSuccess(msg) { showToast(msg, 'success'); }

function showToast(message, type = 'success') {
    document.querySelector('.toast-notification')?.remove();
    const color = type === 'success' ? 'bg-green-600' : 'bg-red-600';
    const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';
    const toast = document.createElement('div');
    toast.className = `toast-notification fixed bottom-6 right-6 z-50 flex items-center gap-3 px-5 py-4 rounded-xl text-white shadow-xl ${color}`;
    toast.innerHTML = `<i class="fas ${icon} text-xl"></i><span>${message}</span>`;
    document.body.appendChild(toast);
    setTimeout(() => {
        toast.style.cssText += 'opacity:0;transition:opacity 0.3s';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// =============================================================================
// UTILITIES
// =============================================================================

function isValidEmail(email) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email); }

function debounce(fn, wait) {
    let t;
    return (...args) => { clearTimeout(t); t = setTimeout(() => fn(...args), wait); };
}

document.addEventListener('DOMContentLoaded', () => {
    // Reset login button in case user navigated back
    const btn = document.querySelector('#login-form button[type="submit"]');
    if (btn) {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-sign-in-alt mr-2"></i>Login';  // match your localizer text
    }

    const loginForm = document.getElementById('login-form');
    if (loginForm) initializeLoginForm(loginForm);

    const registerForm = document.querySelector('form:not(#login-form)');
    if (registerForm) initializeRegisterValidation(registerForm);

    document.querySelectorAll('.feature-card').forEach(c => c.classList.add('fade-in'));
});