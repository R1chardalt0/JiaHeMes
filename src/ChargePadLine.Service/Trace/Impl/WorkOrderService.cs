using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto;
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
  /// 工单服务实现
  /// </summary>
  public class WorkOrderService : IWorkOrderService
  {
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<BomRecipe> _bomRecipeRepo;
    private readonly IRepository<Material> _materialRepo;
    private readonly ILogger<WorkOrderService> _logger;
    private readonly AppDbContext _dbContext;

    public WorkOrderService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<BomRecipe> bomRecipeRepo,
        IRepository<Material> materialRepo,
        ILogger<WorkOrderService> logger,
        AppDbContext dbContext)
    {
      _workOrderRepo = workOrderRepo;
      _bomRecipeRepo = bomRecipeRepo;
      _materialRepo = materialRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询工单列表
    /// </summary>
    public async Task<PaginatedList<WorkOrderDto>> GetWorkOrdersAsync(WorkOrderQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.Current < 1) queryDto.Current = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _workOrderRepo.GetQueryable();

        // 工单编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.Code))
        {
          query = query.Where(w => w.Code.Value.Contains(queryDto.Code));
        }

        // 产品编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.ProductCode))
        {
          query = query.Where(w => w.ProductCode.Value.Contains(queryDto.ProductCode));
        }

        // 工单状态筛选
        if (queryDto.DocStatus.HasValue)
        {
          query = query.Where(w => w.DocStatus == queryDto.DocStatus.Value);
        }

        // 时间范围筛选
        // if (queryDto.StartTime.HasValue)
        // {
        //   query = query.Where(w => w.CreatedAt >= queryDto.StartTime.Value);
        // }

        // if (queryDto.EndTime.HasValue)
        // {
        //   query = query.Where(w => w.CreatedAt <= queryDto.EndTime.Value);
        // }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var workOrders = await query
          .Skip((queryDto.Current - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var workOrderDtos = workOrders.Select(w => w.ToDto()).ToList();

        // 返回分页结果
        return new PaginatedList<WorkOrderDto>(workOrderDtos, totalCount, queryDto.Current, queryDto.PageSize);
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
    public async Task<WorkOrderDto?> GetWorkOrderByIdAsync(int id)
    {
      var workOrder = await _workOrderRepo.GetQueryable()
          .Include(w => w.BomRecipe)
          .FirstOrDefaultAsync(w => w.Id == id);

      if (workOrder == null)
        return null;

      return workOrder.ToDto();
    }

    /// <summary>
    /// 根据工单编码获取工单详情
    /// </summary>
    public async Task<WorkOrderDto?> GetWorkOrderByCodeAsync(string code)
    {
      var workOrder = await _workOrderRepo.GetQueryable()
          .Include(w => w.BomRecipe)
          .FirstOrDefaultAsync(w => w.Code.Value == code);

      if (workOrder == null)
        return null;

      return workOrder.ToDto();
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    public async Task<WorkOrderDto> CreateWorkOrderAsync(CreateWorkOrderDto dto)
    {
      try
      {
        // 根据BomRecipeId获取BomRecipe
        var bomRecipe = await _bomRecipeRepo.GetAsync(r => r.Id == dto.BomRecipeId);
        if (bomRecipe == null)
        {
          _logger.LogError("BOM配方不存在，ID: {BomRecipeId}", dto.BomRecipeId);
          throw new InvalidOperationException($"BOM配方不存在，ID: {dto.BomRecipeId}");
        }

        // 验证工单编码唯一性
        var workOrderCode = new WorkOrderCode(dto.Code);
        var existingWorkOrder = await _workOrderRepo.GetAsync(w => w.Code.Value == dto.Code);
        if (existingWorkOrder != null)
        {
          _logger.LogError("工单编码已存在: {Code}", dto.Code);
          throw new InvalidOperationException($"工单编码已存在: {dto.Code}");
        }

        // 创建工单
        WorkOrder workOrder;
        if (dto.IsInfinite)
        {
          workOrder = WorkOrder.MakeInfiniteWorkOrder(workOrderCode, bomRecipe);
        }
        else
        {
          workOrder = WorkOrder.MakeWorkOrder(workOrderCode, bomRecipe, dto.WorkOrderAmount, dto.PerTraceInfo);
        }

        // 保存工单
        var result = await _workOrderRepo.InsertAsync(workOrder);

        if (result != null)
        {
          _logger.LogInformation($"成功创建工单，ID: {result.Id}");

          // 返回创建的工单信息 - 使用扩展方法
          return result.ToDto();
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
    public async Task<WorkOrderDto> UpdateWorkOrderAsync(UpdateWorkOrderDto dto)
    {
      try
      {
        // 查找现有工单
        var existingWorkOrder = await _workOrderRepo.GetAsync(w => w.Id == dto.Id);
        if (existingWorkOrder == null)
          throw new InvalidOperationException($"工单不存在，ID: {dto.Id}");


        // 检查BOM配方是否存在
        var bomRecipe = await _bomRecipeRepo.GetAsync(b => b.Id == dto.BomRecipeId);
        if (bomRecipe == null)
        {
          _logger.LogWarning("BOM配方不存在: {BomRecipeId}", dto.BomRecipeId);
          throw new InvalidOperationException($"BOM配方不存在，ID: {dto.BomRecipeId}");
        }

        // 更新工单信息
        existingWorkOrder.BomRecipeId = dto.BomRecipeId;
        existingWorkOrder.IsInfinite = dto.IsInfinite;
        existingWorkOrder.PerTraceInfo = dto.PerTraceInfo;

        // 更新工单编码（如果提供了新的编码）
        if (!string.IsNullOrEmpty(dto.Code))
        {
          existingWorkOrder.Code = dto.Code;
        }

        // 根据是否无限生产设置WorkOrderAmount
        if (dto.IsInfinite)
        {
          existingWorkOrder.SetWorkOrderAmount(new WorkOrderAmount.Infinity());
        }
        else
        {
          existingWorkOrder.SetWorkOrderAmount(new WorkOrderAmount.Quota(dto.WorkOrderAmount));
        }

        // 更新工单
        var result = await _workOrderRepo.UpdateAsync(existingWorkOrder);

        if (result != null)
        {
          _logger.LogInformation($"成功更新工单，ID: {result.Id}");

          // 返回更新后的工单信息 - 使用扩展方法
          return result.ToDto();
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
    /// 删除工单
    /// </summary>
    public async Task<bool> DeleteWorkOrderAsync(int id)
    {
      try
      {
        // 查找工单
        var workOrder = await _workOrderRepo.GetAsync(w => w.Id == id);
        if (workOrder == null)
          return false; // 工单不存在

        // 开始事务
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
          // 真正删除工单记录
          await _workOrderRepo.DeleteAsync(workOrder);

          // 提交事务
          await transaction.CommitAsync();

          _logger.LogInformation($"成功删除工单，ID: {workOrder.Id}");
          return true; // 删除成功
        }
        catch
        {
          // 事务会在using块结束时自动回滚
          throw;
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工单时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 批量删除工单
    /// </summary>
    public async Task<int> DeleteWorkOrdersAsync(int[] ids)
    {
      if (ids == null || ids.Length == 0)
        return 0; // 没有要删除的工单

      try
      {
        // 开始事务
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
          // 查询所有需要删除的工单
          var workOrders = await _workOrderRepo.GetQueryable()
              .Where(w => ids.Contains(w.Id))
              .ToListAsync();

          var workOrdersToDelete = new List<WorkOrder>();

          foreach (var id in ids)
          {
            var workOrder = workOrders.FirstOrDefault(w => w.Id == id);
            if (workOrder != null)
            {
              workOrdersToDelete.Add(workOrder);
            }
            else
            {
              _logger.LogWarning("工单不存在: {Id}", id);
            }
          }

          // 批量删除工单
          foreach (var workOrder in workOrdersToDelete)
          {
            await _workOrderRepo.DeleteAsync(workOrder);
          }

          var deletedCount = workOrdersToDelete.Count;

          // 提交事务
          await transaction.CommitAsync();

          _logger.LogInformation($"成功批量删除工单，共删除 {deletedCount} 个");
          return deletedCount; // 返回实际删除数量
        }
        catch
        {
          // 事务会在using块结束时自动回滚
          throw;
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "批量删除工单时发生异常");
        throw;
      }
    }
  }
}