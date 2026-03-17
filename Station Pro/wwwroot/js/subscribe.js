// =============================================================================
// FILE: wwwroot/js/subscribe.js
// Handles the 3-step Subscribe.cshtml page and Rejected.cshtml resubmit form.
// Shared image compression logic used by both upload zones.
// =============================================================================

'use strict';

// ── State ─────────────────────────────────────────────────────────────────────
let _selectedPlan = '';
let _selectedAmount = 0;
let _currentStep = 1;

// =============================================================================
// SHARED IMAGE COMPRESSION UTILITY
// Used by both the subscribe upload and the rejected-page resubmit upload.
// =============================================================================

// Files under this size skip compression entirely — instant preview
const SKIP_COMPRESS_MB = 1.0;
const MAX_DIMENSION = 1280;   // max px on longest side
const ENCODE_QUALITY = 0.75;   // single fixed quality, no iterative loop

/**
 * Fast compression:
 *  - Files <= SKIP_COMPRESS_MB  -> returned as-is (no canvas work)
 *  - Files >  SKIP_COMPRESS_MB  -> resized + single-pass JPEG encode
 */
async function compressImage(file) {
    if (file.size / 1024 / 1024 <= SKIP_COMPRESS_MB) return file;

    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);

        reader.onload = (event) => {
            const img = new Image();
            img.src = event.target.result;

            img.onload = () => {
                let width = img.width;
                let height = img.height;

                if (width > height) {
                    if (width > MAX_DIMENSION) { height = Math.round(height * MAX_DIMENSION / width); width = MAX_DIMENSION; }
                } else {
                    if (height > MAX_DIMENSION) { width = Math.round(width * MAX_DIMENSION / height); height = MAX_DIMENSION; }
                }

                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                canvas.getContext('2d').drawImage(img, 0, 0, width, height);

                canvas.toBlob((blob) => {
                    if (!blob) { reject(new Error('Canvas toBlob failed')); return; }
                    resolve(new File([blob], file.name, { type: 'image/jpeg', lastModified: Date.now() }));
                }, 'image/jpeg', ENCODE_QUALITY);
            };

            img.onerror = () => reject(new Error('Image load failed'));
        };

        reader.onerror = () => reject(new Error('FileReader failed'));
    });
}

/**
 * Core handler: validates, compresses if needed, calls onSuccess(file).
 * Spinner only shown for files that actually need compression.
 */
async function processImageFile(file, loadingContainer, onSuccess) {
    if (!file || !file.type.startsWith('image/')) {
        alert('Please upload a PNG or JPG image.');
        return;
    }

    const needsCompression = file.size / 1024 / 1024 > SKIP_COMPRESS_MB;
    let loaderId = null;

    if (needsCompression) {
        loaderId = 'compress-loading-' + Date.now();
        loadingContainer.insertAdjacentHTML('beforeend', `
            <div id="${loaderId}" class="flex flex-col items-center gap-2 text-blue-600 py-3">
                <i class="fas fa-spinner fa-spin text-xl"></i>
                <span class="text-xs font-medium">Optimizing...</span>
            </div>`);
    }

    try {
        const compressed = await compressImage(file);
        document.getElementById(loaderId)?.remove();

        if (compressed.size > 5 * 1024 * 1024) {
            alert('File is too large even after compression. Please use a smaller image.');
            return;
        }

        onSuccess(compressed);

    } catch {
        document.getElementById(loaderId)?.remove();
        onSuccess(file);
    }
}


// =============================================================================
// SUBSCRIBE PAGE — 3-step form
// =============================================================================

// ── Step navigation ───────────────────────────────────────────────────────────

function goToStep(step) {
    if (_currentStep === 2 && step === 3) {
        if (!validatePaymentSection()) return;
        populateHiddenFields();
    }

    _currentStep = step;

    document.getElementById('section-plans').classList.toggle('hidden', step !== 1);
    document.getElementById('section-payment').classList.toggle('hidden', step !== 2);
    document.getElementById('section-upload').classList.toggle('hidden', step !== 3);

    // Update step indicator circles
    [1, 2, 3].forEach(n => {
        const circle = document.getElementById(`step${n}-circle`)
            || document.querySelector(`#ind-${n} > div`);
        const label = document.getElementById(`step${n}-label`)
            || document.querySelector(`#ind-${n} > span`);
        if (!circle || !label) return;

        if (n < step) {
            circle.className = 'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-green-500 text-white step-circle';
            circle.innerHTML = '<i class="fas fa-check text-xs"></i>';
        } else if (n === step) {
            circle.className = 'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-blue-600 text-white step-circle';
            circle.textContent = n;
            label.classList.replace('text-gray-400', 'text-gray-900');
        } else {
            circle.className = 'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-gray-200 text-gray-500 step-circle';
            circle.textContent = n;
            label.classList.replace('text-gray-900', 'text-gray-400');
        }
    });

    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ── Plan selection ────────────────────────────────────────────────────────────

function selectPlan(name, price, cardEl) {
    _selectedPlan = name;
    _selectedAmount = price;

    document.querySelectorAll('.plan-card').forEach(c => {
        c.classList.remove(
            'border-blue-500', 'border-purple-500', 'border-orange-500',
            'ring-2', 'ring-blue-200', 'ring-purple-200', 'ring-orange-200',
            'bg-blue-50', 'bg-purple-50', 'bg-orange-50'
        );
    });

    const colourMap = { Basic: 'blue', Pro: 'purple', Enterprise: 'orange' };
    const colour = colourMap[name] || 'blue';
    cardEl.classList.add(`border-${colour}-500`, 'ring-2', `ring-${colour}-200`, `bg-${colour}-50`);

    const displayEl = document.getElementById('display-amount');
    if (displayEl) displayEl.textContent = price;

    const summaryPlan = document.getElementById('summary-plan');
    const summaryAmount = document.getElementById('summary-amount');
    if (summaryPlan) summaryPlan.textContent = name;
    if (summaryAmount) summaryAmount.textContent = price;

    goToStep(2);
}

// ── Payment section validation ────────────────────────────────────────────────

function validatePaymentSection() {
    const method = document.getElementById('paymentMethod').value;
    const phone = document.getElementById('phoneNumber').value.trim();

    if (!method) {
        alert('Please select a payment method.');
        document.getElementById('paymentMethod').focus();
        return false;
    }
    if (!phone || phone.replace(/\D/g, '').length < 10) {
        alert('Please enter a valid phone number (at least 10 digits).');
        document.getElementById('phoneNumber').focus();
        return false;
    }
    return true;
}

// ── Populate hidden fields before submitting ──────────────────────────────────

function populateHiddenFields() {
    document.getElementById('f-plan').value = _selectedPlan;
    document.getElementById('f-amount').value = _selectedAmount;
    document.getElementById('f-paymentMethod').value = document.getElementById('paymentMethod').value;
    document.getElementById('f-phone').value = document.getElementById('phoneNumber').value;
    document.getElementById('f-txRef').value = document.getElementById('transactionRef')?.value || '';
    document.getElementById('f-notes').value = document.getElementById('paymentNotes')?.value || '';
}

// ── Subscribe page: file handling ─────────────────────────────────────────────

function handleFileSelect(input) {
    const file = input.files[0];
    if (!file) return;
    handleSubscribeFile(file, input);
}

function handleDrop(event) {
    event.preventDefault();
    const dropZone = document.getElementById('dropZone');
    dropZone.classList.remove('border-blue-500', 'bg-blue-50');

    const dt = event.dataTransfer;
    if (!dt.files.length) return;

    const input = document.getElementById('fileInput');
    handleSubscribeFile(dt.files[0], input);
}

function handleSubscribeFile(file, input) {
    const dropZone = document.getElementById('dropZone');

    processImageFile(file, dropZone, (compressed) => {
        // Inject compressed file back into the input
        try {
            const dt = new DataTransfer();
            dt.items.add(compressed);
            input.files = dt.files;
        } catch {
            // DataTransfer not supported in this browser — use original
        }

        showSubscribePreview(compressed);
    });
}

function showSubscribePreview(file) {
    const reader = new FileReader();
    reader.onload = e => {
        document.getElementById('previewImage').src = e.target.result;
        document.getElementById('previewArea').classList.remove('hidden');
        document.getElementById('dropZone').classList.add('hidden');
    };
    reader.readAsDataURL(file);
}

function removeImage() {
    document.getElementById('fileInput').value = '';
    document.getElementById('previewArea').classList.add('hidden');
    document.getElementById('dropZone').classList.remove('hidden');
}

// =============================================================================
// REJECTED PAGE — resubmit upload (same compression logic)
// =============================================================================

function previewReupload(input) {
    const file = input.files[0];
    if (!file) return;

    // Clear upload error
    document.getElementById('err-file')?.classList.remove('visible');
    document.getElementById('reuploadZone')?.classList.remove('input-invalid');

    const dropZone = document.getElementById('reuploadZone');

    processImageFile(file, dropZone, (compressed) => {
        // Inject compressed file back into the input
        try {
            const dt = new DataTransfer();
            dt.items.add(compressed);
            input.files = dt.files;
        } catch {
            // DataTransfer not supported — use original
        }

        const reader = new FileReader();
        reader.onload = e => {
            document.getElementById('reuploadImg').src = e.target.result;
            document.getElementById('reuploadPreview').classList.remove('hidden');
            document.getElementById('reuploadZone').classList.add('hidden');
        };
        reader.readAsDataURL(compressed);
    });
}

function removeReupload() {
    document.getElementById('reuploadInput').value = '';
    document.getElementById('reuploadPreview').classList.add('hidden');
    document.getElementById('reuploadZone').classList.remove('hidden');
    document.getElementById('reupload-filename').textContent = '';
}

function handleReuploadDrop(e) {
    e.preventDefault();
    document.getElementById('reuploadZone').classList.remove('border-blue-500', 'bg-blue-50');
    const dt = e.dataTransfer;
    if (!dt.files.length) return;

    const input = document.getElementById('reuploadInput');
    // Simulate input change so previewReupload runs
    try {
        const transfer = new DataTransfer();
        transfer.items.add(dt.files[0]);
        input.files = transfer.files;
    } catch {
        // Fallback: directly process
    }
    previewReupload(input);
}

// =============================================================================
// DOM READY — subscribe form submit guard + phone input filter
// =============================================================================

document.addEventListener('DOMContentLoaded', () => {

    // ── Subscribe form submit guard ───────────────────────────────────────────
    const form = document.getElementById('subscriptionForm');
    if (form) {
        form.addEventListener('submit', e => {
            const fileInput = document.getElementById('fileInput');
            if (!fileInput || !fileInput.files.length) {
                e.preventDefault();
                alert('Please upload your payment screenshot before submitting.');
                return;
            }

            populateHiddenFields();

            const btn = document.getElementById('submitBtn');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Submitting…';
            }
        });

        // Phone: digits only
        const phone = document.getElementById('phoneNumber');
        if (phone) {
            phone.addEventListener('input', () => {
                phone.value = phone.value.replace(/\D/g, '').slice(0, 11);
            });
        }
    }

    // ── Resubmit form validation (Rejected.cshtml) ────────────────────────────
    const resubmitForm = document.getElementById('resubmitForm');
    if (resubmitForm) {

        function setError(inputId, errorId, show) {
            const input = document.getElementById(inputId);
            const err = document.getElementById(errorId);
            if (!input || !err) return;
            input.classList.toggle('input-invalid', show);
            err.classList.toggle('visible', show);
        }

        resubmitForm.addEventListener('submit', function (e) {
            let valid = true;

            // Payment method
            const method = document.getElementById('resubmit-method').value;
            setError('resubmit-method', 'err-method', !method);
            if (!method) valid = false;

            // Phone number
            const phone = document.getElementById('resubmit-phone').value.replace(/\D/g, '');
            setError('resubmit-phone', 'err-phone', phone.length < 10);
            if (phone.length < 10) valid = false;

            // Payment proof file
            const fileInput = document.getElementById('reuploadInput');
            const hasFile = fileInput?.files?.length > 0;
            const fileErr = document.getElementById('err-file');
            const dropZone = document.getElementById('reuploadZone');

            if (!hasFile) {
                fileErr?.classList.add('visible');
                if (!dropZone?.classList.contains('hidden')) {
                    dropZone?.classList.add('input-invalid');
                }
                valid = false;
            } else {
                fileErr?.classList.remove('visible');
                dropZone?.classList.remove('input-invalid');
            }

            if (!valid) {
                e.preventDefault();
                document.querySelector('.field-error.visible, .upload-error.visible')
                    ?.closest('div')
                    ?.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return;
            }

            const btn = document.getElementById('resubmitBtn');
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Submitting…';
            }
        });

        // Phone: digits only
        const resubmitPhone = document.getElementById('resubmit-phone');
        if (resubmitPhone) {
            resubmitPhone.addEventListener('input', function () {
                this.value = this.value.replace(/\D/g, '').slice(0, 11);
                if (this.value.length >= 10) {
                    this.classList.remove('input-invalid');
                    document.getElementById('err-phone')?.classList.remove('visible');
                }
            });
        }

        // Clear method error on change
        const methodSelect = document.getElementById('resubmit-method');
        if (methodSelect) {
            methodSelect.addEventListener('change', function () {
                if (this.value) {
                    this.classList.remove('input-invalid');
                    document.getElementById('err-method')?.classList.remove('visible');
                }
            });
        }
    }
});