using ChargePadLine.Entitys.Trace.WorkOrders;

namespace ChargePadLine.Application.Trace.Production.WorkOrders.Errors
{
    /// <summary>
    /// 使用工单创建产品信息
    /// </summary>
    public interface IErr_工单生产 : IErrWorkOrder
    {
        /// <summary>
        /// 未找到对应工单
        /// </summary>
        public record struct WorkOrderNotFound(int WorkOrderId) : IErr_工单生产
        {
            public string Code => WorkOrderErrorDefines.NOT_FOUND;

            public string Message => $"未找到对应工单(id={WorkOrderId})";
        }

        /// <summary>
        /// 工单状态尚未就绪
        /// </summary>
        /// <param name="DocStatus"></param>
        public record struct WorkOrderNotReady(WorkOrderDocStatus DocStatus) : IErr_工单生产
        {
            public string Code => WorkOrderErrorDefines.ERR_未就绪;

            public string Message => $"工单尚未就绪，不可生产：工单状态={DocStatus}";
        }

        /// <summary>
        /// 未找到对应工单
        /// </summary>
        public record struct WorkOrderNotExecuting(int WorkOrderId) : IErr_工单生产
        {
            public string Code => WorkOrderErrorDefines.ERR_未执行;

            public string Message => $"工单尚未执行(id={WorkOrderId})";
        }

        /// <summary>
        /// 工单被标记完成
        /// </summary>
        public record struct WorkOrderFinished() : IErr_工单生产
        {
            public string Code => WorkOrderErrorDefines.ERR_已完工;

            public string Message => $"工单已经完工，不可生产";
        }

        /// <summary>
        /// 超出配额
        /// </summary>
        public record struct WorkOrderQuotaExceeds(decimal quota, decimal acc, decimal curr) : IErr_工单生产
        {
            public string Code => WorkOrderErrorDefines.ERR_超额;

            public string Message => $"已经超额：配额={quota};已产={acc};本次={curr}";
        }

        public record struct MiscError(string Message) : IErr_工单生产
        {
            public string Code => throw new NotImplementedException();
        }
    }

}
