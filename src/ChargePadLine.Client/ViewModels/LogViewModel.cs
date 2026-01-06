using System.Collections.ObjectModel;
using Reactive.Bindings;
using System.Windows;

namespace ChargePadLine.Client.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private const int LOGS_MAX = 1000;
        private readonly object _lock = new object();

        public ReactiveProperty<string> Header { get; }
        public ReactiveProperty<ObservableCollection<LogMessage>> Logs { get; }
        public ReactiveCommand<object> AddLogMsg { get; }

        public LogViewModel()
        {
            Header = new ReactiveProperty<string>("系统日志");
            Logs = new ReactiveProperty<ObservableCollection<LogMessage>>(new ObservableCollection<LogMessage>());

            AddLogMsg = new ReactiveCommand<object>();
            AddLogMsg.Subscribe(param =>
            {
                if (param is LogMessage msg)
                {
                    lock (_lock)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (Logs.Value.Count >= LOGS_MAX) Logs.Value.RemoveAt(0);
                            Logs.Value.Add(msg);
                        });
                    }
                }
            });
        }
    }

    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "Info";
        public string Content { get; set; } = "";
    }
}

