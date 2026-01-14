using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Service.Trace.Dto.Station;
using ChargePadLine.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Service;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 站点服务实现
  /// </summary>
  public class StationListService : IStationListService
  {
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    public StationListService(AppDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    /// <summary>
    /// 创建站点
    /// </summary>
    /// <param name="dto">站点创建数据</param>
    /// <returns>创建的站点信息</returns>
    public async Task<StationListDto> CreateAsync(StationListCreateDto dto)
    {
      // 验证站点编码唯一性
      var existingStation = await _dbContext.StationList.FirstOrDefaultAsync(s => s.StationCode == dto.StationCode);
      if (existingStation != null)
      {
        throw new InvalidOperationException($"站点编码已存在: {dto.StationCode}");
      }

      var station = new StationList
      {
        StationId = Guid.NewGuid(),
        StationName = dto.StationName,
        StationCode = dto.StationCode,
        Remark = dto.Remark,
        CreateTime = DateTimeOffset.Now
      };

      _dbContext.StationList.Add(station);
      await _dbContext.SaveChangesAsync();

      return MapToDto(station);
    }

    /// <summary>
    /// 更新站点
    /// </summary>
    /// <param name="dto">站点更新数据</param>
    /// <returns>更新后的站点信息</returns>
    public async Task<StationListDto> UpdateAsync(StationListUpdateDto dto)
    {
      var station = await _dbContext.StationList.FindAsync(dto.StationId);
      if (station == null)
      {
        throw new Exception("站点不存在");
      }

      // 验证站点编码唯一性（排除当前站点）
      if (station.StationCode != dto.StationCode)
      {
        var existingStation = await _dbContext.StationList.FirstOrDefaultAsync(s => s.StationCode == dto.StationCode && s.StationId != dto.StationId);
        if (existingStation != null)
        {
          throw new InvalidOperationException($"站点编码已存在: {dto.StationCode}");
        }
      }

      station.StationName = dto.StationName;
      station.StationCode = dto.StationCode;
      station.Remark = dto.Remark;
      station.UpdateTime = DateTimeOffset.Now;

      _dbContext.StationList.Update(station);
      await _dbContext.SaveChangesAsync();

      return MapToDto(station);
    }

    /// <summary>
    /// 删除站点
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAsync(Guid stationId)
    {
      var station = await _dbContext.StationList.FindAsync(stationId);
      if (station == null)
      {
        throw new Exception("站点不存在");
      }

      _dbContext.StationList.Remove(station);
      await _dbContext.SaveChangesAsync();

      return true;
    }

    /// <summary>
    /// 获取站点详情
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>站点详情</returns>
    public async Task<StationListDto> GetByIdAsync(Guid stationId)
    {
      var station = await _dbContext.StationList.FindAsync(stationId);
      if (station == null)
      {
        throw new Exception("站点不存在");
      }

      return MapToDto(station);
    }

    /// <summary>
    /// 获取站点列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    public async Task<(int Total, List<StationListDto> Items)> GetListAsync(StationListQueryDto queryDto)
    {
      var query = _dbContext.StationList.AsQueryable();

      // 应用查询条件
      if (!string.IsNullOrEmpty(queryDto.StationName))
      {
        query = query.Where(s => s.StationName.Contains(queryDto.StationName));
      }

      if (!string.IsNullOrEmpty(queryDto.StationCode))
      {
        query = query.Where(s => s.StationCode.Contains(queryDto.StationCode));
      }

      if (queryDto.StartTime.HasValue)
      {
        query = query.Where(s => s.CreateTime >= queryDto.StartTime.Value);
      }

      if (queryDto.EndTime.HasValue)
      {
        query = query.Where(s => s.CreateTime <= queryDto.EndTime.Value);
      }

      // 计算总数
      var total = await query.CountAsync();

      // 应用分页并排序
      var items = await query
          .OrderByDescending(s => s.CreateTime)
          .Skip((queryDto.Current - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

      return (
          total,
          items.Select(MapToDto).ToList()
      );
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    /// <param name="station">站点实体</param>
    /// <returns>站点DTO</returns>
    private StationListDto MapToDto(StationList station)
    {
      return new StationListDto
      {
        StationId = station.StationId,
        StationName = station.StationName,
        StationCode = station.StationCode,
        CreateBy = station.CreateBy,
        CreateTime = station.CreateTime,
        UpdateBy = station.UpdateBy,
        UpdateTime = station.UpdateTime,
        Remark = station.Remark
      };
    }

    /// <summary>
    /// 分页查询站点列表
    /// </summary>
    /// <param name="queryDto">查询条件</param>
    /// <returns>分页结果</returns>
    public async Task<PaginatedList<StationListDto>> PaginationAsync(StationListQueryDto queryDto)
    {
      var query = _dbContext.StationList.AsQueryable();

      // 应用查询条件
      if (!string.IsNullOrEmpty(queryDto.StationName))
      {
        query = query.Where(s => s.StationName.Contains(queryDto.StationName));
      }

      if (!string.IsNullOrEmpty(queryDto.StationCode))
      {
        query = query.Where(s => s.StationCode.Contains(queryDto.StationCode));
      }

      if (queryDto.StartTime.HasValue)
      {
        query = query.Where(s => s.CreateTime >= queryDto.StartTime.Value);
      }

      if (queryDto.EndTime.HasValue)
      {
        query = query.Where(s => s.CreateTime <= queryDto.EndTime.Value);
      }

      // 计算总数
      var total = await query.CountAsync();

      // 应用分页并排序
      var stationItems = await query
          .OrderByDescending(s => s.CreateTime)
          .Skip((queryDto.Current - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

      var items = stationItems.Select(MapToDto).ToList();

      return new PaginatedList<StationListDto>(items, total, queryDto.Current, queryDto.PageSize);
    }

    /// <summary>
    /// 根据ID获取站点详情
    /// </summary>
    /// <param name="stationId">站点ID</param>
    /// <returns>站点详情</returns>
    public async Task<StationListDto> GetStationInfoById(Guid stationId)
    {
      var station = await _dbContext.StationList.FindAsync(stationId);
      if (station == null)
      {
        return null;
      }

      return MapToDto(station);
    }

    /// <summary>
    /// 创建站点
    /// </summary>
    /// <param name="dto">站点信息</param>
    /// <returns>创建的站点信息</returns>
    public async Task<StationListDto> CreateStationInfo(StationListDto dto)
    {
      // 验证站点编码唯一性
      var existingStation = await _dbContext.StationList.FirstOrDefaultAsync(s => s.StationCode == dto.StationCode);
      if (existingStation != null)
      {
        throw new InvalidOperationException($"站点编码已存在: {dto.StationCode}");
      }

      var station = new StationList
      {
        StationId = dto.StationId == Guid.Empty ? Guid.NewGuid() : dto.StationId,
        StationName = dto.StationName,
        StationCode = dto.StationCode,
        Remark = dto.Remark,
        CreateTime = DateTimeOffset.Now
      };

      _dbContext.StationList.Add(station);
      await _dbContext.SaveChangesAsync();

      return MapToDto(station);
    }

    /// <summary>
    /// 更新站点
    /// </summary>
    /// <param name="dto">站点信息</param>
    /// <returns>更新后的站点信息</returns>
    public async Task<StationListDto> UpdateStationInfo(StationListDto dto)
    {
      var station = await _dbContext.StationList.FindAsync(dto.StationId);
      if (station == null)
      {
        throw new Exception("站点不存在");
      }

      // 验证站点编码唯一性（排除当前站点）
      if (station.StationCode != dto.StationCode)
      {
        var existingStation = await _dbContext.StationList.FirstOrDefaultAsync(s => s.StationCode == dto.StationCode && s.StationId != dto.StationId);
        if (existingStation != null)
        {
          throw new InvalidOperationException($"站点编码已存在: {dto.StationCode}");
        }
      }

      station.StationName = dto.StationName;
      station.StationCode = dto.StationCode;
      station.Remark = dto.Remark;
      station.UpdateTime = DateTimeOffset.Now;

      _dbContext.StationList.Update(station);
      await _dbContext.SaveChangesAsync();

      return MapToDto(station);
    }

    /// <summary>
    /// 删除站点
    /// </summary>
    /// <param name="stationIds">站点ID列表</param>
    /// <returns>删除成功的数量</returns>
    public async Task<int> DeleteStationInfoById(Guid[] stationIds)
    {
      var stations = await _dbContext.StationList.Where(s => stationIds.Contains(s.StationId)).ToListAsync();
      if (stations.Count == 0)
      {
        return 0;
      }

      _dbContext.StationList.RemoveRange(stations);
      await _dbContext.SaveChangesAsync();

      return stations.Count;
    }
  }
}
