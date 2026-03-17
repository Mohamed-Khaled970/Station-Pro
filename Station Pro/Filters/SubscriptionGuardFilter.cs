using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Entities;
using System.Security.Claims;

namespace StationPro.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SubscriptionRequiredAttribute : Attribute { }

    public class SubscriptionGuardFilter : IAsyncActionFilter
    {
        private readonly ISubscriptionRequestService _subscriptionService;
        private readonly IAuthService _authService;

        public SubscriptionGuardFilter(
            ISubscriptionRequestService subscriptionService,
            IAuthService authService)
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

            // ── Resolve TenantId from cookie claims ───────────────────────────
            var tenantClaim = context.HttpContext.User?.FindFirst("TenantId");

            if (tenantClaim == null || !int.TryParse(tenantClaim.Value, out var tenantId) || tenantId == 0)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // ── Check if tenant is still active (admin may have deactivated) ──
            var isActive = await _authService.IsTenantActiveAsync(tenantId);
            if (!isActive)
            {
                // Sign out completely — deletes the cookie
                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                context.Result = new RedirectToActionResult("Deactivated", "Auth", null);
                return;
            }

            // ── Check subscription request status ─────────────────────────────
            var latest = await _subscriptionService.GetLatestRequest(tenantId);
            if (latest == null)
            {
                context.Result = new RedirectToActionResult(
                    "Subscribe", "Subscription", new { tenantId });
                return;
            }

            switch (latest.Status)
            {
                case SubscriptionRequestStatus.Pending:
                    context.Result = new RedirectToActionResult(
                        "Pending", "Subscription", new { tenantId });
                    break;

                case SubscriptionRequestStatus.Rejected:
                    context.Result = new RedirectToActionResult(
                        "Rejected", "Subscription", new { tenantId });
                    break;

                case SubscriptionRequestStatus.Approved:
                    await next();
                    break;

                default:
                    context.Result = new RedirectToActionResult(
                        "Subscribe", "Subscription", new { tenantId });
                    break;
            }
        }
    }
}