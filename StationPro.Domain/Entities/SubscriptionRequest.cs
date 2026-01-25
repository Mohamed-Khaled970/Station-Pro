using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Domain.Entities
{
    public class SubscriptionRequest
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public string PaymentProofPath { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public SubscriptionRequestStatus Status { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string ReviewedByUserId { get; set; } = string.Empty;
        public string AdminNotes { get; set; } = string.Empty;

        // Navigation property
        public virtual Tenant Tenant { get; set; }
    }
}
