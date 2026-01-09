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

        public 后盖超声波焊接ExitMiddleWare(ILogger<后盖超声波焊接ExitMiddleWare> logger, ILogService logService)
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
                await _logService.RecordLogAsync(LogLevel.Error, $"后盖超声波焊接ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
