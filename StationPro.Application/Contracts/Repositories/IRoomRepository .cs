using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<IEnumerable<Room>> GetAllActiveAsync();
        Task<IEnumerable<Room>> GetAvailableAsync();
        Task<Room?> GetWithActiveSessionAsync(int roomId);
        Task<IEnumerable<Room>> GetAllWithActiveSessionsAsync();
    }
}
