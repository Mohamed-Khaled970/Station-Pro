using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    public class SubscriptionRequestService : ISubscriptionRequestService
    {
        private readonly ISubscriptionRequestRepository _repo;
        private readonly ITenantRepository _tenantRepo;

        public SubscriptionRequestService(
            ISubscriptionRequestRepository repo,
            ITenantRepository tenantRepo)
        {
            _repo = repo;
            _tenantRepo = tenantRepo;
        }

        public Task<SubscriptionRequest?> GetLatestRequest(int tenantId)
            => _repo.GetLatestRequest(tenantId);

        public Task<SubscriptionRequest?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<List<SubscriptionRequest>> GetAllAsync()
            => _repo.GetAllAsync();

        // ── Submit (first-time) ───────────────────────────────────────────────
        public async Task<SubscriptionRequest> SubmitAsync(
            int tenantId,
            SubscriptionPlan plan,
            decimal amount,
            string paymentMethod,
            string phoneNumber,
            string transactionReference,
            string paymentProofPath,
            string notes)
        {
            var request = new SubscriptionRequest
            {
                TenantId = tenantId,
                SubscriptionPlan = plan,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PhoneNumber = phoneNumber,
                TransactionReference = transactionReference,
                PaymentProofPath = paymentProofPath,
                Notes = notes,
                Status = SubscriptionRequestStatus.Pending,
                SubmittedDate = DateTime.UtcNow,
            };

            return await _repo.AddAsync(request);
        }

        // ── Resubmit (after rejection) ────────────────────────────────────────
        public async Task<(bool Success, string Error)> ResubmitAsync(
            int tenantId,
            string paymentMethod,
            string phoneNumber,
            string transactionReference,
            string paymentProofPath,
            string notes)
        {
            var existing = await _repo.GetLatestRequest(tenantId);
            if (existing == null)
                return (false, "No existing subscription request found.");

            if (existing.Status != SubscriptionRequestStatus.Rejected)
                return (false, "Only rejected requests can be resubmitted.");

            existing.Status = SubscriptionRequestStatus.Pending;
            existing.PaymentMethod = paymentMethod;
            existing.PhoneNumber = phoneNumber;
            existing.TransactionReference = transactionReference;
            existing.PaymentProofPath = paymentProofPath;
            existing.Notes = notes;
            existing.AdminNotes = string.Empty;
            existing.ReviewedDate = null;
            existing.ReviewedByUserId = string.Empty;
            existing.SubmittedDate = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
            return (true, string.Empty);
        }

        // ── Approve ───────────────────────────────────────────────────────────
        public async Task<(bool Success, string Error)> ApproveAsync(int requestId, string reviewedBy)
        {
            var request = await _repo.GetByIdAsync(requestId);
            if (request == null)
                return (false, "Subscription request not found.");

            if (request.Status != SubscriptionRequestStatus.Pending)
                return (false, "Only pending requests can be approved.");

            request.Status = SubscriptionRequestStatus.Approved;
            request.ReviewedDate = DateTime.UtcNow;
            request.ReviewedByUserId = reviewedBy;

            await _repo.UpdateAsync(request);

            // Activate the tenant and set their plan
            var tenant = await _tenantRepo.GetByIdAsync(request.TenantId);
            if (tenant != null)
            {
                tenant.Plan = request.SubscriptionPlan;
                tenant.IsActive = true;
                tenant.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
                tenant.UpdatedAt = DateTime.UtcNow;
                await _tenantRepo.UpdateAsync(tenant);
            }

            return (true, string.Empty);
        }

        // ── Reject ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Error)> RejectAsync(
            int requestId, string reason, string reviewedBy)
        {
            var request = await _repo.GetByIdAsync(requestId);
            if (request == null)
                return (false, "Subscription request not found.");

            if (request.Status != SubscriptionRequestStatus.Pending)
                return (false, "Only pending requests can be rejected.");

            request.Status = SubscriptionRequestStatus.Rejected;
            request.AdminNotes = reason;
            request.ReviewedDate = DateTime.UtcNow;
            request.ReviewedByUserId = reviewedBy;

            await _repo.UpdateAsync(request);
            return (true, string.Empty);
        }
    }
}
