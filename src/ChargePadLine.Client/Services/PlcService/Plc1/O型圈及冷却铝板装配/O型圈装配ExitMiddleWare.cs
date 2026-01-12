using ChargePadLine.Client.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配
{
    /// <summary>
    /// O 型圈装配业务逻辑，单次执行一次轮询，由外部服务控制循环与频率
    /// </summary>
    public class O型圈装配ExitMiddleWare : IPlc1Task
    {
        private readonly ILogger<O型圈装配ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly RingExitModel _routingExitModel;

        public O型圈装配ExitMiddleWare(ILogger<O型圈装配ExitMiddleWare> logger, ILogService logService, RingExitModel routingExitModel)
        {
            _routingExitModel = routingExitModel;
            _logger = logger;
            _logService = logService;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4020.6.4").Content;
                var resp = s7Net.ReadBool("DB4020.12.0").Content;
                var enterok = s7Net.ReadBool("DB4020.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                _routingExitModel.UpdateData(req, resp, sn, enterok, enterng);
                // 更新数据服务
                //_statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "O型圈出站请求收到");
                    s7Net.Write("DB4020.12.0", true);
                    s7Net.Write("DB4020.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.12.0", false);
                    s7Net.Write("DB4020.2.4", false);
                    s7Net.Write("DB4020.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "O型圈出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"O型圈出站异常: {ex.Message}");
            }
        }
    }
}
