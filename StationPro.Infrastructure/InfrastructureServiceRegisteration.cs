using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Application.Settings;
using StationPro.Infrastructure.Data;
using StationPro.Infrastructure.Repositories;
using StationPro.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure
{
    public static class InfrastructureServiceRegisteration
    {
        public static IServiceCollection AddInfrastructureService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                       "Connection string 'DefaultConnection' not found.");

            // ── EF Core ───────────────────────────────────────────────────────
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.Configure<EmailSettings>( configuration.GetSection("EmailSettings"));

            // ── Required by TenantService ─────────────────────────────────────
            // THIS WAS MISSING — causes the entire startup crash
            services.AddHttpContextAccessor();

            // ── Repositories ──────────────────────────────────────────────────
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<ISubscriptionRequestRepository, SubscriptionRequestRepository>();
            services.AddScoped<IAdminTenantRepository, AdminTenantRepository>();

            // ── Services ──────────────────────────────────────────────────────
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISubscriptionRequestService, SubscriptionRequestService>();
            services.AddScoped<IAdminAuthenticationRepository, AdminAuthenticationRepository>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IRoomService , RoomService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IEmailService, SmtpEmailService>();






            return services;
        }
    }
}
