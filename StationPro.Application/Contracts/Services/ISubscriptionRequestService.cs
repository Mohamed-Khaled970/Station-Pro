using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{
    public interface ISubscriptionRequestService
    {
        Task<SubscriptionRequest> GetLatestRequest(int TenantId);
        Task<SubscriptionRequest?> GetByIdAsync(int id);
        Task<List<SubscriptionRequest>> GetAllAsync();

        Task<SubscriptionRequest> SubmitAsync(
            int tenantId,
            SubscriptionPlan plan,
            decimal amount,
            string paymentMethod,
            string phoneNumber,
            string transactionReference,
            string paymentProofPath,
            string notes);

        Task<(bool Success, string Error)> ResubmitAsync(
            int tenantId,
            string paymentMethod,
            string phoneNumber,
            string transactionReference,
            string paymentProofPath,
            string notes);

        Task<(bool Success, string Error)> ApproveAsync(int requestId, string reviewedBy);
        Task<(bool Success, string Error)> RejectAsync(int requestId, string reason, string reviewedBy);
    }
}
