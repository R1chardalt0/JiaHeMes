using Microsoft.FSharp.Core;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    public interface IDeviceInfoCollectionService
    {
        Task<FSharpResult<ValueTuple, (int, string)>> DeviceDataCollectionExAsync(string deviceEnCode, DateTimeOffset sendTime, string alarmMessages, List<Iotdata> updateParams);
    }
}
