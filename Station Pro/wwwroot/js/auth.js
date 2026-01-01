// wwwroot/js/auth.js

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

    // Add animation delays for feature cards
    const cards = document.querySelectorAll('.feature-card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${0.4 + (index * 0.1)}s`;
        card.classList.add('fade-in');
    });
});

// ============================================
// REGISTER FORM
// ============================================

function initializeRegisterForm(form) {
    const passwordInput = form.querySelector('#password');
    const confirmPasswordInput = form.querySelector('#confirm-password');

    // Password strength indicator
    if (passwordInput) {
        passwordInput.addEventListener('input', () => {
            checkPasswordStrength(passwordInput.value);
        });
    }

    // Confirm password validation
    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', () => {
            validatePasswordMatch(passwordInput.value, confirmPasswordInput.value);
        });
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
        // Create strength indicator if it doesn't exist
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
    submitButton.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Creating your store...';

    try {
        const formData = new FormData(form);
        const response = await fetch('/register', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok && data.success) {
            // Show success message
            showSuccess('Account created! Check your email to verify.');

            // Redirect after 2 seconds
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
    submitButton.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Logging in...';

    try {
        const formData = new FormData(form);
        const response = await fetch('/login', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok && data.success) {
            // Show success and redirect
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
    // Create toast notification
    const toast = document.createElement('div');
    toast.className = 'fixed top-4 right-4 error-message z-50 flex items-center space-x-3 px-6 py-4 rounded-lg shadow-2xl';
    toast.innerHTML = `
        <i class="fas fa-exclamation-circle text-2xl"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

function showSuccess(message) {
    const toast = document.createElement('div');
    toast.className = 'fixed top-4 right-4 success-message z-50 flex items-center space-x-3 px-6 py-4 rounded-lg shadow-2xl';
    toast.innerHTML = `
        <i class="fas fa-check-circle text-2xl"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

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