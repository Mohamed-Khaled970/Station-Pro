using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminAuthenticationRepository _adminRepo;
        public AdminService(IAdminAuthenticationRepository adminRepo)
        {
            _adminRepo = adminRepo;
        }
        public Task<Admin?> TryToGetAdmin(string Email)
        {
            return _adminRepo.GetAdmin(Email);
        }
    }
}
