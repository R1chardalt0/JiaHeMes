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
    public class 热铆EnterMiddleWare : IPlc3Task
    {
        private readonly ILogger<热铆EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 热铆EnterModel _enterModel;

        public 热铆EnterMiddleWare(ILogger<热铆EnterMiddleWare> logger, ILogService logService, 热铆EnterModel enterModel)
        {
            _logger = logger;
            _logService = logService;
            _enterModel = enterModel;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4020.6.0").Content;
                var resp = s7Net.ReadBool("DB4020.10.0").Content;
                var enterok = s7Net.ReadBool("DB4020.2.0").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.1").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _enterModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "热铆进站请求收到");
                    s7Net.Write("DB4020.10.0", true);
                    s7Net.Write("DB4020.2.0", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.10.0", false);
                    s7Net.Write("DB4020.2.0", false);
                    s7Net.Write("DB4020.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "热铆进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"热铆EnterMiddleWare异常: {ex.Message}");
            }
        }

    }
}
