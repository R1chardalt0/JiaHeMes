using ChargePadLine.Client.Services;
using ChargePadLine.Client.ViewModels;
using ChargePadLine.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Data;
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

            // 加载配置
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            // 配置依赖注入
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
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
}

