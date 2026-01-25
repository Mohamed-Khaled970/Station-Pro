// wwwroot/js/auth.js - Optimized for Performance

// ============================================
// FORM VALIDATION
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    const registerForm = document.getElementById('register-form');
    const loginForm = document.getElementById('login-form');

    if (registerForm) {
        initializeRegisterForm(registerForm);
    }

    if (loginForm) {
        initializeLoginForm(loginForm);
    }

    // Simple fade-in for feature cards without animation delays
    const cards = document.querySelectorAll('.feature-card');
    cards.forEach(card => {
        card.classList.add('fade-in');
    });
});

// ============================================
// REGISTER FORM
// ============================================

function initializeRegisterForm(form) {
    const passwordInput = form.querySelector('#password');
    const confirmPasswordInput = form.querySelector('#confirm-password');

    // Password strength indicator with debounce
    if (passwordInput) {
        passwordInput.addEventListener('input', debounce(() => {
            checkPasswordStrength(passwordInput.value);
        }, 200));
    }

    // Confirm password validation
    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', debounce(() => {
            validatePasswordMatch(passwordInput.value, confirmPasswordInput.value);
        }, 200));
    }

    // Form submission
    form.addEventListener('submit', (e) => {
        e.preventDefault();
        if (validateRegisterForm(form)) {
            submitRegisterForm(form);
        }
    });
}

// ============================================
// LOGIN FORM
// ============================================

function initializeLoginForm(form) {
    form.addEventListener('submit', (e) => {
        e.preventDefault();
        if (validateLoginForm(form)) {
            submitLoginForm(form);
        }
    });
}

// ============================================
// PASSWORD STRENGTH
// ============================================

function checkPasswordStrength(password) {
    const strengthIndicator = document.getElementById('password-strength');
    if (!strengthIndicator) {
        const passwordInput = document.getElementById('password');
        if (passwordInput && passwordInput.parentElement) {
            const indicator = document.createElement('div');
            indicator.id = 'password-strength';
            indicator.className = 'password-strength';
            indicator.innerHTML = '<div class="password-strength-bar"></div>';
            passwordInput.parentElement.appendChild(indicator);
        }
    }

    let strength = 0;
    const bar = document.querySelector('.password-strength-bar');

    if (!bar) return;

    // Check length
    if (password.length >= 8) strength++;
    if (password.length >= 12) strength++;

    // Check for numbers
    if (/\d/.test(password)) strength++;

    // Check for lowercase and uppercase
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;

    // Check for special characters
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

    // Update bar
    bar.className = 'password-strength-bar';

    if (strength <= 2) {
        bar.classList.add('strength-weak');
    } else if (strength <= 4) {
        bar.classList.add('strength-medium');
    } else {
        bar.classList.add('strength-strong');
    }
}

// ============================================
// PASSWORD MATCH VALIDATION
// ============================================

function validatePasswordMatch(password, confirmPassword) {
    const errorElement = document.getElementById('password-match-error');
    const confirmInput = document.getElementById('confirm-password');

    if (confirmPassword && password !== confirmPassword) {
        errorElement?.classList.remove('hidden');
        confirmInput?.classList.add('error');
        return false;
    } else {
        errorElement?.classList.add('hidden');
        confirmInput?.classList.remove('error');
        return true;
    }
}

// ============================================
// VALIDATE REGISTER FORM
// ============================================

function validateRegisterForm(form) {
    const formData = new FormData(form);
    const password = formData.get('Password');
    const confirmPassword = formData.get('ConfirmPassword');
    const terms = form.querySelector('#terms');

    let isValid = true;

    // Check password match
    if (password !== confirmPassword) {
        showError('Passwords do not match');
        isValid = false;
    }

    // Check password strength
    if (password && password.length < 6) {
        showError('Password must be at least 6 characters');
        isValid = false;
    }

    // Check terms
    if (terms && !terms.checked) {
        showError('Please accept the terms and conditions');
        isValid = false;
    }

    return isValid;
}

// ============================================
// VALIDATE LOGIN FORM
// ============================================

function validateLoginForm(form) {
    const email = form.querySelector('[name="Email"]').value;
    const password = form.querySelector('[name="Password"]').value;

    if (!email || !password) {
        showError('Please fill in all fields');
        return false;
    }

    if (!isValidEmail(email)) {
        showError('Please enter a valid email address');
        return false;
    }

    return true;
}

// ============================================
// SUBMIT REGISTER FORM
// ============================================

async function submitRegisterForm(form) {
    const submitButton = form.querySelector('button[type="submit"]');
    const originalText = submitButton.innerHTML;

    // Show loading state
    submitButton.disabled = true;
    submitButton.innerHTML = '<i class="fas fa-spinner spinner mr-2"></i>Creating your store...';

    try {
        const formData = new FormData(form);
        const response = await fetch('auth/register', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok && data.success) {
            showSuccess('Account created! Check your email to verify.');

            setTimeout(() => {
                window.location.href = '/verify-email-sent';
            }, 2000);
        } else {
            showError(data.message || 'Registration failed. Please try again.');
            submitButton.disabled = false;
            submitButton.innerHTML = originalText;
        }
    } catch (error) {
        console.error('Registration error:', error);
        showError('An error occurred. Please try again.');
        submitButton.disabled = false;
        submitButton.innerHTML = originalText;
    }
}

// ============================================
// SUBMIT LOGIN FORM
// ============================================

async function submitLoginForm(form) {
    const submitButton = form.querySelector('button[type="submit"]');
    const originalText = submitButton.innerHTML;

    // Show loading state
    submitButton.disabled = true;
    submitButton.innerHTML = '<i class="fas fa-spinner spinner mr-2"></i>Logging in...';

    try {
        const formData = new FormData(form);
        const response = await fetch('/login', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok && data.success) {
            showSuccess('Login successful! Redirecting...');

            setTimeout(() => {
                window.location.href = data.redirectUrl || '/dashboard';
            }, 1000);
        } else {
            showError(data.message || 'Invalid email or password');
            submitButton.disabled = false;
            submitButton.innerHTML = originalText;
        }
    } catch (error) {
        console.error('Login error:', error);
        showError('An error occurred. Please try again.');
        submitButton.disabled = false;
        submitButton.innerHTML = originalText;
    }
}

// ============================================
// TOGGLE PASSWORD VISIBILITY
// ============================================

function togglePassword(inputId) {
    const input = document.getElementById(inputId);
    const button = input?.nextElementSibling?.querySelector('i');

    if (!input || !button) return;

    if (input.type === 'password') {
        input.type = 'text';
        button.classList.remove('fa-eye');
        button.classList.add('fa-eye-slash');
    } else {
        input.type = 'password';
        button.classList.remove('fa-eye-slash');
        button.classList.add('fa-eye');
    }
}

// ============================================
// UTILITIES
// ============================================

function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function showError(message) {
    showToast(message, 'error');
}

function showSuccess(message) {
    showToast(message, 'success');
}

function showToast(message, type = 'success') {
    // Remove existing toast
    const existingToast = document.querySelector('.toast-notification');
    if (existingToast) {
        existingToast.remove();
    }

    // Create new toast
    const toast = document.createElement('div');
    toast.className = `toast-notification ${type}-message`;

    const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';

    toast.innerHTML = `
        <i class="fas ${icon} text-xl"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    // Auto-remove after 5 seconds
    setTimeout(() => {
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), wait);
    };
}

// ============================================
// AUTO-FORMAT PHONE NUMBER
// ============================================

const phoneInput = document.querySelector('input[name="PhoneNumber"]');
if (phoneInput) {
    phoneInput.addEventListener('input', (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 11) {
            value = value.slice(0, 11);
        }
        e.target.value = value;
    });
}