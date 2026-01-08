using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Order;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 工单列表服务实现
  /// </summary>
  public class OrderListService : IOrderListService
  {
    private readonly IRepository<OrderList> _orderListRepo;
    private readonly ILogger<OrderListService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="orderListRepo">工单列表仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public OrderListService(
        IRepository<OrderList> orderListRepo,
        ILogger<OrderListService> logger,
        AppDbContext dbContext)
    {
      _orderListRepo = orderListRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询工单列表
    /// </summary>
    public async Task<PaginatedList<OrderListDto>> GetOrderListsAsync(OrderListQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _orderListRepo.GetQueryable();

        // 工单ID筛选
        if (queryDto.OrderListId.HasValue)
        {
          query = query.Where(o => o.OrderListId == queryDto.OrderListId.Value);
        }

        // 工单编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.OrderCode))
        {
          query = query.Where(o => o.OrderCode.Contains(queryDto.OrderCode));
        }

        // 工单名称模糊匹配
        if (!string.IsNullOrEmpty(queryDto.OrderName))
        {
          query = query.Where(o => o.OrderName.Contains(queryDto.OrderName));
        }

        // 产品ID筛选
        if (queryDto.ProductListId.HasValue)
        {
          query = query.Where(o => o.ProductListId == queryDto.ProductListId.Value);
        }

        // BOM ID筛选
        if (queryDto.BomId.HasValue)
        {
          query = query.Where(o => o.BomId == queryDto.BomId.Value);
        }

        // 工艺路线ID筛选
        if (queryDto.ProcessRouteId.HasValue)
        {
          query = query.Where(o => o.ProcessRouteId == queryDto.ProcessRouteId.Value);
        }

        // 工单类型筛选
        if (queryDto.OrderType.HasValue)
        {
          query = query.Where(o => o.OrderType == queryDto.OrderType.Value);
        }

        // 工单状态筛选
        if (queryDto.OrderStatus.HasValue)
        {
          query = query.Where(o => o.OrderStatus == queryDto.OrderStatus.Value);
        }

        // 优先级筛选
        if (queryDto.PriorityLevel.HasValue)
        {
          query = query.Where(o => o.PriorityLevel == queryDto.PriorityLevel.Value);
        }

        // 计划开始时间范围筛选
        if (queryDto.PlanStartTimeStart.HasValue)
        {
          query = query.Where(o => o.PlanStartTime >= queryDto.PlanStartTimeStart.Value);
        }
        if (queryDto.PlanStartTimeEnd.HasValue)
        {
          query = query.Where(o => o.PlanStartTime <= queryDto.PlanStartTimeEnd.Value);
        }

        // 计划结束时间范围筛选
        if (queryDto.PlanEndTimeStart.HasValue)
        {
          query = query.Where(o => o.PlanEndTime >= queryDto.PlanEndTimeStart.Value);
        }
        if (queryDto.PlanEndTimeEnd.HasValue)
        {
          query = query.Where(o => o.PlanEndTime <= queryDto.PlanEndTimeEnd.Value);
        }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var orderLists = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var orderListDtos = orderLists.Select(ToDto).ToList();

        // 返回分页结果
        return new PaginatedList<OrderListDto>(orderListDtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询工单列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据ID获取工单详情
    /// </summary>
    public async Task<OrderListDto?> GetOrderListByIdAsync(Guid orderListId)
    {
      try
      {
        var orderList = await _orderListRepo.GetQueryable()
            .FirstOrDefaultAsync(o => o.OrderListId == orderListId);

        if (orderList == null)
          return null;

        return ToDto(orderList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据ID获取工单详情时发生错误，工单ID: {OrderListId}", orderListId);
        throw;
      }
    }

    /// <summary>
    /// 根据工单编码获取工单详情
    /// </summary>
    public async Task<OrderListDto?> GetOrderListByCodeAsync(string orderCode)
    {
      try
      {
        var orderList = await _orderListRepo.GetQueryable()
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

        if (orderList == null)
          return null;

        return ToDto(orderList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据工单编码获取工单详情时发生错误，工单编码: {OrderCode}", orderCode);
        throw;
      }
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    public async Task<OrderListDto> CreateOrderListAsync(OrderListCreateDto dto)
    {
      try
      {
        // 验证工单编码唯一性
        var existingOrderList = await _orderListRepo.GetAsync(o => o.OrderCode == dto.OrderCode);
        if (existingOrderList != null)
        {
          _logger.LogError("工单编码已存在: {OrderCode}", dto.OrderCode);
          throw new InvalidOperationException($"工单编码已存在: {dto.OrderCode}");
        }

        // 创建工单实体
        var now = DateTime.Now;
        var orderList = new OrderList
        {
          OrderListId = Guid.NewGuid(),
          OrderCode = dto.OrderCode,
          OrderName = dto.OrderName,
          ProductListId = dto.ProductListId,
          BomId = dto.BomId,
          ProcessRouteId = dto.ProcessRouteId,
          OrderType = dto.OrderType,
          OrderStatus = dto.OrderStatus,
          PlanQty = dto.PlanQty,
          CompletedQty = dto.CompletedQty,
          PlanStartTime = dto.PlanStartTime,
          PlanEndTime = dto.PlanEndTime,
          ActualStartTime = null, // 实际开始时间由状态切换为生产中时自动设置
          ActualEndTime = null, // 实际结束时间由状态切换为已完成时自动设置
          PriorityLevel = dto.PriorityLevel,
          CreateTime = DateTimeOffset.Now // 创建时间自动设置为当前时间
        };

        // 保存工单
        var result = await _orderListRepo.InsertAsync(orderList);

        if (result != null)
        {
          _logger.LogInformation($"成功创建工单，ID: {result.OrderListId}");
          return ToDto(result);
        }
        else
        {
          _logger.LogError("创建工单失败");
          throw new InvalidOperationException("创建工单失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工单时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    public async Task<OrderListDto> UpdateOrderListAsync(OrderListUpdateDto dto)
    {
      try
      {
        // 查找现有工单
        var existingOrderList = await _orderListRepo.GetAsync(o => o.OrderListId == dto.OrderListId);
        if (existingOrderList == null)
        {
          _logger.LogError("工单不存在，ID: {OrderListId}", dto.OrderListId);
          throw new InvalidOperationException($"工单不存在，ID: {dto.OrderListId}");
        }

        // 验证工单状态，仅允许更新状态为1（新建）的工单
        if (existingOrderList.OrderStatus != 1)
        {
          _logger.LogError("当前状态: {OrderStatus}，不允许更新工单", existingOrderList.OrderStatus);
          throw new InvalidOperationException($"当前状态: {existingOrderList.OrderStatus}，不允许更新工单");
        }

        // 验证工单编码唯一性（排除当前工单）
        if (existingOrderList.OrderCode != dto.OrderCode)
        {
          var existingOrderListWithSameCode = await _orderListRepo.GetAsync(o => o.OrderCode == dto.OrderCode && o.OrderListId != dto.OrderListId);
          if (existingOrderListWithSameCode != null)
          {
            _logger.LogError("工单编码已存在: {OrderCode}", dto.OrderCode);
            throw new InvalidOperationException($"工单编码已存在: {dto.OrderCode}");
          }
        }

        // 更新工单信息
        existingOrderList.OrderCode = dto.OrderCode;
        existingOrderList.OrderName = dto.OrderName;
        existingOrderList.ProductListId = dto.ProductListId;
        existingOrderList.BomId = dto.BomId;
        existingOrderList.ProcessRouteId = dto.ProcessRouteId;
        existingOrderList.OrderType = dto.OrderType;
        existingOrderList.OrderStatus = dto.OrderStatus;
        existingOrderList.PlanQty = dto.PlanQty;
        existingOrderList.CompletedQty = dto.CompletedQty;
        existingOrderList.PlanStartTime = dto.PlanStartTime;
        existingOrderList.PlanEndTime = dto.PlanEndTime;
        // 实际开始时间和实际结束时间由状态变更自动设置，不允许手动修改
        // existingOrderList.ActualStartTime = dto.ActualStartTime;
        // existingOrderList.ActualEndTime = dto.ActualEndTime;
        existingOrderList.PriorityLevel = dto.PriorityLevel;
        existingOrderList.UpdateTime = DateTime.Now; // 更新时间自动设置为当前时间

        // 更新工单
        var result = await _orderListRepo.UpdateAsync(existingOrderList);

        if (result != null)
        {
          _logger.LogInformation($"成功更新工单，ID: {result.OrderListId}");
          return ToDto(result);
        }
        else
        {
          _logger.LogError("更新工单失败");
          throw new InvalidOperationException("更新工单失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工单时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 删除工单（支持单一和批量删除）
    /// </summary>
    public async Task<int> DeleteOrderListsAsync(Guid[] orderListIds)
    {
      if (orderListIds == null || orderListIds.Length == 0)
        return 0; // 没有要删除的工单

      try
      {
        var orderLists = await _orderListRepo.GetQueryable()
            .Where(o => orderListIds.Contains(o.OrderListId))
            .ToListAsync();

        if (orderLists.Count == 0)
        {
          return 0;
        }

        _dbContext.OrderList.RemoveRange(orderLists);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功删除工单，共删除 {orderLists.Count} 个");
        return orderLists.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工单时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新工单状态
    /// </summary>
    public async Task<OrderListDto> UpdateOrderStatusAsync(Guid orderListId, int orderStatus)
    {
      try
      {
        // 查找工单
        var orderList = await _orderListRepo.GetAsync(o => o.OrderListId == orderListId);
        if (orderList == null)
        {
          _logger.LogError("工单不存在，ID: {OrderListId}", orderListId);
          throw new InvalidOperationException($"工单不存在，ID: {orderListId}");
        }

        // 记录原状态
        var originalStatus = orderList.OrderStatus;

        // 更新状态
        orderList.OrderStatus = orderStatus;

        // 根据状态变更设置相应的时间字段
        var now = DateTime.Now;
        if (originalStatus != 3 && orderStatus == 3)
        {
          // 状态切换为生产中，设置实际开始时间
          orderList.ActualStartTime = now;
        }
        else if (originalStatus != 4 && orderStatus == 4)
        {
          // 状态切换为已完成，设置实际结束时间
          orderList.ActualEndTime = now;
        }

        // 更新时间
        orderList.UpdateTime = now;

        // 保存更新
        var result = await _orderListRepo.UpdateAsync(orderList);

        if (result != null)
        {
          _logger.LogInformation($"成功更新工单状态，ID: {result.OrderListId}，新状态: {orderStatus}");
          return ToDto(result);
        }
        else
        {
          _logger.LogError("更新工单状态失败");
          throw new InvalidOperationException("更新工单状态失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工单状态时发生异常，工单ID: {OrderListId}，新状态: {OrderStatus}", orderListId, orderStatus);
        throw;
      }
    }

    /// <summary>
    /// 获取工单统计信息
    /// </summary>
    public async Task<Dictionary<string, int>> GetOrderStatisticsAsync()
    {
      try
      {
        // 状态映射
        var statusMap = new Dictionary<int, string>
        {
          { 1, "新建" },
          { 2, "已排产" },
          { 3, "生产中" },
          { 4, "已完成" },
          { 5, "已关闭" }
        };

        // 初始化为0
        var statistics = statusMap.ToDictionary(kvp => kvp.Value, kvp => 0);

        // 查询各状态的工单数量
        var statusCounts = await _orderListRepo.GetQueryable()
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        // 更新统计结果
        foreach (var item in statusCounts)
        {
          if (statusMap.ContainsKey(item.Status))
          {
            statistics[statusMap[item.Status]] = item.Count;
          }
        }

        return statistics;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工单统计信息时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 将OrderList实体转换为OrderListDto
    /// </summary>
    /// <param name="orderList">工单实体</param>
    /// <returns>工单DTO</returns>
    private OrderListDto ToDto(OrderList orderList)
    {
      return new OrderListDto
      {
        OrderListId = orderList.OrderListId,
        OrderCode = orderList.OrderCode,
        OrderName = orderList.OrderName,
        ProductListId = orderList.ProductListId,
        BomId = orderList.BomId,
        ProcessRouteId = orderList.ProcessRouteId,
        OrderType = orderList.OrderType,
        OrderStatus = orderList.OrderStatus,
        PlanQty = orderList.PlanQty,
        CompletedQty = orderList.CompletedQty,
        PlanStartTime = orderList.PlanStartTime,
        PlanEndTime = orderList.PlanEndTime,
        ActualStartTime = orderList.ActualStartTime,
        ActualEndTime = orderList.ActualEndTime,
        PriorityLevel = orderList.PriorityLevel,
        CreateBy = orderList.CreateBy,
        CreateTime = orderList.CreateTime,
        UpdateBy = orderList.UpdateBy,
        UpdateTime = orderList.UpdateTime,
        Remark = orderList.Remark
      };
    }
  }
}
