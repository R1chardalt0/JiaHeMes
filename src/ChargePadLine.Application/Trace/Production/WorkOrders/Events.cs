using ChargePadLine.Entitys.Trace.WorkOrders;
using UNIT = System.ValueTuple;

namespace ChargePadLine.Application.Trace.Production.WorkOrders
{

    public interface IWorkOrderEvent 
    {
        void TakePlace(WorkOrder wo);
    }
    public static class WorkOrderEventExtensions
    {
        public static UNIT Fold(this WorkOrder wo, IReadOnlyList<IWorkOrderEvent> events)
        {
            foreach (var evt in events)
            {
                evt.TakePlace(wo);
            }
            return new UNIT();
        }
    }


    #region 执行生产
    public interface IWorkOrderExecutionEvent
    {
        void TakePlace(WorkOrderExecution wo);
    }
    public static class WorkOrderExecutionEventExtensions
    {
        public static UNIT Fold(this WorkOrderExecution wo, IReadOnlyList<IWorkOrderExecutionEvent> events)
        {
            foreach (var evt in events)
            {
                evt.TakePlace(wo);
            }
            return new UNIT();
        }
    }

    public record Evt_WorkOrderExec_StartExecution(WorkOrder WorkOrder) : IWorkOrderExecutionEvent
    {
        public void TakePlace(WorkOrderExecution woe)
        {
            woe.WorkOrder = WorkOrder;
            woe.WorkOrderId = WorkOrder.Id;
            woe.WorkOrderCode = WorkOrder.Code;
            woe.Accumulation = 0;
            woe.CreatedAt = DateTimeOffset.UtcNow;
            woe.HasFinished = false;
        }
    }
    #endregion

}
