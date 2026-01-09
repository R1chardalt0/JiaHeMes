using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;
using ChargePadLine.Client.Services.PlcService.Plc9;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试
{
    public class 湿区气密EnterMiddleWare: IPlc9Task
    {
        private readonly ILogger<湿区气密EnterMiddleWare> _logger;
        private readonly ILogService _logService;

        public 湿区气密EnterMiddleWare(ILogger<湿区气密EnterMiddleWare> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: 在这里实现 湿区气密EnterMiddleWare相关的 PLC 读写逻辑
                // 例如：
                // var req = s7Net.ReadBool("DB201.1000.0").Content;
                // ...

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"湿区气密EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
