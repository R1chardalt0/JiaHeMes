using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService.Dto;
using DeviceManage.Services;

namespace DeviceManage.Services.DeviceMagService
{
    public interface ILogService
    {
        Task LogAsync(OperationLog log);
        Task LogAsync(int? userId,
                      string? username,
                      OperationType operationType,
                      string module,
                      string description,
                      string result = "成功",
                      string? requestParams = null,
                      string? errorMessage = null,
                      string? ipAddress = null);

        /// <summary>
        /// 分页查询日志
        /// </summary>
        Task<PaginatedList<OperationLog>> GetLogsAsync(LogSearchDto dto);
    }
}