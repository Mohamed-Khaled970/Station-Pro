using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StationPro.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();


            var connectionString = "Server=PC-1;Database=StationPro;Trusted_Connection=True;Encrypt=False";
            // Use the same connection string as your appsettings.json.
            // This is only used during migrations — not in production.
            optionsBuilder.UseSqlServer(connectionString);

            // Pass null for ITenantService — perfectly fine at design time.
            // No HTTP context exists during migrations anyway.
            return new ApplicationDbContext(optionsBuilder.Options, tenantService: null);
        }
    }
}
