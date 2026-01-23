using System.Windows;
using ChargePadLine.Client.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;

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
        this.Closing += MainWindow_Closing;
    }

    /// <summary>
    /// 窗口关闭事件处理
    /// </summary>
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // 显示确认关闭对话框
        var result = MessageBox.Show("确定要关闭程序吗?", "确认关闭", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.No)
        {
            // 如果用户点击"否"，则阻止窗口关闭
            e.Cancel = true;
        }
    }
}