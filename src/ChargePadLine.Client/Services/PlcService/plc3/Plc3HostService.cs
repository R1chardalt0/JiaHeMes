using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc3
{
    public class Plc3HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc3HostService> _logger;
        private readonly IEnumerable<IPlc3Task> _tasks;
        private readonly ILogService _logService;

        public Plc3HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc3HostService> logger,
            ILogService logService,
            PCBA性能检测EnterMiddleWare PCBA性能检测Enter,
            PCBA性能检测ExitMiddleWare PCBA性能检测Exit,
            PCBA性能检测MasterMiddleWare PCBA性能检测Master,
            热铆EnterMiddleWare 热铆Enter,
            热铆ExitMiddleWare 热铆Exit,
            热铆MasterMiddleWare 热铆Master
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            // 在这里统一整合 PLC3 下的所有业务任务
            _tasks = new IPlc3Task[]
            {
                PCBA性能检测Enter,
                PCBA性能检测Exit,
                PCBA性能检测Master,
                热铆Enter,
                热铆Exit,
                热铆Master
            };
        }

        private void InitializeModbusConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc3.IpAddress, _plcConfig.Plc3.Port);
                string logMsg = $"PLC3连接初始化成功: {_plcConfig.Plc3.IpAddress}:{_plcConfig.Plc3.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC3连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc3.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeModbusConnection();

            if (_s7Net == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC3 未能成功连接，后台监控任务不会启动。");
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
                    await _logService.RecordLogAsync(LogLevel.Error, $"PLC3 后台监控任务异常: {ex.Message}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            await _logService.RecordLogAsync(LogLevel.Information, "PLC3 后台监控任务已停止。");
        }
    }
}
