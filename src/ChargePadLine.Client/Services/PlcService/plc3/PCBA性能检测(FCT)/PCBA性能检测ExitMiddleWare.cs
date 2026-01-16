using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.Plc2.电机腔气密测试;
using ChargePadLine.Client.Services.PlcService.Plc3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc3.PCBA性能检测_FCT_
{
    public class PCBA性能检测ExitMiddleWare : IPlc3Task
    {
        private readonly ILogger<PCBA性能检测ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly PCBA性能检测ExitModel _exitmodel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【PCBA性能检测】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public PCBA性能检测ExitMiddleWare(ILogger<PCBA性能检测ExitMiddleWare> logger, ILogService logService, PCBA性能检测ExitModel exitmodel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logger = logger;
            _logService = logService;
            _exitmodel = exitmodel;
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
                _exitmodel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    var isok = s7Net.ReadBool("DB4010.16.0").Content;

                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站请求收到");
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
                        resource = _stationconfig.Station5.Resource,
                        stationCode = _stationconfig.Station5.StationCode,
                        workOrderCode = _stationconfig.Station5.WorkOrderCode,
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
