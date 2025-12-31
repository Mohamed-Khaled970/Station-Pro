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
        Task<IEnumerable<Session>> GetActiveSessionsAsync();
        Task<Session?> GetActiveSessionByDeviceAsync(int deviceId);
        Task<IEnumerable<Session>> GetSessionsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Session>> GetTodaySessionsAsync();
    }
}
