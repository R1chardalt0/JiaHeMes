using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Controls
{
    public class PlcConfig
    {
        public PlcItem Plc1 { get; set; } = new PlcItem();
        public PlcItem Plc2 { get; set; } = new PlcItem();
        public PlcItem Plc3 { get; set; } = new PlcItem();
        public PlcItem Plc4 { get; set; } = new PlcItem();
        public PlcItem Plc5 { get; set; } = new PlcItem();
        public PlcItem Plc6 { get; set; } = new PlcItem();
        public PlcItem Plc7 { get; set; } = new PlcItem();
        public PlcItem Plc8 { get; set; } = new PlcItem();
        public PlcItem Plc9 { get; set; } = new PlcItem();
        public PlcItem Plc10 { get; set; } = new PlcItem();
        public PlcItem Plc11 { get; set; } = new PlcItem();
    }

    public class PlcItem
    {
        public bool IsEnabled { get; set; } = true;
        public int ScanInterval { get; set; } = 50;
        public string IpAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 102;
    }
}
