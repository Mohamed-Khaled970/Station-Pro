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
        private readonly ITenantService? _tenantService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            if (_tenantService != null)
            {
                modelBuilder.Entity<Device>()
                    .HasQueryFilter(d => d.TenantId == _tenantService.GetCurrentTenantId());

                modelBuilder.Entity<Room>()
                    .HasQueryFilter(r => r.TenantId == _tenantService.GetCurrentTenantId());

                modelBuilder.Entity<Session>()
                    .HasQueryFilter(s => s.TenantId == _tenantService.GetCurrentTenantId());
            }
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically set TenantId
            if (_tenantService != null)
            {
                var currentTenantId = _tenantService.GetCurrentTenantId();

                foreach (var entry in ChangeTracker.Entries<ITenantEntity>()
                    .Where(e => e.State == EntityState.Added))
                {
                    entry.Entity.TenantId = currentTenantId;
                }
            }

            // Set timestamps
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}

