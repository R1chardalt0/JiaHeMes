using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
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
            services.AddTransient<S7NetConnect>();
            services.AddTransient<ModbusConnect>();
            services.AddSingleton<定子检测MiddleWare>();
            services.AddSingleton<O型圈装配MiddleWare>();
            services.AddHostedService<Plc1HostService>();
            return services;
        }
    }
}