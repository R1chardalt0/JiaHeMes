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
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            if (!viewModel.CanCloseWindow())
            {
                e.Cancel = true; // 取消关闭
            }
        }
    }
}