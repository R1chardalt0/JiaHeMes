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
                        join di in _dbContext.DeviceInfos on et.Resource equals di.Resource
                        join pl in _dbContext.ProductionLines on di.ProductionLineId equals pl.ProductionLineId
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
                        join device in _dbContext.DeviceInfos on trace.Resource equals device.Resource into deviceJoin
                        from device in deviceJoin.DefaultIfEmpty()
                            // 关联ProductionLine表，获取生产线名称
                        join line in _dbContext.ProductionLines on device.ProductionLineId equals line.ProductionLineId into lineJoin
                        from line in lineJoin.DefaultIfEmpty()
                            // 过滤时间范围
                        where trace.CreateTime >= startTime && trace.CreateTime <= endTime
                        // 过滤生产线（如果有）
                        where (string.IsNullOrEmpty(ProductionLineName) || line.ProductionLineName.Contains(ProductionLineName))
                        // 过滤设备名称（如果有）
                        where (string.IsNullOrEmpty(DeviceName) || device.ResourceName.Contains(DeviceName))
                        // 过滤资源号（如果有）
                        where (string.IsNullOrEmpty(resource) || trace.Resource.Contains(resource))
                        // 按Resource分组
                        group trace by new { trace.Resource, DeviceName = device.ResourceName, ProductionLineName = line.ProductionLineName } into g
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

       
    }
}
