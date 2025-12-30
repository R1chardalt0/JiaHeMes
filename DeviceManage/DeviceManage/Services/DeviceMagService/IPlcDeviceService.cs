using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService
{
    public interface IPlcDeviceService
    {
        /// <summary>
        /// 获取所有PLC设备
        /// </summary>
        Task<List<PlcDevice>> GetAllPlcDevicesAsync();

        /// <summary>
        /// 根据ID获取PLC设备
        /// </summary>
        Task<PlcDevice?> GetPlcDeviceByIdAsync(int id);

        /// <summary>
        /// 新增PLC设备
        /// </summary>
        Task<PlcDevice> AddPlcDeviceAsync(PlcDevice plcDevice);

        /// <summary>
        /// 更新PLC设备
        /// </summary>
        Task<PlcDevice> UpdatePlcDeviceAsync(PlcDevice plcDevice);

        /// <summary>
        /// 删除PLC设备
        /// </summary>
        Task DeletePlcDeviceAsync(int id);
    }
}
