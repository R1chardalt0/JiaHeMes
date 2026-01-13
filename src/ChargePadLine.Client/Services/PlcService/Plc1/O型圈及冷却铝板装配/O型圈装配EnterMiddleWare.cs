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
    public class O型圈装配EnterMiddleWare : IPlc1Task
    {
        private readonly ILogger<O型圈装配EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly RingEnterModel _routingEnterModel;

        public O型圈装配EnterMiddleWare(ILogger<O型圈装配EnterMiddleWare> logger, ILogService logService, RingEnterModel routingEnterModel)
        {
            _logger = logger;
            _logService = logService;
            _routingEnterModel = routingEnterModel;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                string statusMessage = "";
                //plc状态读取
                var malfunction = s7Net.ReadBool("DB4020.4.0").Content;//设备故障
                var auto = s7Net.ReadBool("DB4020.4.1").Content;//自动模式
                var idle = s7Net.ReadBool("DB4020.4.2").Content;//设备空闲
                var manual = s7Net.ReadBool("DB4020.4.3").Content;//手动模式
                var check = s7Net.ReadBool("DB4020.4.4").Content;//审核模式

                if (malfunction) statusMessage = "设备故障";
                else if (auto) statusMessage = "自动模式";
                else if (idle) statusMessage = "设备空闲";
                else if (manual) statusMessage = "手动模式";
                else if (check) statusMessage = "审核模式";
                else statusMessage = "无状态";

                var req = s7Net.ReadBool("DB4020.6.0").Content;
                var resp = s7Net.ReadBool("DB4020.10.0").Content;
                var enterok = s7Net.ReadBool("DB4020.2.0").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.1").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                _routingEnterModel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);
                // 更新数据服务
                //_statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "O型圈进站请求收到");
                    s7Net.Write("DB4020.10.0", true);
                    s7Net.Write("DB4020.2.0", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.10.0", false);
                    s7Net.Write("DB4020.2.0", false);
                    s7Net.Write("DB4020.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "O型圈进站请求复位");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"O型圈进站异常: {ex.Message}");
            }
        }
    }
}
