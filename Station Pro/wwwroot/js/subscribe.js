// =============================================================================
// FILE: wwwroot/js/subscribe.js
// Handles the 3-step Subscribe.cshtml page:
//   Step 1 — plan selection
//   Step 2 — payment info form
//   Step 3 — file upload + final submit
// =============================================================================

'use strict';

// ── State ─────────────────────────────────────────────────────────────────────
let _selectedPlan = '';
let _selectedAmount = 0;
let _currentStep = 1;

// ── Step navigation ───────────────────────────────────────────────────────────

function goToStep(step) {
    // Validate before leaving step 2
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
        const circle = document.getElementById(`step${n}-circle`) || document.querySelector(`#ind-${n} > div`);
        const label = document.getElementById(`step${n}-label`) || document.querySelector(`#ind-${n} > span`);
        if (!circle || !label) return;

        if (n < step) {
            // completed
            circle.className = 'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-green-500 text-white';
            circle.innerHTML = '<i class="fas fa-check text-xs"></i>';
        } else if (n === step) {
            // active
            circle.className = 'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-blue-600 text-white';
            circle.textContent = n;
            label.className = label.className.replace('text-gray-400', 'text-gray-900');
        } else {
            // future
            circle.className = 'w-9 h-9 rounded-full flex items-center justify-center font-bold text-sm bg-gray-200 text-gray-500';
            circle.textContent = n;
            label.className = label.className.replace('text-gray-900', 'text-gray-400');
        }
    });

    // Scroll to top of content area
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ── Plan selection ────────────────────────────────────────────────────────────

function selectPlan(name, price, cardEl) {
    _selectedPlan = name;
    _selectedAmount = price;

    // Visual: reset all cards then highlight chosen
    document.querySelectorAll('.plan-card').forEach(c => {
        c.classList.remove('border-blue-500', 'border-purple-500', 'border-orange-500', 'ring-2', 'ring-blue-200', 'bg-blue-50');
    });

    const colourMap = { Basic: 'blue', Pro: 'purple', Enterprise: 'orange' };
    const colour = colourMap[name] || 'blue';
    cardEl.classList.add(`border-${colour}-500`, 'ring-2', `ring-${colour}-200`, `bg-${colour}-50`);

    // Update display amount on step 2
    const displayEl = document.getElementById('display-amount');
    if (displayEl) displayEl.textContent = price;

    // Update summary on step 3
    const summaryPlan = document.getElementById('summary-plan');
    const summaryAmount = document.getElementById('summary-amount');
    if (summaryPlan) summaryPlan.textContent = name;
    if (summaryAmount) summaryAmount.textContent = price;

    // Proceed to step 2
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
    if (!phone || phone.length < 10) {
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

// ── Image Compression ─────────────────────────────────────────────────────────

async function compressImage(file, maxSizeMB = 0.5, maxWidthOrHeight = 1920) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = (event) => {
            const img = new Image();
            img.src = event.target.result;
            img.onload = () => {
                const canvas = document.createElement('canvas');
                let width = img.width;
                let height = img.height;

                // Scale down if needed
                if (width > height) {
                    if (width > maxWidthOrHeight) {
                        height *= maxWidthOrHeight / width;
                        width = maxWidthOrHeight;
                    }
                } else {
                    if (height > maxWidthOrHeight) {
                        width *= maxWidthOrHeight / height;
                        height = maxWidthOrHeight;
                    }
                }

                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);

                // Iteratively reduce quality until under maxSizeMB
                let quality = 0.7;
                const tryCompress = () => {
                    canvas.toBlob((blob) => {
                        if (blob.size / 1024 / 1024 > maxSizeMB && quality > 0.3) {
                            quality -= 0.1;
                            tryCompress();
                        } else {
                            resolve(new File([blob], file.name, {
                                type: 'image/jpeg',
                                lastModified: Date.now()
                            }));
                        }
                    }, 'image/jpeg', quality);
                };
                tryCompress();
            };
            img.onerror = reject;
        };
        reader.onerror = reject;
    });
}

// ── File handling ─────────────────────────────────────────────────────────────

function handleFileSelect(input) {
    const file = input.files[0];
    if (!file) return;
    handleFileWithCompression(file, input);
}

function handleDrop(event) {
    event.preventDefault();
    const dropZone = document.getElementById('dropZone');
    dropZone.classList.remove('border-blue-500', 'bg-blue-50');

    const dt = event.dataTransfer;
    if (!dt.files.length) return;

    const input = document.getElementById('fileInput');
    try {
        handleFileWithCompression(dt.files[0], input);
    } catch {
        // DataTransfer not supported — skip
    }
}

async function handleFileWithCompression(file, input) {
    if (!file.type.startsWith('image/')) {
        alert('Please upload a PNG or JPG image.');
        return;
    }

    // Show loading spinner inside drop zone
    const dropZone = document.getElementById('dropZone');
    dropZone.insertAdjacentHTML('beforeend', `
        <div id="compress-loading" class="flex flex-col items-center gap-2 text-blue-600 py-4">
            <i class="fas fa-spinner fa-spin text-2xl"></i>
            <span class="text-sm font-medium">Optimizing image...</span>
        </div>`);

    try {
        const originalMB = (file.size / 1024 / 1024).toFixed(2);
        const compressed = await compressImage(file, 0.5, 1920);
        const compressedMB = (compressed.size / 1024 / 1024).toFixed(2);
        const savings = ((1 - compressed.size / file.size) * 100).toFixed(0);

        document.getElementById('compress-loading')?.remove();

        if (compressed.size > 5 * 1024 * 1024) {
            alert('File is too large even after compression. Please use a smaller image.');
            return;
        }

        // Replace file input with compressed version
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(compressed);
        input.files = dataTransfer.files;

        showPreview(compressed, `Optimized: ${compressedMB}MB (${savings}% smaller than ${originalMB}MB)`);

    } catch (err) {
        document.getElementById('compress-loading')?.remove();
        console.error('Compression error:', err);
        // Fall back to original file without compression
        showPreview(file, null);
    }
}

function showPreview(file, sizeInfo) {
    const reader = new FileReader();
    reader.onload = e => {
        document.getElementById('previewImage').src = e.target.result;

        const nameEl = document.getElementById('fileName');
        nameEl.textContent = file.name + ' (' + (file.size / 1024).toFixed(1) + ' KB)';
        if (sizeInfo) {
            nameEl.textContent += ' — ' + sizeInfo;
        }

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

// ── Submit guard ──────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('subscriptionForm');
    if (!form) return;

    form.addEventListener('submit', e => {
        const fileInput = document.getElementById('fileInput');
        if (!fileInput || !fileInput.files.length) {
            e.preventDefault();
            alert('Please upload your payment screenshot before submitting.');
            return;
        }

        // Ensure hidden fields are filled
        populateHiddenFields();

        // Disable button to prevent double-submit
        const btn = document.getElementById('submitBtn');
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Submitting…';
        }
    });

    // Phone number: digits only
    const phone = document.getElementById('phoneNumber');
    if (phone) {
        phone.addEventListener('input', () => {
            phone.value = phone.value.replace(/\D/g, '').slice(0, 11);
        });
    }
});