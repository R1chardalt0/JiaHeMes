using Microsoft.Extensions.DependencyInjection;
using ChargePadLine.Client.ViewModels;

namespace ChargePadLine.Client.Helpers;

/// <summary>
/// ViewModel定位器（可选，用于XAML中的设计时绑定）
/// </summary>
public class ViewModelLocator
{
    private static IServiceProvider? _serviceProvider;

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public MainViewModel MainViewModel => _serviceProvider?.GetRequiredService<MainViewModel>() 
        ?? throw new InvalidOperationException("ServiceProvider not initialized");
}

