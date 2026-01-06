using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Product;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace
{
    public interface ICommonInterfaseService
    {
        
        Task<List<ProductList>> GetDeviceInfoList();
 
    }
}
