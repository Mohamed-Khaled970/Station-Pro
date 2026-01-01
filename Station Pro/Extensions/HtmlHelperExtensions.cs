using Microsoft.AspNetCore.Mvc.Rendering;

namespace Station_Pro.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Generates a tenant-specific URL for the given action and controller
        /// </summary>
        public static string TenantUrl(this IHtmlHelper htmlHelper, string controller, string action)
        {
            // For now, return a simple URL without tenant prefix
            // Later you can add tenant subdomain/path logic here
            return $"/{controller}/{action}";
        }

        /// <summary>
        /// Generates a tenant-specific URL with query parameters
        /// </summary>
        public static string TenantUrl(this IHtmlHelper htmlHelper, string controller, string action, object routeValues)
        {
            var baseUrl = $"/{controller}/{action}";

            if (routeValues != null)
            {
                var properties = routeValues.GetType().GetProperties();
                var queryString = string.Join("&",
                    properties.Select(p => $"{p.Name}={p.GetValue(routeValues)}"));

                if (!string.IsNullOrEmpty(queryString))
                {
                    baseUrl += "?" + queryString;
                }
            }

            return baseUrl;
        }
    }
}
