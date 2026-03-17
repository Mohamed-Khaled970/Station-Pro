namespace StationPro.Middlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantClaim = context.User?.FindFirst("TenantId");

            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var tenantId))
            {
                context.Items["TenantId"] = tenantId;
                _logger.LogDebug("Tenant resolved from cookie: {TenantId}", tenantId);
            }

            await _next(context);
        }
    }
}
