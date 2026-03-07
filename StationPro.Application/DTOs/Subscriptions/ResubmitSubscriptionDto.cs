using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{
    public class ResubmitSubscriptionDto
    {
        public int TenantId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public IFormFile? PaymentProof { get; set; }
    }
}
