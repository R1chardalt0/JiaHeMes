using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Service.Trace.Dto.ProcessRoute;
using ChargePadLine.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 工艺路线工位测试服务实现
  /// </summary>
  public class ProcessRouteItemTestService : IProcessRouteItemTestService
  {
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProcessRouteItemTestService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="logger">日志记录器</param>
    public ProcessRouteItemTestService(AppDbContext dbContext, ILogger<ProcessRouteItemTestService> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    /// <summary>
    /// 创建工艺路线工位测试
    /// </summary>
    /// <param name="dto">测试创建数据</param>
    /// <returns>创建的测试信息</returns>
    public async Task<ProcessRouteItemTestDto> CreateAsync(ProcessRouteItemTestCreateDto dto)
    {
      try
      {
        var test = new ProcessRouteItemTest
        {
          ProRouteItemStationTestId = Guid.NewGuid(),
          ProcessRouteItemId = dto.ProcessRouteItemId,
          UpperLimit = dto.UpperLimit,
          LowerLimit = dto.LowerLimit,
          Units = dto.Units,
          ParametricKey = dto.ParametricKey,
          Remark = dto.Remark,
          IsCheck = dto.IsCheck,
          CreateTime = DateTimeOffset.Now
        };

        _dbContext.ProcessRouteItemTest.Add(test);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功创建工艺路线工位测试，ID: {test.ProRouteItemStationTestId}");
        return MapToDto(test);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工艺路线工位测试时发生异常");
        throw new InvalidOperationException($"创建工艺路线工位测试失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 更新工艺路线工位测试
    /// </summary>
    /// <param name="dto">测试更新数据</param>
    /// <returns>更新后的测试信息</returns>
    public async Task<ProcessRouteItemTestDto> UpdateAsync(ProcessRouteItemTestUpdateDto dto)
    {
      try
      {
        var test = await _dbContext.ProcessRouteItemTest.FindAsync(dto.ProRouteItemStationTestId);
        if (test == null)
        {
          throw new InvalidOperationException($"工艺路线工位测试不存在，ID: {dto.ProRouteItemStationTestId}");
        }

        test.ProcessRouteItemId = dto.ProcessRouteItemId;
        test.UpperLimit = dto.UpperLimit;
        test.LowerLimit = dto.LowerLimit;
        test.Units = dto.Units;
        test.ParametricKey = dto.ParametricKey;
        test.Remark = dto.Remark;
        test.IsCheck = dto.IsCheck;
        test.UpdateTime = DateTimeOffset.Now;

        _dbContext.ProcessRouteItemTest.Update(test);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功更新工艺路线工位测试，ID: {test.ProRouteItemStationTestId}");
        return MapToDto(test);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新工艺路线工位测试时发生异常，ID: {ProRouteItemStationTestId}", dto.ProRouteItemStationTestId);
        // 如果是我们自定义的异常，直接抛出
        if (ex is InvalidOperationException && ex.Message.Contains("工艺路线工位测试不存在"))
        {
          throw;
        }
        throw new InvalidOperationException($"更新工艺路线工位测试失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 获取工艺路线工位测试详情
    /// </summary>
    /// <param name="testId">测试ID</param>
    /// <returns>测试详情</returns>
    public async Task<ProcessRouteItemTestDto> GetByIdAsync(Guid testId)
    {
      try
      {
        var test = await _dbContext.ProcessRouteItemTest.FindAsync(testId);
        if (test == null)
        {
          throw new InvalidOperationException($"工艺路线工位测试不存在，ID: {testId}");
        }

        return MapToDto(test);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线工位测试详情时发生异常，ID: {ProRouteItemStationTestId}", testId);
        // 如果是我们自定义的异常，直接抛出
        if (ex is InvalidOperationException && ex.Message.Contains("工艺路线工位测试不存在"))
        {
          throw;
        }
        throw new InvalidOperationException($"获取工艺路线工位测试详情失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 获取工艺路线工位测试列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    public async Task<(int Total, List<ProcessRouteItemTestDto> Items)> GetListAsync(ProcessRouteItemTestQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        var query = _dbContext.ProcessRouteItemTest.AsQueryable();

        // 应用查询条件
        if (queryDto.ProRouteItemStationTestId.HasValue)
        {
          query = query.Where(t => t.ProRouteItemStationTestId == queryDto.ProRouteItemStationTestId.Value);
        }

        if (queryDto.ProcessRouteItemId.HasValue)
        {
          query = query.Where(t => t.ProcessRouteItemId == queryDto.ProcessRouteItemId.Value);
        }

        if (!string.IsNullOrEmpty(queryDto.ParametricKey))
        {
          query = query.Where(t => t.ParametricKey.Contains(queryDto.ParametricKey));
        }

        if (queryDto.IsCheck.HasValue)
        {
          query = query.Where(t => t.IsCheck == queryDto.IsCheck.Value);
        }

        // 计算总数
        var total = await query.CountAsync();

        // 应用分页并排序
        var items = await query
            .OrderByDescending(t => t.CreateTime)
            .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        return (
            total,
            items.Select(MapToDto).ToList()
        );
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "获取工艺路线工位测试列表时发生异常");
        throw new InvalidOperationException($"获取工艺路线工位测试列表失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 分页查询工艺路线工位测试列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    public async Task<ChargePadLine.Service.PaginatedList<ProcessRouteItemTestDto>> PaginationAsync(ProcessRouteItemTestQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        var query = _dbContext.ProcessRouteItemTest.AsQueryable();

        // 应用查询条件
        if (queryDto.ProRouteItemStationTestId.HasValue)
        {
          query = query.Where(t => t.ProRouteItemStationTestId == queryDto.ProRouteItemStationTestId.Value);
        }

        if (queryDto.ProcessRouteItemId.HasValue)
        {
          query = query.Where(t => t.ProcessRouteItemId == queryDto.ProcessRouteItemId.Value);
        }

        if (!string.IsNullOrEmpty(queryDto.ParametricKey))
        {
          query = query.Where(t => t.ParametricKey.Contains(queryDto.ParametricKey));
        }

        if (queryDto.IsCheck.HasValue)
        {
          query = query.Where(t => t.IsCheck == queryDto.IsCheck.Value);
        }

        // 计算总数
        var total = await query.CountAsync();

        // 应用分页并排序
        var testItems = await query
            .OrderByDescending(t => t.CreateTime)
            .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        var items = testItems.Select(MapToDto).ToList();

        return new ChargePadLine.Service.PaginatedList<ProcessRouteItemTestDto>(items, total, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "分页查询工艺路线工位测试列表时发生异常");
        throw new InvalidOperationException($"分页查询工艺路线工位测试列表失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 根据工艺路线明细ID获取测试列表
    /// </summary>
    /// <param name="processRouteItemId">工艺路线明细ID</param>
    /// <returns>测试列表</returns>
    public async Task<List<ProcessRouteItemTestDto>> GetByProcessRouteItemIdAsync(Guid processRouteItemId)
    {
      try
      {
        var tests = await _dbContext.ProcessRouteItemTest
            .Where(t => t.ProcessRouteItemId == processRouteItemId)
            .ToListAsync();

        return tests.Select(MapToDto).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据工艺路线明细ID获取测试列表时发生异常，工艺路线明细ID: {ProcessRouteItemId}", processRouteItemId);
        throw new InvalidOperationException($"根据工艺路线明细ID获取测试列表失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 删除工艺路线工位测试（支持单一和批量删除）
    /// </summary>
    /// <param name="testIds">测试ID数组</param>
    /// <returns>删除成功的数量</returns>
    public async Task<int> DeleteAsync(Guid[] testIds)
    {
      if (testIds == null || testIds.Length == 0)
        return 0; // 没有要删除的测试项

      try
      {
        var tests = await _dbContext.ProcessRouteItemTest.Where(t => testIds.Contains(t.ProRouteItemStationTestId)).ToListAsync();
        if (tests.Count == 0)
        {
          return 0;
        }

        _dbContext.ProcessRouteItemTest.RemoveRange(tests);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功删除工艺路线工位测试，共删除 {tests.Count} 个");
        return tests.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工艺路线工位测试时发生异常");
        // 提供更详细的错误信息
        if (ex.Message.Contains("foreign key") || ex.Message.Contains("外键") || ex.Message.Contains("ForeignKey"))
        {
          throw new InvalidOperationException("删除工艺路线工位测试失败：存在关联的记录，无法删除", ex);
        }
        throw new InvalidOperationException($"删除工艺路线工位测试失败：{ex.Message}", ex);
      }
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    /// <param name="test">工艺路线工位测试实体</param>
    /// <returns>工艺路线工位测试DTO</returns>
    private ProcessRouteItemTestDto MapToDto(ProcessRouteItemTest test)
    {
      return new ProcessRouteItemTestDto
      {
        ProRouteItemStationTestId = test.ProRouteItemStationTestId,
        ProcessRouteItemId = test.ProcessRouteItemId,
        UpperLimit = test.UpperLimit,
        LowerLimit = test.LowerLimit,
        Units = test.Units,
        ParametricKey = test.ParametricKey,
        CreateBy = test.CreateBy,
        CreateTime = test.CreateTime,
        UpdateBy = test.UpdateBy,
        UpdateTime = test.UpdateTime,
        Remark = test.Remark,
        IsCheck = test.IsCheck
      };
    }
  }
}