using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 工艺路线子项服务实现
  /// </summary>
  public class ProcessRouteItemService : IProcessRouteItemService
  {
    private readonly IRepository<ProcessRouteItem> _routeItemRepo;
    private readonly ILogger<ProcessRouteItemService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="routeItemRepo">工艺路线子项仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public ProcessRouteItemService(
        IRepository<ProcessRouteItem> routeItemRepo,
        ILogger<ProcessRouteItemService> logger,
        AppDbContext dbContext)
    {
      _routeItemRepo = routeItemRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询工艺路线子项列表
    /// </summary>
    public async Task<PaginatedList<ProcessRouteItemDto>> GetProcessRouteItemsAsync(ProcessRouteItemQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _routeItemRepo.GetQueryable();

        // 工艺路线子项ID精确匹配
        if (queryDto.Id.HasValue)
        {
          query = query.Where(b => b.Id == queryDto.Id.Value);
        }

        // 工艺路线ID精确匹配
        if (queryDto.HeadId.HasValue)
        {
          query = query.Where(b => b.HeadId == queryDto.HeadId.Value);
        }

        // 工位编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.StationCode))
        {
          query = query.Where(b => b.StationCode.Contains(queryDto.StationCode));
        }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 按工艺路线序号升序排列
        query = query.OrderBy(b => b.RouteSeq);

        // 分页
        var routeItems = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var routeItemDtos = routeItems.Select(b => ToDto(b)).ToList();

        // 返回分页结果
        return new PaginatedList<ProcessRouteItemDto>(routeItemDtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询工艺路线子项列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据ID获取工艺路线子项详情
    /// </summary>
    public async Task<ProcessRouteItemDto?> GetProcessRouteItemByIdAsync(Guid routeItemId)
    {
      var routeItem = await _routeItemRepo.GetQueryable()
          .FirstOrDefaultAsync(b => b.Id == routeItemId);

      if (routeItem == null)
        return null;

      return ToDto(routeItem);
    }

    /// <summary>
    /// 根据HeadId获取工艺路线子项列表
    /// </summary>
    public async Task<IEnumerable<ProcessRouteItemDto>> GetProcessRouteItemsByHeadIdAsync(Guid headId)
    {
      try
      {
        var routeItems = await _routeItemRepo.GetQueryable()
            .Where(b => b.HeadId == headId)
            .OrderBy(b => b.RouteSeq)
            .ToListAsync();

        return routeItems.Select(b => ToDto(b));
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据HeadId获取工艺路线子项列表时发生错误，HeadId: {HeadId}", headId);
        throw;
      }
    }

    /// <summary>
    /// 创建工艺路线子项
    /// </summary>
    public async Task<ProcessRouteItemDto> CreateProcessRouteItemAsync(ProcessRouteItemCreateDto dto)
    {
      try
      {
        // 创建工艺路线子项实体
        var routeItem = new ProcessRouteItem
        {
          Id = Guid.NewGuid(),
          HeadId = dto.HeadId,
          StationCode = dto.StationCode,
          MustPassStation = dto.MustPassStation,
          CheckStationList = dto.CheckStationList,
          FirstStation = dto.FirstStation,
          CheckAll = dto.CheckAll,
          RouteSeq = dto.RouteSeq,
          MaxNGCount = dto.MaxNGCount,
          Remark = dto.Remark,
        };

        // 保存工艺路线子项
        var result = await _routeItemRepo.InsertAsync(routeItem);

        if (result != null)
        {
          _logger.LogInformation("成功创建工艺路线子项，ID: {RouteItemId}", result.Id);
          return ToDto(result);
        }
        else
        {
          _logger.LogError("创建工艺路线子项失败");
          throw new InvalidOperationException("创建工艺路线子项失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工艺路线子项时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新工艺路线子项
    /// </summary>
    public async Task<ProcessRouteItemDto> UpdateProcessRouteItemAsync(ProcessRouteItemUpdateDto dto)
    {
      try
      {
        // 查找现有工艺路线子项
        var existingRouteItem = await _routeItemRepo.GetAsync(b => b.Id == dto.Id);
        if (existingRouteItem == null)
          throw new InvalidOperationException($"工艺路线子项不存在，ID: {dto.Id}");

        // 更新工艺路线子项信息
        existingRouteItem.HeadId = dto.HeadId;
        existingRouteItem.StationCode = dto.StationCode;
        existingRouteItem.MustPassStation = dto.MustPassStation;
        existingRouteItem.CheckStationList = dto.CheckStationList;
        existingRouteItem.SearchValue = dto.SearchValue;
        existingRouteItem.FirstStation = dto.FirstStation;
        existingRouteItem.CheckAll = dto.CheckAll;
        existingRouteItem.RouteSeq = dto.RouteSeq;
        existingRouteItem.MaxNGCount = dto.MaxNGCount;
        existingRouteItem.Remark = dto.Remark;

        // 保存更新
        var result = await _routeItemRepo.UpdateAsync(existingRouteItem);

        if (result != null)
        {
          _logger.LogInformation("成功更新工艺路线子项，ID: {RouteItemId}", result.Id);
          return ToDto(result);
        }
        else
        {
          _logger.LogError("更新工艺路线子项失败");
          throw new InvalidOperationException("更新工艺路线子项失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工艺路线子项时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 删除工艺路线子项（支持单一和批量删除）
    /// </summary>
    public async Task<int> DeleteProcessRouteItemsAsync(Guid[] routeItemIds)
    {
      if (routeItemIds == null || routeItemIds.Length == 0)
        return 0; // 没有要删除的工艺路线子项

      try
      {
        var routeItems = await _dbContext.ProcessRouteItems.Where(b => routeItemIds.Contains(b.Id)).ToListAsync();
        if (routeItems.Count == 0)
        {
          return 0;
        }

        _dbContext.ProcessRouteItems.RemoveRange(routeItems);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("成功删除工艺路线子项，共删除 {Count} 个", routeItems.Count);
        return routeItems.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工艺路线子项时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 将工艺路线子项实体转换为DTO
    /// </summary>
    /// <param name="routeItem">工艺路线子项实体</param>
    /// <returns>工艺路线子项DTO</returns>
    private ProcessRouteItemDto ToDto(ProcessRouteItem routeItem)
    {
      return new ProcessRouteItemDto
      {
        Id = routeItem.Id,
        HeadId = routeItem.HeadId,
        StationCode = routeItem.StationCode,
        MustPassStation = routeItem.MustPassStation,
        CheckStationList = routeItem.CheckStationList,
        FirstStation = routeItem.FirstStation,
        CheckAll = routeItem.CheckAll,
        RouteSeq = routeItem.RouteSeq,
        MaxNGCount = routeItem.MaxNGCount,
        Remark = routeItem.Remark,
        CreateTime = routeItem.CreateTime,
        UpdateTime = routeItem.UpdateTime
      };
    }
  }
}