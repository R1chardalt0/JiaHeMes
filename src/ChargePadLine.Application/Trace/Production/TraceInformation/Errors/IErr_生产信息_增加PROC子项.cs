
namespace ChargePadLine.Application.Trace.Production.TraceInformation.Errors
{
    public interface IErr_追溯信息_增加PROC子项 : IErr_TraceInfo
    {
        public record struct TraceInfoNotFound(string Condition) : IErr_追溯信息_增加PROC子项
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到追溯信息(Condition={Condition})";
        }
        public record struct AlreadyExists(string Station, string Key) : IErr_追溯信息_增加PROC子项
        {
            public string Code => "已经存在同样的工艺生产记录";
            public string Message => $"站({Station})已经存在工艺生产记录(Key={Key})";
        }

        public record struct MiscError(string Message) : IErr_追溯信息_增加PROC子项
        {
            public string Code => TraceInfo_Error_Defines.MISC;
        }
    }
}
