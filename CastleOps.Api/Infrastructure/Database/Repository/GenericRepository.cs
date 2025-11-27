using CastleOps.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CastleOps.Api.Infrastructure.Database.Repository
{
    public class GenericRepository<TModel> : IGenericRepository<TModel>, IDisposable
        where TModel : class, IModel
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<GenericRepository<TModel>> _logger;

        public GenericRepository(ILogger<GenericRepository<TModel>> logger,
            DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TModel>> GetAllAsync(params Expression<Func<TModel, object>>[] includeProperties)
        {
            _logger.LogInformation("Fetching all entities of type {EntityType}", typeof(TModel).Name);
            
            IQueryable<TModel> query = _dbContext.Set<TModel>().AsNoTracking();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<TModel>> GetPaginatedAsync(int page, int size)
        {
            _logger.LogInformation("Fetching paginated entities of type {EntityType}", typeof(TModel).Name);
            return await _dbContext.Set<TModel>()
                .Skip((page - 1) * size)
                .Take(size)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TModel> GetByIdAsync(Guid id, params Expression<Func<TModel, object>>[] includeProperties)
        {
            _logger.LogInformation("Fetching entity of type {EntityType} with ID {EntityId}", typeof(TModel).Name, id);
            
            IQueryable<TModel> query = _dbContext.Set<TModel>().AsNoTracking();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return (await query.FirstOrDefaultAsync(e => e.Id == id))!;
        }

        public async Task<IEnumerable<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate)
        {
            _logger.LogInformation("Finding entities of type {EntityType} matching predicate", typeof(TModel).Name);
            
            return await _dbContext.Set<TModel>()
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TModel> CreateAsync(TModel entity)
        {
            _logger.LogInformation("Creating a new entity of type {EntityType}", typeof(TModel).Name);
            try
            {
                await _dbContext.Set<TModel>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions, such as unique constraint violations
                _logger.LogError(dbEx, "Database update error occurred while creating entity of type {EntityType}", typeof(TModel).Name);
                throw new Exception("Duplicate entry or database update error.");
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging framework)
                _logger.LogError(ex, "Error occurred while creating entity of type {EntityType}", typeof(TModel).Name);
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task UpdateAsync(Guid id, TModel model)
        {
            _logger.LogInformation("Updating entity of type {EntityType} with ID {EntityId}", typeof(TModel).Name, id);
            _dbContext.Set<TModel>().Update(model);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting entity of type {EntityType} with ID {EntityId}", typeof(TModel).Name, id);
            var entity = await GetByIdAsync(id);
            _dbContext.Set<TModel>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }
    }
}