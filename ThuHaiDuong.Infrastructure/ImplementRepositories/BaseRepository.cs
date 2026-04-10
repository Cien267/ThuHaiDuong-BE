using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Shared.Models;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.ImplementRepositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected IDbContext _IDbContext = null;
        protected DbSet<TEntity> _dbset;
        protected DbContext _dbContext;
        protected DbSet<TEntity> DBSet
        {
            get
            {
                if (_dbset == null)
                {
                    _dbset = _dbContext.Set<TEntity>() as DbSet<TEntity>;
                }
                return _dbset;
            }
        }
        public BaseRepository(IDbContext dbContext)
        {
            _IDbContext = dbContext;
            _dbContext = (DbContext) dbContext;
        }
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query = expression != null ? DBSet.Where(expression) : DBSet;
            return await query.CountAsync();
        }

        public async Task<int> CountAsync(string include, Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query;
            if (!string.IsNullOrEmpty(include))
            {
                query = BuildQueryable(new List<string> { include }, expression);
                return await query.CountAsync();
            }
            return await CountAsync(expression);
        }
        public IQueryable<TEntity> BuildQueryable(List<string> includes, Expression<Func<TEntity, bool>> expression)
        {
            IQueryable<TEntity> query = DBSet.AsQueryable();
            if (expression != null)
            {
                query = query.Where(expression);
            }
            if(includes != null & includes.Count > 0)
            {
                foreach(string include in includes){
                    query = query.Include(include.Trim());
                }
            }
            return query;
        }
        
        public IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = typeof(TEntity).GetProperty(sortBy);
            
            if (property == null)
            {
                return query;
            }

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            string methodName = sortDescending ? "OrderByDescending" : "OrderBy";
            
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(TEntity), property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExpression)
            );

            return query.Provider.CreateQuery<TEntity>(resultExpression);
        }
        
        protected IQueryable<TEntity> ApplyPagination(IQueryable<TEntity> query, int pageNumber, int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
        
        protected async Task<PagedResult<TEntity>> GetPagedAsync(
            Expression<Func<TEntity, bool>> filter = null,
            List<string> includes = null,
            string sortBy = "Id",
            bool sortDescending = false,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = BuildQueryable(includes, filter);
            
            var totalCount = await query.CountAsync();
            
            query = ApplySorting(query, sortBy, sortDescending);
            
            query = ApplyPagination(query, pageNumber, pageSize);
            
            var items = await query.ToListAsync();
            
            return new PagedResult<TEntity>(items,  totalCount, pageNumber, pageSize);
        }
    
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            DBSet.Add(entity);
            await _IDbContext.CommitChangeAsync();
            return entity;
        }

        public async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities)
        {
            DBSet.AddRange(entities);
            await _IDbContext.CommitChangeAsync();
            return entities;
        }

        public async Task DeleteAsync(Guid id)
        {
            var dataEntity = await DBSet.FindAsync(id);
            if(dataEntity != null)
            {
                DBSet.Remove(dataEntity);
                await _IDbContext.CommitChangeAsync();
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
            IQueryable<TEntity> query = expression != null ? DBSet.Where(expression) : DBSet;
            var dataQuery = query;
            if (query != null)
            {
                DBSet.RemoveRange(dataQuery);
                await _IDbContext.CommitChangeAsync();
            }
        }

        public async Task<IQueryable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query = expression != null ? DBSet.Where(expression) : DBSet;
            return query;
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await DBSet.FirstOrDefaultAsync(expression);
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await DBSet.FindAsync(id);
        }

        public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            return await DBSet.FirstOrDefaultAsync(expression);
        }

        public async Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            await _IDbContext.CommitChangeAsync();
            return entities;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            /*_dbContext.Entry(entity).State = EntityState.Modified;*/
            await _IDbContext.CommitChangeAsync();
            return entity;
        }
    }
}
