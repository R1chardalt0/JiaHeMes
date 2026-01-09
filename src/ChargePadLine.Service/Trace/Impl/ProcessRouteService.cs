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
  /// 工艺路线服务实现
  /// </summary>
  public class ProcessRouteService : IProcessRouteService
  {
    private readonly IRepository<ProcessRoute> _processRouteRepo;
    private readonly ILogger<ProcessRouteService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="processRouteRepo">工艺路线仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public ProcessRouteService(
        IRepository<ProcessRoute> processRouteRepo,
        ILogger<ProcessRouteService> logger,
        AppDbContext dbContext)
    {
      _processRouteRepo = processRouteRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 验证Status值
    /// </summary>
    /// <param name="status">状态值</param>
    /// <exception cref="InvalidOperationException">状态值无效时抛出异常</exception>
    private void ValidateStatus(int status)
    {
      if (status != 0 && status != 1)
      {
        throw new InvalidOperationException("状态值无效，只能是0（启用）或1（关闭）");
      }
    }

    /// <summary>
    /// 将ProcessRoute实体转换为ProcessRouteDto
    /// </summary>
    /// <param name="processRoute">工艺路线实体</param>
    /// <returns>工艺路线DTO</returns>
    private ProcessRouteDto ToDto(ProcessRoute processRoute)
    {
      return new ProcessRouteDto
      {
        Id = processRoute.Id,
        RouteName = processRoute.RouteName,
        RouteCode = processRoute.RouteCode,
        Status = processRoute.Status,
        Remark = processRoute.Remark,
        CreateTime = processRoute.CreateTime,
        UpdateTime = processRoute.UpdateTime
      };
    }

    /// <summary>
    /// 分页查询工艺路线列表
    /// </summary>
    public async Task<PaginatedList<ProcessRouteDto>> GetProcessRoutesAsync(ProcessRouteQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        IQueryable<ProcessRoute> query = _processRouteRepo.GetQueryable();

        // 工艺路线编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.RouteCode))
        {
          query = query.Where(p => p.RouteCode.Contains(queryDto.RouteCode));
        }

        // 工艺路线名称模糊匹配
        if (!string.IsNullOrEmpty(queryDto.RouteName))
        {
          query = query.Where(p => p.RouteName.Contains(queryDto.RouteName));
        }

        // 状态精确匹配
        if (queryDto.Status.HasValue)
        {
          query = query.Where(p => p.Status == queryDto.Status.Value);
        }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var processRoutes = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var processRouteDtos = processRoutes.Select(p => ToDto(p)).ToList();

        // 返回分页结果
        return new PaginatedList<ProcessRouteDto>(processRouteDtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询工艺路线列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据ID获取工艺路线详情
    /// </summary>
    public async Task<ProcessRouteDto?> GetProcessRouteByIdAsync(Guid routeId)
    {
      var processRoute = await _processRouteRepo.GetQueryable()
          .FirstOrDefaultAsync(p => p.Id == routeId);

      if (processRoute == null)
        return null;

      return ToDto(processRoute);
    }

    /// <summary>
    /// 创建工艺路线
    /// </summary>
    public async Task<ProcessRouteDto> CreateProcessRouteAsync(ProcessRouteCreateDto dto)
    {
      try
      {
        // 验证Status值
        ValidateStatus(dto.Status);

        // 验证工艺路线编码唯一性
        var existingProcessRoute = await _processRouteRepo.GetAsync(p => p.RouteCode == dto.RouteCode);
        if (existingProcessRoute != null)
        {
          _logger.LogError("工艺路线编码已存在: {RouteCode}", dto.RouteCode);
          throw new InvalidOperationException($"工艺路线编码已存在: {dto.RouteCode}");
        }

        // 创建工艺路线实体
        var processRoute = new ProcessRoute
        {
          Id = Guid.NewGuid(),
          RouteName = dto.RouteName,
          RouteCode = dto.RouteCode,
          Status = dto.Status,
          Remark = dto.Remark,
          CreateTime = DateTimeOffset.Now
        };

        // 保存工艺路线
        var result = await _processRouteRepo.InsertAsync(processRoute);

        if (result != null)
        {
          _logger.LogInformation("成功创建工艺路线，ID: {RouteId}", result.Id);
          return ToDto(result);
        }
        else
        {
          _logger.LogError("创建工艺路线失败");
          throw new InvalidOperationException("创建工艺路线失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工艺路线时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新工艺路线
    /// </summary>
    public async Task<ProcessRouteDto> UpdateProcessRouteAsync(ProcessRouteUpdateDto dto)
    {
      try
      {
        // 验证Status值
        ValidateStatus(dto.Status);

        // 查找现有工艺路线
        var existingProcessRoute = await _processRouteRepo.GetAsync(p => p.Id == dto.Id);
        if (existingProcessRoute == null)
          throw new InvalidOperationException($"工艺路线不存在，ID: {dto.Id}");

        // 检查工艺路线编码唯一性（排除当前工艺路线）
        if (dto.RouteCode != existingProcessRoute.RouteCode)
        {
          var existingProcessRouteWithSameCode = await _processRouteRepo.GetAsync(p => p.RouteCode == dto.RouteCode && p.Id != dto.Id);
          if (existingProcessRouteWithSameCode != null)
          {
            _logger.LogError("工艺路线编码已存在: {RouteCode}", dto.RouteCode);
            throw new InvalidOperationException($"工艺路线编码已存在: {dto.RouteCode}");
          }
        }

        // 更新工艺路线信息
        existingProcessRoute.RouteName = dto.RouteName;
        existingProcessRoute.RouteCode = dto.RouteCode;
        existingProcessRoute.Status = dto.Status;
        existingProcessRoute.Remark = dto.Remark;
        existingProcessRoute.UpdateTime = DateTimeOffset.Now;

        // 保存更新
        var result = await _processRouteRepo.UpdateAsync(existingProcessRoute);

        if (result != null)
        {
          _logger.LogInformation("成功更新工艺路线，ID: {RouteId}", result.Id);
          return ToDto(result);
        }
        else
        {
          _logger.LogError("更新工艺路线失败");
          throw new InvalidOperationException("更新工艺路线失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工艺路线时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 删除工艺路线（支持单一和批量删除）
    /// </summary>
    public async Task<int> DeleteProcessRoutesAsync(Guid[] routeIds)
    {
      if (routeIds == null || routeIds.Length == 0)
        return 0; // 没有要删除的工艺路线

      try
      {
        var processRoutes = await _dbContext.ProcessRoutes.Where(p => routeIds.Contains(p.Id)).ToListAsync();
        if (processRoutes.Count == 0)
        {
          return 0;
        }

        _dbContext.ProcessRoutes.RemoveRange(processRoutes);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("成功删除工艺路线，共删除 {Count} 个", processRoutes.Count);
        return processRoutes.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工艺路线时发生异常");
        throw;
      }
    }
  }
}