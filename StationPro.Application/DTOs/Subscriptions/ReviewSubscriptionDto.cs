using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{
    public class ReviewSubscriptionDto
    {
        public int SubscriptionRequestId { get; set; }
        public bool Approved { get; set; }
        public string AdminNotes { get; set; } = string.Empty;          
        public string ReviewedByUserId { get; set; } = string.Empty;
    }
}
