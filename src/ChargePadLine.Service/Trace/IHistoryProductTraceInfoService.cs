using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    /// <summary>
    /// 历史产品追溯信息服务接口
    /// 用于查询迁移到ReportDbContext的历史产品追溯数据
    /// </summary>
    public interface IHistoryProductTraceInfoService
    {
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
        /// <returns>分页历史产品追溯信息列表</returns>
        Task<PaginatedList<ProductTraceInfoDto>> PaginationAsync(int current, int pageSize, string? sfc, string? productionLine, string? deviceName, string? resource, DateTime? startTime, DateTime? endTime);
    }
}

