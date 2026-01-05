namespace DeviceManage.Services.DeviceMagService
{
    /// <summary>
    /// 通用分页请求参数
    /// </summary>
    public class PaginatedRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
