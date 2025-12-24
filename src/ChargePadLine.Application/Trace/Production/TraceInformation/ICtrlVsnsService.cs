using ChargePadLine.Entitys.Trace.TraceInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Application.Trace.Production.TraceInformation
{
    public interface ICtrlVsnsService
    {
        Task<CtrlVsn> GetVsnAsync(string productCode);

        Task SaveChangesAsync();

        Task<CtrlVsn?> TryGetVsnAsync(string productCode);
    }
}
