using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using ChargePadLine.Client.Services.PlcService.Plc3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.热铆
{
    public class 热铆MasterMiddleWare : IPlc3Task
    {
        private readonly ILogger<热铆MasterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private readonly 热铆MasterModel _masterModel;
        private const string PlcName = "【热铆】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public 热铆MasterMiddleWare(ILogger<热铆MasterMiddleWare> logger, ILogService logService, IOptions<StationConfig> stationconfig, IMesApiService mesApi, 热铆MasterModel masterModel)
        {
            _logger = logger;
            _logService = logService;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
            _masterModel = masterModel;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB5010.7.0").Content;
                var resp = s7Net.ReadBool("DB5010.14.0").Content;
                var enterok = s7Net.ReadBool("DB5010.3.0").Content;//出站OK
                var enterng = s7Net.ReadBool("DB5010.3.1").Content;//出站NG
                var sn = s7Net.ReadString("DB5013.66", 100);

                // 更新数据服务
                _masterModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检请求收到");

                    var param1 = s7Net.ReadFloat("DB5015.66").Content;
                    var upper1 = s7Net.ReadFloat("DB5012.92").Content;
                    var lower1 = s7Net.ReadFloat("DB5012.96").Content;
                    var param1Result = (param1 <= upper1 && param1 >= lower1) ? "PASS" : "FAIL";

                    var param2 = s7Net.ReadFloat("DB5015.70").Content;
                    var upper2 = s7Net.ReadFloat("DB5012.100").Content;
                    var lower2 = s7Net.ReadFloat("DB5012.104").Content;
                    var param2Result = (param2 <= upper2 && param2 >= lower2) ? "PASS" : "FAIL";

                    var param3 = s7Net.ReadFloat("DB5015.74").Content;
                    var upper3 = s7Net.ReadFloat("DB5012.108").Content;
                    var lower3 = s7Net.ReadFloat("DB5012.112").Content;
                    var param3Result = (param3 <= upper3 && param3 >= lower3) ? "PASS" : "FAIL";

                    var param4 = s7Net.ReadFloat("DB5015.78").Content;
                    var upper4 = s7Net.ReadFloat("DB5012.116").Content;
                    var lower4 = s7Net.ReadFloat("DB5012.120").Content;
                    var param4Result = (param4 <= upper4 && param4 >= lower4) ? "PASS" : "FAIL";

                    var param5 = s7Net.ReadFloat("DB5015.82").Content;
                    var upper5 = s7Net.ReadFloat("DB5012.124").Content;
                    var lower5 = s7Net.ReadFloat("DB5012.128").Content;
                    var param5Result = (param5 <= upper5 && param5 >= lower5) ? "PASS" : "FAIL";

                    var param6 = s7Net.ReadFloat("DB5015.86").Content;
                    var upper6 = s7Net.ReadFloat("DB5012.132").Content;
                    var lower6 = s7Net.ReadFloat("DB5012.136").Content;
                    var param6Result = (param6 <= upper6 && param6 >= lower6) ? "PASS" : "FAIL";

                    //总结果
                    var paramResultTotal = (param1Result == "PASS" && param2Result == "PASS" && param3Result == "PASS"
                        && param4Result == "PASS" && param5Result == "PASS" && param6Result == "PASS") ? "PASS" : "FAIL";

                    string IsOK = "";
                    var OKRes = s7Net.ReadInt32("DB5015.66").Content;
                    var NGRes = s7Net.ReadInt32("DB5015.70").Content;
                    if (OKRes != 0 && NGRes == 0)
                    {
                        IsOK = "PASS";
                    }
                    else if (OKRes == 0 && NGRes != 0)
                    {
                        IsOK = "FAIL";
                    }
                    else
                    {
                        IsOK = "未知";
                    }

                    if (IsOK != paramResultTotal)
                    {
                        s7Net.Write("DB5010.14.0", true);
                        s7Net.Write("DB5010.3.1", true);
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
                        resource = _stationconfig.Station6.Resource,
                        stationCode = _stationconfig.Station6.StationCode,
                        workOrderCode = _stationconfig.Station6.WorkOrderCode,
                        testResult = paramResultTotal,
                        testData = testDatas
                    };
                    var res = await _mesApi.UploadMaster(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB5010.14.0", true);
                        s7Net.Write("DB5010.3.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检收集完成");
                    }
                    else
                    {
                        s7Net.Write("DB5010.14.0", true);
                        s7Net.Write("DB5010.3.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检收集失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB5010.14.0", false);
                    s7Net.Write("DB5010.3.0", false);
                    s7Net.Write("DB5010.3.1", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}点检MiddleWare异常: {ex.Message}");
            }
        }
    }
}
