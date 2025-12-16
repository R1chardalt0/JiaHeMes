using Microsoft.Extensions.Logging;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;

namespace ChargePadLine.Service.Trace.Impl
{
    public class EqumentTraceinfoService : IEqumentTraceinfoService
    {
        private readonly IRepository<EquipmentTracinfo> _equmentTracRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DeviceInfoCollectionService> _logger;
        public EqumentTraceinfoService(IRepository<EquipmentTracinfo> equmentTracRepo, AppDbContext dbContext, ILogger<DeviceInfoCollectionService> logger)
        {
            _equmentTracRepo = equmentTracRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<PaginatedList<EqumentTraceinfoDto>> PaginationAsync(int current, int pageSize, string? productionLine, string? deviceName, string? DeviceEnCode, DateTime? startTime, DateTime? endTime)
        {
            // 关联查询获取所需字段
            var query = from et in _dbContext.EquipmentTracinfos
                        join di in _dbContext.DeviceInfos on et.DeviceEnCode equals di.DeviceEnCode
                        join pl in _dbContext.ProductionLines on di.ProductionLineId equals pl.ProductionLineId
                        orderby et.CreateTime descending
                        select new EqumentTraceinfoDto
                        {
                            SendTime= et.SendTime,
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

            if (!string.IsNullOrEmpty(DeviceEnCode))
            {
                query = query.Where(r => r.DeviceEnCode.Contains(DeviceEnCode));
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

        public async Task<List<EquipmentTracinfo>> GetEquipmentTracinfosListByDeviceEnCode(string DeviceEnCode,int size)
        {
            var result = await _dbContext.EquipmentTracinfos
                                         .Where(e => e.DeviceEnCode == DeviceEnCode)
                                         .OrderByDescending(e => e.CreateTime)
                                         .Take(size)
                                        .ToListAsync();
            return result ?? new List<EquipmentTracinfo>();
        }

        public async Task<EquipmentTracinfo?> GetParamByDeviceEnCode(string DeviceEnCode)
        {
            var result = await _dbContext.EquipmentTracinfos
                                         .Where(e => e.DeviceEnCode == DeviceEnCode)
                                         .OrderByDescending(e => e.CreateTime)
                                         .FirstOrDefaultAsync();
            return result;
        }
    }
}
