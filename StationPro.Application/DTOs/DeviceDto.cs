using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public string TypeName => Type.ToString();

        // Single and Multi session rates
        public decimal SingleSessionRate { get; set; }
        public decimal? MultiSessionRate { get; set; }
        public bool SupportsMultiSession { get; set; }

        // Keep legacy property for backward compatibility
        public decimal HourlyRate => SingleSessionRate;

        public bool IsAvailable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusName => IsAvailable ? "Available" : "In Use";
        public SessionDto? CurrentSession { get; set; }

        public string RateDisplay
        {
            get
            {
                if (SupportsMultiSession && MultiSessionRate.HasValue)
                {
                    return $"Single: {SingleSessionRate:C} | Multi: {MultiSessionRate.Value:C}";
                }
                return $"{SingleSessionRate:C}/hr";
            }
        }
    }
}
