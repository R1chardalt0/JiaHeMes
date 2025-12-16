using Microsoft.EntityFrameworkCore;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.DbContexts.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext _context;
        public Repository(AppDbContext context)
        {
            _context = context;
        }

        public List<TEntity> GetList()
        {
            var dbSet = _context.Set<TEntity>();
            return dbSet.ToList();
        }

        public List<TEntity> GetList(Func<TEntity, bool> predicate)
        {
            var dbSet = _context.Set<TEntity>();
            return dbSet.Where(predicate).ToList();
        }

        public async Task<List<TEntity>> GetListAsync(string sort, int pageIndex, int pageSize)
        {
            int skip = (pageIndex - 1) * pageSize;
            var dbSet = _context.Set<TEntity>();
            return await dbSet.OrderBy(m => m.GetType().GetProperty(sort).GetValue(m)).Skip(skip).Take(pageSize).ToListAsync();
        }

        public IQueryable<TEntity> GetQueryable()
        {
            var dbSet = _context.Set<TEntity>();
            return dbSet;
        }

        public async Task<List<TEntity>> GetListAsync(PageWithSortDto pageWithSortDto)
        {
            var dbSet = _context.Set<TEntity>();

            // 自动找排序字段
            var sortField = pageWithSortDto.Sort;
            if (string.IsNullOrWhiteSpace(sortField))
            {
                var entityType = _context.Model.FindEntityType(typeof(TEntity));
                var pk = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();
                sortField = pk?.Name
                    ?? throw new ArgumentException($"未找到 {typeof(TEntity).Name} 的主键字段");
            }

            // 检查字段是否存在
            var propInfo = typeof(TEntity).GetProperty(sortField);
            if (propInfo == null)
            {
                throw new ArgumentException(
                    $"实体 {typeof(TEntity).Name} 不存在名为 '{sortField}' 的属性");
            }

            Console.WriteLine($"Sorting by property: {sortField}");

            // 定义表达式
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var property = Expression.Property(parameter, sortField);
            var conversion = Expression.Convert(property, typeof(object));
            var keySelector = Expression.Lambda<Func<TEntity, object>>(conversion, parameter);

            // 排序
            IOrderedQueryable<TEntity> orderedQuery =
                pageWithSortDto.OrderType == OrderType.Asc
                    ? dbSet.OrderBy(keySelector)
                    : dbSet.OrderByDescending(keySelector);

            // 分页（PageSize <= 0 表示不分页）
            if (pageWithSortDto.PageSize > 0)
            {
                int skip = (pageWithSortDto.PageIndex - 1) * pageWithSortDto.PageSize;
                orderedQuery = (IOrderedQueryable<TEntity>)orderedQuery.Skip(skip).Take(pageWithSortDto.PageSize);
            }

            return await orderedQuery.ToListAsync();
        }



        public IQueryable<TEntity> GetQueryableAsync()
        {
            var dbSet = _context.Set<TEntity>();
            return dbSet;
        }

        public IQueryable<TEntity> GetQueryableAsync(PageWithSortDto pageWithSortDto)
        {
            int skip = (pageWithSortDto.PageIndex - 1) * pageWithSortDto.PageSize;
            var dbSet = _context.Set<TEntity>();
            // Log the property name for debugging
            Console.WriteLine($"Sorting by property: {pageWithSortDto.Sort}");
            // Define a lambda expression for the key selector
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var property = Expression.Property(parameter, pageWithSortDto.Sort);
            var conversion = Expression.Convert(property, typeof(object));
            var keySelector = Expression.Lambda<Func<TEntity, object>>(conversion, parameter);
            // Use the key selector in OrderBy or OrderByDescending based on OrderType
            IOrderedQueryable<TEntity> orderedQuery;
            if (pageWithSortDto.OrderType == OrderType.Asc)
                orderedQuery = dbSet.OrderBy(keySelector);
            else
                orderedQuery = dbSet.OrderByDescending(keySelector);
            // Execute the query and return the result
            return orderedQuery.Skip(skip).Take(pageWithSortDto.PageSize);
        }

        public async Task<List<TEntity>> GetListAsync()
        {
            return await GetListAsync(new PageWithSortDto
            {
                Sort = null,
                PageIndex = 1,
                PageSize = int.MaxValue
            });
        }

        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, string sort, int pageIndex, int pageSize)
        {
            int skip = (pageIndex - 1) * pageSize;
            var dbSet = _context.Set<TEntity>();
            return await dbSet.Where(predicate).OrderBy(m => sort).Skip(skip).Take(pageSize).ToListAsync();
        }
        public async Task<List<TEntity>> GetListAsync(Func<TEntity, bool> predicate)
        {
            var dbSet = _context.Set<TEntity>();
            return await dbSet.WhereAsync(predicate).ToListAsync();
        }
        public TEntity Get(Func<TEntity, bool> predicate)
        {
            var dbSet = _context.Set<TEntity>();
            return dbSet.FirstOrDefault(predicate);
        }
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var dbSet = _context.Set<TEntity>();
            return await dbSet.FirstOrDefaultAsync(predicate);
        }
        public TEntity Insert(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            var res = dbSet.Add(entity).Entity;
            _context.SaveChanges();
            return res;
        }
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            var res = (await dbSet.AddAsync(entity)).Entity;
            await _context.SaveChangesAsync();
            return res;
        }
        public TEntity Delete(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            var res = dbSet.Remove(entity).Entity;
            _context.SaveChanges();
            return res;
        }
        public async Task<TEntity> DeleteAsync(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            var res = dbSet.Remove(entity).Entity;
            await _context.SaveChangesAsync();
            return res;
        }
        public TEntity Update(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            var res = dbSet.Update(entity).Entity;
            _context.SaveChanges();
            return res;
        }
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var dbSet = _context.Set<TEntity>();
            var res = dbSet.Update(entity).Entity;
            await _context.SaveChangesAsync();
            return res;
        }
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var dbSet = _context.Set<TEntity>();
            return await dbSet.CountAsync(predicate);
        }
        public async Task<int> CountAsync()
        {
            var dbSet = _context.Set<TEntity>();
            return await dbSet.CountAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var dbSet = _context.Set<TEntity>();
            return await dbSet.AnyAsync(predicate);
        }
        public async Task<bool> AnyAsync()
        {
            var dbSet = _context.Set<TEntity>();
            return await dbSet.AnyAsync();
        }

        public async Task<int> InsertAsyncs(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsyncs(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsyncs(Func<TEntity, bool> predicate)
        {
            var entities = _context.Set<TEntity>().Where(predicate).ToList();
            _context.Set<TEntity>().RemoveRange(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<IDisposable> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return transaction;
        }
    }
}
