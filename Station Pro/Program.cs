// =============================================================================
// FILE: StationPro/Program.cs
// =============================================================================

using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Station_Pro;
using StationPro;
using StationPro.Application.Interfaces;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Application.Services;
using StationPro.Filters;
using StationPro.Infrastructure;
using StationPro.Middlewares;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ── In-memory session service (your existing one) ─────────────────────────────
builder.Services.AddSingleton<ISessionService, SessionService>();

// ── Infrastructure (EF Core + repositories + services + HttpContextAccessor) ──
builder.Services.AddInfrastructureService(builder.Configuration);

// ── SubscriptionGuardFilter — must be AddScoped so DI injects it properly ─────
// Do NOT use options.Filters.Add<SubscriptionGuardFilter>() for scoped filters.
// Use AddService<> inside AddControllersWithViews instead.
builder.Services.AddScoped<SubscriptionGuardFilter>();

builder.Services.AddScoped<AdminAuthFilter>();

// ── Response compression ──────────────────────────────────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = System.IO.Compression.CompressionLevel.Fastest);

// ── Localization ──────────────────────────────────────────────────────────────
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// ── MVC + global filter registration ─────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // AddService<T> correctly resolves scoped filters from the DI container
    options.Filters.AddService<SubscriptionGuardFilter>();
})
.AddViewLocalization()
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(SharedResources));
});

// ── Session ───────────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // ✅ This makes the cookie persist in the browser after closing
    options.Cookie.MaxAge = TimeSpan.FromHours(8); // match IdleTimeout
});
// ── Supported cultures ────────────────────────────────────────────────────────
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ar-EG")
    };

    options.DefaultRequestCulture = new RequestCulture("ar-EG");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// =============================================================================
var app = builder.Build();
// =============================================================================

SessionStore.Seed();

app.UseResponseCompression();
app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
// ── Middleware pipeline order — CRITICAL ──────────────────────────────────────
// 1. Session cookie must be loaded before anything reads it
app.UseSession();

// 2. Resolve tenant from session → populates HttpContext.Items["TenantId"]
app.UseMiddleware<TenantResolutionMiddleware>();

// 3. Block protected routes that have no resolved tenant
app.UseMiddleware<TenantGuardMiddleware>();

// 4. Authorization (after tenant is known)
app.UseAuthorization();
// 3. auth

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();