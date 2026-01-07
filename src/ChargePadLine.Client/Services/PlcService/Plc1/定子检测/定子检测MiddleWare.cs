using ChargePadLine.Client.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1.定子检测
{
    public class 定子检测MiddleWare
    {

        private readonly S7NetConnect _s7Net;
        protected CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<定子检测MiddleWare> _logger;
        public 定子检测MiddleWare(S7NetConnect s7Net, ILogger<定子检测MiddleWare> logger)
        {
            _s7Net = s7Net;
            _logger = logger;
        }
        public 定子检测MiddleWare(S7NetConnect s7Net)
        {
            _s7Net = s7Net;
        }
        public void Start()
        {

            _ = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        var req=_s7Net.ReadBool("DB200.1000.0").Content;
                        var resp = _s7Net.ReadBool("DB200.1000.1").Content;
                        if (req && !resp)
                        {
                            _logger.LogInformation("定子检测请求收到");
                             _s7Net.Write("DB200.1000.1",true);

                        }
                        else if (!req && resp)
                        {
                            _s7Net.Write("DB200.1000.1", false);
                            _logger.LogInformation("定子检测请求复位");
                        }

                        await Task.Delay(1000);                  
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"定子检测MiddleWare异常：{ex.Message}");
                    }
                }
            });

        }
    }
}