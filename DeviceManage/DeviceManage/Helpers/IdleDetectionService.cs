using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeviceManage.Helpers
{
    /// <summary>
    /// 空闲检测服务 - 检测用户无操作时间，超过指定时间后自动退出程序
    /// </summary>
    public class IdleDetectionService
    {
        private DispatcherTimer? _idleTimer;
        private DateTime _lastActivityTime;
        private readonly TimeSpan _idleTimeout;
        private bool _isEnabled;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="idleTimeoutMinutes">空闲超时时间（分钟），默认3分钟</param>
        public IdleDetectionService(int idleTimeoutMinutes = 3)
        {
            _idleTimeout = TimeSpan.FromMinutes(idleTimeoutMinutes);
            _lastActivityTime = DateTime.Now;
            _isEnabled = false;
        }

        /// <summary>
        /// 启动空闲检测
        /// </summary>
        public void Start()
        {
            if (_isEnabled)
                return;

            _isEnabled = true;
            _lastActivityTime = DateTime.Now;

            // 创建定时器，每秒检查一次
            _idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();

            // 监听全局输入事件（鼠标和键盘）
            InputManager.Current.PreProcessInput += OnPreProcessInput;
        }

        /// <summary>
        /// 停止空闲检测
        /// </summary>
        public void Stop()
        {
            if (!_isEnabled)
                return;

            _isEnabled = false;

            if (_idleTimer != null)
            {
                _idleTimer.Stop();
                _idleTimer.Tick -= IdleTimer_Tick;
                _idleTimer = null;
            }

            InputManager.Current.PreProcessInput -= OnPreProcessInput;
        }

        /// <summary>
        /// 重置空闲时间（手动调用，用于某些特殊操作后重置计时）
        /// </summary>
        public void ResetIdleTime()
        {
            _lastActivityTime = DateTime.Now;
        }

        /// <summary>
        /// 处理输入事件，更新最后活动时间
        /// </summary>
        private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            // 检查输入类型
            var input = e.StagingItem?.Input;
            
            // 鼠标事件
            if (input is MouseEventArgs || 
                input is MouseButtonEventArgs ||
                input is MouseWheelEventArgs)
            {
                _lastActivityTime = DateTime.Now;
                return;
            }

            // 键盘事件
            if (input is KeyEventArgs ||
                input is TextCompositionEventArgs)
            {
                _lastActivityTime = DateTime.Now;
                return;
            }
        }

        /// <summary>
        /// 定时器事件处理，检查是否超时
        /// </summary>
        private void IdleTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isEnabled)
                return;

            var idleTime = DateTime.Now - _lastActivityTime;

            if (idleTime >= _idleTimeout)
            {
                // 超时，退出程序
                Stop();
                
                // 在UI线程上执行退出操作
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        // 忽略退出时的异常
                    }
                });
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}

