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
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            // Two rates instead of one
            builder.Property(r => r.SingleHourlyRate)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasComment("Rate per hour for a single-player room session");

            builder.Property(r => r.MultiHourlyRate)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasComment("Rate per hour for a multi-player room session");

            builder.Property(r => r.Capacity)
                   .IsRequired()
                   .HasDefaultValue(1)
                   .HasComment("Max number of players the room supports");

            // DeviceCount is a denormalized counter — updated whenever a device
            // is assigned/unassigned from this room. Avoids a COUNT query on hot paths.
            builder.Property(r => r.DeviceCount)
                   .IsRequired()
                   .HasDefaultValue(0)
                   .HasComment("Cached count of devices assigned to this room");

            builder.Property(r => r.HasAC)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(r => r.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(r => r.TenantId)
                   .HasDatabaseName("IX_Rooms_TenantId");
        }
    }
}
