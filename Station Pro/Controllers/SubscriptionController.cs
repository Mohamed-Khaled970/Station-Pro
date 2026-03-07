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
using StationPro.Application.DTOs.Subscriptions;
using StationPro.Controllers;                         // AdminController.AddSubscriptionRequest()
using StationPro.Domain.Entities;

namespace Station_Pro.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        // ── In-memory store (keyed by TenantId → latest request) ──────────────
        // In production: inject ISubscriptionRepository and use EF Core.
        private static readonly Dictionary<int, SubscriptionRequest> _store = new();
        private static int _nextId = 100;

        // ── Plan pricing lookup ────────────────────────────────────────────────
        private static readonly Dictionary<string, decimal> _planPrices = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Basic"] = 29m,
            ["Pro"] = 79m,
            ["Enterprise"] = 199m,
        };

        public SubscriptionController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // =====================================================================
        // GET /Subscription/Subscribe?tenantId=1
        // Shown immediately after registration (AuthController redirects here).
        // =====================================================================
        [HttpGet]
        public IActionResult Subscribe(int? tenantId)
        {
            var currentTenantId = tenantId ?? 1;

            // Check for an existing pending request
            _store.TryGetValue(currentTenantId, out var existing);

            var tenantModel = AuthController.GetTenantModel();

            var viewModel = new SubscriptionViewModel
            {
                TenantId = currentTenantId,
                TenantName = tenantModel?.StoreName ?? $"Tenant #{currentTenantId}",
                CurrentPlan = "Free",
                CurrentSubscriptionEndDate = null,
                HasPendingSubscription = existing?.Status == SubscriptionRequestStatus.Pending,
                PendingSubscriptionPlan = existing?.SubscriptionPlan.ToString()
            };

            return View(viewModel);
        }

        // =====================================================================
        // POST /Subscription/SubmitSubscription
        // Validates upload, saves request, pushes to AdminController list.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSubscription(SubmitSubscriptionDto dto)
        {
            // ── Basic field validation ────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(dto.SubscriptionPlan) ||
                dto.Amount <= 0 ||
                string.IsNullOrWhiteSpace(dto.PaymentMethod) ||
                string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                TempData["Error"] = "Missing info!|Some required fields are empty. Please complete the form and try again.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            // ── File presence ─────────────────────────────────────────────────
            if (dto.PaymentProof == null || dto.PaymentProof.Length == 0)
            {
                TempData["Error"] = "No proof uploaded!|Please take a screenshot of your payment and upload it.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            // ── File size (5 MB max) ──────────────────────────────────────────
            if (dto.PaymentProof.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File too large!|Please upload an image smaller than 5 MB.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            // ── File type ─────────────────────────────────────────────────────
            var ext = Path.GetExtension(dto.PaymentProof.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png"))
            {
                TempData["Error"] = "Wrong format!|Please upload a JPG or PNG image.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            // ── Parse plan ────────────────────────────────────────────────────
            if (!Enum.TryParse<SubscriptionPlan>(dto.SubscriptionPlan, ignoreCase: true, out var plan))
            {
                TempData["Error"] = "Invalid plan!|The selected subscription plan is not recognised.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }

            try
            {
                // ── Save file ─────────────────────────────────────────────────
                var proofUrl = await SaveProofFile(dto.PaymentProof, dto.TenantId, ext);

                // ── Build domain object ───────────────────────────────────────
                var request = new SubscriptionRequest
                {
                    Id = ++_nextId,
                    TenantId = dto.TenantId,
                    SubscriptionPlan = plan,
                    Amount = dto.Amount,
                    PaymentMethod = dto.PaymentMethod,
                    PhoneNumber = dto.PhoneNumber,
                    TransactionReference = dto.TransactionReference ?? string.Empty,
                    PaymentProofPath = proofUrl,
                    Notes = dto.Notes ?? string.Empty,
                    Status = SubscriptionRequestStatus.Pending,
                    SubmittedDate = DateTime.Now,
                };

                // ── Persist (in-memory) ───────────────────────────────────────
                _store[dto.TenantId] = request;

                // ── Sync to AdminController's shared list ─────────────────────
                var tenantModel = AuthController.GetTenantModel();
                AdminController.AddSubscriptionRequest(new PendingSubscriptionDto
                {
                    Id = request.Id,
                    TenantId = request.TenantId,
                    TenantName = tenantModel?.StoreName ?? $"Tenant #{dto.TenantId}",
                    TenantEmail = tenantModel?.Email ?? string.Empty,
                    Subdomain = GenerateSubdomain(tenantModel?.StoreName ?? "tenant"),
                    SubscriptionPlan = plan.ToString(),
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PhoneNumber = request.PhoneNumber,
                    TransactionReference = request.TransactionReference,
                    PaymentProofUrl = proofUrl,
                    Notes = request.Notes,
                    Status = "Pending",
                    SubmittedDate = request.SubmittedDate,
                });

                // ── Redirect to Pending page ──────────────────────────────────
                TempData["SubscriptionSubmitted"] = "true";
                return RedirectToAction(nameof(Pending), new { tenantId = dto.TenantId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Something went wrong!|{ex.Message}. Please try again or contact support.";
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });
            }
        }

        // =====================================================================
        // GET /Subscription/Pending?tenantId=1
        // "Under review" page — shown after submit and after login if Pending.
        // =====================================================================
        [HttpGet]
        public IActionResult Pending(int tenantId)
        {
            var vm = BuildStatusViewModel(tenantId);
            if (vm == null)
                return RedirectToAction(nameof(Subscribe), new { tenantId });

            // If somehow they land here but are already Approved, send to dashboard
            if (vm.Status == SubscriptionRequestStatus.Approved)
            {
                TempData["ActivationSuccess"] = "true";
                return RedirectToAction("Index", "Dashboard");
            }

            // If Rejected, redirect to the proper rejected page
            if (vm.Status == SubscriptionRequestStatus.Rejected)
                return RedirectToAction(nameof(Rejected), new { tenantId });

            return View(vm);
        }

        // =====================================================================
        // GET /Subscription/Rejected?tenantId=1
        // Shows rejection reason + re-upload form.
        // =====================================================================
        [HttpGet]
        public IActionResult Rejected(int tenantId)
        {
            var vm = BuildStatusViewModel(tenantId);
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

        // =====================================================================
        // POST /Subscription/Resubmit
        // User re-uploads proof after a rejection. Resets status to Pending.
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resubmit(ResubmitSubscriptionDto dto)
        {
            if (!_store.TryGetValue(dto.TenantId, out var existing))
                return RedirectToAction(nameof(Subscribe), new { tenantId = dto.TenantId });

            // ── Validate new file ─────────────────────────────────────────────
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
                var proofUrl = await SaveProofFile(dto.PaymentProof, dto.TenantId, ext);

                // ── Reset the request to Pending ──────────────────────────────
                existing.Status = SubscriptionRequestStatus.Pending;
                existing.PaymentMethod = dto.PaymentMethod;
                existing.PhoneNumber = dto.PhoneNumber;
                existing.TransactionReference = dto.TransactionReference ?? string.Empty;
                existing.PaymentProofPath = proofUrl;
                existing.Notes = dto.Notes ?? string.Empty;
                existing.AdminNotes = string.Empty;
                existing.ReviewedDate = null;
                existing.ReviewedByUserId = string.Empty;
                existing.SubmittedDate = DateTime.Now;

                // ── Sync to AdminController list ──────────────────────────────
                var adminEntry = StationPro.Controllers.AdminController
                    ._subscriptionRequests
                    .FirstOrDefault(r => r.TenantId == dto.TenantId);

                if (adminEntry != null)
                {
                    adminEntry.Status = "Pending";
                    adminEntry.PaymentMethod = dto.PaymentMethod;
                    adminEntry.PhoneNumber = dto.PhoneNumber;
                    adminEntry.TransactionReference = dto.TransactionReference ?? string.Empty;
                    adminEntry.PaymentProofUrl = proofUrl;
                    adminEntry.Notes = dto.Notes;
                    adminEntry.AdminNotes = null;
                    adminEntry.ReviewedDate = null;
                    adminEntry.ReviewedBy = null;
                    adminEntry.SubmittedDate = DateTime.Now;
                }
                else
                {
                    var tenantModel = AuthController.GetTenantModel();
                    StationPro.Controllers.AdminController.AddSubscriptionRequest(new PendingSubscriptionDto
                    {
                        Id = existing.Id,
                        TenantId = existing.TenantId,
                        TenantName = tenantModel?.StoreName ?? $"Tenant #{dto.TenantId}",
                        TenantEmail = tenantModel?.Email ?? string.Empty,
                        SubscriptionPlan = existing.SubscriptionPlan.ToString(),
                        Amount = existing.Amount,
                        PaymentMethod = dto.PaymentMethod,
                        PhoneNumber = dto.PhoneNumber,
                        TransactionReference = dto.TransactionReference ?? string.Empty,
                        PaymentProofUrl = proofUrl,
                        Notes = dto.Notes,
                        Status = "Pending",
                        SubmittedDate = DateTime.Now,
                    });
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

        // =====================================================================
        // GET /Subscription/CheckStatus?tenantId=1
        // JSON endpoint polled by the Pending page every 30 s to auto-redirect.
        // =====================================================================
        [HttpGet]
        public IActionResult CheckStatus(int tenantId)
        {
            if (!_store.TryGetValue(tenantId, out var req))
                return Json(new { status = "none" });

            return Json(new
            {
                status = req.Status.ToString(),          // "Pending" | "Approved" | "Rejected"
                plan = req.SubscriptionPlan.ToString(),
                submittedDate = req.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                reviewedDate = req.ReviewedDate?.ToString("yyyy-MM-dd HH:mm"),
                adminNotes = req.AdminNotes
            });
        }

        // =====================================================================
        // Called by AdminController.ApproveSubscription / RejectSubscription
        // to keep _store in sync with admin actions.
        // =====================================================================

        public static void SyncApproval(int tenantId, string reviewedBy)
        {
            if (!_store.TryGetValue(tenantId, out var req)) return;
            req.Status = SubscriptionRequestStatus.Approved;
            req.ReviewedDate = DateTime.Now;
            req.ReviewedByUserId = reviewedBy;
            req.AdminNotes = string.Empty;
        }

        public static void SyncRejection(int tenantId, string reason, string reviewedBy)
        {
            if (!_store.TryGetValue(tenantId, out var req)) return;
            req.Status = SubscriptionRequestStatus.Rejected;
            req.AdminNotes = reason;
            req.ReviewedDate = DateTime.Now;
            req.ReviewedByUserId = reviewedBy;
        }

        // =====================================================================
        // PRIVATE HELPERS
        // =====================================================================

        private async Task<string> SaveProofFile(IFormFile file, int tenantId, string ext)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payment-proofs");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"tenant-{tenantId}-{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/payment-proofs/{fileName}";
        }

        private SubscriptionStatusViewModel? BuildStatusViewModel(int tenantId)
        {
            if (!_store.TryGetValue(tenantId, out var req)) return null;

            return new SubscriptionStatusViewModel
            {
                TenantId = req.TenantId,
                TenantName = AuthController.GetTenantModel()?.StoreName ?? $"Tenant #{tenantId}",
                PlanName = req.SubscriptionPlan.ToString(),
                PaymentMethod = req.PaymentMethod,
                SubmittedDate = req.SubmittedDate,
                Status = req.Status,
                RejectionReason = string.IsNullOrWhiteSpace(req.AdminNotes) ? null : req.AdminNotes,
                PaymentProofUrl = req.PaymentProofPath,
            };
        }

        private static string GenerateSubdomain(string name)
        {
            var slug = new string(name.ToLower()
                .Replace(" ", "-")
                .Where(c => char.IsLetterOrDigit(c) || c == '-')
                .ToArray());
            return slug + "-" + Guid.NewGuid().ToString()[..6];
        }
    }
}