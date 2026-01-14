using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.Mes;
using ChargePadLine.Client.Services.Mes.Dto;
using ChargePadLine.Client.Services.PlcService.Plc11;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc11.安装支架
{
    public class 安装支架MasterMiddleWare : IPlc11Task
    {
        private readonly ILogger<安装支架ExitMiddleWare> _logger;
        private readonly ILogService _logService;
        private readonly 安装支架MasterModel _masterModel;
        private readonly StationConfig _stationconfig;
        private readonly IMesApiService _mesApi;
        private const string PlcName = "【安装支架】";
        private List<TestDataItem> testDatas = new List<TestDataItem>();

        public 安装支架MasterMiddleWare(ILogger<安装支架ExitMiddleWare> logger, ILogService logService, 安装支架MasterModel masterModel, IOptions<StationConfig> stationconfig, IMesApiService mesApi)
        {
            _logger = logger;
            _logService = logService;
            _masterModel = masterModel;
            _stationconfig = stationconfig.Value;
            _mesApi = mesApi;
        }

        public async Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            try
            {
                var req = s7Net.ReadBool("DB4020.7.0").Content;
                var resp = s7Net.ReadBool("DB4020.14.0").Content;
                var enterok = s7Net.ReadBool("DB4020.3.0").Content;
                var enterng = s7Net.ReadBool("DB4020.3.1").Content;
                var sn = s7Net.ReadString("DB4020.66.0", 100).Content.Trim().Replace("\0", "").Replace("\b", "");

                // 更新数据服务
                _masterModel.UpdateData(req, resp, sn, enterok, enterng);

                if (req && !resp)
                {
                    var isok = s7Net.ReadBool("DB4020.16.0").Content;

                    await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检请求收到");

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
                        resource = _stationconfig.Station16.Resource,
                        stationCode = _stationconfig.Station16.StationCode,
                        workOrderCode = _stationconfig.Station16.WorkOrderCode,
                        testResult = isok ? "Pass" : "Fail",
                        testData = testDatas
                    };
                    var res = await _mesApi.UploadData(reqParam);
                    if (res.code == 0)
                    {
                        s7Net.Write("DB4020.14.0", true);
                        s7Net.Write("DB4020.3.0", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检收集完成");
                    }
                    else
                    {
                        s7Net.Write("DB4020.14.0", true);
                        s7Net.Write("DB4020.3.1", true);
                        await _logService.RecordLogAsync(LogLevel.Information, $"{PlcName}点检收集失败，mes返回:{res.message}");
                    }
                }
                else if (!req && resp)
                {
                    s7Net.Write("DB4020.14.0", false);
                    s7Net.Write("DB4020.3.0", false);
                    s7Net.Write("DB4020.3.1", false);
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
