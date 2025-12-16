using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Dashboard;
using ChargePadLine.Service.Dashboard.Dto;
using ChargePadLine.Service.Trace;

namespace ChargePadLine.WebApi.Controllers.Dashboard
{
    /// <summary>
    /// 公司级数据看板接口
    /// </summary>
    public class CompanyDashboardController : BaseController
    {
        private readonly ICompanyDashboardService _dashboardService;
        private readonly AppDbContext _dbContext;
        private readonly IDeviceInfoService _deviceInfoService;

        public CompanyDashboardController(
            ICompanyDashboardService dashboardService,
            AppDbContext dbContext,
            IDeviceInfoService deviceInfoService)
        {
            _dashboardService = dashboardService;
            _dbContext = dbContext;
            _deviceInfoService = deviceInfoService;
        }

        /// <summary>
        /// 获取公司级看板概览
        /// </summary>
        /// <param name="companyId">公司ID（与companyName二选一）</param>
        /// <param name="companyName">公司名称（支持：精研热能、博研科技、精研集团等）</param>
        /// <param name="startTime">开始时间（默认当天00:00）</param>
        /// <param name="endTime">结束时间（默认当前时间）</param>
        [HttpGet]
        public async Task<Resp<CompanyDashboardOverviewDto>> GetOverview(int? companyId, string? companyName, DateTime? startTime, DateTime? endTime)
        {
            try
            {
                // 默认公司：精研集团
                if (!companyId.HasValue && string.IsNullOrWhiteSpace(companyName))
                {
                    companyName = "精研集团";
                }
                var data = await _dashboardService.GetOverviewAsync(companyId, companyName, startTime, endTime);
                return RespExtensions.MakeSuccess(data);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<CompanyDashboardOverviewDto>("500", ex.Message);
            }
        }

        /// <summary>
        /// 获取公司产线列表
        /// </summary>
        /// <param name="companyId">公司ID</param>
        /// <param name="companyName">公司名称（companyId缺省时可用）</param>
        [HttpGet]
        public async Task<Resp<List<ProductionLine>>> GetProductionLines(int? companyId, string? companyName)
        {
            try
            {
                int? cid = companyId;
                if (!cid.HasValue && !string.IsNullOrWhiteSpace(companyName))
                {
                    var company = await _dbContext.SysCompanys.FirstOrDefaultAsync(c => c.CompanyName == companyName);
                    cid = company?.CompanyId;
                }
                if (!cid.HasValue)
                {
                    return RespExtensions.MakeSuccess(new List<ProductionLine>());
                }
                var lines = await _dbContext.ProductionLines
                    .Where(pl => pl.CompanyId == cid.Value)
                    .OrderBy(pl => pl.ProductionLineName)
                    .ToListAsync();
                return RespExtensions.MakeSuccess(lines);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<ProductionLine>>("500", ex.Message);
            }
        }

        /// <summary>
        /// 获取指定产线下的设备列表
        /// </summary>
        /// <param name="productionLineId">产线ID</param>
        [HttpGet]
        public async Task<Resp<List<DeviceInfo>>> GetDevicesByLine(Guid productionLineId)
        {
            try
            {
                var list = await _deviceInfoService.GetDeviceInfosByProductionLineId(productionLineId);
                return RespExtensions.MakeSuccess(list);
            }
            catch (Exception ex)
            {
                return RespExtensions.MakeFail<List<DeviceInfo>>("500", ex.Message);
            }
        }
    }
}
