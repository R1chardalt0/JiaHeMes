using System.Windows;
using DeviceManage.ViewModels;

namespace DeviceManage.Views;

/// <summary>
/// 主窗口视图
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}