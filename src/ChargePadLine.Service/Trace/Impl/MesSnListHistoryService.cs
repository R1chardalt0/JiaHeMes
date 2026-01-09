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
  /// SN历史记录服务实现
  /// </summary>
  public class MesSnListHistoryService : IMesSnListHistoryService
  {
    private readonly IRepository<MesSnListHistory> _mesSnListHistoryRepo;
    private readonly ILogger<MesSnListHistoryService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mesSnListHistoryRepo">SN历史记录仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public MesSnListHistoryService(
        IRepository<MesSnListHistory> mesSnListHistoryRepo,
        ILogger<MesSnListHistoryService> logger,
        AppDbContext dbContext)
    {
      _mesSnListHistoryRepo = mesSnListHistoryRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询SN历史记录列表
    /// </summary>
    public async Task<PaginatedList<MesSnListHistoryDto>> GetMesSnListHistoriesAsync(MesSnListHistoryQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _mesSnListHistoryRepo.GetQueryable();

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

        // 异常代码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.AbnormalCode))
        {
          query = query.Where(p => p.AbnormalCode != null && p.AbnormalCode.Contains(queryDto.AbnormalCode));
        }

        // 返工原因模糊匹配
        if (!string.IsNullOrEmpty(queryDto.ReworkReason))
        {
          query = query.Where(p => p.ReworkReason != null && p.ReworkReason.Contains(queryDto.ReworkReason));
        }

        // 创建人模糊匹配
        if (!string.IsNullOrEmpty(queryDto.CreateBy))
        {
          query = query.Where(p => p.CreateBy != null && p.CreateBy.Contains(queryDto.CreateBy));
        }

        // 更新人模糊匹配
        if (!string.IsNullOrEmpty(queryDto.UpdateBy))
        {
          query = query.Where(p => p.UpdateBy != null && p.UpdateBy.Contains(queryDto.UpdateBy));
        }

        // 创建时间范围过滤
        if (queryDto.CreateTimeStart.HasValue)
        {
          query = query.Where(p => p.CreateTime >= queryDto.CreateTimeStart.Value);
        }

        if (queryDto.CreateTimeEnd.HasValue)
        {
          query = query.Where(p => p.CreateTime <= queryDto.CreateTimeEnd.Value);
        }

        // 更新时间范围过滤
        if (queryDto.UpdateTimeStart.HasValue)
        {
          query = query.Where(p => p.UpdateTime >= queryDto.UpdateTimeStart.Value);
        }

        if (queryDto.UpdateTimeEnd.HasValue)
        {
          query = query.Where(p => p.UpdateTime <= queryDto.UpdateTimeEnd.Value);
        }

        // 搜索值模糊匹配
        if (!string.IsNullOrEmpty(queryDto.SearchValue))
        {
          var searchValue = queryDto.SearchValue;
          query = query.Where(p =>
              p.SnNumber.Contains(searchValue) ||
              (p.AbnormalCode != null && p.AbnormalCode.Contains(searchValue)) ||
              (p.AbnormalDescription != null && p.AbnormalDescription.Contains(searchValue)) ||
              (p.ReworkReason != null && p.ReworkReason.Contains(searchValue)) ||
              (p.TestData != null && p.TestData.Contains(searchValue)) ||
              (p.BatchResults != null && p.BatchResults.Contains(searchValue))
          );
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
        var mesSnListHistories = await query
            .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        // 转换为DTO
        var dtos = mesSnListHistories.Select(p => ToDto(p)).ToList();

        // 返回分页结果
        return new PaginatedList<MesSnListHistoryDto>(dtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询SN历史记录列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据SNListHistoryId获取SN历史记录详情
    /// </summary>
    public async Task<MesSnListHistoryDto?> GetMesSnListHistoryByIdAsync(Guid sNListHistoryId)
    {
      try
      {
        var mesSnListHistory = await _mesSnListHistoryRepo.GetQueryable()
            .FirstOrDefaultAsync(p => p.SNListHistoryId == sNListHistoryId);

        if (mesSnListHistory == null)
          return null;

        // 转换为DTO
        return ToDto(mesSnListHistory);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据ID查询SN历史记录详情时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据SnNumber获取SN历史记录详情
    /// </summary>
    public async Task<MesSnListHistoryDto?> GetMesSnListHistoryBySnNumberAsync(string snNumber)
    {
      try
      {
        if (string.IsNullOrEmpty(snNumber))
          return null;

        var mesSnListHistory = await _mesSnListHistoryRepo.GetQueryable()
            .FirstOrDefaultAsync(p => p.SnNumber == snNumber);

        if (mesSnListHistory == null)
          return null;

        // 转换为DTO
        return ToDto(mesSnListHistory);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "根据序列号查询SN历史记录详情时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 将MesSnListHistory实体转换为MesSnListHistoryDto
    /// </summary>
    private MesSnListHistoryDto ToDto(MesSnListHistory entity)
    {
      return new MesSnListHistoryDto
      {
        SNListHistoryId = entity.SNListHistoryId,
        SnNumber = entity.SnNumber,
        ProductListId = entity.ProductListId,
        OrderListId = entity.OrderListId,
        StationStatus = entity.StationStatus,
        CurrentStationListId = entity.CurrentStationListId,
        ProductionLineId = entity.ProductionLineId,
        ResourceId = entity.ResourceId,
        TestData = entity.TestData ?? "",
        BatchResults = entity.BatchResults ?? "",
        IsAbnormal = entity.IsAbnormal,
        AbnormalCode = entity.AbnormalCode ?? "",
        AbnormalDescription = entity.AbnormalDescription ?? "",
        IsLocked = entity.IsLocked,
        ReworkCount = entity.ReworkCount,
        IsReworking = entity.IsReworking,
        ReworkReason = entity.ReworkReason ?? "",
        ReworkTime = entity.ReworkTime,
        CreateBy = entity.CreateBy ?? "",
        CreateTime = entity.CreateTime,
        UpdateBy = entity.UpdateBy ?? "",
        UpdateTime = entity.UpdateTime,
        Remark = entity.Remark ?? "",
        SearchValue = ""
      };
    }
  }
}
