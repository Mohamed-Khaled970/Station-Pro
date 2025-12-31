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
            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(r => r.TenantId);

            builder.HasOne(r => r.Tenant)
                 .WithMany(t => t.Rooms)
                 .HasForeignKey(r => r.TenantId)
                 .OnDelete(DeleteBehavior.Restrict); // مهم

            builder.HasMany(r => r.Devices)
                   .WithOne(d => d.Room)
                   .HasForeignKey(d => d.RoomId)
                   .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
