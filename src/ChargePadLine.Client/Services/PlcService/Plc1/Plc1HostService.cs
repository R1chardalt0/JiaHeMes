using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1
{
    /// <summary>
    /// PLC1 后台服务：只负责连接 PLC、加载并调度各个业务 MiddleWare
    /// </summary>
    public class Plc1HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc1HostService> _logger;
        private readonly IEnumerable<IPlc1Task> _tasks;

        public Plc1HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc1HostService> logger,
            定子检测MiddleWare 定子检测,
            O型圈装配MiddleWare o型圈装配)
        {
            _plcConfig = config.Value;
            _logger = logger;

            // 在这里统一整合 PLC1 下的所有业务任务
            _tasks = new IPlc1Task[]
            {
                定子检测,
                o型圈装配
            };
        }

        private void InitializeModbusConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc1.IpAddress, _plcConfig.Plc1.Port);
                string logMsg = $"PLC1连接初始化成功: {_plcConfig.Plc1.IpAddress}:{_plcConfig.Plc1.Port}";
                _logger.LogInformation(logMsg);
            }
            catch (Exception ex)
            {
                string logMsg = "PLC1连接初始化失败: " + ex.Message;
                _logger.LogError(logMsg);
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitializeModbusConnection();

            if (_s7Net == null)
            {
                _logger.LogError("PLC1 未能成功连接，后台监控任务不会启动。");
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

                    // 统一控制轮询周期
                    await Task.Delay(10, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PLC1 后台监控任务异常");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("PLC1 后台监控任务已停止。");
        }
    }
}
