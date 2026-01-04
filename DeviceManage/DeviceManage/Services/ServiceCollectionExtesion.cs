using DeviceManage.DBContext.Repository;
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

            return services;
        }
    }
}
