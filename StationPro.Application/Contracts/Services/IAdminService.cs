using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{
    public interface IAdminService
    {
        Task<Admin?> TryToGetAdmin(string Email);
    }
}
