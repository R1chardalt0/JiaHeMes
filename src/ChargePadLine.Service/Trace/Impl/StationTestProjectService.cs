using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Service.Trace.Dto.Station;
using ChargePadLine.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 站点测试项服务实现
  /// </summary>
  public class StationTestProjectService : IStationTestProjectService
  {
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StationTestProjectService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="logger">日志记录器</param>
    public StationTestProjectService(AppDbContext dbContext, ILogger<StationTestProjectService> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    /// <summary>
    /// 创建站点测试项
    /// </summary>
    /// <param name="dto">测试项创建数据</param>
    /// <returns>创建的测试项信息</returns>
    public async Task<StationTestProjectDto> CreateAsync(StationTestProjectCreateDto dto)
    {
      try
      {
        var testProject = new StationTestProject
        {
          StationTestProjectId = Guid.NewGuid(),
          StationId = dto.StationId,
          UpperLimit = dto.UpperLimit,
          LowerLimit = dto.LowerLimit,
          Units = dto.Units,
          ParametricKey = dto.ParametricKey,
          SearchValue = dto.SearchValue,
          Remark = dto.Remark,
          IsCheck = dto.IsCheck,
          CreateTime = DateTimeOffset.Now
        };

        _dbContext.StationTestProject.Add(testProject);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功创建站点测试项，ID: {testProject.StationTestProjectId}");
        return MapToDto(testProject);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建站点测试项时发生异常");
        throw new InvalidOperationException($"创建站点测试项失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 更新站点测试项
    /// </summary>
    /// <param name="dto">测试项更新数据</param>
    /// <returns>更新后的测试项信息</returns>
    public async Task<StationTestProjectDto> UpdateAsync(StationTestProjectUpdateDto dto)
    {
      try
      {
        var testProject = await _dbContext.StationTestProject.FindAsync(dto.StationTestProjectId);
        if (testProject == null)
        {
          throw new InvalidOperationException($"测试项不存在，ID: {dto.StationTestProjectId}");
        }

        testProject.StationId = dto.StationId;
        testProject.UpperLimit = dto.UpperLimit;
        testProject.LowerLimit = dto.LowerLimit;
        testProject.Units = dto.Units;
        testProject.ParametricKey = dto.ParametricKey;
        testProject.SearchValue = dto.SearchValue;
        testProject.Remark = dto.Remark;
        testProject.IsCheck = dto.IsCheck;
        testProject.UpdateTime = DateTimeOffset.Now;

        _dbContext.StationTestProject.Update(testProject);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功更新站点测试项，ID: {testProject.StationTestProjectId}");
        return MapToDto(testProject);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新站点测试项时发生异常，ID: {StationTestProjectId}", dto.StationTestProjectId);
        // 如果是我们自定义的异常，直接抛出
        if (ex is InvalidOperationException && ex.Message.Contains("测试项不存在"))
        {
          throw;
        }
        throw new InvalidOperationException($"更新站点测试项失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 获取站点测试项详情
    /// </summary>
    /// <param name="testProjectId">测试项ID</param>
    /// <returns>测试项详情</returns>
    public async Task<StationTestProjectDto> GetByIdAsync(Guid testProjectId)
    {
      try
      {
        var testProject = await _dbContext.StationTestProject.FindAsync(testProjectId);
        if (testProject == null)
        {
          throw new InvalidOperationException($"测试项不存在，ID: {testProjectId}");
        }

        return MapToDto(testProject);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取站点测试项详情时发生异常，ID: {StationTestProjectId}", testProjectId);
        // 如果是我们自定义的异常，直接抛出
        if (ex is InvalidOperationException && ex.Message.Contains("测试项不存在"))
        {
          throw;
        }
        throw new InvalidOperationException($"获取站点测试项详情失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 获取站点测试项列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    public async Task<(int Total, List<StationTestProjectDto> Items)> GetListAsync(StationTestProjectQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.Current < 1) queryDto.Current = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        var query = _dbContext.StationTestProject.AsQueryable();

        // 应用查询条件
        if (queryDto.StationId.HasValue)
        {
          query = query.Where(t => t.StationId == queryDto.StationId.Value);
        }

        if (!string.IsNullOrEmpty(queryDto.ParametricKey))
        {
          query = query.Where(t => t.ParametricKey.Contains(queryDto.ParametricKey));
        }

        if (!string.IsNullOrEmpty(queryDto.SearchValue))
        {
          query = query.Where(t => t.SearchValue.Contains(queryDto.SearchValue));
        }

        if (queryDto.StartTime.HasValue)
        {
          query = query.Where(t => t.CreateTime >= queryDto.StartTime.Value);
        }

        if (queryDto.EndTime.HasValue)
        {
          query = query.Where(t => t.CreateTime <= queryDto.EndTime.Value);
        }

        // 计算总数
        var total = await query.CountAsync();

        // 应用分页并排序
        var items = await query
            .OrderByDescending(t => t.CreateTime)
            .Skip((queryDto.Current - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        return (
            total,
            items.Select(MapToDto).ToList()
        );
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取站点测试项列表时发生异常");
        throw new InvalidOperationException($"获取站点测试项列表失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 分页查询站点测试项列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    public async Task<PaginatedList<StationTestProjectDto>> PaginationAsync(StationTestProjectQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.Current < 1) queryDto.Current = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        var query = _dbContext.StationTestProject.AsQueryable();

        // 应用查询条件
        if (queryDto.StationId.HasValue)
        {
          query = query.Where(t => t.StationId == queryDto.StationId.Value);
        }

        if (!string.IsNullOrEmpty(queryDto.ParametricKey))
        {
          query = query.Where(t => t.ParametricKey.Contains(queryDto.ParametricKey));
        }

        if (!string.IsNullOrEmpty(queryDto.SearchValue))
        {
          query = query.Where(t => t.SearchValue.Contains(queryDto.SearchValue));
        }

        if (queryDto.StartTime.HasValue)
        {
          query = query.Where(t => t.CreateTime >= queryDto.StartTime.Value);
        }

        if (queryDto.EndTime.HasValue)
        {
          query = query.Where(t => t.CreateTime <= queryDto.EndTime.Value);
        }

        // 计算总数
        var total = await query.CountAsync();

        // 应用分页并排序
        var testProjectItems = await query
            .OrderByDescending(t => t.CreateTime)
            .Skip((queryDto.Current - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        var items = testProjectItems.Select(MapToDto).ToList();

        return new PaginatedList<StationTestProjectDto>(items, total, queryDto.Current, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "分页查询站点测试项列表时发生异常");
        throw new InvalidOperationException($"分页查询站点测试项列表失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 根据站点ID获取测试项列表
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>测试项列表</returns>
    public async Task<List<StationTestProjectDto>> GetByStationIdAsync(Guid stationId)
    {
      try
      {
        var testProjects = await _dbContext.StationTestProject
            .Where(t => t.StationId == stationId)
            .ToListAsync();

        return testProjects.Select(MapToDto).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据站点ID获取测试项列表时发生异常，站点ID: {StationId}", stationId);
        throw new InvalidOperationException($"根据站点ID获取测试项列表失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 创建站点测试项
    /// </summary>
    /// <param name="dto">测试项信息</param>
    /// <returns>创建的测试项信息</returns>
    public async Task<StationTestProjectDto> CreateStationTestProject(StationTestProjectDto dto)
    {
      try
      {
        var testProject = new StationTestProject
        {
          StationTestProjectId = dto.StationTestProjectId == Guid.Empty ? Guid.NewGuid() : dto.StationTestProjectId,
          StationId = dto.StationId,
          UpperLimit = dto.UpperLimit,
          LowerLimit = dto.LowerLimit,
          Units = dto.Units,
          ParametricKey = dto.ParametricKey,
          SearchValue = dto.SearchValue,
          Remark = dto.Remark,
          IsCheck = dto.IsCheck,
          CreateTime = DateTimeOffset.Now
        };

        _dbContext.StationTestProject.Add(testProject);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功创建站点测试项，ID: {testProject.StationTestProjectId}");
        return MapToDto(testProject);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建站点测试项时发生异常");
        throw new InvalidOperationException($"创建站点测试项失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 更新站点测试项
    /// </summary>
    /// <param name="dto">测试项信息</param>
    /// <returns>更新后的测试项信息</returns>
    public async Task<StationTestProjectDto> UpdateStationTestProject(StationTestProjectDto dto)
    {
      try
      {
        var testProject = await _dbContext.StationTestProject.FindAsync(dto.StationTestProjectId);
        if (testProject == null)
        {
          throw new InvalidOperationException($"测试项不存在，ID: {dto.StationTestProjectId}");
        }

        testProject.StationId = dto.StationId;
        testProject.UpperLimit = dto.UpperLimit;
        testProject.LowerLimit = dto.LowerLimit;
        testProject.Units = dto.Units;
        testProject.ParametricKey = dto.ParametricKey;
        testProject.SearchValue = dto.SearchValue;
        testProject.Remark = dto.Remark;
        testProject.IsCheck = dto.IsCheck;
        testProject.UpdateTime = DateTimeOffset.Now;

        _dbContext.StationTestProject.Update(testProject);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功更新站点测试项，ID: {testProject.StationTestProjectId}");
        return MapToDto(testProject);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新站点测试项时发生异常，ID: {StationTestProjectId}", dto.StationTestProjectId);
        // 如果是我们自定义的异常，直接抛出
        if (ex is InvalidOperationException && ex.Message.Contains("测试项不存在"))
        {
          throw;
        }
        throw new InvalidOperationException($"更新站点测试项失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 删除站点测试项（支持单一和批量删除）
    /// </summary>
    /// <param name="testProjectIds">测试项ID数组</param>
    /// <returns>删除成功的数量</returns>
    public async Task<int> DeleteStationTestProjectsAsync(Guid[] testProjectIds)
    {
      if (testProjectIds == null || testProjectIds.Length == 0)
        return 0; // 没有要删除的测试项

      try
      {
        var testProjects = await _dbContext.StationTestProject.Where(t => testProjectIds.Contains(t.StationTestProjectId)).ToListAsync();
        if (testProjects.Count == 0)
        {
          return 0;
        }

        _dbContext.StationTestProject.RemoveRange(testProjects);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功删除站点测试项，共删除 {testProjects.Count} 个");
        return testProjects.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除站点测试项时发生异常");
        // 提供更详细的错误信息
        if (ex.Message.Contains("foreign key") || ex.Message.Contains("外键") || ex.Message.Contains("ForeignKey"))
        {
          throw new InvalidOperationException("删除站点测试项失败：存在关联的记录，无法删除", ex);
        }
        throw new InvalidOperationException($"删除站点测试项失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    /// <param name="testProject">测试项实体</param>
    /// <returns>测试项DTO</returns>
    private StationTestProjectDto MapToDto(StationTestProject testProject)
    {
      return new StationTestProjectDto
      {
        StationTestProjectId = testProject.StationTestProjectId,
        StationId = testProject.StationId,
        UpperLimit = testProject.UpperLimit,
        LowerLimit = testProject.LowerLimit,
        Units = testProject.Units,
        ParametricKey = testProject.ParametricKey,
        SearchValue = testProject.SearchValue,
        CreateBy = testProject.CreateBy,
        CreateTime = testProject.CreateTime,
        UpdateBy = testProject.UpdateBy,
        UpdateTime = testProject.UpdateTime,
        Remark = testProject.Remark,
        IsCheck = testProject.IsCheck
      };
    }
  }
}