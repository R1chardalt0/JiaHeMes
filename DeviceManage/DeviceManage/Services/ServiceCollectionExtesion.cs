using DeviceManage.DBContext.Repository;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Impl;
using DeviceManage.Services.RecipeMagService;
using DeviceManage.Services.TagMagService;
using DeviceManage.Services.TagMagService.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManage.Services
{
    public static class ServiceCollectionExtesion
    {
        public static IServiceCollection AddDeviceManageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 注册泛型 Repository
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            // 对于 WPF 应用，使用 Transient 而不是 Scoped，因为 WPF 没有请求 scope 的概念
            services.AddTransient<IPlcDeviceService, PlcDeviceService>();

            // 配方管理
            services.AddTransient<IRecipeService, RecipeMagService.Impl.RecipeService>();
            services.AddTransient<IRecipeItemService, RecipeMagService.Impl.RecipeItemService>();

            // 点位管理
            services.AddTransient<ITagService, TagService>();

            return services;
        }
    }
}
