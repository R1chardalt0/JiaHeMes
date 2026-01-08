using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.Plc2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services
{
    public static class ServiceCollectionExtesion
    {
        public static IServiceCollection AddMesManageServices(this IServiceCollection services, IConfiguration configuration)
        {

            #region plc服务注册
            services.AddTransient<S7NetConnect>();
            services.AddTransient<ModbusConnect>();
            services.AddHostedService<Plc1HostService>();
            services.AddHostedService<Plc2HostService>();
            #endregion

            #region 页面推送服务注册      
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<StatorTestDataService>();
            #endregion

            #region plc交互业务注册
            services.AddTransient<定子检测MiddleWare>();
            services.AddTransient<O型圈装配MiddleWare>();
            #endregion

            return services;
        }
    }
}