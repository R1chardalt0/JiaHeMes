using System.Windows.Controls;
using ChargePadLine.Client.ViewModels;

namespace ChargePadLine.Client.Controls;

/// <summary>
/// 用户控件基类
/// </summary>
public abstract class UserControlBase : UserControl
{
    protected ViewModelBase? ViewModel => DataContext as ViewModelBase;
}

