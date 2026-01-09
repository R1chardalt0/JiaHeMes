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

        public 热铆EnterMiddleWare(ILogger<热铆EnterMiddleWare> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: 在这里实现 O 型圈装配相关的 PLC 读写逻辑
                // 例如：
                // var req = s7Net.ReadBool("DB201.1000.0").Content;
                // ...

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"热铆EnterMiddleWare异常: {ex.Message}");
            }
        }

    }
}
