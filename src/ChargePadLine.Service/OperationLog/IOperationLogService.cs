using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.OperationLog.Dto;
using System.Threading.Tasks;

namespace ChargePadLine.Service.OperationLog
{
    /// <summary>
    /// 操作日志服务接口
    /// </summary>
    public interface IOperationLogService
    {
        /// <summary>
        /// 新增操作日志
        /// </summary>
        /// <param name="dto">操作日志数据传输对象</param>
        /// <returns></returns>
        Task AddOperationLogAsync(OperationLogAddDto dto);

        /// <summary>
        /// 分页查询操作日志列表
        /// </summary>
        /// <param name="dto">查询条件</param>
        /// <returns>分页操作日志列表</returns>
        Task<PaginatedList<SysOperationLog>> GetOperationLogListAsync(OperationLogQueryDto dto);

        /// <summary>
        /// 测试查询接口（用于调试）
        /// </summary>
        /// <returns>测试结果</returns>
        Task<object> TestQueryAsync();
    }
}

