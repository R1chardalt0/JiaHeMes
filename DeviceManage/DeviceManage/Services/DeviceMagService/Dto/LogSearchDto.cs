namespace DeviceManage.Services.DeviceMagService.Dto
{
    /// <summary>
    /// 日志分页查询条件
    /// </summary>
    public class LogSearchDto
    {
        public int current { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public string Module { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty; // OperationType 字符串形式
    }
}
