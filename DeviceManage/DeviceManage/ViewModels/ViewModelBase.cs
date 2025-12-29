using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeviceManage.ViewModels;

/// <summary>
/// 视图模型基类，实现INotifyPropertyChanged
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

