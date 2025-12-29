using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DeviceManage.CustomerControl
{
    public enum ToastType
    {
        Success,
        Warning,
        Error
    }

    public partial class Toast : UserControl
    {
        private DispatcherTimer _timer;
        private Storyboard _fadeOutStoryboard;
        
        public Toast()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeFadeOutAnimation();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(4);
            _timer.Tick += (s, e) => 
            {
                _timer.Stop();
                StartFadeOut();
            };
        }

        private void InitializeFadeOutAnimation()
        {
            _fadeOutStoryboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            Storyboard.SetTarget(animation, ToastGrid);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            _fadeOutStoryboard.Children.Add(animation);
            _fadeOutStoryboard.Completed += (s, e) => 
            {
                if (this.Parent is Panel panel)
                {
                    panel.Children.Remove(this);
                }
            };
        }

        private void StartFadeOut()
        {
            _fadeOutStoryboard.Begin();
        }

        public void ShowMessage(string message, ToastType type)
        {
            MessageText.Text = message;
            
            switch (type)
            {
                case ToastType.Success:
                    ToastBorder.Style = FindResource("SuccessStyle") as Style;
                    IconText.Text = "√"; // 更通用的成功图标
                    break;
                case ToastType.Warning:
                    ToastBorder.Style = FindResource("WarningStyle") as Style;
                    IconText.Text = "!"; // 更通用的警告图标
                    break;
                case ToastType.Error:
                    ToastBorder.Style = FindResource("ErrorStyle") as Style;
                    IconText.Text = "×"; // 更通用的错误图标
                    break;
            }
            
            _timer.Start();
        }
    }

    // 静态辅助类，方便在应用程序中使用Toast
    public static class ToastManager
    {
        private static Panel _container;
        private static bool _isAutoInitialized = false;

        public static void Initialize(Panel container)
        {
            _container = container;
            _isAutoInitialized = false;
        }

        public static void ShowSuccess(string message)
        {
            Show(message, ToastType.Success);
        }

        public static void ShowWarning(string message)
        {
            Show(message, ToastType.Warning);
        }

        public static void ShowError(string message)
        {
            Show(message, ToastType.Error);
        }

        private static void Show(string message, ToastType type)
        {
            if (_container == null)
            {
                AutoInitialize();
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                var toast = new Toast();
                _container.Children.Add(toast);
                toast.ShowMessage(message, type);
            });
        }

        private static void AutoInitialize()
        {
            if (_isAutoInitialized)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                // 获取主窗口
                if (App.Current.MainWindow != null)
                {
                    // 查找主窗口中的内容面板
                    var mainGrid = App.Current.MainWindow.Content as Panel;
                    
                    if (mainGrid == null)
                    {
                        // 如果没有找到合适的面板，创建一个新的Grid作为Toast容器
                        var grid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 10, 0, 0)
                        };

                        // 保存原始内容
                        var originalContent = App.Current.MainWindow.Content;
                        
                        // 创建新的Grid作为主容器
                        var rootGrid = new Grid();
                        
                        // 添加原始内容
                        if (originalContent != null)
                        {
                            rootGrid.Children.Add(originalContent as UIElement);
                        }
                        
                        // 添加Toast容器
                        rootGrid.Children.Add(grid);

                        // 设置为窗口内容
                        App.Current.MainWindow.Content = rootGrid;
                        
                        _container = grid;
                    }
                    else
                    {
                        // 如果找到了面板，直接使用
                        _container = mainGrid;
                    }
                }
                else
                {
                    // 如果没有主窗口，创建一个新的面板
                    _container = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                }
                
                _isAutoInitialized = true;
            });
        }
    }
} 