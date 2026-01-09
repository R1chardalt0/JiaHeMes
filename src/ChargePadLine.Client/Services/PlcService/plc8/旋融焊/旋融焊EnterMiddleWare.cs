using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc8;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc8.旋融焊
{
    public class 旋融焊EnterMiddleWare : IPlc8Task
    {
        private readonly ILogger<旋融焊EnterMiddleWare> _logger;
        private readonly ILogService _logService;

        public 旋融焊EnterMiddleWare(ILogger<旋融焊EnterMiddleWare> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }
        public async Task ExecuteOnceAsync(ModbusConnect modbus, CancellationToken cancellationToken)
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
                await _logService.RecordLogAsync(LogLevel.Error, $"旋融焊EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
