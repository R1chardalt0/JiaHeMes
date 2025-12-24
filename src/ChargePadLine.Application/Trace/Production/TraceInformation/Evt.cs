using ChargePadLine.Entitys.Trace.TraceInformation;
using Microsoft.FSharp.Core;
using Newtonsoft.Json.Linq;
using UNIT = System.ValueTuple;


namespace ChargePadLine.Application.Trace.Production.TraceInformation
{
    public interface ITraceInfoEvent 
    {
        public void TakePlace(TraceInfo pi);
    }

    public static class Events
    {
        public static UNIT Apply(this TraceInfo pi, IReadOnlyList<ITraceInfoEvent> evts)
        {
            foreach (var evt in evts)
            {
                evt.TakePlace(pi);
            }
            return new UNIT();
        }
    }



}
