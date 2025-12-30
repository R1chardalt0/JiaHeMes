using System.Windows.Input;
using DeviceManage.Commands;
using DeviceManage.Services;
using GalaSoft.MvvmLight.Command;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;
using System.Collections.Generic;

namespace DeviceManage.ViewModels;

/// <summary>
/// 主窗口视图模型 - 使用简化的MVVM页面加载
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string _currentPageTitle = "仪表盘";
    private object? _currentView;
    private bool _sidebarVisible = true;
    
    // 页面映射字典
    private readonly Dictionary<string, PageInfo> _pageMap;

    public MainViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        
        // 初始化页面映射
        _pageMap = new Dictionary<string, PageInfo>
        {
            { "Dashboard", new PageInfo("仪表盘", typeof(DashboardViewModel)) },
            { "PLCDeviceManagement", new PageInfo("PLC设备管理", typeof(PlcDeviceViewModel)) },
            { "DeviceStatus", new PageInfo("设备状态", typeof(DeviceStatusViewModel)) },
            { "Configuration", new PageInfo("配置管理", typeof(ConfigurationViewModel)) },
            { "SystemSettings", new PageInfo("系统设置", typeof(SystemSettingsViewModel)) },
            { "LogManagement", new PageInfo("日志管理", typeof(LogManagementViewModel)) },
            { "UserManagement", new PageInfo("用户管理", typeof(UserManagementViewModel)) }
        };
        
        NavigateToCommand = new RelayCommand<string>(NavigateTo);
        LogoutCommand = new RelayCommand(Logout);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
        
        // 默认加载仪表盘
        NavigateTo("Dashboard");
    }
    
    /// <summary>
    /// 页面信息类
    /// </summary>
    private class PageInfo
    {
        public string Title { get; }
        public Type ViewModelType { get; }
        
        public PageInfo(string title, Type viewModelType)
        {
            Title = title;
            ViewModelType = viewModelType;
        }
    }

    #region Properties

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
        if (!_pageMap.ContainsKey(pageName))
        {
            return;
        }

        var pageInfo = _pageMap[pageName];
        CurrentPageTitle = pageInfo.Title;
        
        try
        {
            // 根据页面名称加载对应的视图
            object? view = null;
            
            switch (pageName)
            {
                case "Dashboard":
                    view = new Views.DashboardView();
                    break;
                case "PLCDeviceManagement":
                    view = new Views.PlcDeviceView();
                    break;
                default:
                    // 对于其他页面，使用ViewModelLocator获取ViewModel实例
                    var viewModel = DeviceManage.Helpers.ViewModelLocator.Instance.GetViewModel(pageInfo.ViewModelType);
                    view = viewModel;
                    break;
            }
            
            CurrentView = view;
        }
        catch (Exception)
        {
            // 处理导航错误
            CurrentView = null;
        }
    }

    private void Logout()
    {
        // 实现退出登录逻辑
    }

    private void ToggleSidebar()
    {
        SidebarVisible = !SidebarVisible;
    }

    #endregion
}

