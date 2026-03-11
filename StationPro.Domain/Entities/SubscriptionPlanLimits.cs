using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Domain.Entities
{
    public class SubscriptionPlanLimits
    {
        public int Id { get; set; }

        /// <summary>Which plan these limits apply to.</summary>
        public SubscriptionPlan Plan { get; set; }

        /// <summary>Max number of devices allowed. -1 = unlimited.</summary>
        public int MaxDevices { get; set; }

        /// <summary>Max number of rooms allowed. -1 = unlimited.</summary>
        public int MaxRooms { get; set; }

        /// <summary>How many days back reports can go. -1 = unlimited.</summary>
        public int ReportRetentionDays { get; set; }

        /// <summary>Whether the plan allows exporting reports (CSV/PDF).</summary>
        public bool AllowsReportExport { get; set; }

        /// <summary>Monthly price in USD for display purposes.</summary>
        public decimal MonthlyPrice { get; set; }

        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
