using ChargePadLine.Client.Helpers;
using ChargePadLine.Client.Services.PlcService.Plc4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.plc4.干区气密测试
{
    public class 干区气密测试ExitMiddleWare : IPlc4Task
    {
        public Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
