using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Product;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChargePadLine.Service.Trace.Impl.CommonInterfaseService;

namespace ChargePadLine.Service.Trace
{
    public interface ICommonInterfaseService
    {
        
        Task<List<string>> GetList();
        Task<string> GetName();
        Task<FSharpResult<ValueTuple, (int, string)>> UploadCheck(RequestUploadCheckParams request);
        Task<FSharpResult<ValueTuple, (int, string)>> UploadData(RequestUploadCheckParams request);
        Task<FSharpResult<ValueTuple, (int, string)>> FeedMaterial(RequestFeedMaterialParams request);
        Task<FSharpResult<SnTraceDto, (int, string)>> TraceSN(string sn);
        Task<FSharpResult<ValueTuple, (int, string)>> UploadMaster(RequestUploadCheckParams request);

    }
}
