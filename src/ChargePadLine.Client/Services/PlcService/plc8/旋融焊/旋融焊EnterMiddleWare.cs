using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc8;
using HslCommunication.Profinet.LSIS;
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
        private readonly 旋融焊EnterModel _entermodel;

        public 旋融焊EnterMiddleWare(ILogger<旋融焊EnterMiddleWare> logger, ILogService logService, 旋融焊EnterModel entermodel = null)
        {
            _logger = logger;
            _logService = logService;
            _entermodel = entermodel;
        }
        public async Task ExecuteOnceAsync(ModbusConnect modbus, CancellationToken cancellationToken)
        {
            try
            {
                string statusMessage = "";
                //plc状态读取
                var malfunction = modbus.ReadBool("1030.0").Content;//设备故障
                var auto = modbus.ReadBool("1031.0").Content;//自动模式
                var idle = modbus.ReadBool("1032.0").Content;//设备空闲
                var manual = modbus.ReadBool("1033.0").Content;//手动模式
                var check = modbus.ReadBool("1034.0").Content;//审核模式

                if (malfunction) statusMessage = "设备故障";
                else if (auto) statusMessage = "自动模式";
                else if (idle) statusMessage = "设备空闲";
                else if (manual) statusMessage = "手动模式";
                else if (check) statusMessage = "审核模式";
                else statusMessage = "无状态";


                var req = modbus.ReadBool("1000.0").Content;
                var resp = modbus.ReadBool("1001.0").Content;
                var enterok = modbus.ReadBool("1002.0").Content;//进站OK
                var enterng = modbus.ReadBool("1003.0").Content;//进站NG
                var sn = modbus.ReadString("1004", 100).Content.Trim().Replace("\0", "").Replace("\b", "");
                // 更新数据服务
                _entermodel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "旋融焊进站请求收到");
                    modbus.Write("1001.0", true);
                    modbus.Write("1002.0", true);
                }
                else if (!req && resp)
                {
                    modbus.Write("1001.0", false);
                    modbus.Write("1002.0", false);
                    modbus.Write("1003.0", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "旋融焊进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"旋融焊EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
