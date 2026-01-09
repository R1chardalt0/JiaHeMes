using ChargePadLine.Client.Helpers;
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
    public class 止推垫片装配ExitMiddleWare : IPlc7Task
    {
        private readonly ILogger<止推垫片装配ExitMiddleWare> _logger;
        private readonly ILogService _logService;

        public 止推垫片装配ExitMiddleWare(ILogger<止推垫片装配ExitMiddleWare> logger, ILogService logService)
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
                await _logService.RecordLogAsync(LogLevel.Error, $"止推垫片装配ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
