using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using DeviceManage.ViewModels;

namespace DeviceManage.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        // 本地模拟账号 + 对应密码（后续替换成接口/数据库校验即可）
        private static readonly Dictionary<string, string> FakeUsers = new()
        {
             #region
            { "admin", "admin123" },
            { "QE", "QE123" },
            { "ME", "ME123" },
            { "TL", "TL123" },
            { "OP", "OP123" },
            #endregion
        };

        private readonly MainViewModel _mainViewModel;

        // 由 DI 创建（App.xaml.cs 里 services.AddTransient<LoginWindow>() 会走这个构造函数）
        public LoginWindow(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            InitializeComponent();

            // 初始化提示语显示状态
            UpdateUserNamePlaceholder();
            UpdatePasswordPlaceholder();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var userName = (UserNameTextBox.Text ?? string.Empty).Trim();
            var pwd = GetCurrentPassword();

            // 若输入的账号不在上述5个中，提示「用户名不存在」
            if (!FakeUsers.ContainsKey(userName))
            {
                MessageBox.Show("用户名不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 若账号存在但密码不正确，提示「密码错误」
            if (FakeUsers[userName] != pwd)
            {
                MessageBox.Show("密码错误", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 若账号和密码都正确：轻量提示（不阻塞）+ 直接跳转主窗口
            _ = ShowTopToastAsync("登录成功");

            // MainWindow 需要 MainViewModel 构造参数，这里用同一个 ViewModel 实例创建
            var mainWindow = new MainWindow(_mainViewModel);
            mainWindow.Show();

            Close();
        }

        private async Task ShowTopToastAsync(string message)
        {
            try
            {
                TopToastText.Text = message;
                TopToast.Visibility = Visibility.Visible;

                // 淡入
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                TopToast.BeginAnimation(OpacityProperty, fadeIn);

                // 停留 2 秒
                await Task.Delay(2000);

                // 淡出
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
                fadeOut.Completed += (_, __) => { TopToast.Visibility = Visibility.Collapsed; };
                TopToast.BeginAnimation(OpacityProperty, fadeOut);
            }
            catch
            {
                // 忽略提示异常，避免影响登录流程
            }
        }

        private string GetCurrentPassword()
        {
            // 根据“显示/隐藏密码”状态获取当前密码
            if (ShowPasswordToggleButton.IsChecked == true)
            {
                return VisiblePasswordTextBox.Text ?? string.Empty;
            }

            return HiddenPasswordBox.Password ?? string.Empty;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == UserNameTextBox)
            {
                UpdateUserNamePlaceholder();
                return;
            }

            if (sender == VisiblePasswordTextBox)
            {
                // 显示模式下，保持与隐藏 PasswordBox 同步
                HiddenPasswordBox.Password = VisiblePasswordTextBox.Text;
                UpdatePasswordPlaceholder();
                return;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordToggleButton.IsChecked == true)
            {
                // 隐藏模式变化时，同步显示 TextBox
                VisiblePasswordTextBox.Text = HiddenPasswordBox.Password;
            }

            UpdatePasswordPlaceholder();
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            // 切到“显示密码”模式
            VisiblePasswordTextBox.Text = HiddenPasswordBox.Password;
            VisiblePasswordTextBox.Visibility = Visibility.Visible;
            HiddenPasswordBox.Visibility = Visibility.Collapsed;

            VisiblePasswordTextBox.CaretIndex = VisiblePasswordTextBox.Text.Length;
            VisiblePasswordTextBox.Focus();
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            // 切回“隐藏密码”模式
            HiddenPasswordBox.Password = VisiblePasswordTextBox.Text;
            HiddenPasswordBox.Visibility = Visibility.Visible;
            VisiblePasswordTextBox.Visibility = Visibility.Collapsed;

            HiddenPasswordBox.Focus();
        }

        private void UpdateUserNamePlaceholder()
        {
            UserNamePlaceholder.Visibility = string.IsNullOrEmpty(UserNameTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdatePasswordPlaceholder()
        {
            var pwd = ShowPasswordToggleButton.IsChecked == true
                ? VisiblePasswordTextBox.Text
                : HiddenPasswordBox.Password;

            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(pwd)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
