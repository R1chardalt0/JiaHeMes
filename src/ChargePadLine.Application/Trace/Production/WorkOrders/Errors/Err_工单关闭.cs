namespace ChargePadLine.Application.Trace.Production.WorkOrders.Errors
{
    /// <summary>
    /// 标记工单为完工错误
    /// </summary>
    public interface Err_工单关闭 : IErrWorkOrder
    {
        public record struct NotFound() : Err_工单关闭
        {
            public string Code => WorkOrderErrorDefines.NOT_FOUND;

            public string Message => "未找到";
        }

        public record struct AlreadyFinished() : Err_工单关闭
        {
            public string Code => WorkOrderErrorDefines.ERR_已完工;

            public string Message => $"工单已完结，不可重复完工";
        }

        public record struct MiscError(string Message) : Err_工单关闭
        {
            public string Code => WorkOrderErrorDefines.ERR_MISC;
        }
    }

}
