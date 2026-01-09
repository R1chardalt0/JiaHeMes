using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc11;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.安装支架
{
    public class 安装支架ExitMiddleWare:IPlc11Task
    {
        private readonly ILogger<安装支架ExitMiddleWare> _logger;
        private readonly ILogService _logService;

        public 安装支架ExitMiddleWare(ILogger<安装支架ExitMiddleWare> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: 在这里实现 安装支架ExitMiddleWare相关的 PLC 读写逻辑
                // 例如：
                // var req = s7Net.ReadBool("DB201.1000.0").Content;
                // ...

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"安装支架ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
