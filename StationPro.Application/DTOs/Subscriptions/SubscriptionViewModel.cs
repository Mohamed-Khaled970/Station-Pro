using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{

    // DTO for displaying subscription in view model
    public class SubscriptionViewModel
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string CurrentPlan { get; set; } = string.Empty;
        public DateTime? CurrentSubscriptionEndDate { get; set; }
        public string PendingSubscriptionPlan { get; set; } = string.Empty;

        public bool HasPendingSubscription { get; set; }
    }
}
