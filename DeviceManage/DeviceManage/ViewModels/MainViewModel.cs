using DeviceManage.Services;
using Reactive.Bindings;
using System.Windows;
using DeviceManage.Views;
using Microsoft.Extensions.DependencyInjection;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeviceManage.ViewModels;

/// <summary>
/// 主窗口视图模型 - 使用简化的MVVM页面加载
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    public ReactiveProperty<string> CurrentPageTitle { get; }
    public ReactiveProperty<object?> CurrentView { get; }
    public ReactiveProperty<bool> SidebarVisible { get; }

    // 页面映射字典
    private readonly Dictionary<string, PageInfo> _pageMap;

    public MainViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;

        // 初始化ReactiveProperty
        CurrentPageTitle = new ReactiveProperty<string>("仪表盘");
        CurrentView = new ReactiveProperty<object?>();
        SidebarVisible = new ReactiveProperty<bool>(true);

        // 初始化页面映射
        _pageMap = new Dictionary<string, PageInfo>
        {
            { "Dashboard", new PageInfo("仪表盘", typeof(DashboardViewModel)) },
            { "PLCDeviceManagement", new PageInfo("PLC设备管理", typeof(PlcDeviceViewModel)) },
            { "RecipeManagement", new PageInfo("配方管理", typeof(RecipeViewModel)) },
            { "TagManagement", new PageInfo("点位管理", typeof(TagViewModel)) },
            { "DeviceStatus", new PageInfo("设备状态", typeof(DeviceStatusViewModel)) },
            { "Configuration", new PageInfo("配置管理", typeof(ConfigurationViewModel)) },
            { "SystemSettings", new PageInfo("系统设置", typeof(SystemSettingsViewModel)) },
            { "LogManagement", new PageInfo("日志管理", typeof(LogManagementViewModel)) },
            { "UserManagement", new PageInfo("用户管理", typeof(UserManagementViewModel)) }
        };

        NavigateToCommand = new ReactiveCommand<string>().WithSubscribe(NavigateTo);
        LogoutCommand = new ReactiveCommand().WithSubscribe(Logout);
        ToggleSidebarCommand = new ReactiveCommand().WithSubscribe(ToggleSidebar);

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

    public ReactiveCommand<string> NavigateToCommand { get; }
    public ReactiveCommand LogoutCommand { get; }
    public ReactiveCommand ToggleSidebarCommand { get; }

    #endregion

    #region Private Methods

    private void NavigateTo(string pageName)
    {
        if (!_pageMap.ContainsKey(pageName))
        {
            return;
        }

        var pageInfo = _pageMap[pageName];
        CurrentPageTitle.Value = pageInfo.Title;

        try
        {
            // 根据页面名称加载对应的视图
            object? view = null;

            switch (pageName)
            {
                case "Dashboard":
                    {
                        view = new Views.DashboardView();
                        break;
                    }
                case "PLCDeviceManagement":
                    {
                        var plcVm = (PlcDeviceViewModel)DeviceManage.Helpers.ViewModelLocator
                            .Instance
                            .GetViewModel(typeof(PlcDeviceViewModel));

                        var plcView = new Views.PlcDeviceView();
                        plcView.DataContext = plcVm;

                        view = plcView;
                        break;
                    }
                case "RecipeManagement":
                    {
                        var recipeVm = (RecipeViewModel)DeviceManage.Helpers.ViewModelLocator
                            .Instance
                            .GetViewModel(typeof(RecipeViewModel));
                        var recipeView = new Views.RecipeView();
                        recipeView.DataContext = recipeVm;
                        view = recipeView;
                        break;
                    }
                case "TagManagement":
                    {
                        var tagVm = (TagViewModel)DeviceManage.Helpers.ViewModelLocator
                            .Instance
                            .GetViewModel(typeof(TagViewModel));
                        var tagView = new Views.TagView();
                        tagView.DataContext = tagVm;
                        view = tagView;
                        break;
                    }
                default:
                    {
                        var viewModel = DeviceManage.Helpers.ViewModelLocator.Instance.GetViewModel(pageInfo.ViewModelType);
                        view = viewModel;
                        break;
                    }
            }

            CurrentView.Value = view;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"页面切换失败: {ex.Message}\n\n异常详情: {ex.StackTrace}", "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Logout()
    {
        try
        {
            var serviceProvider = DeviceManage.Helpers.ViewModelLocator.Instance.GetServiceProvider();
            if (serviceProvider != null)
            {
                var newMainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                var loginWindow = new LoginWindow(newMainViewModel);
                loginWindow.Show();

                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    mainWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("无法获取服务提供者，退出登录失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"退出登录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ToggleSidebar()
    {
        SidebarVisible.Value = !SidebarVisible.Value;
    }

    #endregion
}
