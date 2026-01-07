using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Client.ViewModels;

namespace ChargePadLine.Client.Services.PlcService
{
    public class LogService: ILogService
    {
        private readonly ILogger<LogService> _logger;
        private readonly LogViewModel _logViewModel;

        public LogService(ILogger<LogService> logger, LogViewModel logViewModel)
        {
            _logger = logger;
            _logViewModel = logViewModel;
        }

        public async Task RecordLogAsync(LogLevel logLevel, string message)
        {
            // Create a log message
            var logMessage = new LogMessage
            {
                Timestamp = DateTime.Now,
                Level = logLevel.ToString(),
                Content = message
            };

            _logViewModel.AddLogMsg.Execute(logMessage);

            switch (logLevel)
            {
                case LogLevel.Trace:
                    _logger.LogTrace(message);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(message);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical(message);
                    break;
            }
        }

        //public enum LogLevel
        //{
        //    Trace,
        //    Debug,
        //    Information,
        //    Warning,
        //    Error,
        //    Critical
        //}
    }
}
