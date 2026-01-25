// subscribe.js - Subscription Page JavaScript (3 Steps) - Optimized Version

let selectedPlan = null;
let selectedAmount = 0;
let paymentData = {};
let compressedFile = null;

// STEP 1: Plan selection
function selectPlan(plan, amount, element) {
    selectedPlan = plan;
    selectedAmount = amount;

    // Remove selected class from all cards
    document.querySelectorAll('.plan-card').forEach(card => {
        card.classList.remove('selected');
    });

    // Add selected class to clicked card
    element.classList.add('selected');

    // Update hidden form fields
    document.getElementById('selectedPlan').value = plan;
    document.getElementById('amount').value = amount;

    // Update display amount
    document.getElementById('display-amount').textContent = amount;

    // Show payment section (Step 2)
    showPaymentSection();
}

// STEP 2: Show payment section
function showPaymentSection() {
    // Update step indicators
    document.getElementById('step-1').classList.remove('bg-blue-600');
    document.getElementById('step-1').classList.add('bg-green-600');
    document.getElementById('step-2').classList.add('bg-blue-600');
    document.getElementById('step-2').classList.remove('bg-gray-300');
    document.getElementById('step-2-text').classList.add('text-gray-900', 'font-bold');
    document.getElementById('step-2-text').classList.remove('text-gray-500');

    // Hide plans section and show payment section
    document.getElementById('plans-section').style.display = 'none';
    document.getElementById('payment-section').classList.remove('hidden');

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Go to upload proof (Step 3)
function goToUploadProof() {
    // Validate payment form
    const paymentMethod = document.getElementById('paymentMethod').value;
    const phoneNumber = document.getElementById('phoneNumber').value;

    if (!paymentMethod) {
        showFriendlyAlert('Oops! 😊', 'Please select how you made the payment (Vodafone Cash or InstaPay)', 'warning');
        return;
    }

    if (!phoneNumber) {
        showFriendlyAlert('Hold on! 📱', 'We need your phone number to verify the payment', 'warning');
        return;
    }

    if (phoneNumber.length < 11) {
        showFriendlyAlert('Almost there! 🎯', 'Please enter a valid 11-digit phone number', 'warning');
        return;
    }

    // Store payment data
    paymentData = {
        paymentMethod: paymentMethod,
        phoneNumber: phoneNumber,
        transactionRef: document.getElementById('transactionRef').value,
        notes: document.getElementById('notes').value
    };

    // Copy data to final form hidden fields
    document.getElementById('finalPlan').value = selectedPlan;
    document.getElementById('finalAmount').value = selectedAmount;
    document.getElementById('finalPaymentMethod').value = paymentData.paymentMethod;
    document.getElementById('finalPhoneNumber').value = paymentData.phoneNumber;
    document.getElementById('finalTransactionRef').value = paymentData.transactionRef;
    document.getElementById('finalNotes').value = paymentData.notes;

    // Update step indicators
    document.getElementById('step-2').classList.remove('bg-blue-600');
    document.getElementById('step-2').classList.add('bg-green-600');
    document.getElementById('step-3').classList.add('bg-blue-600');
    document.getElementById('step-3').classList.remove('bg-gray-300');
    document.getElementById('step-3-text').classList.add('text-gray-900', 'font-bold');
    document.getElementById('step-3-text').classList.remove('text-gray-500');

    // Hide payment section and show upload section
    document.getElementById('payment-section').classList.add('hidden');
    document.getElementById('upload-section').classList.remove('hidden');

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Back to payment info
function backToPayment() {
    // Update step indicators
    document.getElementById('step-2').classList.add('bg-blue-600');
    document.getElementById('step-2').classList.remove('bg-green-600');
    document.getElementById('step-3').classList.remove('bg-blue-600');
    document.getElementById('step-3').classList.add('bg-gray-300');
    document.getElementById('step-3-text').classList.remove('text-gray-900', 'font-bold');
    document.getElementById('step-3-text').classList.add('text-gray-500');

    // Show payment section and hide upload section
    document.getElementById('upload-section').classList.add('hidden');
    document.getElementById('payment-section').classList.remove('hidden');

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Back to plans (from Step 2)
function backToPlans() {
    // Reset step indicators
    document.getElementById('step-1').classList.add('bg-blue-600');
    document.getElementById('step-1').classList.remove('bg-green-600');
    document.getElementById('step-2').classList.remove('bg-blue-600');
    document.getElementById('step-2').classList.add('bg-gray-300');
    document.getElementById('step-2-text').classList.remove('text-gray-900', 'font-bold');
    document.getElementById('step-2-text').classList.add('text-gray-500');

    // Show plans section and hide payment section
    document.getElementById('plans-section').style.display = 'grid';
    document.getElementById('payment-section').classList.add('hidden');

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// File upload handling
const uploadArea = document.getElementById('uploadArea');
const fileInput = document.getElementById('fileInput');
const preview = document.getElementById('preview');
const previewImage = document.getElementById('previewImage');

if (uploadArea && fileInput) {
    // Click to upload
    uploadArea.addEventListener('click', () => {
        fileInput.click();
    });

    // File selection
    fileInput.addEventListener('change', (e) => {
        handleFile(e.target.files[0]);
    });

    // Drag and drop
    uploadArea.addEventListener('dragover', (e) => {
        e.preventDefault();
        uploadArea.classList.add('drag-over');
    });

    uploadArea.addEventListener('dragleave', () => {
        uploadArea.classList.remove('drag-over');
    });

    uploadArea.addEventListener('drop', (e) => {
        e.preventDefault();
        uploadArea.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        handleFile(file);
    });
}

// Compress image function - REDUCES FILE SIZE BY 60-80%
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

                // Calculate new dimensions
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

                // Start with quality 0.7 and reduce if needed
                let quality = 0.7;
                const tryCompress = () => {
                    canvas.toBlob((blob) => {
                        const sizeMB = blob.size / 1024 / 1024;

                        // If still too large and quality can be reduced, try again
                        if (sizeMB > maxSizeMB && quality > 0.3) {
                            quality -= 0.1;
                            tryCompress();
                        } else {
                            // Create a new File object from the blob
                            const compressedFile = new File([blob], file.name, {
                                type: 'image/jpeg',
                                lastModified: Date.now()
                            });
                            resolve(compressedFile);
                        }
                    }, 'image/jpeg', quality);
                };

                tryCompress();
            };
            img.onerror = (error) => reject(error);
        };
        reader.onerror = (error) => reject(error);
    });
}

// Handle file upload with compression
async function handleFile(file) {
    if (!file) return;

    // Validate file type
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png'];
    if (!validTypes.includes(file.type)) {
        showFriendlyAlert('Wrong format! 🖼️', 'Please upload a JPG or PNG image of your payment screenshot', 'warning');
        return;
    }

    // Show compression loading
    const loadingDiv = document.createElement('div');
    loadingDiv.id = 'compression-loading';
    loadingDiv.className = 'text-center py-4';
    loadingDiv.innerHTML = `
        <div class="inline-flex items-center gap-2 text-blue-600">
            <svg class="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span class="text-sm font-medium">Optimizing image...</span>
        </div>
    `;
    uploadArea.appendChild(loadingDiv);

    try {
        const originalSizeMB = (file.size / 1024 / 1024).toFixed(2);

        // Compress the image
        compressedFile = await compressImage(file, 0.5, 1920);

        const compressedSizeMB = (compressedFile.size / 1024 / 1024).toFixed(2);
        const savings = ((1 - compressedFile.size / file.size) * 100).toFixed(0);

        // Remove loading
        loadingDiv.remove();

        // Validate compressed file size (should be much smaller now)
        if (compressedFile.size > 5 * 1024 * 1024) {
            showFriendlyAlert('File too large! 📦', 'Your image is too big even after compression! Please use a smaller image.', 'warning');
            return;
        }

        // Show compression success message
        showFriendlyAlert(
            'Image optimized! 🚀',
            `Reduced from ${originalSizeMB}MB to ${compressedSizeMB}MB (${savings}% smaller). This will upload much faster!`,
            'success'
        );

        // Update file input with compressed file
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(compressedFile);
        fileInput.files = dataTransfer.files;

        // Show preview
        const reader = new FileReader();
        reader.onload = (e) => {
            previewImage.src = e.target.result;
            preview.classList.remove('hidden');
            uploadArea.style.display = 'none';

            // Add file size info to preview
            const sizeInfo = document.createElement('p');
            sizeInfo.className = 'text-xs text-gray-500 mt-2';
            sizeInfo.innerHTML = `📦 Optimized size: ${compressedSizeMB}MB (${savings}% smaller)`;
            preview.appendChild(sizeInfo);
        };
        reader.readAsDataURL(compressedFile);

    } catch (error) {
        loadingDiv.remove();
        showFriendlyAlert('Compression failed! 😕', 'Could not optimize the image. Please try another image.', 'error');
        console.error('Compression error:', error);
    }
}

// Remove image
function removeImage() {
    fileInput.value = '';
    compressedFile = null;
    preview.classList.add('hidden');
    uploadArea.style.display = 'block';
    previewImage.src = '';

    // Remove size info if exists
    const sizeInfo = preview.querySelector('.text-xs.text-gray-500');
    if (sizeInfo) sizeInfo.remove();
}

// Form validation and submission - OPTIMIZED VERSION
document.getElementById('subscriptionForm')?.addEventListener('submit', function (e) {
    e.preventDefault();

    // Validate file upload
    if (!fileInput.files || fileInput.files.length === 0) {
        showFriendlyAlert('Missing proof! 📸', 'Don\'t forget to upload your payment screenshot so we can verify your payment!', 'warning');
        return;
    }

    // Validate all required fields
    const plan = document.getElementById('finalPlan').value;
    const amount = document.getElementById('finalAmount').value;
    const paymentMethod = document.getElementById('finalPaymentMethod').value;
    const phoneNumber = document.getElementById('finalPhoneNumber').value;

    if (!plan || !amount || !paymentMethod || !phoneNumber) {
        showFriendlyAlert('Hold on! ⚠️', 'Looks like something went wrong. Please go back and fill in all the payment details.', 'warning');
        return;
    }

    // Show upload progress
    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalBtnText = submitBtn.innerHTML;
    submitBtn.disabled = true;

    showUploadProgress();

    // Submit the form
    const formData = new FormData(e.target);

    // Use XMLHttpRequest for progress tracking
    const xhr = new XMLHttpRequest();

    // Track upload progress
    xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable) {
            const percentComplete = (e.loaded / e.total) * 100;
            updateUploadProgress(percentComplete);
        }
    });

    // Handle completion
    xhr.addEventListener('load', () => {
        if (xhr.status === 200 || xhr.status === 302) {
            showSuccessMessage();
            setTimeout(() => {
                window.location.href = '/Dashboard/Index';
            }, 2000);
        } else {
            hideUploadProgress();
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalBtnText;
            showFriendlyAlert('Upload failed! 😕', 'Something went wrong. Please try again.', 'error');
        }
    });

    // Handle errors
    xhr.addEventListener('error', () => {
        hideUploadProgress();
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalBtnText;
        showFriendlyAlert('Network error! 📡', 'Please check your internet connection and try again.', 'error');
    });

    // Send the request
    xhr.open('POST', e.target.action);
    xhr.setRequestHeader('RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);
    xhr.send(formData);
});

// Show upload progress overlay
function showUploadProgress() {
    const overlay = document.createElement('div');
    overlay.id = 'upload-progress-overlay';
    overlay.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
    overlay.innerHTML = `
        <div class="bg-white rounded-2xl p-8 max-w-md mx-4 text-center">
            <div class="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg class="w-10 h-10 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"></path>
                </svg>
            </div>
            <h2 class="text-2xl font-bold text-gray-900 mb-2">Uploading... 📤</h2>
            <p class="text-gray-600 mb-4">Please wait while we upload your payment proof</p>
            <div class="w-full bg-gray-200 rounded-full h-3 mb-2">
                <div id="upload-progress-bar" class="bg-blue-600 h-3 rounded-full transition-all duration-300" style="width: 0%"></div>
            </div>
            <p id="upload-progress-text" class="text-sm text-gray-500">0%</p>
            <p class="text-xs text-gray-400 mt-4">💡 Tip: Thanks to image optimization, this should be quick!</p>
        </div>
    `;
    document.body.appendChild(overlay);
}

// Update progress bar
function updateUploadProgress(percent) {
    const bar = document.getElementById('upload-progress-bar');
    const text = document.getElementById('upload-progress-text');
    if (bar && text) {
        bar.style.width = percent + '%';
        text.textContent = Math.round(percent) + '%';
    }
}

// Hide upload progress
function hideUploadProgress() {
    const overlay = document.getElementById('upload-progress-overlay');
    if (overlay) overlay.remove();
}

// Show friendly alert messages
function showFriendlyAlert(title, message, type = 'info') {
    const icons = {
        success: '✅',
        error: '❌',
        warning: '⚠️',
        info: 'ℹ️'
    };

    const colors = {
        success: 'bg-green-50 border-green-500 text-green-800',
        error: 'bg-red-50 border-red-500 text-red-800',
        warning: 'bg-yellow-50 border-yellow-500 text-yellow-800',
        info: 'bg-blue-50 border-blue-500 text-blue-800'
    };

    const alertDiv = document.createElement('div');
    alertDiv.className = `fixed top-4 right-4 z-50 max-w-md ${colors[type]} border-l-4 p-4 rounded-lg shadow-lg transform transition-all duration-300 translate-x-full`;
    alertDiv.innerHTML = `
        <div class="flex items-start">
            <div class="flex-shrink-0 text-2xl mr-3">${icons[type]}</div>
            <div class="flex-1">
                <h3 class="font-bold mb-1">${title}</h3>
                <p class="text-sm">${message}</p>
            </div>
            <button onclick="this.parentElement.parentElement.remove()" class="ml-3 text-gray-500 hover:text-gray-700">
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
                </svg>
            </button>
        </div>
    `;

    document.body.appendChild(alertDiv);
    setTimeout(() => alertDiv.classList.remove('translate-x-full'), 10);
    setTimeout(() => {
        alertDiv.classList.add('translate-x-full');
        setTimeout(() => alertDiv.remove(), 300);
    }, 5000);
}

// Show success message
function showSuccessMessage() {
    hideUploadProgress();

    const overlay = document.createElement('div');
    overlay.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
    overlay.innerHTML = `
        <div class="bg-white rounded-2xl p-8 max-w-md mx-4 text-center transform scale-95 animate-scale-in">
            <div class="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4 animate-bounce-in">
                <svg class="w-10 h-10 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                </svg>
            </div>
            <h2 class="text-2xl font-bold text-gray-900 mb-2">🎉 Awesome!</h2>
            <p class="text-gray-600 mb-4">Your subscription request has been submitted successfully!</p>
            <div class="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
                <p class="text-sm text-blue-800">
                    <strong>What's next?</strong><br>
                    Our team will review your payment and activate your subscription within 24 hours. We'll notify you via email!
                </p>
            </div>
            <div class="flex items-center justify-center text-sm text-gray-500">
                <svg class="animate-spin h-5 w-5 mr-2 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Redirecting to dashboard...
            </div>
        </div>
    `;

    document.body.appendChild(overlay);

    const style = document.createElement('style');
    style.textContent = `
        @keyframes scale-in {
            from { transform: scale(0.9); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
        }
        @keyframes bounce-in {
            0% { transform: scale(0); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
        }
        .animate-scale-in { animation: scale-in 0.3s ease-out; }
        .animate-bounce-in { animation: bounce-in 0.5s ease-out; }
    `;
    document.head.appendChild(style);
}

// Auto-select plan from URL
document.addEventListener('DOMContentLoaded', function () {
    const urlParams = new URLSearchParams(window.location.search);
    const planParam = urlParams.get('plan');

    if (planParam) {
        const planCards = {
            'Basic': { element: document.querySelector('[onclick*="Basic"]'), amount: 29 },
            'Pro': { element: document.querySelector('[onclick*="Pro"]'), amount: 79 },
            'Enterprise': { element: document.querySelector('[onclick*="Enterprise"]'), amount: 199 }
        };

        const selectedPlanData = planCards[planParam];
        if (selectedPlanData && selectedPlanData.element) {
            selectedPlanData.element.click();
        }
    }
});

// Format phone number input
document.querySelector('input[name="PhoneNumber"]')?.addEventListener('input', function (e) {
    let value = e.target.value.replace(/\D/g, '');
    if (value.length > 11) value = value.substring(0, 11);
    e.target.value = value;
});