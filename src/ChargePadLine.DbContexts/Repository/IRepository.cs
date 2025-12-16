using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.DbContexts.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // 同步查询方法
        List<TEntity> GetList();
        List<TEntity> GetList(Func<TEntity, bool> predicate);
        TEntity Get(Func<TEntity, bool> predicate);

        // 异步查询方法
        Task<List<TEntity>> GetListAsync();
        Task<List<TEntity>> GetListAsync(string sort, int pageIndex, int pageSize);
        Task<List<TEntity>> GetListAsync(PageWithSortDto pageWithSortDto);
      
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, string sort, int pageIndex, int pageSize);
        Task<List<TEntity>> GetListAsync(Func<TEntity, bool> predicate);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);

        // 查询构建方法
        IQueryable<TEntity> GetQueryable();
        IQueryable<TEntity> GetQueryableAsync();
        IQueryable<TEntity> GetQueryableAsync(PageWithSortDto pageWithSortDto);

        // CRUD操作
        TEntity Insert(TEntity entity);
        Task<TEntity> InsertAsync(TEntity entity);
        Task<int> InsertAsyncs(TEntity entity);
        TEntity Update(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<int> UpdateAsyncs(TEntity entity);
        TEntity Delete(TEntity entity);
        Task<TEntity> DeleteAsync(TEntity entity);
        Task<int> DeleteAsyncs(Func<TEntity, bool> predicate);

        // 聚合函数
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync();
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IDisposable> BeginTransactionAsync();      
    }
}
