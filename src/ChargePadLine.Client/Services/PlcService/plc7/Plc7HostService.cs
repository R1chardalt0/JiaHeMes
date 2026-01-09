using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc7
{
    public class Plc7HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc7HostService> _logger;
        private readonly IEnumerable<IPlc7Task> _tasks;
        private readonly ILogService _logService;

        public Plc7HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc7HostService> logger
,
            ILogService logService
            //,
            //定子检测MiddleWare 定子检测,
            //O型圈装配MiddleWare o型圈装配
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            // 在这里统一整合 PLC7 下的所有业务任务
            //_tasks = new IPlc7Task[]
            //{
            //    定子检测,
            //    o型圈装配
            //};
        }

        private void InitializeModbusConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc7.IpAddress, _plcConfig.Plc7.Port);
                string logMsg = $"PLC7连接初始化成功: {_plcConfig.Plc7.IpAddress}:{_plcConfig.Plc7.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC7连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc7.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeModbusConnection();

            if (_s7Net == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC7 未能成功连接，后台监控任务不会启动。");
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
                    await _logService.RecordLogAsync(LogLevel.Error, $"PLC7 后台监控任务异常: {ex.Message}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            await _logService.RecordLogAsync(LogLevel.Information, "PLC7 后台监控任务已停止。");
        }
    }
}
