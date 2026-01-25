using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{
    // DTO for tenant subscription submission
    public class SubmitSubscriptionDto
    {
        public  int TenantId { get; set; }
        public string SubscriptionPlan { get; set; } = string.Empty; // "Basic", "Pro", "Enterprise"
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "VodafoneCash", "InstaPay"
        public string PhoneNumber { get; set; }  = string.Empty;     
        public string TransactionReference { get; set; } = string.Empty;
        public IFormFile PaymentProof { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
