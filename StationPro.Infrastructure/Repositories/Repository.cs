using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Repositories;
using StationPro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Repositories
{
    /// <summary>
    /// Generic EF Core repository — implements all CRUD operations.
    /// Specific repositories extend this with domain-specific queries.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<T> _set;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
            => await _set.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _set.ToListAsync();

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _set.Where(predicate).ToListAsync();

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _set.FirstOrDefaultAsync(predicate);

        public virtual async Task<T> AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Entity with id {id} not found.");
            _set.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null
                ? await _set.CountAsync()
                : await _set.CountAsync(predicate);

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _set.AnyAsync(predicate);

        public virtual IQueryable<T> Query()
            => _set.AsQueryable();
    }
}
