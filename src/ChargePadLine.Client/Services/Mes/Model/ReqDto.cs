using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.Mes.Dto
{
    public class ReqDto
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string sn { get; set; } = string.Empty;
        /// <summary>
        /// 资源号
        /// </summary>
        public string resource { get; set; } = string.Empty;
        /// <summary>
        /// 站点
        /// </summary>
        public string stationCode { get; set; } = string.Empty;
        /// <summary>
        /// 工单编码
        /// </summary>
        public string? workOrderCode { get; set; } = string.Empty;
        /// <summary>
        /// 测试结果
        /// </summary>
        public string? testResult { get; set; } = string.Empty;
        /// <summary>
        /// 测试数据
        /// </summary>
        public List<TestDataItem>? testData { get; set; } = new List<TestDataItem>();

    }

    public class TestDataItem
    {
        /// <summary>
        /// 上限
        /// </summary>
        public float? Upperlimit { get; set; }
        /// <summary>
        /// 下限
        /// </summary>
        public float? Lowerlimit { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string? Units { get; set; } = string.Empty;
        /// <summary>
        /// 参数
        /// </summary>
        public string? ParametricKey { get; set; } = string.Empty;
        /// <summary>
        /// 测试结果
        /// </summary>
        public string? TestResult { get; set; } = string.Empty;
        /// <summary>
        /// 测试值
        /// </summary>
        public string? TestValue { get; set; } = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string? Remark { get; set; } = string.Empty;       
    }
}
