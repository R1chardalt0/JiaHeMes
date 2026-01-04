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
using System.Threading;
using System.Windows;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeviceManage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;
        private Mutex? _mutex;
        private const string MutexName = "设备管理软件";

        protected override void OnStartup(StartupEventArgs e)
        {
            // 检查是否已有实例在运行
            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);
            
            if (!createdNew)
            {
                // 如果 Mutex 已存在，说明程序已在运行
                MessageBox.Show("当前程序已启动，无法重复运行。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                _mutex?.Dispose();
                Shutdown(1);
                return;
            }

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
                // 先注册应用自身的服务，再构建 ServiceProvider
                services.AddDeviceManageServices(_configuration);
                _serviceProvider = services.BuildServiceProvider();

                // 初始化 ViewModelLocator，提供全局的 ServiceProvider
                DeviceManage.Helpers.ViewModelLocator.SetServiceProvider(_serviceProvider);
                //使用pgSql
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                InitializeDatabaseAndStartServices(_serviceProvider);

                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                var errorMessage = $"应用程序启动失败！\n\n错误信息: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\n内部异常: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
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
                options.UseNpgsql(_configuration!.GetConnectionString("AppDbContext"));
                options.UseNpgsql(s => s.MigrationsAssembly(typeof(App).Assembly.GetName().Name));
                //抛出sql文本
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // 注册ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<PlcDeviceViewModel>();
            services.AddTransient<DeviceStatusViewModel>();
            services.AddTransient<ConfigurationViewModel>();
            services.AddTransient<SystemSettingsViewModel>();
            services.AddTransient<LogManagementViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<UserViewModel>();
            services.AddTransient<RecipeViewModel>();
            services.AddTransient<TagViewModel>();

            // 注册Windows
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
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
                    var logger = scopedServices.GetRequiredService<ILogger<App>>();

                    dbContext.Database.EnsureCreated();
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
