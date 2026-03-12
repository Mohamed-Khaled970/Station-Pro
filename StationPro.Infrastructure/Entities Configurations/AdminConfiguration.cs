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
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(a => a.PasswordHash)
                   .IsRequired();

            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(a => a.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Admins_Email");
        }
    }
}
