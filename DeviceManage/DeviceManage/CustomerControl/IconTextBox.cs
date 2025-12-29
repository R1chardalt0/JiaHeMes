using System.Windows;
using System.Windows.Controls;

namespace DeviceManage.CustomerControl;

public class IconTextBox : TextBox
{
    public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register(
        nameof(PlaceHolder), typeof(string), typeof(IconTextBox), new PropertyMetadata(default(string)));

    /// <summary>
    /// 水印文本
    /// </summary>
    public string PlaceHolder
    {
        get { return (string)GetValue(PlaceHolderProperty); }
        set { SetValue(PlaceHolderProperty, value); }
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(string), typeof(IconTextBox), new PropertyMetadata(default(string)));

    /// <summary>
    /// 文本图标
    /// </summary>
    public string Icon
    {
        get { return (string)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    // 用于暂存水印文字，方便恢复
    private string waterText = "请输入用户名";
    private bool isInitialized = false;

    public IconTextBox()
    {
        Loaded += (sender, args) =>
        {
            if (string.IsNullOrEmpty(PlaceHolder))
            {
                return;
            }

            waterText = PlaceHolder; // 用于暂存水印文字，方便恢复

            // 判断是否已有绑定的值，如果没有则显示水印
            if (string.IsNullOrEmpty(Text) || Text == waterText)
            {
                Text = waterText;
            }

            isInitialized = true;
        };

        GotFocus += (sender, args) =>
        {
            if (Text == waterText)
            {
                Text = "";
                PlaceHolder = "";
            }
        };

        LostFocus += (sender, args) =>
        {
            if (string.IsNullOrEmpty(Text))
            {
                Text = waterText;
            }
        };

        TextChanged += (sender, args) =>
        {
            if (!isInitialized || Text == waterText)
                return;

            // 确保在不是水印状态下，更新绑定源
            if (IsFocused || Text != waterText)
            {
                GetBindingExpression(TextProperty)?.UpdateSource();
            }
        };
    }
}