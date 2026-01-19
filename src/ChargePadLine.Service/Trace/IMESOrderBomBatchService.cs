using System.Collections.Generic;
using System.Threading.Tasks;
using ChargePadLine.Service.Trace.Dto.Order;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// MES工单BOM批次服务接口
  /// </summary>
  public interface IMESOrderBomBatchService
  {
    /// <summary>
    /// 根据ID查询工单BOM批次
    /// </summary>
    /// <param name="id">工单BOM批次ID</param>
    /// <returns>工单BOM批次数据传输对象</returns>
    Task<MESOrderBomBatchDto> GetByIdAsync(Guid id);

    /// <summary>
    /// 查询工单BOM批次列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    Task<List<MESOrderBomBatchDto>> GetListAsync(MESOrderBomBatchQueryDto queryDto);

    /// <summary>
    /// 分页查询工单BOM批次
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页结果，包含工单BOM批次数据传输对象列表和总记录数</returns>
    Task<PaginatedList<MESOrderBomBatchDto>> GetPagedListAsync(MESOrderBomBatchQueryDto queryDto);

    /// <summary>
    /// 根据工单ID查询工单BOM批次列表
    /// </summary>
    /// <param name="orderListId">工单ID</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    Task<List<MESOrderBomBatchDto>> GetByOrderListIdAsync(Guid orderListId);

    /// <summary>
    /// 根据物料ID查询工单BOM批次列表
    /// </summary>
    /// <param name="productListId">物料ID</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    Task<List<MESOrderBomBatchDto>> GetByProductListIdAsync(Guid productListId);
  }
}