using Microsoft.Extensions.Logging;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.Service.Trace.Impl
{
    public class ProductTraceInfoService : IProductTraceInfoService
    {
        private readonly IRepository<ProductTraceInfo> _ptinfoRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ProductTraceInfoService> _logger;
        public ProductTraceInfoService(IRepository<ProductTraceInfo> ptinfoRepo, AppDbContext dbContext, ILogger<ProductTraceInfoService> logger)
        {
            _ptinfoRepo = ptinfoRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<PaginatedList<ProductTraceInfoDto>> PaginationAsync(int current, int pageSize, string? sfc, string? productionLine, string? deviceName, string? resource, DateTime? startTime, DateTime? endTime)
        {
            // 关联查询获取所需字段
            var query = from et in _dbContext.ProductTraceInfos
                        join di in _dbContext.DeviceInfos on et.Resource equals di.DeviceEnCode
                        join pl in _dbContext.ProductionLines on di.ProductionLineId equals pl.ProductionLineId
                        orderby et.CreateTime descending
                        select new ProductTraceInfoDto
                        {
                            Sfc = et.Sfc,
                            ProductionLine = pl.ProductionLineName,
                            DeviceName = di.DeviceName,
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

        public async Task<List<ProductTraceInfo>> GetProductTraceInfoListByResource(string resource, int size)
        {
            var result = await _dbContext.ProductTraceInfos
                                         .Where(e => e.Resource == resource)
                                         .OrderByDescending(e => e.CreateTime)
                                         .Take(size)
                                        .ToListAsync();
            return result ?? new List<ProductTraceInfo>();
        }

        public async Task<ProductTraceInfo?> GetProductTraceInfoBySfc(string sfc, string resourse)
        {
            var result = await _dbContext.ProductTraceInfos
                                         .Where(e => e.Sfc == sfc && e.Resource == resourse)
                                         .OrderByDescending(e => e.CreateTime)
                                         .FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 通过sfc获取当前sfc经过所有站生产的结果数据
        /// </summary>
        /// <param name="sfc"></param>
        /// <returns></returns>
        public async Task<List<ProductTraceInfo>> GetLatestProductTraceInfoBySfcGroupedByResource(string sfc)
        {
            var latestRecords = await _dbContext.ProductTraceInfos
                   .Where(e => e.Sfc == sfc)
                   // 子查询获取每个Resource的最新记录
                   .Where(e => e.CreateTime == _dbContext.ProductTraceInfos
                   .Where(inner => inner.Sfc == sfc && inner.Resource == e.Resource)
                   .Max(inner => inner.CreateTime))
                   .OrderByDescending(e => e.Resource)
                   .ToListAsync();
            return latestRecords ?? new List<ProductTraceInfo>();
        }

        /// <summary>
        /// 输出产量报表
        /// </summary>
        /// <param name="productionLine"></param>
        /// <param name="DeviceName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task<List<ProductionRecordsDto>> GetProductionRecordsAsync(string? ProductionLineName, string? DeviceName, DateTime startTime, DateTime endTime, string? resource)
        {
            // 构建查询，从ProductTraceInfo表中获取指定时间范围内的记录
            var query = from trace in _dbContext.ProductTraceInfos
                            // 关联DeviceInfo表，通过Resource(设备编码)匹配
                        join device in _dbContext.DeviceInfos on trace.Resource equals device.DeviceEnCode into deviceJoin
                        from device in deviceJoin.DefaultIfEmpty()
                            // 关联ProductionLine表，获取生产线名称
                        join line in _dbContext.ProductionLines on device.ProductionLineId equals line.ProductionLineId into lineJoin
                        from line in lineJoin.DefaultIfEmpty()
                            // 过滤时间范围
                        where trace.CreateTime >= startTime && trace.CreateTime <= endTime
                        // 过滤生产线（如果有）
                        where (string.IsNullOrEmpty(ProductionLineName) || line.ProductionLineName.Contains(ProductionLineName))
                        // 过滤设备名称（如果有）
                        where (string.IsNullOrEmpty(DeviceName) || device.DeviceName.Contains(DeviceName))
                        // 过滤资源号（如果有）
                        where (string.IsNullOrEmpty(resource) || trace.Resource.Contains(resource))
                        // 按Resource分组
                        group trace by new { trace.Resource, DeviceName = device.DeviceName, ProductionLineName = line.ProductionLineName } into g
                        select new ProductionRecordsDto
                        {
                            Resource = g.Key.Resource,
                            DeviceName = g.Key.DeviceName ?? "未知设备",
                            ProductionLineName = g.Key.ProductionLineName ?? "未知产线",
                            TotalProduction = g.Count(),
                            OKNum = g.Count(t => t.IsOK),
                            NGNum = g.Count(t => !t.IsOK),
                            // 计算良率，避免除以0
                            Yield = g.Count() > 0 ? Math.Round((double)g.Count(t => t.IsOK) / g.Count() * 100, 2) : 0
                        };

            // 执行查询并返回结果
            return await query.ToListAsync();
        }

        /// <summary>
        /// 按小时输出产量报表的实现
        /// </summary>
        public async Task<List<HourlyProductionRecordsDto>> GetHourlyProductionRecordsAsync(string? productionLineName, string? deviceName, DateTime startTime, DateTime endTime, string? resource)
        {
            // 确保开始时间是整点
            startTime = startTime.Date.AddHours(startTime.Hour);
            // 确保结束时间是整点
            endTime = endTime.Date.AddHours(endTime.Hour);
            // 为了确保包含完整的时间段，将实际查询的结束时间扩展到下一个小时的开始前一毫秒
            var queryEndTime = endTime.AddHours(1).AddMilliseconds(-1);

            // 获取查询范围内的所有设备信息，用于生成完整的小时数据
            var devicesQuery = from device in _dbContext.DeviceInfos
                               join line in _dbContext.ProductionLines on device.ProductionLineId equals line.ProductionLineId into lineJoin
                               from line in lineJoin.DefaultIfEmpty()
                               where (string.IsNullOrEmpty(productionLineName) || line.ProductionLineName.Contains(productionLineName))
                               where (string.IsNullOrEmpty(deviceName) || device.DeviceName.Contains(deviceName))
                               where (string.IsNullOrEmpty(resource) || device.DeviceEnCode.Contains(resource))
                               select new { device.DeviceEnCode, device.DeviceName, line.ProductionLineName };

            var devices = await devicesQuery.ToListAsync();

            // 生成指定范围内的所有小时时间点，确保包含结束时间点
            var allHours = new List<DateTime>();
            var currentHour = startTime;
            while (currentHour <= endTime)
            {
                allHours.Add(currentHour);
                currentHour = currentHour.AddHours(1);
            }

            // 获取原始数据，直接在数据库端进行过滤
            var query = from trace in _dbContext.ProductTraceInfos
                        where trace.CreateTime >= startTime && trace.CreateTime <= queryEndTime
                        // 关联DeviceInfo表获取设备信息
                        join device in _dbContext.DeviceInfos on trace.Resource equals device.DeviceEnCode
                        // 关联ProductionLine表获取生产线信息
                        join line in _dbContext.ProductionLines on device.ProductionLineId equals line.ProductionLineId
                        // 添加额外的过滤条件
                        where (string.IsNullOrEmpty(productionLineName) || line.ProductionLineName.Contains(productionLineName))
                        where (string.IsNullOrEmpty(deviceName) || device.DeviceName.Contains(deviceName))
                        where (string.IsNullOrEmpty(resource) || trace.Resource.Contains(resource))
                        select new { trace, device, line };

            // 先异步获取所有符合条件的数据
            var allData = await query.ToListAsync();

            // 在客户端进行分组和统计，使用更精确的小时分组方式
            var existingData = allData
                .GroupBy(tdl => new
                {
                    Resource = tdl.trace.Resource,
                    DeviceName = tdl.device.DeviceName,
                    ProductionLineName = tdl.line.ProductionLineName,
                    // 使用更精确的小时分组方式
                    HourKey = new DateTime(tdl.trace.CreateTime.Year, tdl.trace.CreateTime.Month,
                                          tdl.trace.CreateTime.Day, tdl.trace.CreateTime.Hour, 0, 0)
                })
                .Select(g => new
                {
                    Resource = g.Key.Resource,
                    DeviceName = g.Key.DeviceName ?? "未知设备",
                    ProductionLineName = g.Key.ProductionLineName ?? "未知产线",
                    HourKey = g.Key.HourKey,
                    HourString = g.Key.HourKey.ToString("yyyy-MM-dd HH:00"),
                    TotalProduction = g.Count(),
                    OKNum = g.Count(tdl => tdl.trace.IsOK),
                    NGNum = g.Count(tdl => !tdl.trace.IsOK),
                    Yield = g.Count() > 0 ? Math.Round((double)g.Count(tdl => tdl.trace.IsOK) / g.Count() * 100, 2) : 0
                })
                .ToList();

            // 记录现有数据的时间范围
            if (existingData.Any())
            {
                var minHour = existingData.Min(d => d.HourKey);
                var maxHour = existingData.Max(d => d.HourKey);

                // 扩展小时列表，包含现有数据中实际存在但超出原始时间范围的小时点
                // 这解决了为什么跨天后数据才会出现的问题
                if (maxHour > endTime)
                {
                    currentHour = endTime.AddHours(1);
                    while (currentHour <= maxHour)
                    {
                        allHours.Add(currentHour);
                        currentHour = currentHour.AddHours(1);
                    }
                }
            }

            // 生成完整的小时数据，包括产量为0的小时
            var result = new List<HourlyProductionRecordsDto>();

            // 用于记录匹配统计
            int matchedCount = 0;
            int unmatchedCount = 0;

            foreach (var deviceInfo in devices)
            {
                foreach (var hour in allHours)
                {
                    // 查找该设备在该小时的数据
                    var existingRecord = existingData.FirstOrDefault(d =>
                        d.Resource == deviceInfo.DeviceEnCode &&
                        d.HourKey.Year == hour.Year &&
                        d.HourKey.Month == hour.Month &&
                        d.HourKey.Day == hour.Day &&
                        d.HourKey.Hour == hour.Hour);

                    if (existingRecord != null)
                    {
                        matchedCount++;
                        // 记录匹配成功的情况
                        if (matchedCount % 10 == 0 || hour.Hour == 17) // 重点关注17点的数据
                        {
                            _logger.LogInformation("匹配成功: 设备={Device}, 小时={Hour}, 产量={Production}",
                                deviceInfo.DeviceName, hour.ToString("yyyy-MM-dd HH:00"), existingRecord.TotalProduction);
                        }

                        // 如果有数据，使用实际数据
                        result.Add(new HourlyProductionRecordsDto
                        {
                            Resource = existingRecord.Resource,
                            DeviceName = existingRecord.DeviceName,
                            ProductionLineName = existingRecord.ProductionLineName,
                            Hour = existingRecord.HourString,
                            TotalProduction = existingRecord.TotalProduction,
                            OKNum = existingRecord.OKNum,
                            NGNum = existingRecord.NGNum,
                            Yield = existingRecord.Yield
                        });
                    }
                    else
                    {
                        unmatchedCount++;
                        // 记录匹配失败的情况，特别是17点
                        if (hour.Hour == 17)
                        {
                            _logger.LogInformation("匹配失败: 设备={Device}, 小时={Hour}",
                                deviceInfo.DeviceName, hour.ToString("yyyy-MM-dd HH:00"));
                        }

                        // 如果没有数据，创建产量为0的记录
                        result.Add(new HourlyProductionRecordsDto
                        {
                            Resource = deviceInfo.DeviceEnCode,
                            DeviceName = deviceInfo.DeviceName ?? "未知设备",
                            ProductionLineName = deviceInfo.ProductionLineName ?? "未知产线",
                            Hour = hour.ToString("yyyy-MM-dd HH:00"),
                            TotalProduction = 0,
                            OKNum = 0,
                            NGNum = 0,
                            Yield = 0
                        });
                    }
                }
            }
            return result;
        }
    }
}
