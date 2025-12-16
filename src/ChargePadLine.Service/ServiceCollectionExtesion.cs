
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Service.Migration;
using ChargePadLine.Service.Migration.Impl;
using ChargePadLine.Service.OperationLog;
using ChargePadLine.Service.OperationLog.Impl;
using ChargePadLine.Service.Systems;
using ChargePadLine.Service.Systems.Impl;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Impl;
using ChargePadLine.Service.Dashboard;
using ChargePadLine.Service.Dashboard.Impl;

namespace ChargePadLine.Service
{
    public static class ServiceCollectionExtesion
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region System配置
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IDeptService, DeptService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            #endregion

            #region 业务配置
            services.AddScoped<IDeviceInfoCollectionService, DeviceInfoCollectionService>();
            services.AddScoped<IEqumentTraceinfoService, EqumentTraceinfoService>();
            services.AddScoped<IDeviceInfoService, DeviceInfoService>();
            services.AddScoped<IProductionLineService, ProductionLineService>();
            services.AddScoped<IProductTraceInfoCollectionService, ProductTraceInfoCollectionService>();
            services.AddScoped<IProductTraceInfoService, ProductTraceInfoService>();
            services.AddScoped<ICompanyDashboardService, CompanyDashboardService>();
            services.AddScoped<IOperationLogService, OperationLogService>();
            // 历史记录服务（使用ReportDbContext）
            services.AddScoped<IHistoryEqumentTraceinfoService, HistoryEqumentTraceinfoService>();
            services.AddScoped<IHistoryProductTraceInfoService, HistoryProductTraceInfoService>();
            #endregion

            #region 数据迁移服务配置
            services.AddScoped<IMigrationService, MigrationServiceImpl>(sp =>
            {
                var appDbContext = sp.GetRequiredService<AppDbContext>();
                var reportDbContext = sp.GetRequiredService<ReportDbContext>();
                var logger = sp.GetRequiredService<ILogger<MigrationServiceImpl>>();
                var migrationConfig = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ChargePadLine.Common.Config.MigrationConfig>>();
                return new MigrationServiceImpl(appDbContext, reportDbContext, logger, migrationConfig);
            });

            // 添加数据迁移定时作业
            services.AddMigrationQuartzJobs();
            #endregion
            return services;
        }
    }
}
