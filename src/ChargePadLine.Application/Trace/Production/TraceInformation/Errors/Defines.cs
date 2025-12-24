namespace ChargePadLine.Application.Trace.Production.TraceInformation.Errors   
{
    public static class TraceInfo_Error_Defines
    {
        private const string PREFIX = "追溯信息错误_";
        public const string NOT_FOUND = $"{PREFIX}未找到追溯信息";
        public const string BOM子项超额 = $"{PREFIX}BOM子项超额";

        public const string 已经删除= $"{PREFIX}已经删除";

        public const string 已经绑定过PIN  = $"{PREFIX}已经绑定过PIN";

        public const string MISC = $"{PREFIX}其它";

    }


}
