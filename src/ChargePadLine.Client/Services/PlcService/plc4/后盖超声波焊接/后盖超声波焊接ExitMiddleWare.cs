using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using ChargePadLine.Client.Services.PlcService.Plc4;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc4.后盖超声波焊接
{
    public class 后盖超声波焊接ExitMiddleWare : IPlc4Task
    {
        private readonly ILogger<后盖超声波焊接ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 后盖超声波焊接ExitModel _exitModel;

        public 后盖超声波焊接ExitMiddleWare(ILogger<后盖超声波焊接ExitMiddleWare> logger, ILogService logService, 后盖超声波焊接ExitModel exitModel = null)
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
                // 更新数据服务
                //_statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "后盖超声波焊接出站请求收到");
                    s7Net.Write("DB4010.12.0", true);
                    s7Net.Write("DB4010.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.12.0", false);
                    s7Net.Write("DB4010.2.4", false);
                    s7Net.Write("DB4010.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "后盖超声波焊接出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"后盖超声波焊接ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
