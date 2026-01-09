using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc10;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc10.EOL测试
{
    public class EOLExitMiddleWare : IPlc10Task
    {
        private readonly ILogger<EOLExitMiddleWare> _logger;
        private readonly ILogService _logService;

        public EOLExitMiddleWare(ILogger<EOLExitMiddleWare> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: 在这里实现 EOLEnterMiddleWare相关的 PLC 读写逻辑
                // 例如：
                // var req = s7Net.ReadBool("DB201.1000.0").Content;
                // ...

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"EOLEnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
