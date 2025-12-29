using System.Windows;

namespace DeviceManage.Helpers;

/// <summary>
/// 对话框辅助类
/// </summary>
public static class DialogHelper
{
    /// <summary>
    /// 显示消息框
    /// </summary>
    public static MessageBoxResult ShowMessage(string message, string title = "提示", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
    {
        return MessageBox.Show(message, title, button, icon);
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    public static bool ShowConfirm(string message, string title = "确认")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    /// <summary>
    /// 显示错误消息
    /// </summary>
    public static void ShowError(string message, string title = "错误")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

