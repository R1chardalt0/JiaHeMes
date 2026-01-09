using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChargePadLine.Client.Services.PlcService.Plc10
{
    public class Plc10HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc10HostService> _logger;
        private readonly IEnumerable<IPlc10Task> _tasks;
        private readonly ILogService _logService;

        public Plc10HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc10HostService> logger,
            ILogService logService,
            EOLEnterMiddleWare EOLEnter,
            EOLExitMiddleWare EOLEXit
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            //在这里统一整合 PLC10 下的所有业务任务
           _tasks = new IPlc10Task[]
           {
                EOLEnter,
                EOLEXit
           };
        }

        private void InitializeModbusConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc10.IpAddress, _plcConfig.Plc10.Port);
                string logMsg = $"PLC10连接初始化成功: {_plcConfig.Plc10.IpAddress}:{_plcConfig.Plc10.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC10连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc10.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeModbusConnection();

            if (_s7Net == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC10 未能成功连接，后台监控任务不会启动。");
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
                    await _logService.RecordLogAsync(LogLevel.Error, $"PLC10 后台监控任务异常: {ex.Message}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            await _logService.RecordLogAsync(LogLevel.Information, "PLC10 后台监控任务已停止。");
        }
    }
}
