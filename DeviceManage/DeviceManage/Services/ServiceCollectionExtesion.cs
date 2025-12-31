using DeviceManage.DBContext.Repository;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services
{
    public static class ServiceCollectionExtesion
    {
        public static IServiceCollection AddDeviceManageServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IPlcDeviceService, PlcDeviceService>();
            return services;
        }
    }
}
