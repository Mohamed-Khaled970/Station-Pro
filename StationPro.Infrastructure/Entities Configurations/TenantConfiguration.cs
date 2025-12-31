using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Entities_Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Subdomain).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Email).IsRequired().HasMaxLength(100);
            builder.Property(t => t.PasswordHash).IsRequired();
            builder.HasIndex(t => t.Subdomain).IsUnique();
            builder.HasIndex(t => t.Email).IsUnique();

            builder.HasMany(t => t.Devices)
                  .WithOne(d => d.Tenant)
                  .HasForeignKey(d => d.TenantId)
                  .OnDelete(DeleteBehavior.Restrict); // مهم

            builder.HasMany(t => t.Rooms)
                   .WithOne(r => r.Tenant)
                   .HasForeignKey(r => r.TenantId)
                   .OnDelete(DeleteBehavior.Restrict); // مهم

            builder.HasMany(t => t.Sessions)
                   .WithOne(s => s.Tenant)
                   .HasForeignKey(s => s.TenantId)
                   .OnDelete(DeleteBehavior.Cascade); // صح

        }
    }
}
