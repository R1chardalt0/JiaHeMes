using System.Windows.Input;
using DeviceManage.Commands;
using DeviceManage.Services;
using GalaSoft.MvvmLight.Command;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;


namespace DeviceManage.ViewModels;

/// <summary>
/// 主窗口视图模型 - 设备管理系统主界面
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string _statusMessage = "就绪";
    private string _connectionStatus = "已连接";
    private string _currentUser = "管理员";
    private string _currentPageTitle = "仪表盘";
    private object? _currentView;
    private bool _sidebarExpanded = true;
    private bool _sidebarVisible = true;

    public MainViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        NavigateToCommand = new RelayCommand<string>(NavigateTo);
        LogoutCommand = new RelayCommand(Logout);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
        NavigateTo("Dashboard");
    }

    #region Properties

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set { _connectionStatus = value; OnPropertyChanged(); }
    }

    public string CurrentUser
    {
        get => _currentUser;
        set { _currentUser = value; OnPropertyChanged(); }
    }

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        set { _currentPageTitle = value; OnPropertyChanged(); }
    }

    public object? CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    public bool SidebarExpanded
    {
        get => _sidebarExpanded;
        set { _sidebarExpanded = value; OnPropertyChanged(); }
    }

    public bool SidebarVisible
    {
        get => _sidebarVisible;
        set { _sidebarVisible = value; OnPropertyChanged(); }
    }

    public ICommand NavigateToCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand ToggleSidebarCommand { get; }

    #endregion

    #region Private Methods

    private void NavigateTo(string pageName)
    {
        CurrentPageTitle = GetPageTitle(pageName);
        StatusMessage = $"正在加载 {CurrentPageTitle}...";
        
        // 在实际应用中，这里应该创建并设置相应的ViewModel
        // 示例：CurrentView = pageName switch { "DeviceManagement" => new DeviceManagementViewModel(), ... };
        
        StatusMessage = $"{CurrentPageTitle} - 加载完成";
    }

    private void Logout()
    {
        StatusMessage = "用户已退出登录";
        // 实现退出登录逻辑
    }

    private void ToggleSidebar()
    {
        SidebarVisible = !SidebarVisible;
        SidebarExpanded = SidebarVisible; // When sidebar is visible, it's expanded; when hidden, it's collapsed
        StatusMessage = SidebarVisible ? "侧边栏已展开" : "侧边栏已收起";
    }

    private string GetPageTitle(string pageName) => pageName switch
    {
        "Dashboard" => "仪表盘",
        "DeviceManagement" => "设备管理",
        "DeviceStatus" => "设备状态", 
        "Configuration" => "配置管理",
        "SystemSettings" => "系统设置",
        "LogManagement" => "日志管理",
        "UserManagement" => "用户管理",
        _ => "仪表盘"
    };

    #endregion
}

