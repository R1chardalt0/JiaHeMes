using ChargePadLine.Client.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷
{
    public class 导热胶涂敷ExitMiddleWare : IPlc2Task
    {
        private readonly ILogService _logService;
        private readonly ILogger<导热胶涂敷ExitMiddleWare> _logger;
        private readonly 导热胶涂敷ExitModel _exitModel;
        public 导热胶涂敷ExitMiddleWare(ILogService logService, ILogger<导热胶涂敷ExitMiddleWare> logger, 导热胶涂敷ExitModel exitModel)
        {
            _logService = logService;
            _logger = logger;
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
                    await _logService.RecordLogAsync(LogLevel.Information, "导热胶涂敷出站请求收到");
                    s7Net.Write("DB4020.12.0", true);
                    s7Net.Write("DB4020.2.4", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.12.0", false);
                    s7Net.Write("DB4020.2.4", false);
                    s7Net.Write("DB4020.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "导热胶涂敷出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{ex},导热胶涂敷出站MiddleWare异常");
            }
        }
    }
}
