// StationPro.Infrastructure/Helpers/TimeZoneHelper.cs
//
// Egypt is UTC+2 (UTC+3 during DST, but Egypt abolished DST in 2011).
// The server runs in UTC. All "today/yesterday/week/month" boundaries
// must be computed in Egypt local time and then converted to UTC for DB queries.

namespace StationPro.Infrastructure.Helpers
{

        public static class TimeZoneHelper
        {
            private static readonly TimeZoneInfo EgyptTz = GetEgyptTimeZone();

            private static TimeZoneInfo GetEgyptTimeZone()
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); } catch { }
                try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); } catch { }
                // Fallback: hardcode UTC+2 (Egypt abolished DST in 2011)
                return TimeZoneInfo.CreateCustomTimeZone("Egypt+2", TimeSpan.FromHours(2), "Egypt UTC+2", "Egypt UTC+2");
            }

            /// <summary>Current date/time in Egypt local time.</summary>
            public static DateTime NowInEgypt()
                => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTz);

            /// <summary>Convert a UTC DateTime to Egypt local time for display in views.</summary>
            public static DateTime ToEgyptTime(DateTime utc)
                => TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(utc, DateTimeKind.Utc), EgyptTz);

            /// <summary>Convert an Egypt local DateTime to UTC for DB queries.</summary>
            public static DateTime ToUtc(DateTime egyptLocal)
                => TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(egyptLocal, DateTimeKind.Unspecified), EgyptTz);

            /// <summary>
            /// Returns (startUtc, endUtc) for a named period using Egypt-local midnight boundaries.
            /// </summary>
            public static (DateTime startUtc, DateTime endUtc) GetUtcDateRange(string period)
            {
                var nowEgypt = NowInEgypt();
                var todayEgypt = nowEgypt.Date;

                DateTime startEgypt, endEgypt;
                switch (period.ToLower())
                {
                    case "yesterday":
                        startEgypt = todayEgypt.AddDays(-1);
                        endEgypt = todayEgypt.AddSeconds(-1);
                        break;
                    case "week":
                        startEgypt = todayEgypt.AddDays(-7);
                        endEgypt = nowEgypt;
                        break;
                    case "month":
                        startEgypt = todayEgypt.AddDays(-30);
                        endEgypt = nowEgypt;
                        break;
                    case "all":
                        startEgypt = new DateTime(2000, 1, 1);
                        endEgypt = nowEgypt;
                        break;
                    default: // "today"
                        startEgypt = todayEgypt;
                        endEgypt = todayEgypt.AddDays(1).AddSeconds(-1);
                        break;
                }

                return (ToUtc(startEgypt), ToUtc(endEgypt));
            }
        }
   
}