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
        /// <summary>
        /// Returns the TenantId of the currently logged-in tenant.
        /// Throws InvalidOperationException if no tenant is resolved.
        /// </summary>
        int GetCurrentTenantId();

        /// <summary>
        /// Returns null instead of throwing — safe to call in filters/middleware
        /// where a tenant may not be set yet (e.g. login/register pages).
        /// </summary>
        int? TryGetCurrentTenantId();
    }
}
