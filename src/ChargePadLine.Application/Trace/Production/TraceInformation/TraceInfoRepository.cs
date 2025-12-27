using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.TraceInformation
{
    public class TraceInfoRepository : ITraceInfoRepository
    {
        private readonly IRepository<TraceInfo> _repository;
        private readonly AppDbContext _context;

        public TraceInfoRepository(IRepository<TraceInfo> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<TraceInfo?> FindWithVsnAsync(uint vsn)
        {
            var dbSet = _context.Set<TraceInfo>();
            return await dbSet
                .AsNoTracking()
                .Include(t => t.BomItems)
                .Include(t => t.ProcItems)
                .FirstOrDefaultAsync(t => t.Vsn == vsn);
        }

        public async Task<TraceInfo?> FindWithPINAsync(SKU PIN)
        {
            var dbSet = _context.Set<TraceInfo>();
            return await dbSet
                .AsNoTracking()
                .Include(t => t.BomItems)
                .Include(t => t.ProcItems)
                .FirstOrDefaultAsync(t => t.PIN == PIN.Value);
        }

        public async Task<PaginatedList<TraceInfo>> PaginateAsync(QryTraceInfoList qry)
        {
            var dbSet = _context.Set<TraceInfo>();
            
            IQueryable<TraceInfo> query = dbSet
                .Include(t => t.BomItems)
                .Include(t => t.ProcItems);

            // Apply filters based on query parameters
            if (qry.Id.HasValue)
            {
                query = query.Where(t => t.Id == qry.Id.Value);
            }

            if (!string.IsNullOrEmpty(qry.PIN))
            {
                query = query.Where(t => t.PIN == qry.PIN);
            }

            if (qry.Vsn.HasValue)
            {
                query = query.Where(t => t.Vsn == qry.Vsn.Value);
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((qry.PageIndex - 1) * qry.PageSize)
                .Take(qry.PageSize)
                .ToListAsync();

            return new PaginatedList<TraceInfo>(items, totalCount, qry.PageIndex, qry.PageSize);
        }

        public async Task<IList<Guid>> SearchExpiredRecordsAsync(int expiryMinutes, int limit)
        {
            var expiryTime = DateTimeOffset.UtcNow.AddMinutes(-expiryMinutes);
            var dbSet = _context.Set<TraceInfo>();
            
            return await dbSet
                .Where(t => t.CreatedAt < expiryTime && t.PIN == "")
                .OrderBy(t => t.CreatedAt)
                .Take(limit)
                .Select(t => t.Id)
                .ToListAsync();
        }

        public async Task<TraceInfo> AddAsync(TraceInfo traceInfo, bool saveChanges)
        {
            var dbSet = _context.Set<TraceInfo>();
            await dbSet.AddAsync(traceInfo);
            
            if (saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            
            return traceInfo;
        }

        public async Task<TraceInfo?> FindAsync(Guid id)
        {
            var dbSet = _context.Set<TraceInfo>();
            return await dbSet
                .AsNoTracking()
                .Include(t => t.BomItems)
                .Include(t => t.ProcItems)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}