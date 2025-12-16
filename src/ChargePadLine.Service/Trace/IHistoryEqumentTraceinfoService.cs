using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    /// <summary>
    /// 历史设备追溯信息服务接口
    /// 用于查询迁移到ReportDbContext的历史设备追溯数据
    /// </summary>
    public interface IHistoryEqumentTraceinfoService
    {
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
        Task<PaginatedList<EqumentTraceinfoDto>> PaginationAsync(int current, int pageSize, string? productionLine, string? deviceName, string? deviceEnCode, DateTime? startTime, DateTime? endTime);
    }
}

