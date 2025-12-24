using ChargePadLine.Application.Trace.Production.WorkOrders.Errors;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Shared;
using Microsoft.Extensions.Primitives;
using Microsoft.FSharp.Core;
using static ChargePadLine.Application.Trace.Production.WorkOrders.Errors.IErr_工单维护单据;

namespace ChargePadLine.Application.Trace.Production.WorkOrders
{
    #region 工单状态变更
    public record CmdArg_WorkOrder_Commit(WorkOrder WorkOrder);
    public record CmdArg_WorkOrder_Approve(WorkOrder WorkOrder);
    public record CmdArg_WorkOrder_Reject(WorkOrder WorkOrder);
    public record CmdArg_WorkOrder_Delete(WorkOrder WorkOrder);
    public record CmdArg_WorkOrder_ReEdit(WorkOrder WorkOrder);
    #endregion

    /// <summary>
    /// 启动工单
    /// </summary>
    /// <param name="WorkOrderId"></param>
    public record CmdArg_WorkOrder_StartWorkOrder(WorkOrder WorkOrder);

    public record CmdArg_WorkOrder_CreateWorkOrder(BomRecipe BomRecipe, WorkOrderCode WorOrderCode);

    public record CmdSucc_WorkOrder(WorkOrder WorOder, IReadOnlyList<IWorkOrderEvent> Events);

    public static class WorkOrderCommands
    {

        #region 启动执行流程
        public static FSharpResult<WorkOrder, IErr_工单创建> CreateWorkOrder(CmdArg_WorkOrder_CreateWorkOrder arg)
        {
            var wo = WorkOrder.MakeInfiniteWorkOrder(arg.WorOrderCode, arg.BomRecipe);
            return wo.ToOkResult<WorkOrder, IErr_工单创建>();
        }
        #endregion


        #region 启动执行流程
        public static FSharpResult<WorkOrderExecution, IErr_工单启动> StartWorkOrder(CmdArg_WorkOrder_StartWorkOrder arg)
        {
            var wo = arg.WorkOrder;
            if (wo.DocStatus is not WorkOrderDocStatus.Approved)
            {
                return new IErr_工单启动.NotReady(wo.DocStatus).ToErrResult<WorkOrderExecution, IErr_工单启动>();
            }

            return new WorkOrderExecution()
            {
                WorkOrder = wo,
                WorkOrderId = wo.Id,
                WorkOrderCode = wo.Code,

                Accumulation = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                HasFinished = false,
            }.ToOkResult<WorkOrderExecution, IErr_工单启动>();
        }
        #endregion


        #region 工单状态变更
        private static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> MapToEventListOkResult(this WorkOrder wo, IWorkOrderEvent? evt = null)
        {
            IReadOnlyList<IWorkOrderEvent> evts = evt == null ?
                new List<IWorkOrderEvent> { } :
                new List<IWorkOrderEvent> { evt };
            return new CmdSucc_WorkOrder(wo, evts)
                .ToOkResult<CmdSucc_WorkOrder, IErr_工单维护单据>();
        }
        private static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> MapToErrResult(this DocStatusErr err) => 
            err.ToErrResult<CmdSucc_WorkOrder, IErr_工单维护单据>();

        public static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> Commit(CmdArg_WorkOrder_Commit arg)
        {
            var wo = arg.WorkOrder;
            return wo.DocStatus.Match(
                handleDraft: () => {
                    wo.DocStatus = WorkOrderDocStatus.Commited;
                    return wo.MapToEventListOkResult();
                },
                handleCommitted: () => new DocStatusErr($"工单已经提交(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleRejected: () => new DocStatusErr($"工单已经拒绝(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleApproved: () => new DocStatusErr($"工单已经通过(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleDeleted: () => new DocStatusErr($"工单已经删除(id={wo.Id},Code={wo.Code.Value})").MapToErrResult()
            );
        }

        public static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> Approve(CmdArg_WorkOrder_Approve arg)
        {
            var wo = arg.WorkOrder;
            return wo.DocStatus.Match(
                handleDraft: () => new DocStatusErr($"工单尚未提交(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleCommitted: () => {
                    wo.DocStatus = WorkOrderDocStatus.Approved;
                    return wo.MapToEventListOkResult();
                },
                handleRejected: () => new DocStatusErr($"工单已经拒绝(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleApproved: () => new DocStatusErr($"工单已经通过(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleDeleted: () => new DocStatusErr($"工单已经删除(id={wo.Id},Code={wo.Code.Value})").MapToErrResult()
            );
        }

        public static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> Reject(CmdArg_WorkOrder_Reject arg)
        {
            var wo = arg.WorkOrder;
            return wo.DocStatus.Match(
                handleDraft: () => new DocStatusErr($"工单尚未提交(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleCommitted: () => {
                    wo.DocStatus = WorkOrderDocStatus.Rejected;
                    return wo.MapToEventListOkResult();
                },
                handleRejected: () => new DocStatusErr($"工单已经拒绝(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleApproved: () => new DocStatusErr($"工单已经通过(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleDeleted: () => new DocStatusErr($"工单已经删除(id={wo.Id},Code={wo.Code.Value})").MapToErrResult()
            );
        }

        public static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> Delete(CmdArg_WorkOrder_Delete arg)
        {
            var wo = arg.WorkOrder;
            return wo.MapToEventListOkResult();
        }

        public static FSharpResult<CmdSucc_WorkOrder, IErr_工单维护单据> ReEdit(CmdArg_WorkOrder_ReEdit arg)
        {
            var wo = arg.WorkOrder;
            return wo.DocStatus.Match(
                handleDraft: () => {
                    wo.DocStatus = WorkOrderDocStatus.Drafting;
                    return wo.MapToEventListOkResult(); 
                },
                handleCommitted: () => new DocStatusErr($"工单已经提交(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleRejected: () =>{
                    wo.DocStatus = WorkOrderDocStatus.Drafting;
                    return wo.MapToEventListOkResult();
                },
                handleApproved: () => new DocStatusErr($"工单已经通过(id={wo.Id},Code={wo.Code.Value})").MapToErrResult(),
                handleDeleted: () => new DocStatusErr($"工单已经删除(id={wo.Id},Code={wo.Code.Value})").MapToErrResult()
            );
        }
        #endregion
    }
}
