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
    public class AdminAuthenticationRepository : IAdminAuthenticationRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminAuthenticationRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Admin?> GetAdmin(string Email)
        {
            return await _db.Admins
            .FirstOrDefaultAsync(a => a.Email == Email);
        }
    }
}
