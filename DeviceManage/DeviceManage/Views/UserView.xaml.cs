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
using System.Windows.Controls.Primitives;
using DeviceManage.CustomerControl;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;

namespace DeviceManage.Views
{
    /// <summary>
    /// UserView.xaml 的交互逻辑
    /// </summary>
    public partial class UserView : UserControl
    {
        public UserView()
        {
            InitializeComponent();
            this.Loaded += UserView_Loaded;
        }

        private void UserView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.UserViewModel viewModel)
            {
                // 监听对话框打开/关闭事件，清空密码框
                viewModel.IsDialogOpen.Subscribe(isOpen =>
                {
                    if (isOpen)
                    {
                        // 对话框打开时，清空密码框
                        PasswordBox.Clear();
                    }
                });
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.UserViewModel viewModel && sender is PasswordBox passwordBox)
            {
                if (viewModel.EditingUser.Value != null)
                {
                    viewModel.EditingUser.Value.Password = passwordBox.Password;
                }
            }
        }
    }
}
