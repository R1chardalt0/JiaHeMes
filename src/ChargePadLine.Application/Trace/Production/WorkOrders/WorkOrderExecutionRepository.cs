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
    public class WorkOrderExecutionRepository : IWorkOrderExecutionRepository
    {
        private readonly IRepository<WorkOrderExecution> _repository;
        private readonly AppDbContext _context;

        public WorkOrderExecutionRepository(IRepository<WorkOrderExecution> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<WorkOrderExecution?> FindWithWorOrderCodeAsync(WorkOrderCode code)
        {
            var dbSet = _context.Set<WorkOrderExecution>();
            return await dbSet
                .AsNoTracking()
                .Include(woe => woe.WorkOrder)
                .FirstOrDefaultAsync(woe => woe.WorkOrderCode.Value == code.Value);
        }

        public async Task<PaginatedList<WorkOrderExecution>> PaginateAsync(int page, int size)
        {
            var dbSet = _context.Set<WorkOrderExecution>();
            
            var query = dbSet
                .Include(woe => woe.WorkOrder);

            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(woe => woe.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PaginatedList<WorkOrderExecution>(items, totalCount, page, size);
        }
    }
}