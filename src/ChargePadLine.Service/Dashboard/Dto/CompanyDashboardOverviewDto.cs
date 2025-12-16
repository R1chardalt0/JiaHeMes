using System;
using System.Collections.Generic;

namespace ChargePadLine.Service.Dashboard.Dto
{
    /// <summary>
    /// 公司级看板-概览统计DTO
    /// </summary>
    public class CompanyDashboardOverviewDto
    {
        /// <summary>
        /// 公司ID
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// 公司名称（可选）
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// 设备总数
        /// </summary>
        public int DeviceTotal { get; set; }

        /// <summary>
        /// 运行中设备数（Status=1/启用 视为运行中）
        /// </summary>
        public int RunningDevices { get; set; }

        /// <summary>
        /// 告警设备数（依据最新追溯记录AlarmMessages是否为空）
        /// </summary>
        public int WarningDevices { get; set; }

        /// <summary>
        /// 指定时间范围内的总产量
        /// </summary>
        public long TotalProduction { get; set; }

        /// <summary>
        /// 指定时间范围内OK数
        /// </summary>
        public long OKNum { get; set; }

        /// <summary>
        /// 指定时间范围内NG数
        /// </summary>
        public long NGNum { get; set; }

        /// <summary>
        /// 良率（百分比0~100）
        /// </summary>
        public double Yield { get; set; }

        /// <summary>
        /// 设备状态分布（key: 状态，value: 数量）
        /// </summary>
        public Dictionary<string, int> DeviceStatusMap { get; set; } = new();

        /// <summary>
        /// 产量趋势（按小时或天），用于折线图
        /// </summary>
        public List<TimeSeriesPointDto> ProductionTrend { get; set; } = new();
    }
}
