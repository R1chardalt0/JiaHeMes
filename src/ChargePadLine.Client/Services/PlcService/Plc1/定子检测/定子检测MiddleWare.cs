using ChargePadLine.Client.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ChargePadLine.Client.Services.PlcService;

namespace ChargePadLine.Client.Services.PlcService.Plc1.定子检测
{
    /// <summary>
    /// 定子检测业务逻辑，单次执行一次轮询，由外部服务控制循环与频率
    /// </summary>
    public class 定子检测MiddleWare : IPlc1Task
    {
        private readonly ILogger<定子检测MiddleWare> _logger;
        private readonly StatorTestDataService _statorTestDataService;
        private readonly ILogService _logService;

        public 定子检测MiddleWare(ILogger<定子检测MiddleWare> logger, StatorTestDataService statorTestDataService, ILogService logService)
        {
            _logger = logger;
            _statorTestDataService = statorTestDataService;
            _logService = logService;
        }

        private bool _isInitialized = false;

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {         
                var req = s7Net.ReadBool("DB200.1000.0").Content;
                var resp = s7Net.ReadBool("DB200.1000.1").Content;
                var sn = s7Net.ReadString("DB200.100",100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _statorTestDataService.UpdateData(req, resp, sn);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "定子检测请求收到");
                    s7Net.Write("DB200.1000.1", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB200.1000.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "定子检测请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"定子检测MiddleWare异常: {ex.Message}");
            }
        }
    }
}