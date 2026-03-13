// =============================================================================
// FILE: StationPro/Filters/SubscriptionGuardFilter.cs
// =============================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Entities;

namespace StationPro.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SubscriptionRequiredAttribute : Attribute { }

    public class SubscriptionGuardFilter : IAsyncActionFilter
    {
        private readonly ISubscriptionRequestService _subscriptionService;
        private readonly IAuthService _authService; // ← add this

        public SubscriptionGuardFilter(
            ISubscriptionRequestService subscriptionService,
            IAuthService authService) // ← inject
        {
            _subscriptionService = subscriptionService;
            _authService = authService;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
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

            // ── Check if tenant is still active (admin may have deactivated) ──
            var isActive = await _authService.IsTenantActiveAsync(tenantId.Value);
            if (!isActive)
            {
                // Clear session so they have to log in again after reactivation
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult(
                    "Deactivated", "Auth", null);
                return;
            }

            // ── Check subscription request status ─────────────────────────────
            var latest = await _subscriptionService.GetLatestRequest(tenantId.Value);
            if (latest == null)
            {
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