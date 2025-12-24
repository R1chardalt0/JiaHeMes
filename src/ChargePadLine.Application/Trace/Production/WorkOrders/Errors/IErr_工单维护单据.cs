namespace ChargePadLine.Application.Trace.Production.WorkOrders.Errors
{
    public interface IErr_工单维护单据 : IErrWorkOrder
    {

        public record DocNotFound(int WorkOrderId) : IErr_工单维护单据
        {
            public string Code => WorkOrderErrorDefines.NOT_FOUND; 
            public string Message => $"找不到指定工单(id={WorkOrderId})";
        }


        public record DocStatusErr(string Message) : IErr_工单维护单据
        {
            public string Code => WorkOrderErrorDefines.WO_DOC_STATUS_ERR;
        }
    }



}
