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
    /// 历史设备追溯信息服务实现类
    /// 从ReportDbContext（迁移后的历史数据表）读取数据
    /// </summary>
    public class HistoryEqumentTraceinfoService : IHistoryEqumentTraceinfoService
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<HistoryEqumentTraceinfoService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reportDbContext">报表数据库上下文（历史数据）</param>
        /// <param name="logger">日志记录器</param>
        public HistoryEqumentTraceinfoService(
            ReportDbContext reportDbContext,
            ILogger<HistoryEqumentTraceinfoService> logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }

        /// <summary>
        /// 分页查询历史设备追溯信息
        /// 从ReportDbContext读取历史数据，查询逻辑与EqumentTraceinfoService保持一致
        /// </summary>
        public async Task<PaginatedList<EqumentTraceinfoDto>> PaginationAsync(
            int current,
            int pageSize,
            string? productionLine,
            string? deviceName,
            string? deviceEnCode,
            DateTime? startTime,
            DateTime? endTime)
        {
            // 关联查询获取所需字段，使用ReportDbContext
            var query = from et in _reportDbContext.EquipmentTracinfos
                        join di in _reportDbContext.DeviceInfos on et.DeviceEnCode equals di.DeviceEnCode
                        join pl in _reportDbContext.ProductionLines on di.ProductionLineId equals pl.ProductionLineId
                        orderby et.CreateTime descending
                        select new EqumentTraceinfoDto
                        {
                            SendTime = et.SendTime,
                            ProductionLine = pl.ProductionLineName,
                            DeviceName = di.DeviceName,
                            DeviceEnCode = et.DeviceEnCode,
                            AlarMessages = et.AlarmMessages,
                            CreateTime = et.CreateTime,
                            Parameters = et.Parameters
                        };

            // 应用过滤条件
            if (!string.IsNullOrEmpty(productionLine))
            {
                query = query.Where(r => r.ProductionLine.Contains(productionLine));
            }

            if (!string.IsNullOrEmpty(deviceName))
            {
                query = query.Where(r => r.DeviceName.Contains(deviceName));
            }

            if (!string.IsNullOrEmpty(deviceEnCode))
            {
                query = query.Where(r => r.DeviceEnCode.Contains(deviceEnCode));
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

