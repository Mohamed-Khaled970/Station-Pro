namespace StationPro.Middlewares
{
    /// <summary>
    /// Runs after TenantResolutionMiddleware.
    /// Short-circuits any request to a tenant-protected route when no TenantId
    /// could be resolved — preventing the global query filter from falling back
    /// to TenantId == 0 and leaking or returning empty data.
    ///
    /// Public routes are explicitly allow-listed below.
    /// Everything else requires a resolved tenant.
    /// </summary>
    public class TenantGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantGuardMiddleware> _logger;

        /// <summary>
        /// Path prefixes that are always public — no tenant needed.
        /// Add any new public controllers here.
        /// </summary>
        private static readonly HashSet<string> _publicPrefixes = new(
            StringComparer.OrdinalIgnoreCase)
        {
            "/auth",          // AuthController — login, register, logout, deactivated
            "/admin",         // super-admin panel (has its own auth)
            "/home",          // HomeController — landing page
            "/subscription",  // SubscriptionController — subscribe, pending, rejected
            "/language",      // LanguageController — culture switcher
            "/error",         // error pages
            "/favicon.ico",
             "/hangfire"
        };

        /// <summary>
        /// The exact path segments that are public even without a prefix match.
        /// "/" alone is not caught by StartsWithSegments("/home") so we list it separately.
        /// </summary>
        private static readonly HashSet<string> _publicExactPaths = new(
            StringComparer.OrdinalIgnoreCase)
        {
            "/",
            "/index",
        };

        public TenantGuardMiddleware(
            RequestDelegate next,
            ILogger<TenantGuardMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;

            // Static files are handled before this middleware — skip them.
            // Also skip all allow-listed public routes.
            if (IsPublicRoute(path))
            {
                await _next(context);
                return;
            }

            // Check whether TenantResolutionMiddleware already resolved a tenant.
            var tenantResolved = context.Items.ContainsKey("TenantId")
                                 && context.Items["TenantId"] is int tenantId
                                 && tenantId > 0;

            if (!tenantResolved)
            {
                _logger.LogWarning(
                    "TenantGuard blocked {Path} — no TenantId resolved. " +
                    "User may not be logged in or the session may have expired.",
                    path);

                // AJAX / HTMX: return 401 so the client JS can redirect.
                if (IsAjaxRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Session expired. Please log in again.",
                        redirectUrl = "/Auth/Login"
                    });
                    return;
                }

                // Regular browser request: redirect to login.
                // ⚠️  Change "/Account/Login" to match your actual login action path
                //     if your AccountController uses a different route.
                var returnUrl = Uri.EscapeDataString(path + context.Request.QueryString);
                context.Response.Redirect($"/Auth/Login?returnUrl={returnUrl}");
                return;
            }

            await _next(context);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool IsPublicRoute(PathString path)
        {
            // Exact match first (handles "/")
            if (_publicExactPaths.Contains(path.Value ?? string.Empty))
                return true;

            // Prefix match (handles "/account/login", "/admin/dashboard", …)
            foreach (var prefix in _publicPrefixes)
            {
                if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool IsAjaxRequest(HttpRequest request)
            => request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || request.Headers["HX-Request"] == "true"   // HTMX
            || (request.ContentType?.Contains("application/json") ?? false);
    }
}