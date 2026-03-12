using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(int id);
        Task<Tenant?> GetByEmailAsync(string email);
        Task<Tenant?> GetByResetTokenAsync(string token);
        Task<bool> EmailExistsAsync(string email);
        Task<Tenant> AddAsync(Tenant tenant);
        Task UpdateAsync(Tenant tenant);
    }
}
