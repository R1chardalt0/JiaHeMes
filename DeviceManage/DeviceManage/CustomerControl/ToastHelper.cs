using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeviceManage.CustomerControl
{
    /// <summary>
    /// Toast帮助类，用于管理Toast的初始化
    /// </summary>
    public static class ToastHelper
    {
        /// <summary>
        /// 初始化窗口或控件中的Toast容器
        /// </summary>
        /// <param name="element">包含ToastContainer的元素</param>
        public static void InitializeToast(FrameworkElement element)
        {
            if (element == null)
                return;

            // 查找名为ToastContainer的面板
            var container = FindElementByName(element, "ToastContainer") as Panel;
            if (container != null)
            {
                // 初始化ToastManager
                ToastManager.Initialize(container);
            }
        }

        /// <summary>
        /// 在窗口加载完成后初始化Toast
        /// </summary>
        /// <param name="window">要初始化Toast的窗口</param>
        public static void InitializeToastForWindow(Window window)
        {
            if (window == null)
                return;

            // 当窗口加载完成后初始化Toast
            window.Loaded += (s, e) =>
            {
                InitializeToast(window);
            };
        }

        /// <summary>
        /// 在用户控件加载完成后初始化Toast
        /// </summary>
        /// <param name="control">要初始化Toast的用户控件</param>
        public static void InitializeToastForControl(UserControl control)
        {
            if (control == null)
                return;

            // 当控件加载完成后初始化Toast
            control.Loaded += (s, e) =>
            {
                InitializeToast(control);
            };
        }

        /// <summary>
        /// 根据名称查找元素
        /// </summary>
        /// <param name="parent">父元素</param>
        /// <param name="name">要查找的元素名称</param>
        /// <returns>找到的元素，如果未找到则返回null</returns>
        private static DependencyObject FindElementByName(DependencyObject parent, string name)
        {
            if (parent == null)
                return null;

            // 检查当前元素
            if (parent is FrameworkElement element && element.Name == name)
                return element;

            // 递归查找子元素
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindElementByName(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}