using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using ChargePadLine.Client.Services.PlcService.Plc4;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc4.干区气密测试
{
    public class 干区气密测试EnterMiddleWare : IPlc4Task
    {
        private readonly ILogger<干区气密测试EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 干区气密测试EnterModel _enterModel;

        public 干区气密测试EnterMiddleWare(ILogger<干区气密测试EnterMiddleWare> logger, ILogService logService, 干区气密测试EnterModel enterModel = null)
        {
            _logger = logger;
            _logService = logService;
            _enterModel = enterModel;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                string statusMessage = "";
                //plc状态读取
                var malfunction = s7Net.ReadBool("DB4020.4.0").Content;//设备故障
                var auto = s7Net.ReadBool("DB4020.4.1").Content;//自动模式
                var idle = s7Net.ReadBool("DB4020.4.2").Content;//设备空闲
                var manual = s7Net.ReadBool("DB4020.4.3").Content;//手动模式
                var check = s7Net.ReadBool("DB4020.4.4").Content;//审核模式

                if (malfunction) statusMessage = "设备故障";
                else if (auto) statusMessage = "自动模式";
                else if (idle) statusMessage = "设备空闲";
                else if (manual) statusMessage = "手动模式";
                else if (check) statusMessage = "审核模式";
                else statusMessage = "无状态";

                var req = s7Net.ReadBool("DB4020.6.0").Content;
                var resp = s7Net.ReadBool("DB4020.10.0").Content;
                var enterok = s7Net.ReadBool("DB4020.2.0").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.1").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _enterModel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, "热铆进站请求收到");
                    s7Net.Write("DB4020.10.0", true);
                    s7Net.Write("DB4020.2.0", true);
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.10.0", false);
                    s7Net.Write("DB4020.2.0", false);
                    s7Net.Write("DB4020.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "热铆进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"干区气密测试EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
