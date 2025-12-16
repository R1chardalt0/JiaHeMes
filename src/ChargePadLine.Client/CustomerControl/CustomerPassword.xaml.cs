using System.Windows;
using System.Windows.Controls;

namespace ChargePadLine.Client.CustomerControl;

public partial class CustomerPassword : UserControl
{
    public static readonly DependencyProperty PasswordProperty = 
        DependencyProperty.Register("Password", typeof(string), typeof(CustomerPassword), 
            new PropertyMetadata(string.Empty, OnPasswordPropertyChanged));
    
    public static readonly DependencyProperty WatermarkProperty = 
        DependencyProperty.Register("Watermark", typeof(string), typeof(CustomerPassword), 
            new PropertyMetadata("请输入密码"));
    
    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }
    
    private bool isUpdatingPassword = false;
    
    public CustomerPassword()
    {
        InitializeComponent();
        PART_Watermark.Text = Watermark;
        
        this.Loaded += (sender, args) =>
        {
            // 检查是否有初始值
            if (!string.IsNullOrEmpty(Password))
            {
                isUpdatingPassword = true;
                PART_PasswordBox.Password = Password;
                isUpdatingPassword = false;
                UpdateWatermarkVisibility();
            }
        };
    }
    
    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomerPassword passwordBox && e.NewValue is string newPassword)
        {
            if (!passwordBox.isUpdatingPassword && passwordBox.PART_PasswordBox != null)
            {
                passwordBox.isUpdatingPassword = true;
                passwordBox.PART_PasswordBox.Password = newPassword;
                passwordBox.isUpdatingPassword = false;
                passwordBox.UpdateWatermarkVisibility();
            }
        }
    }
    
    private void UpdateWatermarkVisibility()
    {
        PART_Watermark.Visibility = 
            string.IsNullOrEmpty(PART_PasswordBox.Password) && !PART_PasswordBox.IsFocused 
                ? Visibility.Visible 
                : Visibility.Collapsed;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (!isUpdatingPassword)
        {
            isUpdatingPassword = true;
            Password = PART_PasswordBox.Password;
            isUpdatingPassword = false;
        }
        UpdateWatermarkVisibility();
    }

    private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
    {
        UpdateWatermarkVisibility();
    }

    private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
    {
        PART_Watermark.Visibility = Visibility.Collapsed;
    }
}