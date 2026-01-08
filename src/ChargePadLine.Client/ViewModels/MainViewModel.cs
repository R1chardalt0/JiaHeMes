using System.Windows.Input;
using ChargePadLine.Client.Commands;
using ChargePadLine.Client.Services;
using Reactive.Bindings;

namespace ChargePadLine.Client.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly MonitorViewModel _monitorViewModel;

    public MainViewModel(MonitorViewModel monitorViewModel, LogViewModel logViewModel)
    {

        CpuUsage = new ReactiveProperty<string>("18%");
        MemoryUsage = new ReactiveProperty<string>("18%");
        HardDisk = new ReactiveProperty<string>("18%");
        Health = new ReactiveProperty<string>("18%");
        CurrentTime = new ReactiveProperty<DateTime>(DateTime.Now);
        CurrentShift = new ReactiveProperty<string>("A");
        _monitorViewModel = monitorViewModel;
        LogViewModel = logViewModel;
    }

    public LogViewModel LogViewModel { get; }

    public MonitorViewModel MonitorViewModel => _monitorViewModel;


    public ReactiveProperty<string> CpuUsage { get; }
    public ReactiveProperty<string> MemoryUsage { get; }
    public ReactiveProperty<string> HardDisk { get; }
    public ReactiveProperty<string> Health { get; }
    public ReactiveProperty<DateTime> CurrentTime { get; }
    public ReactiveProperty<string> CurrentShift { get; }
}

