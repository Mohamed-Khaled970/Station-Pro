using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StationPro.Domain.Entities;

namespace StationPro.Infrastructure.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            // Primary Key
            builder.HasKey(d => d.Id);

            // Properties
            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Single Session Rate (Required)
            builder.Property(d => d.SingleSessionRate)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Hourly rate for single-player sessions");

            // Multi Session Rate (Optional - Nullable)
            builder.Property(d => d.MultiSessionRate)
                .HasPrecision(18, 2)
                .HasComment("Hourly rate for multi-player sessions");

            // Multi-Session Support Flag
            builder.Property(d => d.SupportsMultiSession)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Indicates if device supports multi-player sessions");

            // Device Type
            builder.Property(d => d.Type)
                .IsRequired()
                .HasComment("Type of device (PS5, Xbox, Pool Table, etc.)");

            // Status
            builder.Property(d => d.Status)
                .IsRequired()
                .HasDefaultValue(DeviceStatus.Available)
                .HasComment("Current status of the device");

            // IsActive
            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(d => d.TenantId)
                .HasDatabaseName("IX_Devices_TenantId");


            // Relationships

            // Tenant Relationship (Required)
            builder.HasOne(d => d.Tenant)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Room Relationship (Optional)
            builder.HasOne(d => d.Room)
                .WithMany(r => r.Devices)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.SetNull); // Set to null if room is deleted

            // Sessions Relationship (One-to-Many)
            builder.HasMany(d => d.Sessions)
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if sessions exist

            // Table Configuration
            builder.ToTable("Devices", schema: "dbo", t =>
            {
                t.HasComment("Gaming and recreational devices in the gaming station");
            });
        }
    }
}