using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Repositories
{
    public interface ISubscriptionRequestRepository
    {
        Task<SubscriptionRequest?> GetLatestRequest(int TenantId);
        Task<SubscriptionRequest?> GetByIdAsync(int id);
        Task<List<SubscriptionRequest>> GetAllAsync();
        Task<SubscriptionRequest> AddAsync(SubscriptionRequest request);
        Task UpdateAsync(SubscriptionRequest request);
    }
}
