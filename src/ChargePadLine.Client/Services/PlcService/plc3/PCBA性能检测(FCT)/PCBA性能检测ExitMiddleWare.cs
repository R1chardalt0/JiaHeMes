using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试;
using ChargePadLine.Client.Services.PlcService.Plc3;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_
{
    public class PCBA性能检测ExitMiddleWare : IPlc3Task
    {
        private readonly ILogger<PCBA性能检测ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly PCBA性能检测ExitModel _exitmodel;

        public PCBA性能检测ExitMiddleWare(ILogger<PCBA性能检测ExitMiddleWare> logger, ILogService logService, PCBA性能检测ExitModel exitmodel)
        {
            _logger = logger;
            _logService = logService;
            _exitmodel = exitmodel;
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
                _exitmodel.UpdateData(req, resp, sn, enterok, enterng);
                // 更新数据服务
                //_statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "PCBA性能检测出站请求收到");
                    s7Net.Write("DB4010.12.0", true);
                    s7Net.Write("DB4010.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.12.0", false);
                    s7Net.Write("DB4010.2.4", false);
                    s7Net.Write("DB4010.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "PCBA性能检测出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"PCBA性能检测ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
