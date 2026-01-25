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
        public string Subdomain { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;        
        public string PaymentProofUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;   
        public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
        public string AdminNotes { get; set; } = string.Empty;  
    }
}
