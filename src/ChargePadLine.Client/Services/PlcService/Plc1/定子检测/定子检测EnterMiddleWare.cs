using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChargePadLine.Client.Services.PlcService.Plc1.定子检测
{

    /// <summary>
    /// 定子检测业务逻辑，单次执行一次轮询，由外部服务控制循环与频率
    /// </summary>
    public class 定子检测EnterMiddleWare : IPlc1Task
    {
        private readonly ILogger<定子检测EnterMiddleWare> _logger;
        private readonly StatorEnterModel _statorEnterModel;
        private readonly ILogService _logService;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【定子检测】";

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

        public 定子检测EnterMiddleWare(ILogger<定子检测EnterMiddleWare> logger, StatorEnterModel statorEnterModel, ILogService logService, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logger = logger;
            _statorEnterModel = statorEnterModel;
            _logService = logService;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }

        private bool _isInitialized = false;

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                string statusMessage = "";
                //plc状态读取
                var stationStatus = s7Net.ReadByte("DB4010.4").Content;//设备状态
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

                var req = s7Net.ReadBool("DB4010.6.0").Content;

                var respContent = s7Net.ReadByte("DB4010.2").Content;
                var respFlag = (RespFlag)respContent;
                var resp = respFlag.HasFlag(RespFlag.EnterResp); //进站响应
                var enterok = respFlag.HasFlag(RespFlag.EnterOK);//进站OK
                var enterng = respFlag.HasFlag(RespFlag.EnterNG);//进站NG
                var sn = s7Net.ReadString("DB4013.66", 100);
                // 更新数据服务
                _statorEnterModel.UpdateData(req, resp, sn, enterok, enterng, statusMessage);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求收到");

                    var reqParam = new ReqDto
                    {
                        sn = sn,
                        resource = _stationconfig.Station1.Resource,
                        stationCode = _stationconfig.Station1.StationCode,
                        workOrderCode = _stationconfig.Station1.WorkOrderCode
                    };
                    var res = await _mesApi.UploadCheck(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB4010.2.6", true);
                        s7Net.Write("DB4010.2.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验成功");
                    }
                    else
                    {
                        s7Net.Write("DB4010.2.6", true);
                        s7Net.Write("DB4010.2.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站校验失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.2.6", false);
                    s7Net.Write("DB4010.2.0", false);
                    s7Net.Write("DB4010.2.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}进站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}进站MiddleWare异常: {ex.Message}");
            }
        }
    }
}