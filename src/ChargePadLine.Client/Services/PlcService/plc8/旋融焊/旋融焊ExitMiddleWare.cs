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
