using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配
{
    /// <summary>
    /// O 型圈装配业务逻辑，单次执行一次轮询，由外部服务控制循环与频率
    /// </summary>
    public class O型圈装配ExitMiddleWare : IPlc1Task
    {
        private readonly ILogger<O型圈装配ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly RingExitModel _routingExitModel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【O型圈装配】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public O型圈装配ExitMiddleWare(ILogger<O型圈装配ExitMiddleWare> logger, ILogService logService, RingExitModel routingExitModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _routingExitModel = routingExitModel;
            _logger = logger;
            _logService = logService;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB5010.6.4").Content;
                var resp = s7Net.ReadBool("DB5010.12.0").Content;
                var enterok = s7Net.ReadBool("DB5010.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB5010.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB5013.66", 100);

                // 更新数据服务
                _routingExitModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站请求收到");

                    //上下限参数数组
                    var upLowArray = s7Net.ReadFloatBatch("DB5012.92", 12).Content;
                    //参数数组
                    var param = s7Net.ReadFloatBatch("DB5014.70", 6).Content;

                    var param1 = param[0];
                    var upper1 = upLowArray[0];
                    var lower1 = upLowArray[1];
                    var param1Result = (param1 <= upper1 && param1 >= lower1) ? "PASS" : "FAIL";

                    var param2 = param[1];
                    var upper2 = upLowArray[2];
                    var lower2 = upLowArray[3];
                    var param2Result = (param2 <= upper2 && param2 >= lower2) ? "PASS" : "FAIL";

                    var param3 = param[2];
                    var upper3 = upLowArray[4];
                    var lower3 = upLowArray[5];
                    var param3Result = (param3 <= upper3 && param3 >= lower3) ? "PASS" : "FAIL";

                    var param4 = param[3];
                    var upper4 = upLowArray[6];
                    var lower4 = upLowArray[7];
                    var param4Result = (param4 <= upper4 && param4 >= lower4) ? "PASS" : "FAIL";

                    var param5 = param[4];
                    var upper5 = upLowArray[8];
                    var lower5 = upLowArray[9];
                    var param5Result = (param5 <= upper5 && param5 >= lower5) ? "PASS" : "FAIL";

                    var param6 = param[5];
                    var upper6 = upLowArray[10];
                    var lower6 = upLowArray[11];
                    var param6Result = (param6 <= upper6 && param6 >= lower6) ? "PASS" : "FAIL";

                    //总结果
                    var paramResultTotal = (param1Result == "PASS" && param2Result == "PASS" && param3Result == "PASS"
                        && param4Result == "PASS" && param5Result == "PASS" && param6Result == "PASS") ? "PASS" : "FAIL";

                    string IsOK = "";
                    var OKRes = s7Net.ReadInt32("DB5014.62").Content;
                    var NGRes = s7Net.ReadInt32("DB5014.66").Content;

                    if (OKRes != 0 && NGRes == 0)
                    {
                        IsOK= "PASS";
                    }
                    else if (OKRes == 0 && NGRes != 0)
                    {
                        IsOK= "FAIL";
                    }
                    else
                    {
                        IsOK= "未知";
                    }

                    if (IsOK != paramResultTotal)
                    {
                        s7Net.Write("DB5010.12.0", true);
                        s7Net.Write("DB5010.2.5", true);
                        await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}MES与PLC返回OK/NG不一致，mes为:{paramResultTotal}，plc为:{IsOK}");
                        return;
                    }

                    testDatas = new List<TestDataItem>()
                    {
                        new TestDataItem
                        {
                            ParametricKey = "绝缘电阻",
                            TestValue = param1.ToString(),
                            Units = "MΩ",
                            Upperlimit = upper1,
                            Lowerlimit = lower1,
                            TestResult = param1Result,
                            Remark = "电阻测试结果"
                        },
                        new TestDataItem
                        {
                            ParametricKey = "耐压测试",
                            TestValue = param2.ToString(),
                            Units = "V",
                            Upperlimit =upper2,
                            Lowerlimit = lower2,
                            TestResult =param2Result,
                            Remark = "耐压测试结果"
                        },
                        new TestDataItem
                        {
                            ParametricKey = "耐压测试",
                            TestValue = param3.ToString(),
                            Units = "V",
                            Upperlimit =upper3,
                            Lowerlimit = lower3,
                            TestResult =param3Result,
                            Remark = "耐压测试结果"
                        },
                        new TestDataItem
                        {
                            ParametricKey = "耐压测试",
                            TestValue = param4.ToString(),
                            Units = "V",
                            Upperlimit =upper4,
                            Lowerlimit = lower4,
                            TestResult =param4Result,
                            Remark = "耐压测试结果"
                        },
                        new TestDataItem
                        {
                            ParametricKey = "耐压测试",
                            TestValue = param5.ToString(),
                            Units = "V",
                            Upperlimit =upper5,
                            Lowerlimit = lower5,
                            TestResult =param5Result,
                            Remark = "耐压测试结果"
                        }
                    };

                    var reqParam = new ReqDto
                    {
                        sn = sn,
                        resource = _stationconfig.Station2.Resource,
                        stationCode = _stationconfig.Station2.StationCode,
                        workOrderCode = _stationconfig.Station2.WorkOrderCode,
                        testResult = paramResultTotal,
                        testData = testDatas
                    };

                    var res = await _mesApi.UploadData(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB5010.12.0", true);
                        s7Net.Write("DB5010.2.4", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集完成");
                    }
                    else
                    {
                        s7Net.Write("DB5010.12.0", true);
                        s7Net.Write("DB5010.2.5", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集失败，mes返回:{res.message}");
                    }

                }
                else if (!req && resp)
                {
                    s7Net.Write("DB5010.12.0", false);
                    s7Net.Write("DB5010.2.4", false);
                    s7Net.Write("DB5010.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}出站异常: {ex.Message}");
            }
        }
    }
}
