using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface ITenantRepository : IRepository<Tenant>
    {
        Task<Tenant?> GetBySubdomainAsync(string subdomain);
        Task<Tenant?> GetByEmailAsync(string email);
        Task<bool> SubdomainExistsAsync(string subdomain);
        Task<bool> EmailExistsAsync(string email);
    }
}
