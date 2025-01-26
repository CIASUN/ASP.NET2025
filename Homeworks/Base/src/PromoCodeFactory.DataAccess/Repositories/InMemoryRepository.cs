using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Domain.Administration;
namespace PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected IEnumerable<T> Data { get; set; }

        public InMemoryRepository(IEnumerable<T> data)
        {
            Data = data;
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data);
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
        }

        public Task AddAsync(T entity)
        {
            Data = Data.Append(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity)
        {
            Data = Data.Select(x => x.Id == entity.Id ? entity : x);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            Data = Data.Where(x => x.Id != entity.Id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> GetByIdsAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Task.FromResult(Enumerable.Empty<T>());
            }

            var result = Data
                .Where(item => ids.Contains(item.Id))
                .ToList();

            return Task.FromResult(result.AsEnumerable());
        }

    }
}