using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;
using System;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    /// <summary>
    /// 历史设备追溯信息控制器
    /// 用于查询迁移到ReportDbContext的历史设备追溯数据
    /// </summary>
    public class HistoryEqumentTraceinfoController : BaseController
    {
        private readonly IHistoryEqumentTraceinfoService _historyEqumentTraceinfoService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="historyEqumentTraceinfoService">历史设备追溯信息服务</param>
        public HistoryEqumentTraceinfoController(IHistoryEqumentTraceinfoService historyEqumentTraceinfoService)
        {
            _historyEqumentTraceinfoService = historyEqumentTraceinfoService;
        }

        /// <summary>
        /// 分页查询历史设备追溯信息
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="productionLine">生产线名称</param>
        /// <param name="deviceName">设备名称</param>
        /// <param name="deviceEnCode">设备编码</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>历史设备追溯分页数据</returns>
        [HttpGet]
        public async Task<PagedResp<EqumentTraceinfoDto>> GetHistoryEqumentTraceinfoList(
            int current,
            int pageSize,
            string? productionLine,
            string? deviceName,
            string? deviceEnCode,
            DateTime? startTime,
            DateTime? endTime)
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
                var list = await _historyEqumentTraceinfoService.PaginationAsync(
                    current,
                    pageSize,
                    productionLine,
                    deviceName,
                    deviceEnCode,
                    startTime,
                    endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<EqumentTraceinfoDto>();
            }
        }
    }
}

