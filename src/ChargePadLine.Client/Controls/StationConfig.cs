using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Controls
{
    public class StationConfig
    {       
        public StationItem Station1 { get; set; } = new StationItem();
        public StationItem Station2 { get; set; } = new StationItem();
        public StationItem Station3 { get; set; } = new StationItem();
        public StationItem Station4 { get; set; } = new StationItem();
        public StationItem Station5 { get; set; } = new StationItem();
        public StationItem Station6 { get; set; } = new StationItem();
        public StationItem Station7 { get; set; } = new StationItem();
        public StationItem Station8 { get; set; } = new StationItem();
        public StationItem Station9 { get; set; } = new StationItem();
        public StationItem Station10 { get; set; } = new StationItem();
        public StationItem Station11 { get; set; } = new StationItem();

        public StationItem Station12 { get; set; } = new StationItem();
        public StationItem Station13 { get; set; } = new StationItem();
        public StationItem Station14{ get; set; } = new StationItem();
        public StationItem Station15 { get; set; } = new StationItem();
        public StationItem Station16 { get; set; } = new StationItem();
        public StationItem Station17{ get; set; } = new StationItem();
    }

    public class StationItem
    {
        public string Resource { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string WorkOrderCode { get; set; } = string.Empty;
    }
}
