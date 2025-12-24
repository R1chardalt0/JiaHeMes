using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service;

namespace ChargePadLine.Application.Trace.Production.WorkOrders
{
    public interface IWorkOrderExecutionRepository
    {
        Task<WorkOrderExecution?> FindWithWorOrderCodeAsync(WorkOrderCode code);
        Task<PaginatedList<WorkOrderExecution>> PaginateAsync(int page, int size);
    }
}

