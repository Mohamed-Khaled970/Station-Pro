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
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.HourlyRate).HasPrecision(18, 2);
            builder.HasIndex(d => d.TenantId);
            builder.HasOne(d => d.Tenant)
                               .WithMany(t => t.Devices)
                               .HasForeignKey(d => d.TenantId)
                               .OnDelete(DeleteBehavior.Restrict); // مهم

            builder.HasOne(d => d.Room)
                   .WithMany(r => r.Devices)
                   .HasForeignKey(d => d.RoomId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(d => d.Sessions)
                   .WithOne(s => s.Device)
                   .HasForeignKey(s => s.DeviceId)
                   .OnDelete(DeleteBehavior.Restrict); // مهم

        }
    }
}
