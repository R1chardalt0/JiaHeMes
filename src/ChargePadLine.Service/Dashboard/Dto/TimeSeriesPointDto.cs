using System;

namespace ChargePadLine.Service.Dashboard.Dto
{
    /// <summary>
    /// 通用时间序列点（用于折线图/柱状图）。
    /// </summary>
    public class TimeSeriesPointDto
    {
        /// <summary>
        /// 时间点（精确到小时或天）。
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public long Value { get; set; }
    }
}
