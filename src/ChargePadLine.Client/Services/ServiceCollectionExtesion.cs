using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.Plc10;
using ChargePadLine.Client.Services.PlcService.Plc11;
using ChargePadLine.Client.Services.PlcService.Plc2;
using ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷;
using ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试;
using ChargePadLine.Client.Services.PlcService.Plc3;
using ChargePadLine.Client.Services.PlcService.Plc4;
using ChargePadLine.Client.Services.PlcService.Plc5;
using ChargePadLine.Client.Services.PlcService.Plc7;
using ChargePadLine.Client.Services.PlcService.plc8.旋融焊;
using ChargePadLine.Client.Services.PlcService.Plc8;
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
            services.AddHostedService<Plc3HostService>();
            services.AddHostedService<Plc4HostService>();
            services.AddHostedService<Plc5HostService>();
            services.AddHostedService<Plc7HostService>();
            services.AddHostedService<Plc9HostService>();
            services.AddHostedService<Plc10HostService>();
            services.AddHostedService<Plc11HostService>();
            #endregion

            #region 页面推送服务注册      
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<StatorTestDataService>();
            #endregion

            #region plc交互业务注册
            services.AddTransient<定子检测EnterMiddleWare>();
            services.AddTransient<定子检测ExitMiddleWare>();
            services.AddTransient<O型圈装配EnterMiddleWare>();
            services.AddTransient<O型圈装配ExitMiddleWare>();
            services.AddTransient<导热胶涂敷EnterMiddleWare>();
            services.AddTransient<导热胶涂敷ExitMiddleWare>();
            services.AddTransient<电机腔气密测试EnterMiddleWare>();
            services.AddTransient<电机腔气密测试ExitMiddleWare>();
            services.AddTransient<旋融焊EnterMiddleWare>();
            services.AddTransient<旋融焊ExitMiddleWare>();
            services.AddTransient<电机腔气密测试EnterMiddleWare>();
            services.AddTransient<电机腔气密测试ExitMiddleWare>();
            services.AddTransient<旋融焊EnterMiddleWare>();
            services.AddTransient<旋融焊ExitMiddleWare>();
            #endregion

            #region 系统信息服务注册
            // services.AddSingleton<ISystemInfoService, SystemInfoService>();
            #endregion

            return services;
        }
    }
}