using ChargePadLine.Client.Controls;
using ChargePadLine.Client.DBContext;
using ChargePadLine.Client.Services;
using ChargePadLine.Client.ViewModels;
using ChargePadLine.Client.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace ChargePadLine.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;
    private Mutex? _mutex;
    private const string MutexName = "嘉和产线mes监控软件";

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

            // 设置 HslCommunication 授权码
            if (!HslCommunication.Authorization.SetAuthorizationCode("5e8e65ad-ed01-4fbe-b4c2-5f65765b626f"))
            {
                throw new Exception("HslCommunication 授权码设置失败，请检查授权码是否正确");
            }

            // 加载配置
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            // 检查并创建数据库目录
            EnsureDatabaseDirectoryExists();

            // 配置依赖注入
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            InitializeDatabaseAndStartServices(_serviceProvider);
            services.AddMesManageServices(_configuration);

            // 创建主窗口
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
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
        //添加PLC配置绑定
        services.AddOptions<PlcConfig>().Bind(_configuration.GetSection("PlcConfig"));
        // 注册 DbContext
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(_configuration!.GetConnectionString("AppDbContext"));
            options.UseSqlite(s => s.MigrationsAssembly(typeof(App).Assembly.GetName().Name));
            //抛出sql文本
            if (System.Diagnostics.Debugger.IsAttached)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // 添加日志服务
        services.AddLogging(logging =>
        {
            logging.AddLog4Net();
        });
        // 注册ViewModels
        services.AddTransient<MainViewModel>();

        // 注册Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    #region 数据库服务
    /// <summary>
    /// 自动初始化数据库并启动相关服务
    /// </summary>
    /// <param name="services"></param>
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

    /// <summary>
    /// 创建sqlite数据库文件夹及其文件
    /// </summary>
    private void EnsureDatabaseDirectoryExists()
    {
        var connectionString = _configuration!.GetConnectionString("AppDbContext");
        if (!string.IsNullOrEmpty(connectionString))
        {
            // 从连接字符串中提取数据库文件路径
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                {
                    var dataSource = part.Substring("Data Source=".Length).Trim();
                    var databasePath = Path.GetFullPath(dataSource);
                    var databaseDirectory = Path.GetDirectoryName(databasePath);

                    if (!string.IsNullOrEmpty(databaseDirectory) && !Directory.Exists(databaseDirectory))
                    {
                        Directory.CreateDirectory(databaseDirectory);
                    }
                    break;
                }
            }
        }
    }
    #endregion
}

