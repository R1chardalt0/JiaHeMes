using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using ChargePadLine.Client.Services.PlcService.plc4.后盖超声波焊接;
using ChargePadLine.Client.Services.PlcService.Plc5;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc5.转子充磁与装配
{
    public class 转子充磁与装配EnterMiddleWare : IPlc5Task
    {
        private readonly ILogger<转子充磁与装配EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 转子充磁与装配EnterModel _enterModel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【转子充磁与装配】";

        public 转子充磁与装配EnterMiddleWare(ILogger<转子充磁与装配EnterMiddleWare> logger, ILogService logService, 转子充磁与装配EnterModel enterModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logger = logger;
            _logService = logService;
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
                var stationStatus = s7Net.ReadByte("DB4010.4").Content;//设备故障

                bool[] bitStatus = new bool[8];
                for (int i = 0; i < 8; i++)
                {
                    // 右移i位，然后与1进行与操作
                    bitStatus[i] = ((stationStatus >> i) & 1) == 1;

                    Console.WriteLine($"位 {i} 的状态: {bitStatus[i]}");
                }
                var malfunction = (stationStatus & 0x01) == 0x01;//设备故障
                var auto = (stationStatus & 0x02) == 0x02;//自动模式
                var idle = (stationStatus & 0x04) == 0x04;//设备空闲
                var manual = (stationStatus & 0x08) == 0x08; //手动模式
                var check = (stationStatus & 0x10) == 0x10;//审核模式

                if (malfunction) statusMessage = "设备故障";
                else if (auto) statusMessage = "自动模式";
                else if (idle) statusMessage = "设备空闲";
                else if (manual) statusMessage = "手动模式";
                else if (check) statusMessage = "审核模式";
                else statusMessage = "无状态";


                var req = s7Net.ReadBool("DB4010.6.0").Content;
                var resp = s7Net.ReadBool("DB4010.10.0").Content;
                var enterok = s7Net.ReadBool("DB4010.2.0").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4010.2.1").Content;//进站NG
                var sn = s7Net.ReadString("DB4013.66", 100);

                // 更新数据服务
                _enterModel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求收到");

                    var reqParam = new ReqDto
                    {
                        sn = sn,
                        resource = _stationconfig.Station9.Resource,
                        stationCode = _stationconfig.Station9.StationCode,
                        workOrderCode = _stationconfig.Station9.WorkOrderCode
                    };
                    var res = await _mesApi.UploadCheck(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB4010.10.0", true);
                        s7Net.Write("DB4010.2.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验成功");
                    }
                    else
                    {
                        s7Net.Write("DB4010.10.0", true);
                        s7Net.Write("DB4010.2.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.10.0", false);
                    s7Net.Write("DB4010.2.0", false);
                    s7Net.Write("DB4010.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}EnterMiddleWare异常: {ex.Message}");
            }
        }
    }
}
