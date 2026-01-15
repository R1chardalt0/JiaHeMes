using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc2.导热胶涂敷
{
    public class 导热胶涂敷ExitMiddleWare : IPlc2Task
    {
        private readonly ILogService _logService;
        private readonly ILogger<导热胶涂敷ExitMiddleWare> _logger;
        private readonly 导热胶涂敷ExitModel _exitModel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【导热胶涂敷】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public 导热胶涂敷ExitMiddleWare(ILogService logService, ILogger<导热胶涂敷ExitMiddleWare> logger, 导热胶涂敷ExitModel exitModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logService = logService;
            _logger = logger;
            _exitModel = exitModel;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }
        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4020.6.4").Content;
                var resp = s7Net.ReadBool("DB4020.12.0").Content;
                var enterok = s7Net.ReadBool("DB4020.2.4").Content;//进站OK
                var enterng = s7Net.ReadBool("DB4020.2.5").Content;//进站NG
                var sn = s7Net.ReadString("DB4020.200", 100);
                _exitModel.UpdateData(req, resp, sn, enterok, enterng);
                // 更新数据服务
                //_statorTestDataService.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    var isok = s7Net.ReadBool("DB4020.16.0").Content;

                    await _logService.RecordLogAsync(LogLevel.Information, "导热胶涂敷出站请求收到");

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
                        resource = _stationconfig.Station4.Resource,
                        stationCode = _stationconfig.Station4.StationCode,
                        workOrderCode = _stationconfig.Station4.WorkOrderCode,
                        testResult = isok ? "Pass" : "Fail",
                        testData = testDatas
                    };
                    var res = await _mesApi.UploadData(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB4020.12.0", true);
                        s7Net.Write("DB4020.2.4", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集完成");
                    }
                    else
                    {
                        s7Net.Write("DB4020.12.0", true);
                        s7Net.Write("DB4020.2.5", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站收集失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.12.0", false);
                    s7Net.Write("DB4020.2.4", false);
                    s7Net.Write("DB4020.2.5", false);
                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}出站请求复位");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _logService.RecordLogAsync(LogLevel.Error, $"{PlcName}出站MiddleWare异常,{ex.Message}");
            }
        }
    }
}
