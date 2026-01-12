using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc11;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.客户码激光刻印
{
    public class 激光刻印ExitMiddleWare : IPlc11Task
    {
        private readonly ILogger<激光刻印ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 激光刻印ExitModel _exitModel;

        public 激光刻印ExitMiddleWare(ILogger<激光刻印ExitMiddleWare> logger, ILogService logService, 激光刻印ExitModel exitModel)
        {
            _logger = logger;
            _logService = logService;
            _exitModel = exitModel;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4010.6.4").Content;
                var resp = s7Net.ReadBool("DB4010.12.0").Content;
                var enterok = s7Net.ReadBool("DB4010.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4010.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB4010.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                _exitModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "激光刻印出站请求收到");
                    s7Net.Write("DB4010.12.0", true);
                    s7Net.Write("DB4010.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.12.0", false);
                    s7Net.Write("DB4010.2.4", false);
                    s7Net.Write("DB4010.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "激光刻印出站请求复位");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"激光刻印ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
