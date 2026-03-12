// =============================================================================
// FILE: StationPro/Filters/SubscriptionGuardFilter.cs
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Entities;

namespace StationPro.Filters
{
    // ── Marker attribute ──────────────────────────────────────────────────────
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SubscriptionRequiredAttribute : Attribute { }

    // ── The filter ────────────────────────────────────────────────────────────
    // IAsyncActionFilter is required here because we await a DB call.
    // Registered in Program.cs:
    //   builder.Services.AddScoped<SubscriptionGuardFilter>();
    //   options.Filters.AddService<SubscriptionGuardFilter>();
    public class SubscriptionGuardFilter : IAsyncActionFilter
    {
        private readonly ISubscriptionRequestService _subscriptionService;

        public SubscriptionGuardFilter(ISubscriptionRequestService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Only run on actions/controllers tagged with [SubscriptionRequired]
            var hasAttribute = context.ActionDescriptor.EndpointMetadata
                                      .OfType<SubscriptionRequiredAttribute>()
                                      .Any();

            if (!hasAttribute)
            {
                await next();
                return;
            }

            // ── Resolve TenantId from session ─────────────────────────────────
            var tenantId = context.HttpContext.Session.GetInt32("TenantId");

            if (!tenantId.HasValue || tenantId.Value == 0)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // ── Query latest subscription request from DB ──────────────────────
            var latest = await _subscriptionService.GetLatestRequest(tenantId.Value);

            if (latest == null)
            {
                // No subscription request at all → send to plan picker
                context.Result = new RedirectToActionResult(
                    "Subscribe", "Subscription", new { tenantId = tenantId.Value });
                return;
            }

            switch (latest.Status)
            {
                case SubscriptionRequestStatus.Pending:
                    context.Result = new RedirectToActionResult(
                        "Pending", "Subscription", new { tenantId = tenantId.Value });
                    break;

                case SubscriptionRequestStatus.Rejected:
                    context.Result = new RedirectToActionResult(
                        "Rejected", "Subscription", new { tenantId = tenantId.Value });
                    break;

                case SubscriptionRequestStatus.Approved:
                    // ✅ Subscription is valid — let the request through
                    await next();
                    break;

                default:
                    context.Result = new RedirectToActionResult(
                        "Subscribe", "Subscription", new { tenantId = tenantId.Value });
                    break;
            }
        }
    }
}