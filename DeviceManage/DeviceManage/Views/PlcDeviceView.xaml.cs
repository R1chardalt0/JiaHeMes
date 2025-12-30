using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DeviceManage.CustomerControl;
using Microsoft.Extensions.Logging;

namespace DeviceManage.Views
{
    /// <summary>
    /// PlcDeviceView.xaml 的交互逻辑
    /// </summary>
    public partial class PlcDeviceView : UserControl
    {
        private readonly ILogger<PlcDeviceView> _logger;

        public PlcDeviceView(ILogger<PlcDeviceView> logger = null)
        {
            InitializeComponent();
            _logger = logger;
            this.DataContextChanged += PlcDeviceView_DataContextChanged;
        }

        private void PlcDeviceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _logger?.LogDebug($"PlcDeviceView的DataContext已更改: {e.OldValue?.GetType().Name ?? "null"} -> {e.NewValue?.GetType().Name ?? "null"}");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _logger?.LogDebug($"按钮被点击: {button.Content}, DataContext: {DataContext?.GetType().Name ?? "null"}");

                if (DataContext == null)
                {
                    _logger?.LogError("DataContext为null，命令无法执行！");
                    return;
                }

                // 检查命令绑定
                var binding = button.GetValue(Button.CommandProperty) as System.Windows.Input.ICommand;
                if (binding != null)
                {
                    _logger?.LogDebug($"命令绑定找到: {binding.GetType().Name}");
                    if (binding.CanExecute(button.CommandParameter))
                    {
                        _logger?.LogDebug("命令可以执行");
                    }
                    else
                    {
                        _logger?.LogDebug("命令不能执行");
                    }
                }
                else
                {
                    _logger?.LogError("没有找到命令绑定！");
                }
            }
        }
    }
}
