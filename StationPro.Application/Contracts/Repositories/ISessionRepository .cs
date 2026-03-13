using StationPro.Application.DTOs;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<IEnumerable<Session>> GetAllAsync(
                   DateTime? from = null, DateTime? to = null);

        /// <summary>Only Active sessions for a tenant.</summary>
        Task<IEnumerable<Session>> GetActiveAsync();

        /// <summary>Active sessions for a specific device.</summary>
        Task<IEnumerable<Session>> GetActiveForDeviceAsync(int deviceId);

        /// <summary>Active sessions for a specific room.</summary>
        Task<IEnumerable<Session>> GetActiveForRoomAsync(int roomId);

        /// <summary>Paged + filtered result set.</summary>
        Task<(IEnumerable<Session> Items, int Total)> GetPagedAsync(
                   SessionFilterRequest filter);

        /// <summary>Revenue sum for a tenant on a given calendar day.</summary>
        Task<decimal> GetDailyRevenueAsync(DateTime date);
    }
}
