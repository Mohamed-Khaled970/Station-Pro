using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface IDeviceRepository : IRepository<Device>
    {
        /// <summary>Returns all active (non-soft-deleted) devices for a tenant.</summary>
        Task<IEnumerable<Device>> GetAllActiveAsync();

        /// <summary>Returns only available (not InUse) devices for a tenant.</summary>
        Task<IEnumerable<Device>> GetAvailableAsync();

        /// <summary>Returns a device with its currently active sessions pre-loaded.</summary>
        Task<Device?> GetWithActiveSessionAsync(int deviceId);
        Task<IEnumerable<Device>> GetAllWithActiveSessionsAsync();
    }
}
