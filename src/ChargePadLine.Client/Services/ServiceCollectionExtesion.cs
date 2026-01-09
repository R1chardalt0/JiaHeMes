using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;
using ChargePadLine.Client.Services.PlcService.Plc10;
using ChargePadLine.Client.Services.PlcService.plc11.安装支架;
using ChargePadLine.Client.Services.PlcService.plc11.客户码激光刻印;
using ChargePadLine.Client.Services.PlcService.Plc11;
using ChargePadLine.Client.Services.PlcService.Plc2;
using ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷;
using ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试;
using ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using ChargePadLine.Client.Services.PlcService.Plc3;
using ChargePadLine.Client.Services.PlcService.plc4.后盖超声波焊接;
using ChargePadLine.Client.Services.PlcService.plc4.干区气密测试;
using ChargePadLine.Client.Services.PlcService.Plc4;
using ChargePadLine.Client.Services.PlcService.plc5.转子充磁与装配;
using ChargePadLine.Client.Services.PlcService.Plc5;
using ChargePadLine.Client.Services.PlcService.plc7.止推垫片装配;
using ChargePadLine.Client.Services.PlcService.Plc7;
using ChargePadLine.Client.Services.PlcService.plc8.旋融焊;
using ChargePadLine.Client.Services.PlcService.Plc8;
using ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<StatorEnterModel>();
            services.AddSingleton<StatorExitModel>();
            services.AddSingleton<RingEnterModel>();
            services.AddSingleton<RingExitModel>();

            services.AddSingleton<导热胶涂敷DataService>();
            services.AddSingleton<电机腔气密测试DataService>();
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

            services.AddTransient<电机腔气密测试EnterMiddleWare>();
            services.AddTransient<电机腔气密测试ExitMiddleWare>();
   
            services.AddTransient<PCBA性能检测EnterMiddleWare>();
            services.AddTransient<PCBA性能检测ExitMiddleWare>();
            services.AddTransient<热铆EnterMiddleWare>();
            services.AddTransient<热铆ExitMiddleWare>();

            services.AddTransient<干区气密测试EnterMiddleWare>();
            services.AddTransient<干区气密测试ExitMiddleWare>();
            services.AddTransient<后盖超声波焊接EnterMiddleWare>();
            services.AddTransient<后盖超声波焊接ExitMiddleWare>();

            services.AddTransient<转子充磁与装配EnterMiddleWare>();
            services.AddTransient<转子充磁与装配ExitMiddleWare>();

            services.AddTransient<止推垫片装配EnterMiddleWare>();
            services.AddTransient<止推垫片装配ExitMiddleWare>();

            services.AddTransient<旋融焊EnterMiddleWare>();
            services.AddTransient<旋融焊ExitMiddleWare>();

            services.AddTransient<湿区气密EnterMiddleWare>();
            services.AddTransient<湿区气密ExitMiddleWare>();

            services.AddTransient<EOLEnterMiddleWare>();
            services.AddTransient<EOLExitMiddleWare>();

            services.AddTransient<激光刻印EnterMiddleWare>();
            services.AddTransient<激光刻印ExitMiddleWare>();
            services.AddTransient<安装支架EnterMiddleWare>();
            services.AddTransient<安装支架ExitMiddleWare>();
            #endregion

            #region 系统信息服务注册
            // services.AddSingleton<ISystemInfoService, SystemInfoService>();
            #endregion

            return services;
        }
    }
}