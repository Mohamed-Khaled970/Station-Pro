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

    public class SubscriptionPlanLimitsConfiguration : IEntityTypeConfiguration<SubscriptionPlanLimits>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlanLimits> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Plan)
                   .HasConversion<int>()
                   .IsRequired();

            builder.HasIndex(p => p.Plan)
                   .IsUnique();   // one row per plan

            builder.Property(p => p.DisplayName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(p => p.Description)
                   .HasMaxLength(300);

            builder.Property(p => p.MonthlyPrice)
                   .HasPrecision(18, 2);

            // ── Seed data ─────────────────────────────────────────────────────
            builder.HasData(
                new SubscriptionPlanLimits
                {
                    Id = 1,
                    Plan = SubscriptionPlan.Basic,
                    DisplayName = "Basic",
                    Description = "Perfect for small gaming stores",
                    MaxDevices = 20,
                    MaxRooms = 5,
                    ReportRetentionDays = 7,    // 1 week
                    AllowsReportExport = false,
                    MonthlyPrice = 29
                },
                new SubscriptionPlanLimits
                {
                    Id = 2,
                    Plan = SubscriptionPlan.Pro,
                    DisplayName = "Pro",
                    Description = "For growing gaming centers",
                    MaxDevices = 50,
                    MaxRooms = 20,
                    ReportRetentionDays = 30,   // 1 month
                    AllowsReportExport = true,
                    MonthlyPrice = 79
                },
                new SubscriptionPlanLimits
                {
                    Id = 3,
                    Plan = SubscriptionPlan.Enterprise,
                    DisplayName = "Enterprise",
                    Description = "Unlimited — for large chains",
                    MaxDevices = -1,            // -1 = unlimited
                    MaxRooms = -1,
                    ReportRetentionDays = -1,
                    AllowsReportExport = true,
                    MonthlyPrice = 199
                }
            );
        }
    }
}
