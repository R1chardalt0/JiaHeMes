using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.plc8.旋融焊;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc8
{
    public class Plc9HostService : BackgroundService
    {
        private ModbusConnect? _modbus;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc9HostService> _logger;
        private readonly IEnumerable<IPlc8Task> _tasks;
        private readonly ILogService _logService;

        public Plc9HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc9HostService> logger,
            ILogService logService,
            旋融焊EnterMiddleWare 旋融焊Enter,
            旋融焊ExitMiddleWare 旋融焊Exit
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            // 在这里统一整合 PLC8 下的所有业务任务
            _tasks = new IPlc9Task[]
            {
                旋融焊Enter,
                旋融焊Exit
            };
        }

        private void InitializeModbusConnection()
        {
            if (_modbus != null) return;

            try
            {
                _modbus = new ModbusConnect();
                _modbus.Connect(_plcConfig.Plc8.IpAddress, _plcConfig.Plc8.Port);
                string logMsg = $"PLC8连接初始化成功: {_plcConfig.Plc8.IpAddress}:{_plcConfig.Plc8.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC8连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc8.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeModbusConnection();

            if (_modbus == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC8 未能成功连接，后台监控任务不会启动。");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var task in _tasks)
                    {
                        await task.ExecuteOnceAsync(_modbus, stoppingToken);
                    }
                    await Task.Delay(10, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await _logService.RecordLogAsync(LogLevel.Error, $"PLC8 后台监控任务异常: {ex.Message}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            await _logService.RecordLogAsync(LogLevel.Information, "PLC8 后台监控任务已停止。");
        }
    }
}
