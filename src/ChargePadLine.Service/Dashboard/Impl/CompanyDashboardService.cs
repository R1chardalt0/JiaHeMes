using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.DbContexts;
using ChargePadLine.Service.Dashboard.Dto;
using ChargePadLine.Entitys.Trace;

namespace ChargePadLine.Service.Dashboard.Impl
{
    /// <summary>
    /// 公司级数据看板服务实现
    /// </summary>
    public class CompanyDashboardService : ICompanyDashboardService
    {
        private readonly AppDbContext _dbContext;

        public CompanyDashboardService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取公司级看板概览（可按时间范围）
        /// </summary>
        public async Task<CompanyDashboardOverviewDto> GetOverviewAsync(int? companyId, string? companyName, DateTime? startTime, DateTime? endTime)
        {
            // 1. 确定公司ID
            int? cid = companyId;
            if (!cid.HasValue && !string.IsNullOrWhiteSpace(companyName))
            {
                var company = await _dbContext.SysCompanys.FirstOrDefaultAsync(c => c.CompanyName == companyName);
                cid = company?.CompanyId;
            }
            if (!cid.HasValue)
            {
                throw new InvalidOperationException("必须提供公司ID或公司名称");
            }

            var companyInfo = await _dbContext.SysCompanys.FirstOrDefaultAsync(c => c.CompanyId == cid.Value);
            var overview = new CompanyDashboardOverviewDto
            {
                CompanyId = cid.Value,
                CompanyName = companyInfo?.CompanyName
            };

            // 2. 时间范围：如果未提供时间参数，则查询所有数据
            bool hasTimeFilter = startTime.HasValue || endTime.HasValue;
            DateTimeOffset? start = startTime.HasValue ? new DateTimeOffset(startTime.Value) : (DateTimeOffset?)null;
            DateTimeOffset? end = endTime.HasValue ? new DateTimeOffset(endTime.Value) : (DateTimeOffset?)null;

            // 3. 该公司所有产线、设备查询基准
            var lineIdsQuery = _dbContext.ProductionLines
                .Where(pl => pl.CompanyId == cid.Value)
                .Select(pl => pl.ProductionLineId);

            var devicesQuery = _dbContext.DeviceInfos
                .Where(d => lineIdsQuery.Contains(d.ProductionLineId));

            // 4. 设备总数
            overview.DeviceTotal = await devicesQuery.CountAsync();

            // 5. 设备状态分布
            var statusGroups = await devicesQuery
                .GroupBy(d => d.Status ?? string.Empty)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            foreach (var g in statusGroups)
            {
                var key = string.IsNullOrWhiteSpace(g.Status) ? "未配置" : g.Status.Trim();
                overview.DeviceStatusMap[key] = g.Count;
            }
            // 运行中设备：兼容多种标记
            var runningStatus = new HashSet<string>(new[] { "1", "启用", "running", "运行中", "online", "在线", "正常", "Active", "Success" }, StringComparer.OrdinalIgnoreCase);
            overview.RunningDevices = statusGroups
                .Where(s => runningStatus.Contains((s.Status ?? string.Empty).Trim()))
                .Sum(s => s.Count);

            // 6. 告警设备数：统计各设备最新一次追溯记录是否含告警
            var tracesBaseQuery = from t in _dbContext.EquipmentTracinfos
                                  join d in devicesQuery on t.DeviceEnCode equals d.DeviceEnCode
                                  select t;
            
            // 如果提供了时间过滤，则添加时间条件
            if (hasTimeFilter && start.HasValue && end.HasValue)
            {
                tracesBaseQuery = tracesBaseQuery.Where(t => t.CreateTime >= start.Value && t.CreateTime <= end.Value);
            }

            var latestTimeByDevice = tracesBaseQuery
                .GroupBy(t => t.DeviceEnCode)
                .Select(g => new { DeviceEnCode = g.Key, LatestTime = g.Max(x => x.CreateTime) });

            var latestTraces = from t in tracesBaseQuery
                               join lt in latestTimeByDevice on new { t.DeviceEnCode, t.CreateTime } equals new { lt.DeviceEnCode, CreateTime = lt.LatestTime }
                               select t;

            overview.WarningDevices = await latestTraces.CountAsync(t => t.AlarmMessages != null && t.AlarmMessages != "");

            // 7. 产量相关：统计 ProductTraceInfo
            // 直接查询所有 ProductTraceInfo 数据（不通过设备关联，确保能获取到数据）
            var productionQuery = _dbContext.ProductTraceInfos.AsQueryable();
            
            // 如果提供了时间过滤，则添加时间条件（使用 SendTime 字段，因为这是实际的生产时间）
            if (hasTimeFilter && start.HasValue && end.HasValue)
            {
                productionQuery = productionQuery.Where(p => p.SendTime >= start.Value && p.SendTime <= end.Value);
            }

            // 统计总数和合格/不合格数量
            // 使用 CountAsync 而不是 LongCountAsync，确保查询正确执行
            overview.TotalProduction = await productionQuery.CountAsync();
            
            // 分别统计合格和不合格数量（业务定义：IsOK == false 为合格，true 为不合格）
            overview.OKNum = await productionQuery.CountAsync(p => p.IsOK == false);
            overview.NGNum = await productionQuery.CountAsync(p => p.IsOK == true);
            
            // 如果统计结果不一致，使用总数减去合格数
            if (overview.TotalProduction != overview.OKNum + overview.NGNum)
            {
                overview.NGNum = overview.TotalProduction - overview.OKNum;
            }
            overview.Yield = overview.TotalProduction > 0
                ? Math.Round(overview.OKNum * 100.0 / overview.TotalProduction, 2)
                : 0;

            // 8. 产量趋势（按小时，使用 SendTime 字段）
            var trend = await productionQuery
                .GroupBy(p => new DateTime(p.SendTime.Year, p.SendTime.Month, p.SendTime.Day, p.SendTime.Hour, 0, 0))
                .Select(g => new { Time = g.Key, Value = g.LongCount() })
                .OrderBy(x => x.Time)
                .ToListAsync();

            overview.ProductionTrend = trend.Select(x => new TimeSeriesPointDto { Time = x.Time, Value = x.Value }).ToList();

            return overview;
        }
    }
}
