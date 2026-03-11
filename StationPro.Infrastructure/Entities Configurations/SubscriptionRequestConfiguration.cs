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
    public class SubscriptionRequestConfiguration : IEntityTypeConfiguration<SubscriptionRequest>
    {
        public void Configure(EntityTypeBuilder<SubscriptionRequest> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.PaymentMethod)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(s => s.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(s => s.TransactionReference)
                   .HasMaxLength(100);

            builder.Property(s => s.PaymentProofPath)
                   .HasMaxLength(500);

            builder.Property(s => s.Notes)
                   .HasMaxLength(1000);

            builder.Property(s => s.AdminNotes)
                   .HasMaxLength(1000);

            builder.Property(s => s.ReviewedByUserId)
                   .HasMaxLength(200);

            builder.Property(s => s.Amount)
                   .HasPrecision(18, 2);

            builder.Property(s => s.Status)
                   .HasConversion<int>();

            builder.Property(s => s.SubscriptionPlan)
                   .HasConversion<int>();

            // Indexes for admin dashboard queries
            builder.HasIndex(s => s.TenantId)
                   .HasDatabaseName("IX_SubscriptionRequests_TenantId");

            builder.HasIndex(s => s.Status)
                   .HasDatabaseName("IX_SubscriptionRequests_Status");

            // Tenant relationship is defined in TenantConfiguration
        }
    }
}
