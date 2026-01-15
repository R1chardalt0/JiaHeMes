using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.MesSnList;
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
  /// SN实时状态服务实现
  /// </summary>
  public class MesSnListCurrentService : IMesSnListCurrentService
  {
    private readonly IRepository<MesSnListCurrent> _mesSnListCurrentRepo;
    private readonly ILogger<MesSnListCurrentService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mesSnListCurrentRepo">SN实时状态仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public MesSnListCurrentService(
        IRepository<MesSnListCurrent> mesSnListCurrentRepo,
        ILogger<MesSnListCurrentService> logger,
        AppDbContext dbContext)
    {
      _mesSnListCurrentRepo = mesSnListCurrentRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询SN实时状态列表
    /// </summary>
    public async Task<PaginatedList<MesSnListCurrentDto>> GetMesSnListCurrentsAsync(MesSnListCurrentQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _mesSnListCurrentRepo.GetQueryable();

        // 序列号模糊匹配
        if (!string.IsNullOrEmpty(queryDto.SnNumber))
        {
          query = query.Where(p => p.SnNumber.Contains(queryDto.SnNumber));
        }

        // 产品ID精确匹配
        if (queryDto.ProductListId.HasValue)
        {
          query = query.Where(p => p.ProductListId == queryDto.ProductListId.Value);
        }

        // 工单ID精确匹配
        if (queryDto.OrderListId.HasValue)
        {
          query = query.Where(p => p.OrderListId == queryDto.OrderListId.Value);
        }

        // 站点状态精确匹配
        if (queryDto.StationStatus.HasValue)
        {
          query = query.Where(p => p.StationStatus == queryDto.StationStatus.Value);
        }

        // 当前站点ID精确匹配
        if (queryDto.CurrentStationListId.HasValue)
        {
          query = query.Where(p => p.CurrentStationListId == queryDto.CurrentStationListId.Value);
        }

        // 线体ID精确匹配
        if (queryDto.ProductionLineId.HasValue)
        {
          query = query.Where(p => p.ProductionLineId == queryDto.ProductionLineId.Value);
        }

        // 当前设备ID精确匹配
        if (queryDto.ResourceId.HasValue)
        {
          query = query.Where(p => p.ResourceId == queryDto.ResourceId.Value);
        }

        // 是否异常精确匹配
        if (queryDto.IsAbnormal.HasValue)
        {
          query = query.Where(p => p.IsAbnormal == queryDto.IsAbnormal.Value);
        }

        // 是否锁定精确匹配
        if (queryDto.IsLocked.HasValue)
        {
          query = query.Where(p => p.IsLocked == queryDto.IsLocked.Value);
        }

        // 是否正在返工精确匹配
        if (queryDto.IsReworking.HasValue)
        {
          query = query.Where(p => p.IsReworking == queryDto.IsReworking.Value);
        }

        // 搜索值模糊匹配
        if (!string.IsNullOrEmpty(queryDto.SearchValue))
        {
          var searchValue = queryDto.SearchValue;
          query = query.Where(p =>
              p.SnNumber.Contains(searchValue) ||
              (p.AbnormalCode != null && p.AbnormalCode.Contains(searchValue)) ||
              (p.AbnormalDescription != null && p.AbnormalDescription.Contains(searchValue)) ||
              (p.ReworkReason != null && p.ReworkReason.Contains(searchValue))
          );
        }

        // 时间范围过滤
        if (queryDto.StartTime.HasValue)
        {
          query = query.Where(p => p.CreateTime >= queryDto.StartTime.Value);
        }

        if (queryDto.EndTime.HasValue)
        {
          query = query.Where(p => p.CreateTime <= queryDto.EndTime.Value);
        }

        // 返工时间范围过滤
        if (queryDto.ReworkStartTime.HasValue)
        {
          query = query.Where(p => p.ReworkTime >= queryDto.ReworkStartTime.Value);
        }

        if (queryDto.ReworkEndTime.HasValue)
        {
          query = query.Where(p => p.ReworkTime <= queryDto.ReworkEndTime.Value);
        }

        // 排序
        if (!string.IsNullOrEmpty(queryDto.SortField))
        {
          switch (queryDto.SortField)
          {
            case "SnNumber":
              query = queryDto.SortOrder == "asc" ? query.OrderBy(p => p.SnNumber) : query.OrderByDescending(p => p.SnNumber);
              break;
            case "StationStatus":
              query = queryDto.SortOrder == "asc" ? query.OrderBy(p => p.StationStatus) : query.OrderByDescending(p => p.StationStatus);
              break;
            case "CreateTime":
              query = queryDto.SortOrder == "asc" ? query.OrderBy(p => p.CreateTime) : query.OrderByDescending(p => p.CreateTime);
              break;
            default:
              // 默认按创建时间倒序
              query = query.OrderByDescending(p => p.CreateTime);
              break;
          }
        }
        else
        {
          // 默认按创建时间倒序
          query = query.OrderByDescending(p => p.CreateTime);
        }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var mesSnListCurrents = await query
            .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        // 转换为DTO
        var dtos = mesSnListCurrents.Select(p => ToDto(p)).ToList();

        // 返回分页结果
        return new PaginatedList<MesSnListCurrentDto>(dtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询SN实时状态列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据SNListCurrentId获取SN实时状态详情
    /// </summary>
    public async Task<MesSnListCurrentDto?> GetMesSnListCurrentByIdAsync(Guid sNListCurrentId)
    {
      try
      {
        var mesSnListCurrent = await _mesSnListCurrentRepo.GetQueryable()
            .FirstOrDefaultAsync(p => p.SNListCurrentId == sNListCurrentId);

        if (mesSnListCurrent == null)
          return null;

        // 转换为DTO
        return ToDto(mesSnListCurrent);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据ID查询SN实时状态详情时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据SnNumber获取SN实时状态详情
    /// </summary>
    public async Task<MesSnListCurrentDto?> GetMesSnListCurrentBySnNumberAsync(string snNumber)
    {
      try
      {
        if (string.IsNullOrEmpty(snNumber))
          return null;

        var mesSnListCurrent = await _mesSnListCurrentRepo.GetQueryable()
            .FirstOrDefaultAsync(p => p.SnNumber == snNumber);

        if (mesSnListCurrent == null)
          return null;

        // 转换为DTO
        return ToDto(mesSnListCurrent);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据序列号查询SN实时状态详情时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 将MesSnListCurrent实体转换为MesSnListCurrentDto
    /// </summary>
    private MesSnListCurrentDto ToDto(MesSnListCurrent entity)
    {
      return new MesSnListCurrentDto
      {
        SNListCurrentId = entity.SNListCurrentId,
        SnNumber = entity.SnNumber,
        ProductListId = entity.ProductListId,
        OrderListId = entity.OrderListId,
        StationStatus = entity.StationStatus,
        CurrentStationListId = entity.CurrentStationListId,
        ProductionLineId = entity.ProductionLineId,
        ResourceId = entity.ResourceId,
        IsAbnormal = entity.IsAbnormal,
        AbnormalCode = entity.AbnormalCode ?? "",
        AbnormalDescription = entity.AbnormalDescription ?? "",
        IsLocked = entity.IsLocked,
        ReworkCount = entity.ReworkCount,
        IsReworking = entity.IsReworking,
        ReworkReason = entity.ReworkReason ?? "",
        ReworkTime = entity.ReworkTime,
        SearchValue = entity.SearchValue,
        CreateBy = entity.CreateBy ?? "",
        CreateTime = entity.CreateTime,
        UpdateBy = entity.UpdateBy ?? "",
        UpdateTime = entity.UpdateTime,
        Remark = entity.Remark ?? ""
      };
    }
  }
}