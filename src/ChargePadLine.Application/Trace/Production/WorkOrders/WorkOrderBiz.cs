// using ChargePadLine.Application.Trace.Production.Recipes;
// using ChargePadLine.Application.Trace.Production.WorkOrders.Errors;
// using ChargePadLine.DbContexts.Repository;
// using ChargePadLine.Entitys.Trace.Recipes.Entities;
// using ChargePadLine.Entitys.Trace.WorkOrders;
// using ChargePadLine.Shared;
// using Microsoft.FSharp.Core;
// using System.Linq.Expressions;
// using System.Runtime.CompilerServices;

// namespace ChargePadLine.Application.Trace.Production.WorkOrders
// {
//     /*
//     public record Input_WorkOrder_CommitDoc(int WorkOrderId);
//     public record Input_WorkOrder_ApproveDoc(int WorkOrderId);
//     public record Input_WorkOrder_RejectDoc(int WorkOrderId);
//     public record Input_WorkOrder_DeleteDoc(int WorkOrderId);
//     public record Input_WorkOrder_ReEditDoc(int WorkOrderId);
//     */
//     public record Input_WorkOrder_StartWorkOrder(int WorkOrderId);
//     public record Input_WorkOrder_CreateWorkOrder(int BomRecipeId, string? Description, decimal PerTraceInfo);

//     public class WorkOrderBiz
//     {
//         private readonly IRepository<WorkOrder> _repo;
//         private readonly IRepository<WorkOrderExecution> _execRepo;
//         private readonly IRepository<BomRecipe> _bomRepo;

//         public WorkOrderBiz(IRepository<WorkOrder> repo, IRepository<WorkOrderExecution> execRepo, IRepository<BomRecipe> bomRepo)
//         {
//             this._repo = repo;
//             this._execRepo = execRepo;
//             this._bomRepo = bomRepo;
//         }

//         #region 新建工单
//         public async Task<FSharpResult<CmdArg_WorkOrder_CreateWorkOrder, IErr_工单创建>> MapInputToCmdArgAsync(Input_WorkOrder_CreateWorkOrder input)
//         {
//             // 检查对应的BOM是否存在
//             var bomRecipe = await this._bomRepo.GetAsync(b => b.Id == input.BomRecipeId);
//             if (bomRecipe == null)
//             {
//                 return new IErr_工单创建.RecipeNotFound(input.BomRecipeId)
//                     .ToErrResult<CmdArg_WorkOrder_CreateWorkOrder, IErr_工单创建>();
//             }

//             var code = Guid.NewGuid().ToString();
//             return new CmdArg_WorkOrder_CreateWorkOrder(bomRecipe, code)
//                 .ToOkResult<CmdArg_WorkOrder_CreateWorkOrder, IErr_工单创建>();
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单创建>> CreateWorkOrderAsync(CmdArg_WorkOrder_CreateWorkOrder cmdArg)
//         {
//             var query = from wo in WorkOrderCommands.CreateWorkOrder(cmdArg)
//                             .ToTask()
//                         from wo2 in this._repo.InsertAsync(wo)
//                             .WithOkResult<WorkOrder, IErr_工单创建>(wo)
//                         select wo2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单创建>> CreateWorkOrderAsync(Input_WorkOrder_CreateWorkOrder input)
//         {
//             var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                         from wo in this.CreateWorkOrderAsync(cmdArg)
//                         select wo;
//             return query;
//         }
//         #endregion


//         #region 启动执行
//         public async Task<FSharpResult<CmdArg_WorkOrder_StartWorkOrder, IErr_工单启动>> MapInputToCmdArgAsync(Input_WorkOrder_StartWorkOrder input)
//         {
//             var wo = await this._repo.GetAsync(w => w.Id == input.WorkOrderId);
//             if (wo == null)
//             {
//                 return new IErr_工单启动.NotFound(WorkOrderId: input.WorkOrderId)
//                     .ToErrResult<CmdArg_WorkOrder_StartWorkOrder, IErr_工单启动>();
//             }
//             var woe = await this._execRepo.GetAsync(we => we.WorkOrderCode == wo.Code.Value);
//             if (woe != null)
//             {
//                 return new IErr_工单启动.AlreadyStarted(woe.Id)
//                     .ToErrResult<CmdArg_WorkOrder_StartWorkOrder, IErr_工单启动>();
//             }

//             return new CmdArg_WorkOrder_StartWorkOrder(wo)
//                 .ToOkResult<CmdArg_WorkOrder_StartWorkOrder, IErr_工单启动>();
//         }

//         public Task<FSharpResult<WorkOrderExecution, IErr_工单启动>> StartWorkOrderAsync(CmdArg_WorkOrder_StartWorkOrder cmdArg)
//         {
//             var wo = cmdArg.WorkOrder;
//             var query = from woe1 in WorkOrderCommands.StartWorkOrder(cmdArg)
//                             .ToTask()
//                         from woe2 in this._execRepo.InsertAsync(woe1)
//                             .WithOkResult<WorkOrderExecution, IErr_工单启动>(woe1)
//                         select woe2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrderExecution, IErr_工单启动>> StartWorkOrderAsync(Input_WorkOrder_StartWorkOrder input)
//         {
//             var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                         from wo in this.StartWorkOrderAsync(cmdArg)
//                         select wo;
//             return query;
//         }
//         #endregion


//         #region 提交单据
//         public Task<FSharpResult<CmdArg_WorkOrder_Commit, IErr_工单维护单据>> MapInputToCmdArgAsync(Input_WorkOrder_CommitDoc input)
//         {
//             var query = from x in this._repo.GetAsync(w => w.Id == input.WorkOrderId)
//                             .MapNullableToResult<WorkOrder, IErr_工单维护单据>(() => new IErr_工单维护单据.DocNotFound(input.WorkOrderId))
//                         select new CmdArg_WorkOrder_Commit(x);
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> CommitDocAsync(CmdArg_WorkOrder_Commit cmdArg)
//         {
//             var wo = cmdArg.WorkOrder;
//             var query = from cmdsucc in WorkOrderCommands.Commit(cmdArg)
//                             .ToTask()
//                         from wo2 in this._repo.UpdateAsync(wo)
//                             .WithOkResult<WorkOrder, IErr_工单维护单据>(wo)
//                         select wo2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> CommitDocAsync(Input_WorkOrder_CommitDoc input)
//         {
//             var query =
//                     from cmdArg in this.MapInputToCmdArgAsync(input)
//                     from wo in this.CommitDocAsync(cmdArg)
//                         .SelectError(e => e as IErr_工单维护单据)
//                     select wo;
//             return query;
//         }
//         #endregion


//         #region 批准单据
//         public Task<FSharpResult<CmdArg_WorkOrder_Approve, IErr_工单维护单据>> MapInputToCmdArgAsync(Input_WorkOrder_ApproveDoc input)
//         {
//             var query = from x in this._repo.GetAsync(w => w.Id == input.WorkOrderId)
//                             .MapNullableToResult<WorkOrder, IErr_工单维护单据>(() => new IErr_工单维护单据.DocNotFound(input.WorkOrderId))
//                         select new CmdArg_WorkOrder_Approve(x);
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> ApproveDocAsync(CmdArg_WorkOrder_Approve cmdArg)
//         {
//             var wo = cmdArg.WorkOrder;
//             var query = from cmdsucc in WorkOrderCommands.Approve(cmdArg)
//                             .ToTask()
//                         from wo2 in this._repo.UpdateAsync(wo)
//                             .WithOkResult<WorkOrder, IErr_工单维护单据>(wo)
//                         select wo2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> ApproveDocAsync(Input_WorkOrder_ApproveDoc input)
//         {
//             var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                         from wo in this.ApproveDocAsync(cmdArg)
//                         select wo;
//             return query;
//         }
//         #endregion


//         #region 拒绝单据
//         public Task<FSharpResult<CmdArg_WorkOrder_Reject, IErr_工单维护单据>> MapInputToCmdArgAsync(Input_WorkOrder_RejectDoc input)
//         {
//             var query = from x in this._repo.GetAsync(w => w.Id == input.WorkOrderId)
//                             .MapNullableToResult<WorkOrder, IErr_工单维护单据>(() => new IErr_工单维护单据.DocNotFound(input.WorkOrderId))
//                         select new CmdArg_WorkOrder_Reject(x);
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> RejectDocAsync(CmdArg_WorkOrder_Reject cmdArg)
//         {
//             var wo = cmdArg.WorkOrder;
//             var query = from cmdsucc in WorkOrderCommands.Reject(cmdArg)
//                             .ToTask()
//                         from wo2 in this._repo.UpdateAsync(wo)
//                             .WithOkResult<WorkOrder, IErr_工单维护单据>(wo)
//                         select wo2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> RejectDocAsync(Input_WorkOrder_RejectDoc input)
//         {
//             var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                         from wo in this.RejectDocAsync(cmdArg)
//                         select wo;
//             return query;
//         }
//         #endregion



//         #region 删除单据
//         public Task<FSharpResult<CmdArg_WorkOrder_Delete, IErr_工单维护单据>> MapInputToCmdArgAsync(Input_WorkOrder_DeleteDoc input)
//         {
//             var query = from x in this._repo.GetAsync(w => w.Id == input.WorkOrderId)
//                             .MapNullableToResult<WorkOrder, IErr_工单维护单据>(() => new IErr_工单维护单据.DocNotFound(input.WorkOrderId))
//                         select new CmdArg_WorkOrder_Delete(x);
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> DeleteDocAsync(CmdArg_WorkOrder_Delete cmdArg)
//         {
//             var wo = cmdArg.WorkOrder;
//             var query = from cmdsucc in WorkOrderCommands.Delete(cmdArg)
//                             .ToTask()
//                         from wo2 in this._repo.UpdateAsync(wo)
//                             .WithOkResult<WorkOrder, IErr_工单维护单据>(wo)
//                         select wo2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> DeleteDocAsync(Input_WorkOrder_DeleteDoc input)
//         {
//             var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                         from wo in this.DeleteDocAsync(cmdArg)
//                         select wo;
//             return query;
//         }
//         #endregion


//         #region 重新提交单据
//         public Task<FSharpResult<CmdArg_WorkOrder_ReEdit, IErr_工单维护单据>> MapInputToCmdArgAsync(Input_WorkOrder_ReEditDoc input)
//         {
//             var query = from x in this._repo.GetAsync(w => w.Id == input.WorkOrderId)
//                             .MapNullableToResult<WorkOrder, IErr_工单维护单据>(() => new IErr_工单维护单据.DocNotFound(input.WorkOrderId))
//                         select new CmdArg_WorkOrder_ReEdit(x);
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> ReEditDocAsync(CmdArg_WorkOrder_ReEdit cmdArg)
//         {
//             var wo = cmdArg.WorkOrder;
//             var query = from cmdsucc in WorkOrderCommands.ReEdit(cmdArg)
//                             .ToTask()
//                         from wo2 in this._repo.UpdateAsync(wo)
//                             .WithOkResult<WorkOrder, IErr_工单维护单据>(wo)
//                         select wo2;
//             return query;
//         }

//         public Task<FSharpResult<WorkOrder, IErr_工单维护单据>> ReEditDocAsync(Input_WorkOrder_ReEditDoc input)
//         {
//             var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                         from wo in this.ReEditDocAsync(cmdArg)
//                         select wo;
//             return query;
//         }
//         #endregion
//     }
// }
