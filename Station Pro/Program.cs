// =============================================================================
// FILE: StationPro/Program.cs
// =============================================================================

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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

// ── Data Protection — persist keys to disk so app pool restarts don't ─────────
// invalidate existing auth cookies. Keys are auto-generated XML files.
// ContentRootPath resolves automatically on any server — no hardcoded path.
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(keysPath); // creates the folder if it doesn't exist yet

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("StationPro");

// ── Cookie Authentication ─────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;          // resets 8h timer on activity
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "StationPro.Auth";
    });

builder.Services.AddAuthorization();

// ── In-memory session service (your custom one, NOT ASP.NET sessions) ─────────
builder.Services.AddSingleton<ISessionService, SessionService>();

// ── Infrastructure (EF Core + repositories + services + HttpContextAccessor) ──
builder.Services.AddInfrastructureService(builder.Configuration);

// ── Filters ───────────────────────────────────────────────────────────────────
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

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<SubscriptionGuardFilter>();
})
.AddViewLocalization()
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(SharedResources));
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
// Order matters. Never swap these around.

// 1. Decrypt the auth cookie → populates HttpContext.User with claims
app.UseAuthentication();

// 2. Resolve TenantId from claims → populates HttpContext.Items["TenantId"]
app.UseMiddleware<TenantResolutionMiddleware>();

// 3. Block protected routes that have no resolved tenant
app.UseMiddleware<TenantGuardMiddleware>();

// 4. Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();