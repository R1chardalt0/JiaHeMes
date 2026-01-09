using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto.Order;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// 工单列表服务接口
  /// </summary>
  public interface IOrderListService
  {
    /// <summary>
    /// 分页查询工单列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页工单列表</returns>
    Task<PaginatedList<OrderListDto>> GetOrderListsAsync(OrderListQueryDto queryDto);

    /// <summary>
    /// 根据ID获取工单详情
    /// </summary>
    /// <param name="orderListId">工单ID</param>
    /// <returns>工单详情</returns>
    Task<OrderListDto?> GetOrderListByIdAsync(Guid orderListId);

    /// <summary>
    /// 根据工单编码获取工单详情
    /// </summary>
    /// <param name="orderCode">工单编码</param>
    /// <returns>工单详情</returns>
    Task<OrderListDto?> GetOrderListByCodeAsync(string orderCode);

    /// <summary>
    /// 创建工单
    /// </summary>
    /// <param name="dto">创建工单DTO</param>
    /// <returns>创建成功的工单信息</returns>
    Task<OrderListDto> CreateOrderListAsync(OrderListCreateDto dto);

    /// <summary>
    /// 更新工单
    /// </summary>
    /// <param name="dto">更新工单DTO</param>
    /// <returns>更新后的工单信息</returns>
    Task<OrderListDto> UpdateOrderListAsync(OrderListUpdateDto dto);

    /// <summary>
    /// 删除工单（支持单一和批量删除）
    /// </summary>
    /// <param name="orderListIds">工单ID数组</param>
    /// <returns>删除成功的数量</returns>
    Task<int> DeleteOrderListsAsync(Guid[] orderListIds);

    /// <summary>
    /// 更新工单状态
    /// </summary>
    /// <param name="orderListId">工单ID</param>
    /// <param name="orderStatus">新状态</param>
    /// <returns>更新后的工单信息</returns>
    Task<OrderListDto> UpdateOrderStatusAsync(Guid orderListId, int orderStatus);

    /// <summary>
    /// 获取工单统计信息
    /// </summary>
    /// <returns>工单状态统计，键为状态名称，值为数量</returns>
    Task<Dictionary<string, int>> GetOrderStatisticsAsync();
  }
}
