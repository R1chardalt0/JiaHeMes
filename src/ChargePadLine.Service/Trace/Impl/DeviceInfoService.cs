using ChargePadLine.DbContexts.Repository;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.Service.Trace.Impl
{
  public class DeviceInfoService : IDeviceInfoService
  {
    private readonly IRepository<Deviceinfo> _deviceRepo;
    private readonly AppDbContext _dbContext;
    private ILogger<DeviceInfoService> _logger;

    public DeviceInfoService(IRepository<Deviceinfo> deviceRepo, AppDbContext dbContext, ILogger<DeviceInfoService> logge)
    {
      _deviceRepo = deviceRepo;
      _dbContext = dbContext;
      _logger = logge;
    }

    /// <summary>
    /// 分页查询设备信息列表
    /// </summary>
    public async Task<PaginatedList<Deviceinfo>> PaginationAsync(int current, int pageSize, string? deviceName, string? deviceEnCode, string? deviceType, string? productionLineId, string? status, string? workOrderCode, DateTime? startTime, DateTime? endTime)
    {
      var query = _dbContext.DeviceInfos
          .Include(d => d.ProductionLine) // 关联生产线表
          .OrderByDescending(s => s.CreateTime)
          .AsQueryable();

      // 过滤设备名称
      if (!string.IsNullOrEmpty(deviceName))
      {
        query = query.Where(r => r.ResourceName.Contains(deviceName));
      }

      // 过滤设备编码
      if (!string.IsNullOrEmpty(deviceEnCode))
      {
        query = query.Where(r => r.Resource.Contains(deviceEnCode));

      }

      // 过滤设备类型
      if (!string.IsNullOrEmpty(deviceType))
      {
        query = query.Where(r => r.ResourceType == deviceType);
      }

      // 过滤生产线ID
      if (!string.IsNullOrEmpty(productionLineId) && Guid.TryParse(productionLineId, out var lineId))
      {
        query = query.Where(r => r.ProductionLineId == lineId);
      }

      // 过滤设备状态
      if (!string.IsNullOrEmpty(status))
      {
        query = query.Where(r => r.Status == status);
      }

      // 过滤工单编码
      if (!string.IsNullOrEmpty(workOrderCode))
      {
        query = query.Where(r => r.WorkOrderCode.Contains(workOrderCode));
      }

      // 过滤创建时间范围
      if (startTime.HasValue)
      {
        query = query.Where(r => r.CreateTime >= startTime.Value);
      }

      if (endTime.HasValue)
      {
        query = query.Where(r => r.CreateTime <= endTime.Value);
      }

      // 分页查询
      var list = await query.RetrievePagedListAsync(current, pageSize);

      // 确保每个设备的 ProductionLineName 被正确设置
      // PaginatedList<T> 继承自 List<T>，可以直接遍历
      foreach (var device in list)
      {
        if (device.ProductionLine != null && string.IsNullOrEmpty(device.ProductionLineName))
        {
          device.ProductionLineName = device.ProductionLine.ProductionLineName;
        }
      }

      return list;
    }

    /// <summary>
    /// 获取设备详情
    /// </summary>
    public async Task<Deviceinfo> GetDeviceInfoById(Guid deviceId)
    {
      return await _dbContext.DeviceInfos
          .Include(d => d.ProductionLine) // 关联生产线表，以便获取生产线名称
          .FirstOrDefaultAsync(r => r.ResourceId == deviceId);
    }

    /// <summary>
    /// 根据设备编码获取设备信息
    /// </summary>
    public async Task<Deviceinfo> GetDeviceInfoByEnCode(string deviceEnCode)
    {
      return await _dbContext.DeviceInfos
          .Include(d => d.ProductionLine) // 关联生产线表，以便获取生产线名称
          .FirstOrDefaultAsync(r => r.Resource == deviceEnCode);
    }

    /// <summary>
    /// 创建设备信息
    /// </summary>
    public async Task<int> CreateDeviceInfo(Deviceinfo deviceInfo)
    {
      // 验证设备编码唯一性
      var exists = await _deviceRepo.GetAsync(r => r.Resource == deviceInfo.Resource);
      if (exists != null)
        return -1; // 设备编码已存在

      deviceInfo.CreateTime = DateTime.Now;
      deviceInfo.UpdateTime = DateTime.Now;
      return await _deviceRepo.InsertAsyncs(deviceInfo);
    }

    /// <summary>
    /// 更新设备信息
    /// </summary>
    public async Task<int> UpdateDeviceInfo(Deviceinfo deviceInfo)
    {
      // 验证设备编码唯一性（排除当前设备）
      var exists = await _deviceRepo.GetAsync(r => r.Resource == deviceInfo.Resource && r.ResourceId != deviceInfo.ResourceId);
      if (exists != null)
        return -1; // 设备编码已存在

      deviceInfo.UpdateTime = DateTime.Now;
      return await _deviceRepo.UpdateAsyncs(deviceInfo);
    }

    /// <summary>
    /// 批量删除设备信息
    /// </summary>
    public async Task<int> DeleteDeviceInfoByIds(Guid[] deviceIds)
    {
      // 使用DbContext直接管理事务
      using (var transaction = await _dbContext.Database.BeginTransactionAsync())
      {
        try
        {
          // 删除设备
          var result = await _deviceRepo.DeleteAsyncs(r => deviceIds.Contains(r.ResourceId));

          await transaction.CommitAsync();
          return result;
        }
        catch
        {
          await transaction.RollbackAsync();
          throw;
        }
      }
    }

    /// <summary>
    /// 获取所有设备列表
    /// </summary>
    public async Task<List<Deviceinfo>> GetAllDeviceInfos(string? workOrderCode = null)
    {
      if (!string.IsNullOrEmpty(workOrderCode))
      {
        return await _deviceRepo.GetListAsync(r => r.WorkOrderCode.Contains(workOrderCode));
      }
      return await _deviceRepo.GetListAsync();
    }

    /// <summary>
    /// 根据生产线ID获取设备列表
    /// </summary>
    public async Task<List<Deviceinfo>> GetDeviceInfosByProductionLineId(Guid productionLineId, string? workOrderCode = null)
    {
      if (!string.IsNullOrEmpty(workOrderCode))
      {
        return await _deviceRepo.GetListAsync(r => r.ProductionLineId == productionLineId && r.WorkOrderCode.Contains(workOrderCode));
      }
      return await _deviceRepo.GetListAsync(r => r.ProductionLineId == productionLineId);
    }
  }
}
