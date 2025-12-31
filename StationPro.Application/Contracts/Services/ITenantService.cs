using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{
    public interface ITenantService
    {
        Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
        int GetCurrentTenantId();
        Tenant GetCurrentTenant();
        Task<bool> SubdomainExistsAsync(string subdomain);
    }
}
