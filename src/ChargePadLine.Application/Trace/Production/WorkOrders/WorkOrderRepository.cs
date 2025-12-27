using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.WorkOrders
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly IRepository<WorkOrder> _repository;
        private readonly AppDbContext _context;

        public WorkOrderRepository(IRepository<WorkOrder> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<WorkOrder?> FindWithCodeAsync(WorkOrderCode code)
        {
            var dbSet = _context.Set<WorkOrder>();
            return await dbSet
                .AsNoTracking()
                .Include(w => w.BomRecipe)
                .FirstOrDefaultAsync(w => w.Code == code.Value);
        }

        public async Task<WorkOrder?> FindAsync(int workOrderId)
        {
            var dbSet = _context.Set<WorkOrder>();
            return await dbSet
                .AsNoTracking()
                .Include(w => w.BomRecipe)
                .FirstOrDefaultAsync(w => w.Id == workOrderId);
        }

        public async Task<PaginatedList<WorkOrder>> PaginateAsync(int page, int size)
        {
            var dbSet = _context.Set<WorkOrder>();
            
            var query = dbSet
                .Include(w => w.BomRecipe);

            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(w => w.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PaginatedList<WorkOrder>(items, totalCount, page, size);
        }
    }
}