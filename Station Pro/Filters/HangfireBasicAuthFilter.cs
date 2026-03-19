using Hangfire.Dashboard;
using System.Text;

namespace StationPro.Filters
{
    public class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
    {
        private readonly string _username;
        private readonly string _password;

        public HangfireBasicAuthFilter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow if already authenticated as admin (optional convenience)
            if (httpContext.User.Identity?.IsAuthenticated == true &&
                httpContext.User.IsInRole("Admin"))
                return true;

            var header = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (header == null || !header.StartsWith("Basic "))
            {
                Challenge(httpContext);
                return false;
            }

            try
            {
                var encoded = header["Basic ".Length..].Trim();
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = decoded.Split(':', 2);
                var username = parts[0];
                var password = parts[1];

                if (username == _username && password == _password)
                    return true;
            }
            catch
            {
                // malformed header — fall through to challenge
            }

            Challenge(httpContext);
            return false;
        }

        private static void Challenge(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"StationPro Hangfire\"";
        }
    }
}
