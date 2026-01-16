using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.plc10.EOL测试;
using ChargePadLine.Client.Services.PlcService.plc3.热铆;
using ChargePadLine.Client.Services.PlcService.Plc9;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc9.湿区气密测试
{
    public class 湿区气密ExitMiddleWare : IPlc9Task
    {
        private readonly ILogger<湿区气密ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 湿区气密ExitModel _exitModel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【湿区气密】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public 湿区气密ExitMiddleWare(ILogger<湿区气密ExitMiddleWare> logger, ILogService logService, 湿区气密ExitModel exitModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logger = logger;
            _logService = logService;
            _exitModel = exitModel;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4010.6.4").Content;
                var resp = s7Net.ReadBool("DB4010.12.0").Content;
                var enterok = s7Net.ReadBool("DB4010.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4010.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB4013.66", 100);
                // 更新数据服务
                _exitModel.UpdateData(req, resp, sn, enterok, enterng);            

                if (req && !resp)
                {
                    var isok = s7Net.ReadBool("DB4010.16.0").Content;

                    await _logService.RecordLogAsync(LogLevel.Information, "湿区气密出站请求收到");

                    testDatas = new List<TestDataItem>()
                    {
                        new TestDataItem
                        {
                            ParametricKey = "绝缘电阻",
                            TestValue = "",
                            Units = "MΩ",
                            Upperlimit = 1000,
                            Lowerlimit = 50,
                            TestResult = "Pass",
                            Remark = ""
                        },
                        new TestDataItem
                        {
                            ParametricKey = "耐压测试",
                            TestValue = "",
                            Units = "V",
                            Upperlimit = 2000,
                            Lowerlimit = 1500,
                            TestResult ="",
                            Remark = ""
                        }
                    };

                    var reqParam = new ReqDto
                    {
                        sn = sn,
                        resource = _stationconfig.Station13.Resource,
                        stationCode = _stationconfig.Station13.StationCode,
                        workOrderCode = _stationconfig.Station13.WorkOrderCode,
                        testResult = isok ? "Pass" : "Fail",
                        testData = testDatas
                    };
                    var res = await _mesApi.UploadData(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB4010.12.0", true);
                        s7Net.Write("DB4010.2.4", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集完成");
                    }
                    else
                    {
                        s7Net.Write("DB4010.12.0", true);
                        s7Net.Write("DB4010.2.5", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4010.12.0", false);
                    s7Net.Write("DB4010.2.4", false);
                    s7Net.Write("DB4010.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, "湿区气密出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"湿区气密ExitMiddleWare 异常: {ex.Message}");
            }
        }
    }
}
