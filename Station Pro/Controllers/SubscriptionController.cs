// =============================================================================
// FILE: Station_Pro/Controllers/SubscriptionController.cs
//
// Handles the full user-facing subscription flow:
//   GET  /Subscription/Subscribe        → plan picker
//   POST /Subscription/SubmitSubscription → save request + redirect to Pending
//   GET  /Subscription/Pending          → "under review" page
//   GET  /Subscription/Rejected         → rejection page with re-upload form
//   POST /Subscription/Resubmit         → save new attempt, back to Pending
//   GET  /Subscription/CheckStatus      → JSON poll used by Pending page
//
// Storage: static in-memory list (replace with EF Core + repository pattern).
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers;                        // AuthController.GetTenantModel()
using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Application.DTOs.Subscriptions;
using StationPro.Controllers;                         // AdminController.AddSubscriptionRequest()
using StationPro.Domain.Entities;

namespace Station_Pro.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionRequestService _subscriptionService;
        private readonly ITenantRepository _tenantRepo;
        private readonly IWebHostEnvironment _environment;

        public SubscriptionController(
            ISubscriptionRequestService subscriptionService,
            ITenantRepository tenantRepo,
            IWebHostEnvironment environment)
        {
            _subscriptionService = subscriptionService;
            _tenantRepo = tenantRepo;
            _environment = environment;
        }

        // ── GET /Subscription/Subscribe ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Subscribe(int? tenantId)
        {
            // Prefer session over query string — more reliable
            var currentTenantId = HttpContext.Session.GetInt32("TenantId")
                                  ?? tenantId
                                  ?? 0;

            if (currentTenantId == 0)
                return RedirectToAction("Login", "Auth");

            var tenant = await _tenantRepo.GetByIdAsync(currentTenantId);
            var latest = await _subscriptionService.GetLatestRequest(currentTenantId);

            var viewModel = new SubscriptionViewModel
            {
                TenantId = currentTenantId,
                TenantName = tenant?.Name ?? $"Tenant #{currentTenantId}",
                CurrentPlan = tenant?.Plan.ToString() ?? "Free",
                CurrentSubscriptionEndDate = tenant?.SubscriptionEndDate,
                HasPendingSubscription = latest?.Status == SubscriptionRequestStatus.Pending,
                PendingSubscriptionPlan = latest?.SubscriptionPlan.ToString()
            };

            return View(viewModel);
        }

        // ── POST /Subscription/SubmitSubscription ─────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSubscription(SubmitSubscriptionDto dto)
        {
            // ── Validation ────────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(dto.SubscriptionPlan) ||
                dto.Amount <= 0 ||
                string.IsNullOrWhiteSpace(dto.PaymentMethod) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                TempData["Error"] = "Missing info!|Some required fields are empty.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            if (dto.PaymentProof == null || dto.PaymentProof.Length == 0)
            {
                TempData["Error"] = "No proof uploaded!|Please upload a screenshot of your payment.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            if (dto.PaymentProof.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File too large!|Please upload an image smaller than 5 MB.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            var ext = Path.GetExtension(dto.PaymentProof.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png"))
            {
                TempData["Error"] = "Wrong format!|Please upload a JPG or PNG image.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            if (!Enum.TryParse<SubscriptionPlan>(dto.SubscriptionPlan, true, out var plan))
            {
                TempData["Error"] = "Invalid plan!|The selected subscription plan is not recognised.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            try
            {
                var proofPath = await SaveProofFileAsync(dto.PaymentProof, dto.TenantId, ext);

                await _subscriptionService.SubmitAsync(
                    tenantId: dto.TenantId,
                    plan: plan,
                    amount: dto.Amount,
                    paymentMethod: dto.PaymentMethod,
                    phoneNumber: dto.PhoneNumber,
                    transactionReference: dto.TransactionReference ?? string.Empty,
                    paymentProofPath: proofPath,
                    notes: dto.Notes ?? string.Empty);

                TempData["SubscriptionSubmitted"] = "true";
                return RedirectToAction(nameof(Pending), new { tenantId = dto.TenantId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Something went wrong!|{ex.Message}";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }
        }

        // ── GET /Subscription/Pending ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Pending(int tenantId)
        {
            var vm = await BuildStatusViewModelAsync(tenantId);
            if (vm == null)
                return RedirectToAction(nameof(Subscribe), new { tenantId });

            if (vm.Status == SubscriptionRequestStatus.Approved)
            {
                TempData["ActivationSuccess"] = "true";
                return RedirectToAction("Index", "Dashboard");
            }

            if (vm.Status == SubscriptionRequestStatus.Rejected)
                return RedirectToAction(nameof(Rejected), new { tenantId });

            return View(vm);
        }

        // ── GET /Subscription/Rejected ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Rejected(int tenantId)
        {
            var vm = await BuildStatusViewModelAsync(tenantId);
            if (vm == null)
                return RedirectToAction(nameof(Subscribe), new { tenantId });

            if (vm.Status == SubscriptionRequestStatus.Approved)
            {
                TempData["ActivationSuccess"] = "true";
                return RedirectToAction("Index", "Dashboard");
            }

            if (vm.Status == SubscriptionRequestStatus.Pending)
                return RedirectToAction(nameof(Pending), new { tenantId });

            return View(vm);
        }

        // ── POST /Subscription/Resubmit ───────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Resubmit(ResubmitSubscriptionDto dto)
        {
            if (dto.PaymentProof == null || dto.PaymentProof.Length == 0)
            {
                TempData["Error"] = "No proof uploaded!|Please upload a new payment screenshot.";
                return RedirectToAction(nameof(Rejected), new { tenantId = dto.TenantId });
            }

            if (dto.PaymentProof.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File too large!|Please upload an image smaller than 5 MB.";
                return RedirectToAction(nameof(Rejected), new { tenantId = dto.TenantId });
            }

            var ext = Path.GetExtension(dto.PaymentProof.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png"))
            {
                TempData["Error"] = "Wrong format!|Please upload a JPG or PNG image.";
                return RedirectToAction(nameof(Rejected), new { tenantId = dto.TenantId });
            }

            try
            {
                var proofPath = await SaveProofFileAsync(dto.PaymentProof, dto.TenantId, ext);

                var (success, error) = await _subscriptionService.ResubmitAsync(
                    tenantId: dto.TenantId,
                    paymentMethod: dto.PaymentMethod,
                    phoneNumber: dto.PhoneNumber,
                    transactionReference: dto.TransactionReference ?? string.Empty,
                    paymentProofPath: proofPath,
                    notes: dto.Notes ?? string.Empty);

                if (!success)
                {
                    TempData["Error"] = $"Resubmit failed!|{error}";
                    return RedirectToAction(nameof(Rejected), new { tenantId = dto.TenantId });
                }

                TempData["ResubmitSuccess"] = "true";
                return RedirectToAction(nameof(Pending), new { tenantId = dto.TenantId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Upload failed!|{ex.Message}";
                return RedirectToAction(nameof(Rejected), new { tenantId = dto.TenantId });
            }
        }

        // ── GET /Subscription/CheckStatus (polled by Pending page JS) ─────────
        [HttpGet]
        public async Task<IActionResult> CheckStatus(int tenantId)
        {
            var request = await _subscriptionService.GetLatestRequest(tenantId);
            if (request == null)
                return Json(new { status = "none" });

            return Json(new
            {
                status = request.Status.ToString(),
                plan = request.SubscriptionPlan.ToString(),
                submittedDate = request.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                reviewedDate = request.ReviewedDate?.ToString("yyyy-MM-dd HH:mm"),
                adminNotes = request.AdminNotes
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<string> SaveProofFileAsync(IFormFile file, int tenantId, string ext)
        {
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "payment-proofs");
            Directory.CreateDirectory(folder);

            var fileName = $"tenant-{tenantId}-{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/payment-proofs/{fileName}";
        }

        private async Task<SubscriptionStatusViewModel?> BuildStatusViewModelAsync(int tenantId)
        {
            var request = await _subscriptionService.GetLatestRequest(tenantId);
            if (request == null) return null;

            var tenant = await _tenantRepo.GetByIdAsync(tenantId);

            return new SubscriptionStatusViewModel
            {
                TenantId = tenantId,
                TenantName = tenant?.Name ?? $"Tenant #{tenantId}",
                PlanName = request.SubscriptionPlan.ToString(),
                PaymentMethod = request.PaymentMethod,
                SubmittedDate = request.SubmittedDate,
                Status = request.Status,
                RejectionReason = string.IsNullOrWhiteSpace(request.AdminNotes)
                                    ? null
                                    : request.AdminNotes,
                PaymentProofUrl = request.PaymentProofPath,
            };
        }
    }
}