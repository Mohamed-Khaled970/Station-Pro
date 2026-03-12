using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{
    // DTO for pending subscriptions admin view
    public class PendingSubscriptionDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public string? PaymentProofUrl { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewedBy { get; set; }
    }
}
