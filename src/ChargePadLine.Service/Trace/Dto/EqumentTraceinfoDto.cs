using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
    public class EqumentTraceinfoDto
    {
        public string? ProductionLine { get; set; }
        public string? DeviceName { get; set; }
        public string? AlarMessages { get; set; }
        public string? DeviceEnCode { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset SendTime { get; set; }
        public List<Iotdata>? Parameters { get; set; } = new List<Iotdata>();
    }
}
