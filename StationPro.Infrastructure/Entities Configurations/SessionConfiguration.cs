using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Entities_Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {

            builder.HasKey(s => s.Id);
            builder.Property(s => s.HourlyRate).HasPrecision(18, 2);
            builder.Property(s => s.TotalCost).HasPrecision(18, 2);
            builder.HasIndex(s => s.TenantId);

            builder.HasOne(s => s.Device)
                   .WithMany(d => d.Sessions)
                   .HasForeignKey(s => s.DeviceId)
                   .OnDelete(DeleteBehavior.Restrict); // مهم

            builder.HasOne(s => s.Tenant)
                   .WithMany(t => t.Sessions)
                   .HasForeignKey(s => s.TenantId)
                   .OnDelete(DeleteBehavior.Cascade); // صح
        }
    }
}
