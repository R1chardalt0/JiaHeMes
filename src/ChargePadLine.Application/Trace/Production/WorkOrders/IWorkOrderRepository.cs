using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service;

namespace ChargePadLine.Application.Trace.Production.WorkOrders
{
    public interface IWorkOrderRepository 
    {
        Task<WorkOrder?> FindWithCodeAsync(WorkOrderCode code);
        public Task<PaginatedList<WorkOrder>> PaginateAsync(int page, int size);
    }
}

