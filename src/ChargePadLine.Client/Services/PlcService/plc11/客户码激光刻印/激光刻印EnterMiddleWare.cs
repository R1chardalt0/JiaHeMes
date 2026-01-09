using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc11.安装支架;
using ChargePadLine.Client.Services.PlcService.Plc11;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.客户码激光刻印
{
    public class 激光刻印EnterMiddleWare: IPlc11Task
    {
        private readonly ILogger<激光刻印EnterMiddleWare> _logger;
        private readonly ILogService _logService;

        public 激光刻印EnterMiddleWare(ILogger<激光刻印EnterMiddleWare> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: 在这里实现 激光刻印EnterMiddleWare相关的 PLC 读写逻辑
                // 例如：
                // var req = s7Net.ReadBool("DB201.1000.0").Content;
                // ...

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"激光刻印EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
