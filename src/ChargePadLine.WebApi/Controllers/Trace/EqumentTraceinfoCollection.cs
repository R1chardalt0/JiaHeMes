using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    public class EqumentTraceinfoController : BaseController
    {
        private readonly IEqumentTraceinfoService _equmentTraceinfoService;

        public EqumentTraceinfoController(IEqumentTraceinfoService equmentTraceinfoService)
        {
            _equmentTraceinfoService = equmentTraceinfoService;
        }

        /// <summary>
        /// 分页查询设备追溯信息
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="productionLine">生产线名称</param>
        /// <param name="deviceName">设备名称</param>
        /// <param name="DeviceEnCode">设备编码</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>设备追溯分页数据</returns>
        [HttpGet]
        public async Task<PagedResp<EqumentTraceinfoDto>> GetEqumentTraceinfoList(int current, int pageSize, string? productionLine, string? deviceName, string? DeviceEnCode, DateTime? startTime, DateTime? endTime)
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
                var list = await _equmentTraceinfoService.PaginationAsync(current, pageSize, productionLine, deviceName, DeviceEnCode, startTime, endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<EqumentTraceinfoDto>();
            }
        }
        /// <summary>
        /// 根据设备编码获取追溯记录
        /// </summary>
        /// <param name="deviceEnCode">设备编码</param>
        /// <param name="size">返回记录数量</param>
        /// <returns>追溯记录列表</returns>
        [HttpGet]
        public async Task<Resp<List<EquipmentTracinfo>>> GetEquipmentTracinfosListByDeviceEnCode(string deviceEnCode, int size)
        {
            try
            {
                var list = await _equmentTraceinfoService.GetEquipmentTracinfosListByDeviceEnCode(deviceEnCode, size);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<EquipmentTracinfo>>("500", ex.Message);
            }
        }

        /// <summary>
        /// 获取设备最新参数
        /// </summary>
        /// <param name="deviceEnCode">设备编码</param>
        /// <returns>设备参数明细</returns>
        [HttpGet]
        public async Task<Resp<EquipmentTracinfo>> GetParamByDeviceEnCode(string deviceEnCode)
        {
            try
            {
                var device = await _equmentTraceinfoService.GetParamByDeviceEnCode(deviceEnCode);
                return RespExtensions.MakeSuccess(device);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<EquipmentTracinfo>("500", ex.Message);
            }
        }
    }
}
