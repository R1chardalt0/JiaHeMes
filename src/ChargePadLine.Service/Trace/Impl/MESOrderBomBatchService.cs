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
  /// MES工单BOM批次服务实现
  /// </summary>
  public class MESOrderBomBatchService : IMESOrderBomBatchService
  {
    private readonly IRepository<MesOrderBomBatch> _repository;
    private readonly AppDbContext _context;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="repository">仓库</param>
    /// <param name="context">数据库上下文</param>
    public MESOrderBomBatchService(IRepository<MesOrderBomBatch> repository, AppDbContext context)
    {
      _repository = repository;
      _context = context;
    }

    /// <summary>
    /// 根据ID查询工单BOM批次
    /// </summary>
    /// <param name="id">工单BOM批次ID</param>
    /// <returns>工单BOM批次数据传输对象</returns>
    public async Task<MESOrderBomBatchDto> GetByIdAsync(Guid id)
    {
      var entity = await _context.Set<MesOrderBomBatch>().FindAsync(id);
      if (entity == null)
      {
        return null;
      }

      return MapToDto(entity);
    }

    /// <summary>
    /// 查询工单BOM批次列表
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    public async Task<List<MESOrderBomBatchDto>> GetListAsync(MESOrderBomBatchQueryDto queryDto)
    {
      var query = BuildQuery(queryDto);
      var entities = await query.ToListAsync();
      return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 分页查询工单BOM批次
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>分页结果，包含工单BOM批次数据传输对象列表和总记录数</returns>
    public async Task<PaginatedList<MESOrderBomBatchDto>> GetPagedListAsync(MESOrderBomBatchQueryDto queryDto)
    {
      var query = BuildQuery(queryDto);
      var total = await query.CountAsync();

      var entities = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

      var data = entities.Select(MapToDto).ToList();
      return new PaginatedList<MESOrderBomBatchDto>(data, total, queryDto.PageIndex, queryDto.PageSize);
    }

    /// <summary>
    /// 根据工单ID查询工单BOM批次列表
    /// </summary>
    /// <param name="orderListId">工单ID</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    public async Task<List<MESOrderBomBatchDto>> GetByOrderListIdAsync(Guid orderListId)
    {
      var entities = await _context.Set<MesOrderBomBatch>()
          .Where(e => e.OrderListId == orderListId)
          .ToListAsync();
      return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 根据物料ID查询工单BOM批次列表
    /// </summary>
    /// <param name="productListId">物料ID</param>
    /// <returns>工单BOM批次数据传输对象列表</returns>
    public async Task<List<MESOrderBomBatchDto>> GetByProductListIdAsync(Guid productListId)
    {
      var entities = await _context.Set<MesOrderBomBatch>()
          .Where(e => e.ProductListId == productListId)
          .ToListAsync();
      return entities.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 构建查询
    /// </summary>
    /// <param name="queryDto">查询参数</param>
    /// <returns>查询对象</returns>
    private IQueryable<MesOrderBomBatch> BuildQuery(MESOrderBomBatchQueryDto queryDto)
    {
      var query = _context.Set<MesOrderBomBatch>().AsQueryable();

      // 根据ID查询
      if (queryDto.OrderBomBatchId != Guid.Empty)
      {
        query = query.Where(e => e.OrderBomBatchId == queryDto.OrderBomBatchId);
      }

      // 根据物料ID查询
      if (queryDto.ProductListId.HasValue)
      {
        query = query.Where(e => e.ProductListId == queryDto.ProductListId.Value);
      }

      // 根据批次编码查询
      if (!string.IsNullOrEmpty(queryDto.BatchCode))
      {
        query = query.Where(e => e.BatchCode.Contains(queryDto.BatchCode));
      }

      // 根据站点查询
      if (queryDto.StationListId.HasValue)
      {
        query = query.Where(e => e.StationListId == queryDto.StationListId.Value);
      }

      // 根据状态查询
      if (queryDto.OrderBomBatchStatus.HasValue)
      {
        query = query.Where(e => e.OrderBomBatchStatus == queryDto.OrderBomBatchStatus.Value);
      }

      // 根据工单ID查询
      if (queryDto.OrderListId.HasValue)
      {
        query = query.Where(e => e.OrderListId == queryDto.OrderListId.Value);
      }

      // 根据设备ID查询
      if (queryDto.ResourceId.HasValue)
      {
        query = query.Where(e => e.ResourceId == queryDto.ResourceId.Value);
      }

      // 根据搜索值查询
      if (!string.IsNullOrEmpty(queryDto.SearchValue))
      {
        query = query.Where(e =>
            e.BatchCode.Contains(queryDto.SearchValue) ||
            e.Remark.Contains(queryDto.SearchValue)
        );
      }

      // 排序
      if (!string.IsNullOrEmpty(queryDto.SortField))
      {
        if (queryDto.SortOrder == "desc")
        {
          query = queryDto.SortField switch
          {
            "OrderBomBatchId" => query.OrderByDescending(e => e.OrderBomBatchId),
            "BatchCode" => query.OrderByDescending(e => e.BatchCode),
            "OrderBomBatchStatus" => query.OrderByDescending(e => e.OrderBomBatchStatus),
            "BatchQty" => query.OrderByDescending(e => e.BatchQty),
            "CompletedQty" => query.OrderByDescending(e => e.CompletedQty),
            "CreateTime" => query.OrderByDescending(e => e.CreateTime),
            "UpdateTime" => query.OrderByDescending(e => e.UpdateTime),
            _ => query.OrderByDescending(e => e.OrderBomBatchId)
          };
        }
        else
        {
          query = queryDto.SortField switch
          {
            "OrderBomBatchId" => query.OrderBy(e => e.OrderBomBatchId),
            "BatchCode" => query.OrderBy(e => e.BatchCode),
            "OrderBomBatchStatus" => query.OrderBy(e => e.OrderBomBatchStatus),
            "BatchQty" => query.OrderBy(e => e.BatchQty),
            "CompletedQty" => query.OrderBy(e => e.CompletedQty),
            "CreateTime" => query.OrderBy(e => e.CreateTime),
            "UpdateTime" => query.OrderBy(e => e.UpdateTime),
            _ => query.OrderBy(e => e.OrderBomBatchId)
          };
        }
      }
      else
      {
        // 默认按ID降序排序
        query = query.OrderByDescending(e => e.OrderBomBatchId);
      }

      return query;
    }

    /// <summary>
    /// 将实体映射到数据传输对象
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>数据传输对象</returns>
    private MESOrderBomBatchDto MapToDto(MesOrderBomBatch entity)
    {
      return new MESOrderBomBatchDto
      {
        OrderBomBatchId = entity.OrderBomBatchId,
        ProductListId = entity.ProductListId,
        BatchCode = entity.BatchCode,
        StationListId = entity.StationListId,
        OrderBomBatchStatus = entity.OrderBomBatchStatus,
        BatchQty = entity.BatchQty,
        CompletedQty = entity.CompletedQty,
        OrderListId = entity.OrderListId,
        ResourceId = entity.ResourceId,
        SearchValue = entity.SearchValue,
        CreateBy = entity.CreateBy,
        CreateTime = entity.CreateTime,
        UpdateBy = entity.UpdateBy,
        UpdateTime = entity.UpdateTime,
        Remark = entity.Remark
      };
    }
  }
}
