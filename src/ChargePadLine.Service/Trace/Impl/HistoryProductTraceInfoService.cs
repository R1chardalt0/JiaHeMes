using Microsoft.Extensions.Logging;
using ChargePadLine.DbContexts;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
    /// <summary>
    /// 历史产品追溯信息服务实现类
    /// 从ReportDbContext（迁移后的历史数据表）读取数据
    /// </summary>
    public class HistoryProductTraceInfoService : IHistoryProductTraceInfoService
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<HistoryProductTraceInfoService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reportDbContext">报表数据库上下文（历史数据）</param>
        /// <param name="logger">日志记录器</param>
        public HistoryProductTraceInfoService(
            ReportDbContext reportDbContext,
            ILogger<HistoryProductTraceInfoService> logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }

        /// <summary>
        /// 分页查询历史产品追溯信息
        /// 从ReportDbContext读取历史数据，查询逻辑与ProductTraceInfoService保持一致
        /// </summary>
        public async Task<PaginatedList<ProductTraceInfoDto>> PaginationAsync(
            int current,
            int pageSize,
            string? sfc,
            string? productionLine,
            string? deviceName,
            string? resource,
            DateTime? startTime,
            DateTime? endTime)
        {
            // 关联查询获取所需字段，使用ReportDbContext
            var query = from et in _reportDbContext.ProductTraceInfos
                        join di in _reportDbContext.DeviceInfos on et.Resource equals di.Resource
                        join pl in _reportDbContext.ProductionLines on di.ProductionLineId equals pl.ProductionLineId
                        orderby et.CreateTime descending
                        select new ProductTraceInfoDto
                        {
                            Sfc = et.Sfc,
                            ProductionLine = pl.ProductionLineName,
                            DeviceName = di.ResourceName,
                            Site = et.Site,
                            ActivityId = et.ActivityId,
                            Resource = et.Resource,
                            DcGroupRevision = et.DcGroupRevision,
                            SendTime = et.SendTime,
                            CreateTime = et.CreateTime,
                            IsOK = et.IsOK,
                            parametricDataArray = et.parametricDataArray
                        };

            // 应用过滤条件
            if (!string.IsNullOrEmpty(sfc))
            {
                query = query.Where(r => r.Sfc.Contains(sfc));
            }

            if (!string.IsNullOrEmpty(productionLine))
            {
                query = query.Where(r => r.ProductionLine.Contains(productionLine));
            }

            if (!string.IsNullOrEmpty(deviceName))
            {
                query = query.Where(r => r.DeviceName.Contains(deviceName));
            }

            if (!string.IsNullOrEmpty(resource))
            {
                query = query.Where(r => r.Resource.Contains(resource));
            }

            // 过滤创建时间范围
            if (startTime.HasValue)
            {
                query = query.Where(r => r.CreateTime >= startTime.Value);
            }

            if (endTime.HasValue)
            {
                query = query.Where(r => r.CreateTime <= endTime.Value);
            }

            // 分页查询
            var list = await query.RetrievePagedListAsync(current, pageSize);
            return list;
        }
    }
}

