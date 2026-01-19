using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试
{
    public class 电机腔气密MasterMiddleWare : IPlc2Task
    {
        private readonly ILogger<电机腔气密MasterMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private readonly 电机腔气密MasterModel _masterModel;
        private const string PlcName = "【电机枪气密测试】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public 电机腔气密MasterMiddleWare(ILogger<电机腔气密MasterMiddleWare> logger, ILogService logService, IOptions<StationConfig> stationconfig, IMesApiService mesApi, 电机腔气密MasterModel masterModel)
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
                var req = s7Net.ReadBool("DB4010.7.0").Content;
                var resp = s7Net.ReadBool("DB4010.14.0").Content;
                var enterok = s7Net.ReadBool("DB4010.3.0").Content;//出站OK
                var enterng = s7Net.ReadBool("DB4010.3.1").Content;//出站NG
                var sn = s7Net.ReadString("DB4013.66", 100);

                // 更新数据服务
                _masterModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    var isok = s7Net.ReadBool("DB4010.16.0").Content;

                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检请求收到");

                    var param1 = s7Net.ReadFloat("DB4015.66").Content;
                    var upper1 = s7Net.ReadFloat("DB4012.92").Content;
                    var lower1 = s7Net.ReadFloat("DB4012.96").Content;
                    var param1Result = (param1 <= upper1 && param1 >= lower1) ? "PASS" : "FAIL";

                    var param2 = s7Net.ReadFloat("DB4015.70").Content;
                    var upper2 = s7Net.ReadFloat("DB4012.100").Content;
                    var lower2 = s7Net.ReadFloat("DB4012.104").Content;
                    var param2Result = (param2 <= upper2 && param2 >= lower2) ? "PASS" : "FAIL";

                    var param3 = s7Net.ReadFloat("DB4015.74").Content;
                    var upper3 = s7Net.ReadFloat("DB4012.108").Content;
                    var lower3 = s7Net.ReadFloat("DB4012.112").Content;
                    var param3Result = (param3 <= upper3 && param3 >= lower3) ? "PASS" : "FAIL";

                    var param4 = s7Net.ReadFloat("DB4015.78").Content;
                    var upper4 = s7Net.ReadFloat("DB4012.116").Content;
                    var lower4 = s7Net.ReadFloat("DB4012.120").Content;
                    var param4Result = (param4 <= upper4 && param4 >= lower4) ? "PASS" : "FAIL";

                    var param5 = s7Net.ReadFloat("DB4015.82").Content;
                    var upper5 = s7Net.ReadFloat("DB4012.124").Content;
                    var lower5 = s7Net.ReadFloat("DB4012.128").Content;
                    var param5Result = (param5 <= upper5 && param5 >= lower5) ? "PASS" : "FAIL";

                    var param6 = s7Net.ReadFloat("DB4015.86").Content;
                    var upper6 = s7Net.ReadFloat("DB4012.132").Content;
                    var lower6 = s7Net.ReadFloat("DB4012.136").Content;
                    var param6Result = (param6 <= upper6 && param6 >= lower6) ? "PASS" : "FAIL";

                    //总结果
                    var paramResultTotal = (param1Result == "PASS" && param2Result == "PASS" && param3Result == "PASS"
                        && param4Result == "PASS" && param5Result == "PASS" && param6Result == "PASS") ? "PASS" : "FAIL";

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
                        resource = _stationconfig.Station3.Resource,
                        stationCode = _stationconfig.Station3.StationCode,
                        workOrderCode = _stationconfig.Station3.WorkOrderCode,
                        testResult = isok ? "Pass" : "Fail",
                        testData = testDatas
                    };
                    var res = await _mesApi.UploadMaster(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB4010.14.0", true);
                        s7Net.Write("DB4010.3.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检收集完成");
                    }
                    else
                    {
                        s7Net.Write("DB4010.14.0", true);
                        s7Net.Write("DB4010.3.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检收集失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.14.0", false);
                    s7Net.Write("DB4010.3.0", false);
                    s7Net.Write("DB4010.3.1", false);
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
