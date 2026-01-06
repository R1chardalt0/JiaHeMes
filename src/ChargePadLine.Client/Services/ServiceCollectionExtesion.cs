using ChargePadLine.Client.Helpers;
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
            return services;
        }
    }
}