using ChargePadLine.Entitys.Trace.Production;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNIT = System.ValueTuple;


namespace ChargePadLine.Entitys.Trace.WorkOrders
{
    public class WorkOrderExecution
    {
        public int Id { get; set; }

        #region
        public WorkOrder WorkOrder { get; set; } = null!;
        public int WorkOrderId { get; set; }
        public WorkOrderCode WorkOrderCode { get; set; } = null!;
        #endregion

        public decimal Accumulation { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 是否结束
        /// </summary>
        public bool HasFinished { get; set; }
    }

    public static class WorkOrderExtensions
    {
        #region 获取/设置生产工位数量
        public static WorkOrderAmount GetWorkOrderAmount(this WorkOrder wo)
        {
            return wo.IsInfinite ?
                new WorkOrderAmount.Infinity() :
                new WorkOrderAmount.Quota(wo.WorkOrderAmount);
        }

        public static UNIT SetWorkOrderAmount(this WorkOrder wo, WorkOrderAmount amount) =>
            amount.Match(
                inf =>
                {
                    wo.IsInfinite = true;
                    wo.WorkOrderAmount = 0;
                    return new UNIT();
                },
                quo =>
                {
                    wo.IsInfinite = false;
                    wo.WorkOrderAmount = quo.Amount;
                    return new UNIT();
                }
            );
        #endregion
        //public static FSharpResult<TraceInfo, IErr_工单生产> MakeNewTraceInfo(this WorkOrderExecution woe, uint vsn)
        //{
        //    if (woe == null || woe.WorkOrder == null)
        //    {
        //        return new IErr_工单生产.WorkOrderNotFound().ToErrResult<TraceInfo, IErr_工单生产>();
        //    }
        //    var wo = woe.WorkOrder;

        //    if (woe.HasFinished)
        //    {
        //        return new IErr_工单生产.WorkOrderFinished().ToErrResult<TraceInfo, IErr_工单生产>();
        //    }

        //    if (wo.DocStatus is not WorkOrderDocStatus.Approved)
        //    {
        //        return new IErr_工单生产.WorkOrderNotReady(wo.DocStatus).ToErrResult<TraceInfo, IErr_工单生产>();
        //    }

        //    var amount = wo.GetWorkOrderAmount();

        //    var x = amount.Match(
        //        inf => {
        //            var pi = new TraceInfo
        //            {
        //                ProductCode = wo.ProductCode.Value,
        //                Vsn = vsn,
        //                WorkOrderId = wo.Id,
        //                BomRecipeId = wo.BomRecipeId,
        //                PIN = "",
        //                CreatedAt = DateTimeOffset.UtcNow,
        //            };
        //            return pi.ToOkResult<TraceInfo, IErr_工单生产>();
        //        },
        //        quo => {
        //            if (woe.Accumulation > quo.Amount)
        //            {
        //                return new IErr_工单生产.WorkOrderQuotaExceeds(quo.Amount, woe.Accumulation, wo.PerTraceInfo)
        //                    .ToErrResult<TraceInfo, IErr_工单生产>();
        //            }
        //            var pi = new TraceInfo
        //            {
        //                ProductCode = wo.ProductCode.Value,
        //                Vsn = vsn,
        //                WorkOrderId = wo.Id,
        //                BomRecipeId = wo.BomRecipeId,
        //                PIN = "",
        //                CreatedAt = DateTimeOffset.UtcNow,
        //            };
        //            return pi.ToOkResult<TraceInfo, IErr_工单生产>();
        //        }
        //    );
        //    return x;
        //}



        //public static FSharpResult<UNIT, Err_工单关闭> Finish(this WorkOrderExecution woe)
        //{
        //    if (woe == null)
        //    {
        //        return new Err_工单关闭.NotFound().ToErrResult<UNIT, Err_工单关闭>();
        //    }

        //    if (woe.HasFinished)
        //    {
        //        return new Err_工单关闭.AlreadyFinished().ToErrResult<UNIT, Err_工单关闭>();
        //    }

        //    woe.HasFinished = true;
        //    return new UNIT().ToOkResult<UNIT, Err_工单关闭>();
        //}

    }
}