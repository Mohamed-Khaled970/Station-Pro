using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{
    public class SubscriptionStatusViewModel
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public SubscriptionRequestStatus Status { get; set; }
        public string? RejectionReason { get; set; }   // populated only when Rejected
        public string PaymentProofUrl { get; set; } = string.Empty;
    }
}
