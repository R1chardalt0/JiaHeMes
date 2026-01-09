using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试
{
    public class 电机腔气密测试ExitMiddleWare : IPlc2Task
    {
        private readonly ILogger<电机腔气密测试ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 电机腔气密测试ExitModel _exitModel;

        public 电机腔气密测试ExitMiddleWare(ILogger<电机腔气密测试ExitMiddleWare> logger, ILogService logService, 电机腔气密测试ExitModel exitModel)
        {
            _logger = logger;
            _logService = logService;
            _exitModel = exitModel;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4020.6.4").Content;
                var resp = s7Net.ReadBool("DB4020.12.0").Content;
                var enterok = s7Net.ReadBool("DB4020.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                _exitModel.UpdateData(req, resp, sn, enterok, enterng);
                // 更新数据服务
                //_statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "电机腔气密测试出站请求收到");
                    s7Net.Write("DB4020.12.0", true);
                    s7Net.Write("DB4020.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.12.0", false);
                    s7Net.Write("DB4020.2.4", false);
                    s7Net.Write("DB4020.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "电机腔气密测试出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"电机腔气密测试出站ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
