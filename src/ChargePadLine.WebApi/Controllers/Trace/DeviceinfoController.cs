using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceinfoController : ControllerBase
    {
        private readonly IDeviceInfoService _deviceInfoService;

        public DeviceinfoController(IDeviceInfoService deviceInfoService)
        {
            _deviceInfoService = deviceInfoService;
        }

        /// <summary>
        /// 分页查询设备信息列表
        /// </summary>
        [HttpGet("GetDeviceInfoList")]
        public async Task<PagedResp<Deviceinfo>> GetDeviceInfoList(int current, int pageSize, string? deviceName, string? deviceEnCode, string? deviceType, string? productionLineId, string? status, string? workOrderCode, DateTime? startTime, DateTime? endTime)
        {
            try
            {
                if (current < 1)
                {
                    current = 1;
                }
                if (pageSize < 1)
                {
                    pageSize = 50;
                }
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                var list = await _deviceInfoService.PaginationAsync(current, pageSize, deviceName, deviceEnCode, deviceType, productionLineId, status, workOrderCode, startTime, endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakePagedEmpty<Deviceinfo>();
            }
        }

        /// <summary>
        /// 根据设备ID获取设备详情
        /// </summary>
        [HttpGet("{deviceId}")]
        public async Task<Resp<Deviceinfo>> GetDeviceInfoById(Guid deviceId)
        {
            try
            {
                var deviceInfo = await _deviceInfoService.GetDeviceInfoById(deviceId);
                if (deviceInfo == null)
                {
                    return RespExtensions.MakeFail<Deviceinfo>("404", "设备不存在");
                }
                // 确保 ProductionLineName 被正确设置
                // 如果 ProductionLine 导航属性已加载，手动设置 ProductionLineName 以确保序列化时包含此值
                if (deviceInfo.ProductionLine != null)
                {
                    deviceInfo.ProductionLineName = deviceInfo.ProductionLine.ProductionLineName;
                }
                return RespExtensions.MakeSuccess(deviceInfo);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<Deviceinfo>("500", ex.Message);
            }
        }

        /// <summary>
        /// 根据设备编码获取设备信息
        /// </summary>
        [HttpGet("ByEnCode/{deviceEnCode}")]
        public async Task<Resp<Deviceinfo>> GetDeviceInfoByEnCode(string deviceEnCode)
        {
            try
            {
                var deviceInfo = await _deviceInfoService.GetDeviceInfoByEnCode(deviceEnCode);
                if (deviceInfo == null)
                {
                    return RespExtensions.MakeFail<Deviceinfo>("404", "设备不存在");
                }
                return RespExtensions.MakeSuccess(deviceInfo);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<Deviceinfo>("500", ex.Message);
            }
        }

        /// <summary>
        /// 创建设备信息
        /// </summary>
        [HttpPost("CreateDeviceInfo")]
        public async Task<Resp<bool>> CreateDeviceInfo([FromBody] Deviceinfo deviceInfo)
        {
            try
            {
                // 验证参数
                if (deviceInfo == null || string.IsNullOrEmpty(deviceInfo.Resource) || string.IsNullOrEmpty(deviceInfo.ResourceName))
                {
                    return RespExtensions.MakeFail<bool>("400", "设备编码和设备名称不能为空");
                }

                // 生成新的设备ID
                deviceInfo.ResourceId = Guid.NewGuid();

                var result = await _deviceInfoService.CreateDeviceInfo(deviceInfo);
                if (result == -1)
                {
                    return RespExtensions.MakeFail<bool>("400", "设备编码已存在");
                }
                return RespExtensions.MakeSuccess(result > 0);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<bool>("500", ex.Message);
            }
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        [HttpPost("UpdateDeviceInfo")]
        public async Task<Resp<bool>> UpdateDeviceInfo([FromBody] Deviceinfo deviceInfo)
        {
            try
            {
                // 验证参数
                if (deviceInfo == null || deviceInfo.ResourceId == Guid.Empty || string.IsNullOrEmpty(deviceInfo.Resource) || string.IsNullOrEmpty(deviceInfo.ResourceName))
                {
                    return RespExtensions.MakeFail<bool>("400", "设备ID、编码和名称不能为空");
                }

                var result = await _deviceInfoService.UpdateDeviceInfo(deviceInfo);
                if (result == -1)
                {
                    return RespExtensions.MakeFail<bool>("400", "设备编码已存在");
                }
                return RespExtensions.MakeSuccess(result > 0);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<bool>("500", ex.Message);
            }
        }

        /// <summary>
        /// 批量删除设备信息
        /// </summary>
        [HttpPost("DeleteDeviceInfoByIds")]
        public async Task<Resp<bool>> DeleteDeviceInfoByIds([FromBody] List<Guid> deviceIds)
        {
            try
            {
                if (deviceIds == null || deviceIds.Count == 0)
                {
                    return RespExtensions.MakeFail<bool>("400", "请选择要删除的设备");
                }

                var result = await _deviceInfoService.DeleteDeviceInfoByIds(deviceIds.ToArray());
                return RespExtensions.MakeSuccess(result > 0);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<bool>("500", ex.Message);
            }
        }

        /// <summary>
        /// 获取所有设备列表
        /// </summary>
        [HttpGet("All")]
        public async Task<Resp<List<Deviceinfo>>> GetAllDeviceInfos(string? workOrderCode = null)
        {
            try
            {
                var list = await _deviceInfoService.GetAllDeviceInfos(workOrderCode);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<Deviceinfo>>("500", ex.Message);
            }
        }

        /// <summary>
        /// 根据生产线ID获取设备列表
        /// </summary>
        [HttpGet("ByProductionLineId/{productionLineId}")]
        public async Task<Resp<List<Deviceinfo>>> GetDeviceInfosByProductionLineId(Guid productionLineId, string? workOrderCode = null)
        {
            try
            {
                var list = await _deviceInfoService.GetDeviceInfosByProductionLineId(productionLineId, workOrderCode);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<Deviceinfo>>("500", ex.Message);
            }
        }
    }
}
