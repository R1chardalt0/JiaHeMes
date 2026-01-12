using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using ChargePadLine.Client.Services.PlcService.plc5.转子充磁与装配;
using ChargePadLine.Client.Services.PlcService.Plc7;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc7.止推垫片装配
{
    public class 止推垫片装配EnterMiddleWare : IPlc7Task
    {
        private readonly ILogger<止推垫片装配EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 止推垫片装配EnterModel _enterModel;

        public 止推垫片装配EnterMiddleWare(ILogger<止推垫片装配EnterMiddleWare> logger, ILogService logService, 止推垫片装配EnterModel enterModel)
        {
            _logger = logger;
            _logService = logService;
            _enterModel = enterModel;
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
                _enterModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "止推垫片装配进站请求收到");
                    s7Net.Write("DB4010.10.0", true);
                    s7Net.Write("DB4010.2.0", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.10.0", false);
                    s7Net.Write("DB4010.2.0", false);
                    s7Net.Write("DB4010.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "止推垫片装配进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"止推垫片装配EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
