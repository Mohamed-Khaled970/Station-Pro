using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Helpers
{

    public static class EgyptTimeExtensions
    {
        private static readonly TimeZoneInfo EgyptTz = GetEgyptTimeZone();

        private static TimeZoneInfo GetEgyptTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); } catch { }
            try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); } catch { }
            return TimeZoneInfo.CreateCustomTimeZone("Egypt+2", TimeSpan.FromHours(2), "Egypt UTC+2", "Egypt UTC+2");
        }

        public static string ToEgyptDisplay(this DateTime utc, string format = "MMM dd, yyyy h:mm tt")
        {
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utc, DateTimeKind.Utc), EgyptTz);
            return egyptTime.ToString(format);
        }

        public static string ToEgyptDisplay(this DateTime? utc, string format = "MMM dd, yyyy h:mm tt")
            => utc.HasValue ? utc.Value.ToEgyptDisplay(format) : string.Empty;

        public static DateTime ToEgyptTime(this DateTime utc)
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), EgyptTz);
    }
}
