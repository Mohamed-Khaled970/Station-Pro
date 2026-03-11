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

            // ── Rates & cost ──────────────────────────────────────────────────
            builder.Property(s => s.HourlyRate)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasComment("Rate snapshotted at session start — device/room rate may change later");

            builder.Property(s => s.TotalCost)
                   .IsRequired()
                   .HasPrecision(18, 2);

            // ── Time ──────────────────────────────────────────────────────────
            builder.Property(s => s.StartTime)
                   .IsRequired();

            builder.Property(s => s.EndTime)
                   .IsRequired(false);   // null while session is still active

            // Duration is a computed property — not stored in DB
            builder.Ignore(s => s.Duration);

            // ── Discriminators ────────────────────────────────────────────────
            builder.Property(s => s.SourceType)
                   .IsRequired()
                   .HasMaxLength(10)
                   .HasComment("'Device' or 'Room' — which source this session belongs to");

            builder.Property(s => s.SessionType)
                   .IsRequired()
                   .HasMaxLength(10)
                   .HasComment("'Single' or 'Multi'");

            // ── Customer info ─────────────────────────────────────────────────
            builder.Property(s => s.CustomerName)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(s => s.CustomerPhone)
                   .HasMaxLength(20)
                   .IsRequired(false);

            builder.Property(s => s.GuestCount)
                   .IsRequired()
                   .HasDefaultValue(1);

            // ── Payment ───────────────────────────────────────────────────────
            builder.Property(s => s.PaymentMethod)
                   .HasMaxLength(30)
                   .IsRequired(false)
                   .HasComment("Filled when session is closed: Cash, Card, Wallet, etc.");

            // ── Status ────────────────────────────────────────────────────────
            builder.Property(s => s.Status)
                   .HasConversion<int>()
                   .IsRequired();

            // ── Indexes ───────────────────────────────────────────────────────
            builder.HasIndex(s => s.TenantId)
                   .HasDatabaseName("IX_Sessions_TenantId");

            // Primary reporting index: tenant + date range
            builder.HasIndex(s => new { s.TenantId, s.StartTime })
                   .HasDatabaseName("IX_Sessions_TenantId_StartTime");

            // Useful for "active sessions" dashboard widget
            builder.HasIndex(s => new { s.TenantId, s.Status })
                   .HasDatabaseName("IX_Sessions_TenantId_Status");

            // ── Relationships ─────────────────────────────────────────────────

            // Device: optional — null when SourceType = "Room"
            builder.HasOne(s => s.Device)
                   .WithMany(d => d.Sessions)
                   .HasForeignKey(s => s.DeviceId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);  // can't delete device with sessions

            // Room: optional — null when SourceType = "Device"
            builder.HasOne(s => s.Room)
                   .WithMany(r => r.Sessions)
                   .HasForeignKey(s => s.RoomId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);  // can't delete room with sessions

            // Tenant relationship is owned by TenantConfiguration
        }
    }
}
