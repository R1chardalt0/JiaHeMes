using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.Order;
using ChargePadLine.Service.Trace.Dto.Order;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// MES工单BOM批次明细服务实现
  /// </summary>
  public class MESOrderBomBatchItemService : IMESOrderBomBatchItemService
  {
    private readonly IRepository<MesOrderBomBatchItem> _repository;
    private readonly AppDbContext _context;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="repository">仓库</param>
    /// <param name="context">数据库上下文</param>
    public MESOrderBomBatchItemService(IRepository<MesOrderBomBatchItem> repository, AppDbContext context)
    {
      _repository = repository;
      _context = context;
    }

    /// <summary>
    /// 根据ID查询工单BOM批次明细
    /// </summary>
    /// <param name="id">工单BOM批次明细ID</param>
    /// <returns>工单BOM批次明细数据传输对象</returns>
    public async Task<MesOrderBomBatchItemDto> GetByIdAsync(Guid id)
    {
      var entity = await _context.Set<MesOrderBomBatchItem>()
          .Include(e => e.OrderBomBatch)
          .FirstOrDefaultAsync(e => e.OrderBomBatchItemId == id);
      if (entity == null)
      {
        return null;
      }

      return MapToDto(entity);
    }

    /// <summary>
    /// 查询工单BOM批次明细列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>工单BOM批次明细数据传输对象列表</returns>
    public async Task<List<MesOrderBomBatchItemDto>> GetListAsync(MESOrderBomBatchItemQueryDto queryDto)
    {
      var query = BuildQuery(queryDto);
      var entities = await query.ToListAsync();
      return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 分页查询工单BOM批次明细
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页结果，包含工单BOM批次明细数据传输对象列表和总记录数</returns>
    public async Task<(List<MesOrderBomBatchItemDto> Data, int Total)> GetPagedListAsync(MESOrderBomBatchItemQueryDto queryDto)
    {
      var query = BuildQuery(queryDto);
      var total = await query.CountAsync();

      var entities = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

      var data = entities.Select(MapToDto).ToList();
      return (data, total);
    }

    /// <summary>
    /// 根据工单BOM批次ID查询明细列表
    /// </summary>
    /// <param name="orderBomBatchId">工单BOM批次ID</param>
    /// <returns>工单BOM批次明细数据传输对象列表</returns>
    public async Task<List<MesOrderBomBatchItemDto>> GetByOrderBomBatchIdAsync(Guid orderBomBatchId)
    {
      var entities = await _context.Set<MesOrderBomBatchItem>()
          .Include(e => e.OrderBomBatch)
          .Where(e => e.OrderBomBatchId == orderBomBatchId)
          .ToListAsync();
      return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 根据SN编码查询工单BOM批次明细
    /// </summary>
    /// <param name="snNumber">SN编码</param>
    /// <returns>工单BOM批次明细数据传输对象</returns>
    public async Task<MesOrderBomBatchItemDto> GetBySnNumberAsync(string snNumber)
    {
      var entity = await _context.Set<MesOrderBomBatchItem>()
          .Include(e => e.OrderBomBatch)
          .FirstOrDefaultAsync(e => e.SnNumber == snNumber);
      if (entity == null)
      {
        return null;
      }

      return MapToDto(entity);
    }

    /// <summary>
    /// 构建查询
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>查询对象</returns>
    private IQueryable<MesOrderBomBatchItem> BuildQuery(MESOrderBomBatchItemQueryDto queryDto)
    {
      var query = _context.Set<MesOrderBomBatchItem>()
          .Include(e => e.OrderBomBatch)
          .AsQueryable();

      // 根据ID查询
      if (queryDto.OrderBomBatchItemId != Guid.Empty)
      {
        query = query.Where(e => e.OrderBomBatchItemId == queryDto.OrderBomBatchItemId);
      }

      // 根据工单BOM批次ID查询
      if (queryDto.OrderBomBatchId != Guid.Empty)
      {
        query = query.Where(e => e.OrderBomBatchId == queryDto.OrderBomBatchId);
      }

      // 根据SN编码查询
      if (!string.IsNullOrEmpty(queryDto.SnNumber))
      {
        query = query.Where(e => e.SnNumber.Contains(queryDto.SnNumber));
      }

      // 根据搜索值查询
      if (!string.IsNullOrEmpty(queryDto.SearchValue))
      {
        query = query.Where(e =>
            e.SnNumber.Contains(queryDto.SearchValue)
        );
      }

      // 根据是否解绑查询
      if (queryDto.IsUnbind.HasValue)
      {
        query = query.Where(e => e.IsUnbind == queryDto.IsUnbind);
      }

      // 排序
      if (!string.IsNullOrEmpty(queryDto.SortField))
      {
        if (queryDto.SortOrder == "desc")
        {
          query = queryDto.SortField switch
          {
            "OrderBomBatchItemId" => query.OrderByDescending(e => e.OrderBomBatchItemId),
            "SnNumber" => query.OrderByDescending(e => e.SnNumber),
            "CreateTime" => query.OrderByDescending(e => e.CreateTime),
            "UpdateTime" => query.OrderByDescending(e => e.UpdateTime),
            _ => query.OrderByDescending(e => e.OrderBomBatchItemId)
          };
        }
        else
        {
          query = queryDto.SortField switch
          {
            "OrderBomBatchItemId" => query.OrderBy(e => e.OrderBomBatchItemId),
            "SnNumber" => query.OrderBy(e => e.SnNumber),
            "CreateTime" => query.OrderBy(e => e.CreateTime),
            "UpdateTime" => query.OrderBy(e => e.UpdateTime),
            _ => query.OrderBy(e => e.OrderBomBatchItemId)
          };
        }
      }
      else
      {
        // 默认按ID降序排序
        query = query.OrderByDescending(e => e.OrderBomBatchItemId);
      }

      return query;
    }

    /// <summary>
    /// 将实体映射到数据传输对象
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>数据传输对象</returns>
    private MesOrderBomBatchItemDto MapToDto(MesOrderBomBatchItem entity)
    {
      var dto = new MesOrderBomBatchItemDto
      {
        OrderBomBatchItemId = entity.OrderBomBatchItemId,
        OrderBomBatchId = entity.OrderBomBatchId,
        SnNumber = entity.SnNumber,
        SearchValue = entity.SearchValue,
        CreateBy = entity.CreateBy,
        CreateTime = entity.CreateTime,
        UpdateBy = entity.UpdateBy,
        UpdateTime = entity.UpdateTime,
        Remark = entity.Remark,
        IsUnbind = entity.IsUnbind
      };

      // 映射关联的工单BOM批次信息
      if (entity.OrderBomBatch != null)
      {
        dto.MesOrderBomBatch = new MESOrderBomBatchDto
        {
          OrderBomBatchId = entity.OrderBomBatch.OrderBomBatchId,
          ProductListId = entity.OrderBomBatch.ProductListId,
          BatchCode = entity.OrderBomBatch.BatchCode,
          StationListId = entity.OrderBomBatch.StationListId,
          OrderBomBatchStatus = entity.OrderBomBatch.OrderBomBatchStatus,
          BatchQty = entity.OrderBomBatch.BatchQty,
          CompletedQty = entity.OrderBomBatch.CompletedQty,
          OrderListId = entity.OrderBomBatch.OrderListId
        };
      }

      return dto;
    }
  }
}