using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers;
using StationPro.Application.DTOs.Subscriptions;
using StationPro.Domain.Entities;

namespace StationPro.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        // Static storage for demo - replace with actual database
        private static List<SubscriptionRequest> _subscriptionRequests = new List<SubscriptionRequest>();
        private static int _nextId = 1;

        public SubscriptionController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // GET: Subscription/Subscribe
        public IActionResult Subscribe(int? tenantId)
        {
            // In real implementation, get from authenticated user
            var currentTenantId = tenantId ?? 1;

            // Check if tenant has pending subscription
            var pendingSubscription = _subscriptionRequests
                .FirstOrDefault(s => s.TenantId == currentTenantId &&
                                s.Status == SubscriptionRequestStatus.Pending);

            var viewModel = new SubscriptionViewModel
            {
                TenantId = currentTenantId,
                TenantName = "Demo Store", // Get from database
                CurrentPlan = "Free",
                CurrentSubscriptionEndDate = null,
                HasPendingSubscription = true,
                PendingSubscriptionPlan = pendingSubscription?.SubscriptionPlan.ToString()
            };

            return View(viewModel);
        }

        // POST: Subscription/SubmitSubscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSubscription(SubmitSubscriptionDto dto)
        {
            // Don't rely on ModelState for hidden fields - validate manually
            if (string.IsNullOrEmpty(dto.SubscriptionPlan) ||
                dto.Amount <= 0 ||
                string.IsNullOrEmpty(dto.PaymentMethod) ||
                string.IsNullOrEmpty(dto.PhoneNumber))
            {
                TempData["Error"] = "Oops! Some required information is missing. Please try again.";
                return RedirectToAction("Subscribe");
            }

            // Validate payment proof
            if (dto.PaymentProof == null || dto.PaymentProof.Length == 0)
            {
                TempData["Error"] = "Missing proof! 📸|Don't forget to upload your payment screenshot.";
                return RedirectToAction("Subscribe");
            }

            // Validate file size (5MB max)
            if (dto.PaymentProof.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File too large! 📦|Your image is too big! Please upload an image smaller than 5MB.";
                return RedirectToAction("Subscribe");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(dto.PaymentProof.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "Wrong format! 🖼️|Please upload a JPG or PNG image only.";
                return RedirectToAction("Subscribe");
            }

            try
            {
                // Save payment proof file
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payment-proofs");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.PaymentProof.CopyToAsync(fileStream);
                }

                // Parse subscription plan
                SubscriptionPlan plan;
                if (!Enum.TryParse<SubscriptionPlan>(dto.SubscriptionPlan, out plan))
                {
                    TempData["Error"] = "Invalid plan! 🤔|The selected subscription plan is not valid.";
                    return RedirectToAction("Subscribe");
                }

                // Create subscription request
                var subscriptionRequest = new SubscriptionRequest
                {
                    Id = _nextId++,
                    TenantId = dto.TenantId,
                    SubscriptionPlan = plan,
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    PhoneNumber = dto.PhoneNumber,
                    TransactionReference = dto.TransactionReference ?? string.Empty,
                    PaymentProofPath = $"/uploads/payment-proofs/{fileName}",
                    Notes = dto.Notes ?? string.Empty,
                    Status = SubscriptionRequestStatus.Pending,
                    SubmittedDate = DateTime.Now,
                    ReviewedDate = null,
                    ReviewedByUserId = string.Empty,
                    AdminNotes = string.Empty
                };

                // Save to static list (replace with database save)
                _subscriptionRequests.Add(subscriptionRequest);
                var PendingSubscription = new PendingSubscriptionDto
                {
                    Id = subscriptionRequest.Id,
                    TenantId = subscriptionRequest.TenantId,
                    TenantName = AuthController.GetTenantModel().StoreName,
                    TenantEmail = AuthController.GetTenantModel().Email,
                    Subdomain = $"{AuthController.GetTenantModel().StoreName}.Com",
                    SubscriptionPlan = plan.ToString(),
                    Amount = subscriptionRequest.Amount,
                    PaymentMethod = subscriptionRequest.PaymentMethod,
                    PhoneNumber = subscriptionRequest.PhoneNumber,
                    TransactionReference = subscriptionRequest.TransactionReference ?? string.Empty,
                    PaymentProofUrl = $"/uploads/payment-proofs/{fileName}",
                    Notes = subscriptionRequest.Notes ?? string.Empty,
                    Status = "Pending",
                    SubmittedDate = DateTime.Now,
                    ReviewedDate = null,
                    ReviewedBy = null,
                    AdminNotes = string.Empty
                };
                AdminController.AddSubscriptionRequest(PendingSubscription);

                TempData["Success"] = "🎉 Awesome!|Your subscription request has been submitted! We'll review it within 24 hours and notify you via email.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Something went wrong! 😕|Error: {ex.Message}. Please try again or contact support.";
                return RedirectToAction("Subscribe");
            }
        }

        // GET: Subscription/SubscriptionSuccess
        public IActionResult SubscriptionSuccess()
        {
            return View();
        }

        // GET: Subscription/CheckStatus
        public IActionResult CheckStatus(int tenantId)
        {
            var subscription = _subscriptionRequests
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.SubmittedDate)
                .FirstOrDefault();

            if (subscription == null)
            {
                return Json(new { status = "none" });
            }

            return Json(new
            {
                status = subscription.Status.ToString(),
                plan = subscription.SubscriptionPlan.ToString(),
                submittedDate = subscription.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                reviewedDate = subscription.ReviewedDate?.ToString("yyyy-MM-dd HH:mm"),
                adminNotes = subscription.AdminNotes
            });
        }
    }
}