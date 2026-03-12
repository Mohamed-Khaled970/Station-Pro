using Microsoft.AspNetCore.Http;
using StationPro.Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Throws if no tenant resolved — use inside secured actions only
        public int GetCurrentTenantId()
        {
            var id = TryGetCurrentTenantId();
            if (!id.HasValue)
                throw new InvalidOperationException(
                    "TenantId could not be resolved. Ensure the user is logged in.");
            return id.Value;
        }

        // Null-safe — use in middleware, filters, SaveChangesAsync
        public int? TryGetCurrentTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Primary: Items dict (set by TenantResolutionMiddleware, fast)
            if (context.Items.TryGetValue("TenantId", out var value) && value is int tenantId)
                return tenantId;

            // Fallback: session directly
            return context.Session.GetInt32("TenantId");
        }
    }
}
