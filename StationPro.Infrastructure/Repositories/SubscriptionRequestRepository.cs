using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Repositories;
using StationPro.Domain.Entities;
using StationPro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Repositories
{
    public class SubscriptionRequestRepository : ISubscriptionRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public SubscriptionRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<SubscriptionRequest?> GetLatestRequest(int TenantId)
        {
            var latestRequest = await _db.SubscriptionRequests
                                          .AsNoTracking()
                                          .Where(s => s.TenantId == TenantId)
                                          .OrderByDescending(s => s.SubmittedDate)
                                          .FirstOrDefaultAsync();

            return latestRequest;
        }

        public async Task<SubscriptionRequest?> GetByIdAsync(int id)
          => await _db.SubscriptionRequests.FindAsync(id);

        // All requests — used by admin dashboard (no tenant filter needed here)
        public async Task<List<SubscriptionRequest>> GetAllAsync()
            => await _db.SubscriptionRequests
                        .Include(s => s.Tenant)
                        .OrderByDescending(s => s.SubmittedDate)
                        .ToListAsync();

        public async Task<SubscriptionRequest> AddAsync(SubscriptionRequest request)
        {
            _db.SubscriptionRequests.Add(request);
            await _db.SaveChangesAsync();
            return request;
        }

        public async Task UpdateAsync(SubscriptionRequest request)
        {
            _db.SubscriptionRequests.Update(request);
            await _db.SaveChangesAsync();
        }
    }
}
