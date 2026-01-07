using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChargePadLine.Client.Services.PlcService
{
    public interface ILogService
    {
        Task RecordLogAsync(LogLevel logLevel, string message);
    }
}
