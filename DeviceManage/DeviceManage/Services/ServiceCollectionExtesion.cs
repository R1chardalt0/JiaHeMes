using DeviceManage.DBContext.Repository;
using DeviceManage.Helpers;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManage.Services
{
    public static class ServiceCollectionExtesion
    {
        public static IServiceCollection AddDeviceManageServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IPlcDeviceService, PlcDeviceService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ILogService, LogService>();

            services.AddTransient<ISwitchRecipeService, SwitchRecipeService>();
            services.AddTransient<S7NetConnect>();
            services.AddTransient<ModbusConnect>();
            return services;
        }
    }
}
