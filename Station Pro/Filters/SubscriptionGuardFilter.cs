using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using StationPro.Controllers;

namespace StationPro.Filters
{
    // ── Marker attribute ──────────────────────────────────────────────────────

    /// <summary>
    /// Apply to controllers (or individual actions) that require an Approved
    /// subscription before the user can access them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SubscriptionRequiredAttribute : Attribute { }

    // ── The filter itself ─────────────────────────────────────────────────────

    /// <summary>
    /// Registered globally in Program.cs → reads every request tagged with
    /// [SubscriptionRequired] and redirects non-approved tenants.
    /// </summary>
    public class SubscriptionGuardFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var hasAttribute = context.ActionDescriptor.EndpointMetadata
                                      .OfType<SubscriptionRequiredAttribute>()
                                      .Any();
            if (!hasAttribute) return;

            // ── Resolve TenantId from session ─────────────────────────────────────
            var tenantId = context.HttpContext.Session.GetInt32("TenantId");

            // Not logged in → go to login page
            if (!tenantId.HasValue || tenantId.Value == 0)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // ── Check subscription status ─────────────────────────────────────────
            var subscription = AdminController._subscriptionRequests
                .Where(s => s.TenantId == tenantId.Value)
                .OrderByDescending(s => s.SubmittedDate)
                .FirstOrDefault();

            if (subscription == null)
            {
                context.Result = new RedirectToActionResult(
                    "Subscribe", "Subscription", new { tenantId = tenantId.Value });
                return;
            }

            switch (subscription.Status)
            {
                case "Pending":
                    context.Result = new RedirectToActionResult(
                        "Pending", "Subscription", new { tenantId = tenantId.Value });
                    break;
                case "Rejected":
                    context.Result = new RedirectToActionResult(
                        "Rejected", "Subscription", new { tenantId = tenantId.Value });
                    break;
                case "Approved":
                    // ✅ Allow through
                    break;
                default:
                    context.Result = new RedirectToActionResult(
                        "Subscribe", "Subscription", new { tenantId = tenantId.Value });
                    break;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { /* no-op */ }
    }
}
