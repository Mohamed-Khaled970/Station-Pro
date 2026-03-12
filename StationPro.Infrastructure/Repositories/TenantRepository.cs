using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Repositories;
using StationPro.Domain.Entities;
using StationPro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _db;

        public TenantRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Tenant?> GetByIdAsync(int id)
            => await _db.Tenants.FindAsync(id);

        public async Task<Tenant?> GetByEmailAsync(string email)
            => await _db.Tenants
                        .FirstOrDefaultAsync(t => t.Email == email);

        public async Task<Tenant?> GetByResetTokenAsync(string token)
            => await _db.Tenants
                        .FirstOrDefaultAsync(t =>
                            t.PasswordResetToken == token &&
                            t.PasswordResetTokenExpiry > DateTime.UtcNow);

        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Tenants.AnyAsync(t => t.Email == email);

        public async Task<Tenant> AddAsync(Tenant tenant)
        {
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();
            return tenant;   // EF fills tenant.Id after SaveChanges
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            _db.Tenants.Update(tenant);
            await _db.SaveChangesAsync();
        }
    }
}
