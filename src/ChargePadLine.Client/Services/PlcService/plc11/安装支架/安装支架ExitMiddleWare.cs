using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc11;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.安装支架
{
    public class 安装支架ExitMiddleWare:IPlc11Task
    {
        private readonly ILogger<安装支架ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 安装支架ExitModel _exitModel;

        public 安装支架ExitMiddleWare(ILogger<安装支架ExitMiddleWare> logger, ILogService logService, 安装支架ExitModel exitModel)
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
                var enterok = s7Net.ReadBool("DB4020.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                _exitModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "安装支架出站请求收到");
                    s7Net.Write("DB4020.12.0", true);
                    s7Net.Write("DB4020.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.12.0", false);
                    s7Net.Write("DB4020.2.4", false);
                    s7Net.Write("DB4020.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "安装支架出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"安装支架ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
