using System;
using System.Threading.Tasks;
using ChargePadLine.Service.Dashboard.Dto;

namespace ChargePadLine.Service.Dashboard
{
    /// <summary>
    /// 公司级数据看板服务
    /// </summary>
    public interface ICompanyDashboardService
    {
        /// <summary>
        /// 获取公司级看板概览（含状态分布与产量趋势）
        /// </summary>
        /// <remarks>
        /// 产量趋势统计方法：
        /// 1. 数据来源：生产订单表(ProductionOrder)中的完成订单数量
        /// 2. 时间分组：按小时/天维度将时间段分割成多个时间点
        /// 3. 数据聚合：统计每个时间段内完成的订单数(产量值)
        /// 4. 平均线计算：计算整个时间段的平均产量(如图中的3.6/小时)
        ///    公式：总完成订单数 / 时间段总数
        /// 5. 展示形式：折线图展示产量变化趋势，虚线表示平均产量基准线
        /// </remarks>
        /// <param name="companyId">公司ID，可选</param>
        /// <param name="companyName">公司名称，可选（与ID二选一）</param>
        /// <param name="startTime">开始时间，默认当天00:00:00</param>
        /// <param name="endTime">结束时间，默认当前时间</param>
        Task<CompanyDashboardOverviewDto> GetOverviewAsync(int? companyId, string? companyName, DateTime? startTime, DateTime? endTime);
    }
}
