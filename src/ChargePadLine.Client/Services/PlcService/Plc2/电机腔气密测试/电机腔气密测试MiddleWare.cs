using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试
{
    public class 电机腔气密测试MiddleWare : IPlc2Task
    {
        private readonly ILogService _logService;
        private readonly ILogger<电机腔气密测试MiddleWare> _logger;
        public 电机腔气密测试MiddleWare(ILogService logService, ILogger<电机腔气密测试MiddleWare> logger)
        {
            _logService = logService;
            _logger = logger;
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
                await _logService.RecordLogAsync(LogLevel.Error, $"{ex},电机腔气密测试MiddleWare异常");
            }
        }
    }
}
