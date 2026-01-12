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
    public class 旋融焊ExitMiddleWare : IPlc8Task
    {
        private readonly ILogger<旋融焊ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 旋融焊ExitModel _exitModel;

        public 旋融焊ExitMiddleWare(ILogger<旋融焊ExitMiddleWare> logger, ILogService logService, 旋融焊ExitModel exitModel)
        {
            _logger = logger;
            _logService = logService;
            _exitModel = exitModel;
        }
        public async Task ExecuteOnceAsync(ModbusConnect modbus, CancellationToken cancellationToken)
        {
            try
            {
                var req = modbus.ReadBool("2000.0").Content;
                var resp = modbus.ReadBool("2001.0").Content;
                var exitok = modbus.ReadBool("2002.0").Content;//进站OK
                var exitng = modbus.ReadBool("2003.0").Content;//进站NG
                var sn = modbus.ReadString("2004", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                // 更新数据服务
                _exitModel.UpdateData(req, resp, sn, exitok, exitng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "旋融焊出站请求收到");
                    modbus.Write("2001.0", true);
                    modbus.Write("2002.0", true);
                }
                else if (!req && resp)
                {
                    modbus.Write("2001.0", false);
                    modbus.Write("2002.0", false);
                    modbus.Write("2003.0", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "旋融焊出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"旋融焊ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
