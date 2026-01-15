using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChargePadLine.Client.Services.PlcService.Plc9
{
    public class Plc9HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc9HostService> _logger;
        private readonly IEnumerable<IPlc9Task> _tasks;
        private readonly ILogService _logService;

        public Plc9HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc9HostService> logger,
            ILogService logService,
            湿区气密EnterMiddleWare 湿区气密Enter,
            湿区气密ExitMiddleWare 湿区气密Exit,
            湿区气密MasterMiddleWare 湿区气密Master
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            // 在这里统一整合 PLC9 下的所有业务任务
            _tasks = new IPlc9Task[]
            {
                湿区气密Enter,
                湿区气密Exit,
                湿区气密Master
            };
        }

        private void InitializeModbusConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc9.IpAddress, _plcConfig.Plc9.Port);
                string logMsg = $"PLC9连接初始化成功: {_plcConfig.Plc9.IpAddress}:{_plcConfig.Plc9.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC9连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc9.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeModbusConnection();

            if (_s7Net == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC9 未能成功连接，后台监控任务不会启动。");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var task in _tasks)
                    {
                        await task.ExecuteOnceAsync(_s7Net, stoppingToken);
                    }
                    await Task.Delay(10, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await _logService.RecordLogAsync(LogLevel.Error, $"PLC9 后台监控任务异常: {ex.Message}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            await _logService.RecordLogAsync(LogLevel.Information, "PLC9 后台监控任务已停止。");
        }
    }
}
