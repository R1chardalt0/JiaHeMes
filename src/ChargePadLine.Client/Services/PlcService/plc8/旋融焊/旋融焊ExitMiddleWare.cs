using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.Plc1.O型圈及冷却铝板装配;
using ChargePadLine.Client.Services.PlcService.Plc8;
using HslCommunication.Profinet.LSIS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【旋融焊】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public 旋融焊ExitMiddleWare(ILogger<旋融焊ExitMiddleWare> logger, ILogService logService, 旋融焊ExitModel exitModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logger = logger;
            _logService = logService;
            _exitModel = exitModel;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }
        public async Task ExecuteOnceAsync(ModbusConnect modbus, CancellationToken cancellationToken)
        {
            try
            {
                var req = modbus.ReadBool("2000.0").Content;
                var resp = modbus.ReadBool("2001.0").Content;
                var exitok = modbus.ReadBool("2002.0").Content;//进站OK
                var exitng = modbus.ReadBool("2003.0").Content;//进站NG
                var sn = modbus.ReadString("2004", 100);
                // 更新数据服务
                _exitModel.UpdateData(req, resp, sn, exitok, exitng);

                if (req && !resp)
                {
                    var isok = modbus.ReadBool("2001.0").Content;

                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站请求收到");

                    var param1 = modbus.ReadFloat("DB4014.70").Content;
                    var upper1 = modbus.ReadFloat("DB4012.92").Content;
                    var lower1 = modbus.ReadFloat("DB4012.96").Content;
                    var param1Result = (param1 <= upper1 && param1 >= lower1) ? "PASS" : "FAIL";

                    var param2 = modbus.ReadFloat("DB4014.74").Content;
                    var upper2 = modbus.ReadFloat("DB4012.100").Content;
                    var lower2 = modbus.ReadFloat("DB4012.104").Content;
                    var param2Result = (param2 <= upper2 && param2 >= lower2) ? "PASS" : "FAIL";

                    var param3 = modbus.ReadFloat("DB4014.78").Content;
                    var upper3 = modbus.ReadFloat("DB4012.108").Content;
                    var lower3 = modbus.ReadFloat("DB4012.112").Content;
                    var param3Result = (param3 <= upper3 && param3 >= lower3) ? "PASS" : "FAIL";

                    var param4 = modbus.ReadFloat("DB4014.82").Content;
                    var upper4 = modbus.ReadFloat("DB4012.116").Content;
                    var lower4 = modbus.ReadFloat("DB4012.120").Content;
                    var param4Result = (param4 <= upper4 && param4 >= lower4) ? "PASS" : "FAIL";

                    var param5 = modbus.ReadFloat("DB4014.86").Content;
                    var upper5 = modbus.ReadFloat("DB4012.124").Content;
                    var lower5 = modbus.ReadFloat("DB4012.128").Content;
                    var param5Result = (param5 <= upper5 && param5 >= lower5) ? "PASS" : "FAIL";

                    var param6 = modbus.ReadFloat("DB4014.90").Content;
                    var upper6 = modbus.ReadFloat("DB4012.124").Content;
                    var lower6 = modbus.ReadFloat("DB4012.128").Content;
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
                        resource = _stationconfig.Station12.Resource,
                        stationCode = _stationconfig.Station12.StationCode,
                        workOrderCode = _stationconfig.Station12.WorkOrderCode,
                        testResult = isok ? "Pass" : "Fail",
                        testData = testDatas
                    };
                    var res = await _mesApi.UploadData(reqParam);
                    if (res.code == 0)
                    {
                        modbus.Write("2001.0", true);
                        modbus.Write("2002.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集完成");
                    }
                    else
                    {
                        modbus.Write("2001.0", true);
                        modbus.Write("2003.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集失败，mes返回:{res.message}");
                    }

                }
                else if (!req && resp)
                {
                    modbus.Write("2001.0", false);
                    modbus.Write("2002.0", false);
                    modbus.Write("2003.0", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}ExitMiddleWare异常: {ex.Message}");
            }
        }
    }
}
