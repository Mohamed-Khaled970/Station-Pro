using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class SessionFilterRequest
    {
        public string DateFilter { get; set; } = "today";   // today|yesterday|week|month|all
        public string Status { get; set; } = "all";         // all|active|completed
        public int? DeviceId { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }
}
