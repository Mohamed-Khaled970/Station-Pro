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
            var tenantId = context.Session.GetInt32("TenantId");

            if (tenantId.HasValue)
            {
                context.Items["TenantId"] = tenantId.Value;
                _logger.LogDebug("Tenant resolved: {TenantId}", tenantId.Value);
            }

            await _next(context);
        }
    }
}
