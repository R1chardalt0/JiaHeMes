using ChargePadLine.Entitys.Trace.WorkOrders;

namespace ChargePadLine.Application.Trace.Production.WorkOrders.Errors
{
    public interface IErr_工单启动 : IErrWorkOrder
    {
        public record struct NotFound(int WorkOrderId) : IErr_工单启动
        {
            public string Code => WorkOrderErrorDefines.NOT_FOUND;

            public string Message => $"未找到id={WorkOrderId}的工单";
        }

        public record struct NotReady(WorkOrderDocStatus DocStatus) : IErr_工单启动
        {
            public string Code => WorkOrderErrorDefines.ERR_未就绪;

            public string Message => $"尚未就绪，不可启动。当前状态={DocStatus}";

        }

        public record struct AlreadyStarted(int WorkOrderExecutionId) : IErr_工单启动
        {
            public string Code => WorkOrderErrorDefines.ERR_已启动;

            public string Message => $"已启动，不可重复启动.ExecutionId={WorkOrderExecutionId}";
        }
    }
}
