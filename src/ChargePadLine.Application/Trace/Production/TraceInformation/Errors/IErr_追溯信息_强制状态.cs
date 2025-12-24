namespace ChargePadLine.Application.Trace.Production.TraceInformation.Errors
{
    public interface IErr_追溯信息_强制状态 : IErr_TraceInfo
    {
        public record struct TraceInfoNotFound(string SearchCondition) : IErr_追溯信息_强制状态
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到追溯信息（检索条件={SearchCondition}）";
        }
    }
}
