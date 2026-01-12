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
    public class 定子检测ExitMiddleWare : IPlc1Task
    {
        private readonly ILogger<定子检测ExitMiddleWare> _logger;
        private readonly StatorExitModel  _statorTestDataService;
        private readonly ILogService _logService;

        public 定子检测ExitMiddleWare(ILogger<定子检测ExitMiddleWare> logger, StatorExitModel  statorTestDataService, ILogService logService)
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
                var req = s7Net.ReadBool("DB4010.6.4").Content;
                var resp = s7Net.ReadBool("DB4010.12.0").Content;
                var enterok = s7Net.ReadBool("DB4010.2.4").Content;//出站OK
                var enterng = s7Net.ReadBool("DB4010.2.5").Content;//出站NG
                var sn = s7Net.ReadString("DB4010.66.0",100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "定子检测出站请求收到");
                    s7Net.Write("DB4010.12.0", true);
                    s7Net.Write("DB4010.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.12.0", false);
                    s7Net.Write("DB4010.2.4", false);
                    s7Net.Write("DB4010.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "定子检测出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"定子检测出站MiddleWare异常: {ex.Message}");
            }
        }
    }
}