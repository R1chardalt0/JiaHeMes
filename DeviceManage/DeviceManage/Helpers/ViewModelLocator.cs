using Microsoft.Extensions.DependencyInjection;
using DeviceManage.ViewModels;
using System;

namespace DeviceManage.Helpers;

/// <summary>
/// ViewModel定位器 - 简化MVVM模式
/// </summary>
public class ViewModelLocator
{
    private static IServiceProvider? _serviceProvider;
    private static ViewModelLocator? _instance;

    public static ViewModelLocator Instance => _instance ??= new ViewModelLocator();

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public MainViewModel MainViewModel => _serviceProvider?.GetRequiredService<MainViewModel>() 
        ?? throw new InvalidOperationException("ServiceProvider not initialized");

    /// <summary>
    /// 获取指定类型的ViewModel实例
    /// </summary>
    public object GetViewModel(Type viewModelType)
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("ServiceProvider not initialized");
        
        // 直接解析 ViewModel，其依赖的服务（如 IPlcDeviceService）现在都是 Transient
        return _serviceProvider.GetRequiredService(viewModelType);
    }

    // 各个ViewModel的属性访问器
    public PlcDeviceViewModel PlcDeviceViewModel => _serviceProvider?.GetRequiredService<PlcDeviceViewModel>() 
        ?? throw new InvalidOperationException("ServiceProvider not initialized");

    /// <summary>
    /// 获取 ServiceProvider（用于创建窗口等）
    /// </summary>
    public IServiceProvider? GetServiceProvider() => _serviceProvider;
}

