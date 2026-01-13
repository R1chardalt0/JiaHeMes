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
        public List<TestDataItem>? testData { get; set; } = new List<TestDataItem>();

    }

    public class TestDataItem
    {
        public float? Upperlimit { get; set; }
        public float? Lowerlimit { get; set; }
        public string? Units { get; set; } = string.Empty;
        public string? ParametricKey { get; set; } = string.Empty;
        public string? TestResult { get; set; } = string.Empty;
        public string? TestValue { get; set; } = string.Empty;
        public string? Remark { get; set; } = string.Empty;       
    }
}
