using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface IAdminAuthenticationRepository
    {
        Task<Admin?> GetAdmin(string Email);  
    }
}
