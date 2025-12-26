using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Production.BatchQueue;
using ChargePadLine.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.BatchQueue
{
    public class MaterialBatchQueueItemRepo : IMaterialBatchQueueItemRepo
    {
        private readonly IRepository<BatchMaterialQueueItem> _repository;
        private readonly AppDbContext _context;

        public MaterialBatchQueueItemRepo(IRepository<BatchMaterialQueueItem> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<BatchMaterialQueueItem> AddAsync(BatchMaterialQueueItem entity, bool saveChanges = true)
        {
            if (saveChanges)
            {
                return await _repository.InsertAsync(entity);
            }
            else
            {
                var dbSet = _context.Set<BatchMaterialQueueItem>();
                var res = (await dbSet.AddAsync(entity)).Entity;
                return res;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<BatchMaterialQueueItem?> FindAsync(int id)
        {
            var dbSet = _context.Set<BatchMaterialQueueItem>();
            return await dbSet.FindAsync(id);
        }

        public async Task<BatchMaterialQueueItem?> CheckBatchExistsAsync(string bomItemCode, string batchNum)
        {
            var dbSet = _context.Set<BatchMaterialQueueItem>();
            return await dbSet
                .Where(x => x.BomItemCode == bomItemCode && x.BatchCode == batchNum)
                .FirstOrDefaultAsync();
        }

        public async Task<IList<BatchMaterialQueueItem>> FindTopCandicatesAsync(string bomItemCode, int limit)
        {
            var dbSet = _context.Set<BatchMaterialQueueItem>();
            return await dbSet
                .Where(x => x.BomItemCode == bomItemCode && !x.IsDeleted && x.RemainingAmount > 0)
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<PaginatedList<BatchMaterialQueueItem>> PaginateAsync(Qry_物料批_队列 qry)
        {
            var dbSet = _context.Set<BatchMaterialQueueItem>();
            var query = dbSet.AsQueryable();

            if (qry.Id > 0)
            {
                query = query.Where(x => x.Id == qry.Id);
            }

            if (!string.IsNullOrWhiteSpace(qry.BomItemCode))
            {
                query = query.Where(x => x.BomItemCode.Contains(qry.BomItemCode));
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            return await query.RetrievePagedListAsync(qry.PageIndex, qry.PageSize);
        }
    }
}
