using DeviceManage.DBContext;
using DeviceManage.Services;
using DeviceManage.ViewModels;
using DeviceManage.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DeviceManage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;
        
        // 静态ServiceProvider供View访问
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // 加载配置
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                _configuration = builder.Build();

                // 配置依赖注入
                var services = new ServiceCollection();
                ConfigureServices(services);
                services.AddDeviceManageServices(_configuration);

            //使用pgSql
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            InitializeDatabaseAndStartServices(_serviceProvider);
            // 创建主窗口
            //var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            //mainWindow.Show();

            var mainWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 注册配置
            services.AddSingleton(_configuration!);

            // 从配置读取API地址
            var apiBaseUrl = _configuration!["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            var timeout = int.Parse(_configuration["ApiSettings:Timeout"] ?? "30");

            // 注册HttpClient和ApiClient
            services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(timeout);
            });

            // 注册 DbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_configuration.GetConnectionString("AppDbContext"));
                options.UseNpgsql(s => s.MigrationsAssembly(typeof(App).Assembly.GetName().Name));
                //抛出sql文本
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // 注册ViewModels - 简化的MVVM模式
            services.AddSingleton<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<PlcDeviceViewModel>();
            services.AddTransient<DeviceStatusViewModel>();
            services.AddTransient<ConfigurationViewModel>();
            services.AddTransient<SystemSettingsViewModel>();
            services.AddTransient<LogManagementViewModel>();
            services.AddTransient<UserManagementViewModel>();

            // 注册Windows
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

        #region 数据库服务
        void InitializeDatabaseAndStartServices(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                try
                {
                    // 初始化AppDbContext数据库
                    var dbContext = scopedServices.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                    var logger = scopedServices.GetRequiredService<ILogger<App>>();
                    logger.LogInformation("数据库创建成功");
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<App>>();
                    logger.LogError(ex, "初始化过程中发生错误");
                    throw;
                }
            }
        }
        #endregion
    }
}

