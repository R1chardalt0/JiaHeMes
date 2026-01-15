using System.Collections.ObjectModel;
using Reactive.Bindings;
using System.Windows;
using System.Linq;

namespace ChargePadLine.Client.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private const int LOGS_MAX = 1000;
        private readonly object _lock = new object();

        public ReactiveProperty<string> Header { get; }
        public ReactiveProperty<ObservableCollection<LogMessage>> Logs { get; }
        public ReactiveProperty<LogType> FilterType { get; }
        public ReactiveCommand<object> AddLogMsg { get; }
        public ReactiveCommand ClearLogs { get; }

        public LogViewModel()
        {
            Header = new ReactiveProperty<string>("系统日志");
            Logs = new ReactiveProperty<ObservableCollection<LogMessage>>(new ObservableCollection<LogMessage>());
            FilterType = new ReactiveProperty<LogType>(LogType.All);

            AddLogMsg = new ReactiveCommand<object>();
            AddLogMsg.Subscribe(param =>
            {
                if (param is LogMessage msg)
                {
                    AddLog(msg);
                }
            });

            ClearLogs = new ReactiveCommand();
            ClearLogs.Subscribe(() =>
            {
                lock (_lock)
                {
                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Logs.Value.Clear();
                        });
                    }
                    else
                    {
                        Logs.Value.Clear();
                    }
                }
            });
        }

        /// <summary>
        /// 添加日志消息（线程安全）
        /// </summary>
        public void AddLog(LogMessage logMessage)
        {
            if (logMessage == null) return;

            lock (_lock)
            {
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Logs.Value.Count >= LOGS_MAX) Logs.Value.RemoveAt(0);
                        Logs.Value.Add(logMessage);
                    });
                }
                else
                {
                    // 如果 Dispatcher 不可用，直接添加（这种情况不应该发生）
                    if (Logs.Value.Count >= LOGS_MAX) Logs.Value.RemoveAt(0);
                    Logs.Value.Add(logMessage);
                }
            }
        }
    }

    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "Info";
        public LogType Type { get; set; } = LogType.System;
        public string Content { get; set; } = "";
        public string UserName { get; set; } = "";
    }

    public enum LogType
    {
        All = 0,
        System = 1,
        Operation = 2
    }
}

