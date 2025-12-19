using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.WorkOrders
{
    public enum WorkOrderDocStatus
    {
        Drafting = 0,
        Commited = 1,
        Rejected = 2,
        Approved = 3,
        Deleted = 999,
    }

    public static class WorkOrderDocStatusExtensions
    {
        public static T Match<T>(this WorkOrderDocStatus status,
            Func<T> handleDraft,
            Func<T> handleCommitted,
            Func<T> handleRejected,
            Func<T> handleApproved,
            Func<T> handleDeleted
            ) =>
            status switch
            {
                WorkOrderDocStatus.Drafting => handleDraft(),
                WorkOrderDocStatus.Commited => handleCommitted(),
                WorkOrderDocStatus.Rejected => handleRejected(),
                WorkOrderDocStatus.Approved => handleApproved(),
                WorkOrderDocStatus.Deleted => handleDeleted(),
                _ => throw new NotImplementedException($"未知的工单状态={status}"),
            };
    }
}
