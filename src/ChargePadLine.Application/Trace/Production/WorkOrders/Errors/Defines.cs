namespace ChargePadLine.Application.Trace.Production.WorkOrders.Errors
{
    /// <summary>
    /// 预定义的错误码
    /// </summary>
    public static class WorkOrderErrorDefines
    {
        private const string PREFIX = "WO_";
        public const string WO_DOC_STATUS_ERR = $"{PREFIX}状态错误";
        public const string NOT_FOUND = $"{PREFIX}工单未找到";
        public const string ERR_未就绪 = $"{PREFIX}工单未就绪";
        public const string ERR_已启动= $"{PREFIX}工单重复启动";
        public const string ERR_已完工 = $"{PREFIX}已完工";
        public const string ERR_未执行 = $"{PREFIX}工单未执行";


        public const string ERR_超额= $"{PREFIX}超出配额";

        public const string ERR_配方未找到 = $"{PREFIX}配方未找到";


        public const string ERR_MISC = $"{PREFIX}其它错误";
    }

}
