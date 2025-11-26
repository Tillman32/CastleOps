using CastleOps.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace CastleOps.Api.Infrastructure.Database.Repository
{
    public interface IGenericRepository<TModel>
    where TModel : class, IModel
    {
        Task<IEnumerable<TModel>> GetAllAsync(params Expression<Func<TModel, object>>[] includeProperties);

        Task<IEnumerable<TModel>> GetPaginatedAsync(int page, int size);

        Task<TModel> GetByIdAsync(Guid id, params Expression<Func<TModel, object>>[] includeProperties);

        Task<TModel> CreateAsync(TModel entity);

        Task UpdateAsync(Guid id, TModel model);

        Task DeleteAsync(Guid id);
    }
}