using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc11.安装支架;
using ChargePadLine.Client.Services.PlcService.plc11.客户码激光刻印;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChargePadLine.Client.Services.PlcService.Plc11
{
    public class Plc11HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc11HostService> _logger;
        private readonly IEnumerable<IPlc11Task> _tasks;
        private readonly ILogService _logService;

        public Plc11HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc11HostService> logger,
            ILogService logService,
            激光刻印EnterMiddleWare 激光刻印Enter,
            激光刻印ExitMiddleWare 激光刻印Exit,
            激光刻印MasterMiddleWare 激光刻印Master,
            安装支架EnterMiddleWare 安装支架Enter,
            安装支架ExitMiddleWare 安装支架Exit,
            安装支架MasterMiddleWare 安装支架Master
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            //  在这里统一整合 PLC11 下的所有业务任务
            _tasks = new IPlc11Task[]
            {
                激光刻印Enter,
                激光刻印Exit,
                激光刻印Master,
                安装支架Enter,
                安装支架Exit,
                安装支架Master
            };
        }

        private void InitializeModbusConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc11.IpAddress, _plcConfig.Plc11.Port);
                string logMsg = $"PLC11连接初始化成功: {_plcConfig.Plc11.IpAddress}:{_plcConfig.Plc11.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC11连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc11.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeModbusConnection();

            if (_s7Net == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC11 未能成功连接，后台监控任务不会启动。");
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
                    await Task.Delay(_plcConfig.Plc11.ScanInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await _logService.RecordLogAsync(LogLevel.Error, $"PLC11 后台监控任务异常: {ex.Message}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            await _logService.RecordLogAsync(LogLevel.Information, "PLC11 后台监控任务已停止。");
        }
    }
}
