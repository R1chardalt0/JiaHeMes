using ChargePadLine.Client.Controls;
using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc1.定子检测;
using HslCommunication.ModBus;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1
{
    public class Plc1Service
    {
        private S7NetConnect? _s7Net;
        private PlcConfig _plcConfig;
        private ILogger<Plc1Service> _logger;

        public Plc1Service(IOptions<PlcConfig> config, ILogger<Plc1Service> logger)
        {
            _plcConfig = config.Value;
            _logger = logger;
            InitializeModbusConnection();

        }

        private void InitializeModbusConnection()
        {
            try
            {
                _s7Net = new S7NetConnect();
                _s7Net.Connect(_plcConfig.Plc1.IpAddress, _plcConfig.Plc1.Port);
                string logMsg = $"PLC1连接初始化成功: {_plcConfig.Plc1.IpAddress}:{_plcConfig.Plc1.Port}";
                _logger.LogInformation(logMsg);
                定子检测MiddleWare middleWare = new 定子检测MiddleWare(_s7Net);
                middleWare.Start();
            }
            catch (Exception ex)
            {
                string logMsg = "PLC1连接初始化失败: " + ex.Message;
                _logger.LogError(logMsg);
            }
        }
    }
}
