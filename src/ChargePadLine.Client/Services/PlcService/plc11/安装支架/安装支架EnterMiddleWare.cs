using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;
using ChargePadLine.Client.Services.PlcService.Plc11;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.安装支架
{
    public class 安装支架EnterMiddleWare : IPlc11Task
    {
        private readonly ILogger<安装支架EnterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 安装支架EnterModel _enterModel ;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【安装支架】";

        /// <summary>
        /// 设备状态标志
        /// </summary>
        [Flags]
        public enum StatusFlag : byte
        {
            None = 0,
            Malfunction = 1 << 0,
            Auto = 1 << 1,
            Idle = 1 << 2, // 00000100
            Manual = 1 << 3,      // 00001000
            Check = 1 << 4, // 00010000          
        }

        /// <summary>
        /// 响应标志
        /// </summary>
        [Flags]
        public enum RespFlag : byte
        {
            None = 0,
            EnterOK = 1 << 0,   // 进站ok
            EnterNG = 1 << 1,   // 进站ng
            EnterCheckOK = 1 << 2, // 进站审核 OK 为返工件
            NotComplete = 1 << 3,  // 进站审核 NOK，在上工位没有做过
            ExitOK = 1 << 4,    // 出站OK
            ExitNG = 1 << 5,    // 出站NG
            EnterResp = 1 << 6, // 进站响应
            ExitResp = 1 << 7   // 出站响应
        }
        public 安装支架EnterMiddleWare(ILogger<安装支架EnterMiddleWare> logger, ILogService logService, 安装支架EnterModel enterModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
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
                var stationStatus = s7Net.ReadByte("DB5010.4").Content;//设备故障
                StatusFlag flags = (StatusFlag)stationStatus;

                statusMessage = flags switch
                {
                    var f when f.HasFlag(StatusFlag.Malfunction) => "设备故障",
                    var f when f.HasFlag(StatusFlag.Auto) => "自动模式",
                    var f when f.HasFlag(StatusFlag.Idle) => "设备空闲",
                    var f when f.HasFlag(StatusFlag.Manual) => "手动模式",
                    var f when f.HasFlag(StatusFlag.Check) => "审核模式",
                    _ => "无状态"
                };

                var req = s7Net.ReadBool("DB5010.6.0").Content;

                var respContent = s7Net.ReadByte("DB5010.2").Content;
                var respFlag = (RespFlag)respContent;
                var resp = respFlag.HasFlag(RespFlag.EnterResp); //进站响应
                var enterok = respFlag.HasFlag(RespFlag.EnterOK);//进站OK
                var enterng = respFlag.HasFlag(RespFlag.EnterNG);//进站NG
                var sn = s7Net.ReadString("DB5013.66", 100);

                // 更新数据服务
                _enterModel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求收到");

                    var reqParam = new ReqDto
                    {
                        sn = sn,
                        resource = _stationconfig.Station16.Resource,
                        stationCode = _stationconfig.Station16.StationCode,
                        workOrderCode = _stationconfig.Station16.WorkOrderCode
                    };
                    var res = await _mesApi.UploadCheck(reqParam);
                    if (res.code == 200)
                    {
                        s7Net.Write("DB5010.2.6", true);
                        s7Net.Write("DB5010.2.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验成功");
                    }
                    else
                    {
                        s7Net.Write("DB5010.2.6", true);
                        s7Net.Write("DB5010.2.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验失败，mes返回:{res.message}");
                    }

                }
                else if (!req && resp)
                {
                    s7Net.Write("DB5010.2.6", false);
                    s7Net.Write("DB5010.2.0", false);
                    s7Net.Write("DB5010.2.1", false);
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
