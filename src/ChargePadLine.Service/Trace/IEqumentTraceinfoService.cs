using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    public interface IEqumentTraceinfoService
    {
        /// <summary>
        /// 分页查询设备运行信息
        /// </summary>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="productionLine"></param>
        /// <param name="deviceName"></param>
        /// <param name="DeviceEnCode"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<PaginatedList<EqumentTraceinfoDto>> PaginationAsync(int current, int pageSize, string? productionLine, string? deviceName, string? DeviceEnCode, DateTime? startTime, DateTime? endTime);
        /// <summary>
        /// 通过设备编码获取当前设备100次运行的结果
        /// </summary>
        /// <param name="DeviceEnCode"></param>
        /// <returns></returns>
        Task<List<EquipmentTracinfo>> GetEquipmentTracinfosListByDeviceEnCode(string DeviceEnCode,int size);
        /// <summary>
        /// 通过设备编码获取当前设备运行的状态
        /// </summary>
        /// <param name="DeviceEnCode"></param>
        /// <returns></returns>
        Task<EquipmentTracinfo?> GetParamByDeviceEnCode(string DeviceEnCode);
    }
}
