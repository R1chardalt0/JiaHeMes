namespace ChargePadLine.Application.Trace.Production.TraceInformation.Errors
{
    public interface IErr_追溯信息_BindPIN : IErr_TraceInfo
    {
        public record struct TraceInfoNotFound(Guid TraceInfoId) : IErr_追溯信息_BindPIN
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到id={TraceInfoId}的追溯信息";
        }

        public record struct AlreadyBound(Guid TraceInfoId) : IErr_追溯信息_BindPIN
        {
            public string Code => TraceInfo_Error_Defines.已经绑定过PIN;
            public string Message => $"Proc子项已经绑定过PIN,无法重复绑定(子项id={TraceInfoId})";
        }
    }
}
