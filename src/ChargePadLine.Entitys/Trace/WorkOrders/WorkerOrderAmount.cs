using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.WorkOrders
{
    /// <summary>
    /// 工单数量
    /// </summary>
    public interface WorkOrderAmount
    {
        /// <summary>
        /// 无限数量
        /// </summary>
        public record Infinity() : WorkOrderAmount;

        /// <summary>
        /// 规定配额
        /// </summary>
        /// <param name="Amount"></param>
        public record Quota(decimal Amount) : WorkOrderAmount;
    }

    public static class ProdAmountExtensions
    {
        public static T Match<T>(this WorkOrderAmount prodAmount,
            Func<WorkOrderAmount.Infinity, T> handleInfinity,
            Func<WorkOrderAmount.Quota, T> handleQuota
            )
            => prodAmount switch
            {
                WorkOrderAmount.Infinity i => handleInfinity(i),
                WorkOrderAmount.Quota q => handleQuota(q),
                _ => throw new NotImplementedException($"未知的工单生产数量类型{typeof(WorkOrderAmount)}={prodAmount.GetType()}"),
            };
    }
}
