using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Subscriptions
{
    /// <summary>
    /// Body of POST /Admin/RejectSubscription — carries the mandatory reason.
    /// NOTE: AdminController already declares a local RejectReasonDto class.
    /// If you consolidate, delete that local class and use this one.
    /// </summary>
    public class RejectReasonDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
