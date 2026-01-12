using Reactive.Bindings;

namespace ChargePadLine.Client.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly MonitorViewModel _monitorViewModel;
    //private readonly ISystemInfoService _systemInfoService;
    private readonly System.Timers.Timer _updateTimer;
    private bool _disposed = false;

    public MainViewModel(MonitorViewModel monitorViewModel, LogViewModel logViewModel
       // ,ISystemInfoService systemInfoService
       )
    {
        _monitorViewModel = monitorViewModel;
        LogViewModel = logViewModel;
        //_systemInfoService = systemInfoService;

        CpuUsage = new ReactiveProperty<string>("0%");
        MemoryUsage = new ReactiveProperty<string>("0%");
        HardDisk = new ReactiveProperty<string>("0%");
        Health = new ReactiveProperty<string>("就绪");
        CurrentTime = new ReactiveProperty<string>(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        CurrentShift = new ReactiveProperty<string>(GetCurrentShift());

        // 创建定时器，每1秒更新一次系统信息
        _updateTimer = new System.Timers.Timer(1000); // 1秒更新一次
        _updateTimer.Elapsed += OnTimerElapsed;
        _updateTimer.AutoReset = true;
        _updateTimer.Start();

        // 立即更新一次数据
        UpdateSystemInfo();
    }

    public LogViewModel LogViewModel { get; }

    public MonitorViewModel MonitorViewModel => _monitorViewModel;

    public ReactiveProperty<string> CpuUsage { get; }
    public ReactiveProperty<string> MemoryUsage { get; }
    public ReactiveProperty<string> HardDisk { get; }
    public ReactiveProperty<string> Health { get; }
    public ReactiveProperty<string> CurrentTime { get; }
    public ReactiveProperty<string> CurrentShift { get; }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        UpdateSystemInfo();
    }

    private void UpdateSystemInfo()
    {
        // 更新CPU使用率
        //var cpuUsage = _systemInfoService.GetCpuUsage();
        //CpuUsage.Value = $"{cpuUsage}%";

        //// 更新内存使用率
        //var memoryUsage = _systemInfoService.GetMemoryUsage();
        //MemoryUsage.Value = $"{memoryUsage}%";

        //// 更新硬盘使用率
        //var diskUsage = _systemInfoService.GetDiskUsage();
        //HardDisk.Value = $"{diskUsage}%";

        // 更新时间和班次
        CurrentTime.Value = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
        CurrentShift.Value = GetCurrentShift();
    }

    private string GetCurrentShift()
    {
        var now = DateTime.Now;
        var time = now.TimeOfDay;
        
        // A班: 8:25 ~ 20:25
        var startTimeA = new TimeSpan(8, 25, 0);
        var endTimeA = new TimeSpan(20, 25, 0);
        
        // 如果当前时间在A班时间内
        if ((time >= startTimeA && time < endTimeA))
        {
            return "当班次：A";
        }
        else
        {
            // B班: 20:25 ~ 8:25 (跨天)
            return "当班次：B";
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            _disposed = true;
        }
    }
}

