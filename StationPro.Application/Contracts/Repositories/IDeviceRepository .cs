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
        Task<IEnumerable<Device>> GetAvailableDevicesAsync();
        Task<IEnumerable<Device>> GetDevicesByTypeAsync(DeviceType type);
        Task<IEnumerable<Device>> GetDevicesByRoomAsync(int roomId);
        Task<Device?> GetDeviceWithSessionsAsync(int id);
        Task<int> CountAsync();
    }
}
