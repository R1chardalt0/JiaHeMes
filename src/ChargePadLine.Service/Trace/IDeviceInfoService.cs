using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
  public interface IDeviceInfoService
  {
    /// <summary>
    /// 分页查询设备信息列表
    /// </summary>
    /// <param name="current">当前页码</param>
    /// <param name="pageSize">每页记录数</param>
    /// <param name="deviceName">设备名称(模糊匹配)</param>
    /// <param name="deviceEnCode">设备编码(模糊匹配)</param>
    /// <param name="deviceType">设备类型(精确匹配)</param>
    /// <param name="productionLineId">生产线ID</param>
    /// <param name="status">设备状态(精确匹配)</param>
    /// <param name="workOrderCode">工单编码</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>分页设备信息列表</returns>
    Task<PaginatedList<Deviceinfo>> PaginationAsync(int current, int pageSize, string? deviceName, string? deviceEnCode, string? deviceType, string? productionLineId, string? status, string? workOrderCode, DateTime? startTime, DateTime? endTime);

    /// <summary>
    /// 获取设备详情
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <returns>设备信息</returns>
    Task<Deviceinfo> GetDeviceInfoById(Guid deviceId);

    /// <summary>
    /// 根据设备编码获取设备信息
    /// </summary>
    /// <param name="deviceEnCode">设备编码</param>
    /// <returns>设备信息</returns>
    Task<Deviceinfo> GetDeviceInfoByEnCode(string deviceEnCode);

    /// <summary>
    /// 创建设备信息
    /// </summary>
    /// <param name="deviceInfo">设备信息实体</param>
    /// <returns>影响的行数，-1表示设备编码已存在</returns>
    Task<int> CreateDeviceInfo(Deviceinfo deviceInfo);

    /// <summary>
    /// 更新设备信息
    /// </summary>
    /// <param name="deviceInfo">设备信息实体</param>
    /// <returns>影响的行数，-1表示设备编码已存在</returns>
    Task<int> UpdateDeviceInfo(Deviceinfo deviceInfo);

    /// <summary>
    /// 批量删除设备信息
    /// </summary>
    /// <param name="deviceIds">设备ID数组</param>
    /// <returns>影响的行数</returns>
    Task<int> DeleteDeviceInfoByIds(Guid[] deviceIds);

    /// <summary>
    /// 获取所有设备列表
    /// </summary>
    /// <param name="workOrderCode">工单编码</param>
    /// <returns>设备信息列表</returns>
    Task<List<Deviceinfo>> GetAllDeviceInfos(string? workOrderCode = null);

    /// <summary>
    /// 根据生产线ID获取设备列表
    /// </summary>
    /// <param name="productionLineId">生产线ID</param>
    /// <param name="workOrderCode">工单编码</param>
    /// <returns>设备信息列表</returns>
    Task<List<Deviceinfo>> GetDeviceInfosByProductionLineId(Guid productionLineId, string? workOrderCode = null);
  }
}
