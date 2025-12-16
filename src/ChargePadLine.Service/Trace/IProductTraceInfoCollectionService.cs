using Microsoft.FSharp.Core;
using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    public interface IProductTraceInfoCollectionService
    {
        Task<FSharpResult<ValueTuple, (int, string)>> DataCollectForSfcExAsync(RequestParametricData request);
    }
}
