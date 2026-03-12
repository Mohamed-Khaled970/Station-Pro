using StationPro.Application.DTOs.Admin;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface IAdminTenantRepository
    {
        Task<List<TenantAdminDto>> GetAllTenantsAsync();
        Task<TenantAdminDto?> GetTenantAdminDtoAsync(int tenantId);
        Task<bool> ToggleTenantStatusAsync(int tenantId);
        Task<bool> UpdateTenantPlanAsync(int tenantId, SubscriptionPlan plan);
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
    }
}
