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

// ── Critical order: Session before TenantResolutionMiddleware ─────────────────
app.UseSession();                                    // 1. load session cookie
app.UseMiddleware<TenantResolutionMiddleware>();      // 2. session → Items["TenantId"]
app.UseAuthorization();                              // 3. auth

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();