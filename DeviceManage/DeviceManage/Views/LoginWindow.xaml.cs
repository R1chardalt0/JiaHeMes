using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using DeviceManage.Helpers;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.ViewModels;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DeviceManage.Views
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : System.Windows.Window
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IUserService _userService;

        public LoginWindow(MainViewModel mainViewModel, IUserService userService)
        {
            _mainViewModel = mainViewModel;
            _userService = userService;

            InitializeComponent();

            // 初始化提示语显示状态
            UpdateUserNamePlaceholder();
            UpdatePasswordPlaceholder();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var userName = (UserNameTextBox.Text ?? string.Empty).Trim();
            var pwd = GetCurrentPassword();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(pwd))
            {
                Growl.Warning("请输入用户名或密码！");
                return;
            }

            try
            {
                // 从数据库查询用户
                var user = await _userService.GetUserByUsernameAsync(userName);
                
                if (user == null)
                {
                    Growl.Error("该用户名或密码不存在！");
                    return;
                }

                // 检查用户是否已删除
                if (user.IsDeleted)
                {
                    Growl.Error("该用户已被删除！");
                    return;
                }

                // 检查用户是否启用
                if (!user.IsEnabled)
                {
                    Growl.Error("该用户已被禁用，请联系管理员！");
                    return;
                }

                // 对输入的密码进行MD5加密后与数据库中的密码比较
                var encryptedPassword = MD5Helper.Encrypt(pwd);
                if (user.Password != encryptedPassword)
                {
                    Growl.Error("密码错误,请重新输入！");
                    return;
                }

                // 更新最后登录时间（只更新LastLoginAt，不更新其他字段）
                var userToUpdate = new DeviceManage.Models.User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = string.Empty, // 不更新密码，保持原密码不变
                    Role = user.Role,
                    RealName = user.RealName,
                    Email = user.Email,
                    Phone = user.Phone,
                    IsEnabled = user.IsEnabled,
                    LastLoginAt = DateTime.Now, // 只更新最后登录时间
                    Remarks = user.Remarks
                };
                await _userService.UpdateUserAsync(userToUpdate);

                _ = ShowTopToastAsync("登录成功");

                var mainWindow = new MainWindow(_mainViewModel);
                mainWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                Growl.Error($"登录失败：{ex.Message}");
            }
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
