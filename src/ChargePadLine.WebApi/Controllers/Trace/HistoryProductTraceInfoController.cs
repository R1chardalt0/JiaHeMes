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
    /// 历史产品追溯信息控制器
    /// 用于查询迁移到ReportDbContext的历史产品追溯数据
    /// </summary>
    public class HistoryProductTraceInfoController : BaseController
    {
        private readonly IHistoryProductTraceInfoService _historyProductTraceInfoService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="historyProductTraceInfoService">历史产品追溯信息服务</param>
        public HistoryProductTraceInfoController(IHistoryProductTraceInfoService historyProductTraceInfoService)
        {
            _historyProductTraceInfoService = historyProductTraceInfoService;
        }

        /// <summary>
        /// 分页查询历史产品追溯信息
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="sfc">产品编码(模糊匹配)</param>
        /// <param name="productionLine">产线名称(模糊匹配)</param>
        /// <param name="deviceName">设备名称(模糊匹配)</param>
        /// <param name="resource">设备编码(模糊匹配)</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>历史产品追溯分页数据</returns>
        [HttpGet]
        public async Task<PagedResp<ProductTraceInfoDto>> GetHistoryProductTraceInfoList(
            int current,
            int pageSize,
            string? sfc,
            string? productionLine,
            string? deviceName,
            string? resource,
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
                var list = await _historyProductTraceInfoService.PaginationAsync(
                    current,
                    pageSize,
                    sfc,
                    productionLine,
                    deviceName,
                    resource,
                    startTime,
                    endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<ProductTraceInfoDto>();
            }
        }
    }
}

