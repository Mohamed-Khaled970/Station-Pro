using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationPro.Domain.Entities;

namespace StationPro.Infrastructure.Entities_Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(d => d.Type)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(d => d.Status)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(DeviceStatus.Available);

            builder.Property(d => d.SingleSessionRate)
                   .IsRequired()
                   .HasPrecision(18, 2);

            // MultiSessionRate is nullable — only set when SupportsMultiSession = true
            builder.Property(d => d.MultiSessionRate)
                   .HasPrecision(18, 2);

            builder.Property(d => d.SupportsMultiSession)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(d => d.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(d => d.TenantId)
                   .HasDatabaseName("IX_Devices_TenantId");

            builder.HasIndex(d => new { d.TenantId, d.Status })
                   .HasDatabaseName("IX_Devices_TenantId_Status");  // for dashboard queries

            // ── Room relationship (optional) ──────────────────────────────────
            // Defined here because Room is the optional side.
            // If a room is deleted, device becomes unassigned (RoomId = null).
            builder.HasOne(d => d.Room)
                   .WithMany(r => r.Devices)
                   .HasForeignKey(d => d.RoomId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}