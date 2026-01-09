using ChargePadLine.Client.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷
{
    public class 导热胶涂敷EnterMiddleWare : IPlc2Task
    {
        private readonly ILogService _logService;
        private readonly ILogger<导热胶涂敷EnterMiddleWare> _logger;
        public 导热胶涂敷EnterMiddleWare(ILogService logService, ILogger<导热胶涂敷EnterMiddleWare> logger)
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
                await _logService.RecordLogAsync(LogLevel.Error, $"{ex},导热胶涂敷MiddleWare异常");
            }
        }
    }
}
