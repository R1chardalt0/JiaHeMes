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
    public class PCBA性能检测EnterMiddleWare : IPlc3Task
    {
        private readonly ILogger<PCBA性能检测EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly PCBA性能检测EnterModel _entermodel;

        public PCBA性能检测EnterMiddleWare(ILogger<PCBA性能检测EnterMiddleWare> logger, ILogService logService, PCBA性能检测EnterModel entermodel)
        {
            _logger = logger;
            _logService = logService;
            _entermodel = entermodel;
        }


        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4010.6.0").Content;
                var resp = s7Net.ReadBool("DB4010.10.0").Content;
                var enterok = s7Net.ReadBool("DB4010.2.0").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4010.2.1").Content;//进站NG
                var sn = s7Net.ReadString("DB4010.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _entermodel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "PCBA性能检测进站请求收到");
                    s7Net.Write("DB4010.10.0", true);
                    s7Net.Write("DB4010.2.0", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.10.0", false);
                    s7Net.Write("DB4010.2.0", false);
                    s7Net.Write("DB4010.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "PCBA性能检测进站请求复位");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"PCBA性能检测EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
