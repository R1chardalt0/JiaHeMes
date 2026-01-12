using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.Mes.Model
{
    public class ReqDto
    {
        public string sn { get; set; } = string.Empty;
        public string resource { get; set; } = string.Empty;
        public string stationCode { get; set; } = string.Empty;
        public string? workOrderCode { get; set; } = string.Empty;
        public string? testResult { get; set; } = string.Empty;
        public string? testData { get; set; } = string.Empty;

    }
}
