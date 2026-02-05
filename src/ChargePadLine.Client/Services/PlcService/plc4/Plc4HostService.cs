using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.plc4.后盖超声波焊接;
using ChargePadLine.Client.Services.PlcService.plc4.干区气密测试;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc4
{
    public class Plc4HostService : BackgroundService
    {
        private S7NetConnect? _s7Net;
        private readonly PlcConfig _plcConfig;
        private readonly ILogger<Plc4HostService> _logger;
        private readonly IEnumerable<IPlc4Task> _tasks;
        private readonly ILogService _logService;

        public Plc4HostService(
            IOptions<PlcConfig> config,
            ILogger<Plc4HostService> logger,
            ILogService logService,
            干区气密EnterMiddleWare 干区气密测试Enter,
            干区气密ExitMiddleWare 干区气密测试Exit,
            干区气密MasterMiddleWare 干区气密测试Master,
            后盖超声波焊接EnterMiddleWare 后盖超声波焊接Enter,
            后盖超声波焊接ExitMiddleWare 后盖超声波焊接Exit,
            后盖超声波焊接MasterMiddleWare 后盖超声波焊接Master
            )
        {
            _plcConfig = config.Value;
            _logger = logger;
            _logService = logService;

            // 在这里统一整合 PLC4 下的所有业务任务
            _tasks = new IPlc4Task[]
            {
                干区气密测试Enter,
                干区气密测试Exit,
                干区气密测试Master,
                后盖超声波焊接Enter,
                后盖超声波焊接Exit,
                后盖超声波焊接Master
            };
        }

        private void InitializeS7NetConnection()
        {
            if (_s7Net != null) return;

            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc4.IpAddress, _plcConfig.Plc4.Port);
                string logMsg = $"PLC4连接初始化成功: {_plcConfig.Plc4.IpAddress}:{_plcConfig.Plc4.Port}";
                _logService.RecordLogAsync(LogLevel.Information, logMsg).Wait();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC4连接初始化失败: " + ex.Message;
                _logService.RecordLogAsync(LogLevel.Error, logMsg).Wait();
            }
        }

        /// <summary>
        /// 长期运行任务：循环调用各个业务任务的 ExecuteOnceAsync
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_plcConfig.Plc4.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                return;
            }
            InitializeS7NetConnection();

            if (_s7Net == null)
            {
                await _logService.RecordLogAsync(LogLevel.Error, "PLC4 未能成功连接，后台监控任务不会启动。");
                return;
            }

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        foreach (var task in _tasks)
                        {
                            await task.ExecuteOnceAsync(_s7Net, stoppingToken);
                        }
                        await Task.Delay(_plcConfig.Plc4.ScanInterval, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        await _logService.RecordLogAsync(LogLevel.Error, $"PLC4 后台监控任务异常: {ex.Message}");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            finally
            {
                // 确保在服务停止时释放连接资源
                await _logService.RecordLogAsync(LogLevel.Information, "PLC4 后台监控任务正在停止，释放资源...");
                _s7Net?.Dispose();
                await _logService.RecordLogAsync(LogLevel.Information, "PLC4 后台监控任务已停止。");
            }
        }

        public void Dispose()
        {
            _s7Net?.Dispose();
        }
    }
}
