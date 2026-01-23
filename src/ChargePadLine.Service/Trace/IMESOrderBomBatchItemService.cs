using System.Collections.Generic;
using System.Threading.Tasks;
using ChargePadLine.Service.Trace.Dto.Order;

namespace ChargePadLine.Service.Trace
{
    /// <summary>
    /// MES工单BOM批次明细服务接口
    /// </summary>
    public interface IMESOrderBomBatchItemService
    {
        /// <summary>
        /// 根据ID查询工单BOM批次明细
        /// </summary>
        /// <param name="id">工单BOM批次明细ID</param>
        /// <returns>工单BOM批次明细数据传输对象</returns>
        Task<MesOrderBomBatchItemDto> GetByIdAsync(Guid id);

        /// <summary>
        /// 查询工单BOM批次明细列表
        /// </summary>
        /// <param name="queryDto">查询参数</param>
        /// <returns>工单BOM批次明细数据传输对象列表</returns>
        Task<List<MesOrderBomBatchItemDto>> GetListAsync(MESOrderBomBatchItemQueryDto queryDto);

        /// <summary>
        /// 分页查询工单BOM批次明细
        /// </summary>
        /// <param name="queryDto">查询参数</param>
        /// <returns>分页结果，包含工单BOM批次明细数据传输对象列表和总记录数</returns>
        Task<(List<MesOrderBomBatchItemDto> Data, int Total)> GetPagedListAsync(MESOrderBomBatchItemQueryDto queryDto);

        /// <summary>
        /// 根据工单BOM批次ID查询明细列表
        /// </summary>
        /// <param name="orderBomBatchId">工单BOM批次ID</param>
        /// <returns>工单BOM批次明细数据传输对象列表</returns>
        Task<List<MesOrderBomBatchItemDto>> GetByOrderBomBatchIdAsync(Guid orderBomBatchId);

        /// <summary>
        /// 根据SN编码查询工单BOM批次明细
        /// </summary>
        /// <param name="snNumber">SN编码</param>
        /// <returns>工单BOM批次明细数据传输对象</returns>
        Task<List<MesOrderBomBatchItemDto>> GetBySnNumberAsync(string snNumber);
    }
}