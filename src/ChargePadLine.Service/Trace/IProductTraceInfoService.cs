using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    public interface IProductTraceInfoService
    {
        /// <summary>
        /// 分页查询产品追溯信息
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="sfc">产品编码(模糊匹配)</param>
        /// <param name="productionLine">产线名称(模糊匹配)</param>
        /// <param name="deviceName">设备名称(模糊匹配)</param>
        /// <param name="resource">设备编码(模糊匹配)</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>分页产品追溯信息列表</returns>
        Task<PaginatedList<ProductTraceInfoDto>> PaginationAsync(int current, int pageSize, string? sfc, string? productionLine, string? deviceName, string? resource, DateTime? startTime, DateTime? endTime);

        /// <summary>
        /// 根据设备编码获取产品追溯信息列表
        /// </summary>
        /// <param name="resource">设备编码</param>
        /// <param name="size">获取的记录数</param>
        /// <returns>产品追溯信息列表</returns>
        Task<List<ProductTraceInfo>> GetProductTraceInfoListByResource(string resource, int size);

        /// <summary>
        /// 根据产品编码获取最新的产品追溯信息
        /// </summary>
        /// <param name="sfc">产品编码</param>
        /// <returns>产品追溯信息，如不存在则返回null</returns>
        Task<ProductTraceInfo?> GetProductTraceInfoBySfc(string sfc, string resourse);

        /// <summary>
        /// 根据产品编码获取最新的产品追溯信息，按设备分组
        /// </summary>
        /// <param name="sfc"></param>
        /// <returns></returns>
        Task<List<ProductTraceInfo>> GetLatestProductTraceInfoBySfcGroupedByResource(string sfc);

        /// <summary>
        /// 输出产量报表
        /// </summary>
        /// <param name="productionLine"></param>
        /// <param name="DeviceName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        Task<List<ProductionRecordsDto>> GetProductionRecordsAsync(string? ProductionLineName, string? DeviceName, DateTime startTime, DateTime endTime, string? resource);
    }
}
