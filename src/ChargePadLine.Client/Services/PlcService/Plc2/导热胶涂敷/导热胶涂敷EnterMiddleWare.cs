using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly 导热胶涂敷EnterModel _enterModel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【导热胶涂敷】";

        public 导热胶涂敷EnterMiddleWare(ILogService logService, ILogger<导热胶涂敷EnterMiddleWare> logger, 导热胶涂敷EnterModel enterModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logService = logService;
            _logger = logger;
            _enterModel = enterModel;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }
        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                string statusMessage = "";
                //plc状态读取
                var malfunction = s7Net.ReadBool("DB5010.4.0").Content;//设备故障
                var auto = s7Net.ReadBool("DB5010.4.1").Content;//自动模式
                var idle = s7Net.ReadBool("DB5010.4.2").Content;//设备空闲
                var manual = s7Net.ReadBool("DB5010.4.3").Content;//手动模式
                var check = s7Net.ReadBool("DB5010.4.4").Content;//审核模式

                if (malfunction) statusMessage = "设备故障";
                else if (auto) statusMessage = "自动模式";
                else if (idle) statusMessage = "设备空闲";
                else if (manual) statusMessage = "手动模式";
                else if (check) statusMessage = "审核模式";
                else statusMessage = "无状态";

                var req = s7Net.ReadBool("DB5010.6.0").Content;
                var resp = s7Net.ReadBool("DB5010.10.0").Content;
                var enterok = s7Net.ReadBool("DB5010.2.0").Content;//进站OK
                var enterng = s7Net.ReadBool("DB5010.2.1").Content;//进站NG
                var sn = s7Net.ReadString("DB5013.66", 100);

                // 更新数据服务
                _enterModel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求收到");

                    var reqParam = new ReqDto
                    {
                        sn = sn,
                        resource = _stationconfig.Station4.Resource,
                        stationCode = _stationconfig.Station4.StationCode,
                        workOrderCode = _stationconfig.Station4.WorkOrderCode
                    };
                    var res = await _mesApi.UploadCheck(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB5010.10.0", true);
                        s7Net.Write("DB5010.2.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验成功");
                    }
                    else
                    {
                        s7Net.Write("DB5010.10.0", true);
                        s7Net.Write("DB5010.2.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验失败，mes返回:{res.message}");
                    }    
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB5010.10.0", false);
                    s7Net.Write("DB5010.2.0", false);
                    s7Net.Write("DB5010.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{ex},{PlcName}进站MiddleWare异常");
            }
        }
    }
}
