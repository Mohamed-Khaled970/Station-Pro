using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Common;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        // ITenantService is optional — null at design time (migrations),
        // injected by DI at runtime.
        private readonly ITenantService? _tenantService;

        // Single constructor — DI passes both; EF CLI tools only pass options.
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantService? tenantService = null)   // <-- optional parameter solves ambiguity
            : base(options)
        {
            _tenantService = tenantService;
        }

        // ── Tables ────────────────────────────────────────────────────────────
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SubscriptionRequest> SubscriptionRequests { get; set; }
        public DbSet<SubscriptionPlanLimits> SubscriptionPlanLimits { get; set; }
        public DbSet<Admin> Admins => Set<Admin>();

        // ── Model configuration ───────────────────────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Global query filters — only registered when TenantService is available (runtime).
            // At migration/design time _tenantService is null so no filter is applied.
            // The lambda is evaluated LAZILY per query, not at startup.
            if (_tenantService != null)
            {
                modelBuilder.Entity<Device>()
                    .HasQueryFilter(d => d.TenantId == (_tenantService.TryGetCurrentTenantId() ?? 0));

                modelBuilder.Entity<Room>()
                    .HasQueryFilter(r => r.TenantId == (_tenantService.TryGetCurrentTenantId() ?? 0));

                modelBuilder.Entity<Session>()
                    .HasQueryFilter(s => s.TenantId == (_tenantService.TryGetCurrentTenantId() ?? 0));
            }
        }

        // ── SaveChanges ───────────────────────────────────────────────────────
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-assign TenantId on new tenant-scoped entities.
            // TryGetCurrentTenantId() is null-safe — won't throw on public pages.
            if (_tenantService != null)
            {
                var currentTenantId = _tenantService.TryGetCurrentTenantId();

                if (currentTenantId.HasValue)
                {
                    foreach (var entry in ChangeTracker.Entries<ITenantEntity>()
                        .Where(e => e.State == EntityState.Added))
                    {
                        entry.Entity.TenantId = currentTenantId.Value;
                    }
                }
            }

            // Auto-set UpdatedAt on modified entities
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}

