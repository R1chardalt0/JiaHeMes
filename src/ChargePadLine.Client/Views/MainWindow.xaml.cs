using System.Windows;
using ChargePadLine.Client.ViewModels;

namespace ChargePadLine.Client.Views;

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