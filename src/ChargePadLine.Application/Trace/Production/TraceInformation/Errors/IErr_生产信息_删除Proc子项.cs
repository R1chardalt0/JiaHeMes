namespace ChargePadLine.Application.Trace.Production.TraceInformation.Errors
{
    public interface IErr_追溯信息_删除Proc子项 : IErr_TraceInfo
    {
        public record struct TraceInfoNotFound(Guid TraceInfoId) : IErr_追溯信息_删除Proc子项
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到id={TraceInfoId}的追溯信息";
        }

        public record struct ProcItemNotFound(Guid ProcItemId) : IErr_追溯信息_删除Proc子项
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到Proc子项信息：子项id={ProcItemId}";
        }

        public record struct AlreadyDeleted(Guid ProcItemId) : IErr_追溯信息_删除Proc子项
        {
            public string Code => TraceInfo_Error_Defines.已经删除;
            public string Message => $"Proc子项已经被删除,无法重复删除(子项id={ProcItemId})";
        }
    }
}
