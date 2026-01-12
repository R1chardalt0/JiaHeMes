using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_;
using ChargePadLine.Client.Services.PlcService.Plc3;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.热铆
{
    public class 热铆ExitMiddleWare : IPlc3Task
    {
        private readonly ILogger<热铆ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 热铆ExitModel _exitModel;

        public 热铆ExitMiddleWare(ILogger<热铆ExitMiddleWare> logger, ILogService logService, 热铆ExitModel exitModel)
        {
            _logger = logger;
            _logService = logService;
            _exitModel = exitModel;
        }
        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4020.6.4").Content;
                var resp = s7Net.ReadBool("DB4020.12.0").Content;
                var exitok = s7Net.ReadBool("DB4020.2.4").Content;//出站OK
                var exitng = s7Net.ReadBool("DB4020.2.5").Content;//出站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _exitModel.UpdateData(req, resp, sn, exitok, exitng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "热铆出站请求收到");
                    s7Net.Write("DB4020.12.0", true);
                    s7Net.Write("DB4020.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.12.0", false);
                    s7Net.Write("DB4020.2.4", false);
                    s7Net.Write("DB4020.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "热铆出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"热铆ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
