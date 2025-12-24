//using ChargePadLine.Application.Trace.Production.Traceinfo;
//using ChargePadLine.Application.Trace.Production.TraceInformation.Errors;
//using ChargePadLine.Application.Trace.Production.WorkOrders;
//using ChargePadLine.Application.Trace.Production.WorkOrders.Errors;
//using ChargePadLine.Entitys.Trace.TraceInformation;
//using Microsoft.Extensions.Options;
//using Microsoft.FSharp.Core;
//using Newtonsoft.Json.Linq;

//namespace ChargePadLine.Application.Trace.Production.TraceInformation
//{
//    #region
//    public record Input_TraceInfo_AddMain(
//        int WorkOrderId
//        );

//    public record Input_TraceInfo_BindPIN(
//        Guid TraceInfoId,
//        string PIN
//    );

//    public record Input_TraceInfo_AddBomItem(
//        Guid TraceInfoId,
//        string BomItemCode,
//        string SKU,
//        decimal Consumption
//    );
//    public record Input_TraceInfo_RemoveBomItem(
//        Guid TraceInfoId,
//        Guid BomItemId
//    );
//    public record Input_TraceInfo_AddProcItem(
//        string PIN,
//        string Station, 
//        string Key, 
//        JToken Value,
//        bool DeleteExisting
//    );
//    public record Input_TraceInfo_RemoveProcItem(
//        Guid TraceInfoId,
//        Guid ProcItemId
//    );

//    public record Input_TraceInfo_ForceOkNg(Guid TraceInfoId, bool IsNg, string NgReason);
//    public record Input_TraceInfo_ForceDestroyed(Guid TraceInfoId, bool Destroyed);
//    #endregion




//    public class TraceInfoBiz
//    {
//        private readonly AppOpt _appOpts;
//        private readonly ITraceInfoRepository _repo;
//        private readonly IWorkOrderRepository _woRepo;
//        private readonly IWorkOrderExecutionRepository _woeRepo;
//        private readonly ICtrlVsnsService _vsnSvc;

//        public TraceInfoBiz( IOptions<AppOpt> appOpts ,ITraceInfoRepository repo, IWorkOrderRepository woRepo, IWorkOrderExecutionRepository woeRepo, ICtrlVsnsService vsnsService)
//        {
//            this._appOpts = appOpts.Value;
//            this._repo = repo;
//            this._woRepo = woRepo;
//            this._woeRepo = woeRepo;
//            this._vsnSvc = vsnsService;
//        }

//        #region 工单生产-增加TraceInfo主表
//        public Task<FSharpResult<CmdArg_TraceInfo_AddMain, IErr_工单生产>> MapInputToCmdArgAsync(Input_TraceInfo_AddMain input)
//        {

//            var plineCode = this._appOpts.ProductLineCode;
//            var query =
//                        from wo in this._woRepo.FindAsync(input.WorkOrderId)
//                            .MapNullableToResult(new IErr_工单生产.WorkOrderNotFound(input.WorkOrderId) as IErr_工单生产)
//                        from woe in _woeRepo.FindWithWorOrderCodeAsync(wo.Code.Value)
//                            .MapNullableToResult(new IErr_工单生产.WorkOrderNotExecuting(input.WorkOrderId) as IErr_工单生产)
//                        from counter in this._vsnSvc.TryGetVsnAsync(wo.ProductCode.Value)
//                            .MapNullableToResult(new IErr_工单生产.MiscError($"VSN控制器未找到！ProductCode={wo.ProductCode.Value}") as IErr_工单生产)
//                        let _ = woe.WorkOrder = wo
//                        select new CmdArg_TraceInfo_AddMain(woe, plineCode, counter);
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_工单生产>> AddTraceInfoMainAsync(CmdArg_TraceInfo_AddMain cmdArg)
//        {
//            var counter = cmdArg.CtrlVsn;
//            var vsn = cmdArg.CtrlVsn.Current;
//            var query =
//                from cmdsucc in TraceInfoCommands.AddMain(cmdArg)
//                    .ToTask()
//                let _2 = counter.Current = vsn + 1
//                from pi2 in this._repo.AddAsync(cmdsucc.TraceInfo, true)
//                    .WithOkResult<TraceInfo, IErr_工单生产>(cmdsucc.TraceInfo)
//                select pi2;
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_工单生产>> AddTraceInfoMainAsync(Input_TraceInfo_AddMain input)
//        {
//            var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                        from pi in this.AddTraceInfoMainAsync(cmdArg)
//                        select pi;
//            return query;
//        }
//        #endregion


//        #region 工单生产-绑定PIN
//        public Task<FSharpResult<CmdArg_TraceInfo_BindPIN, IErr_追溯信息_BindPIN>> MapInputToCmdArgAsync(Input_TraceInfo_BindPIN input)
//        {
//            var query = from pi in this._repo.FindAsync(input.TraceInfoId)
//                            .MapNullableToResult(new IErr_追溯信息_BindPIN.TraceInfoNotFound(input.TraceInfoId) as IErr_追溯信息_BindPIN)
//                        select new CmdArg_TraceInfo_BindPIN(pi, input.PIN);
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_BindPIN>> BindPinAsync(CmdArg_TraceInfo_BindPIN cmdArg)
//        {
//            var query =
//                from cmdsucc in TraceInfoCommands.BindPIN(cmdArg)
//                    .ToTask()
//                from pi2 in this._repo.SaveChangesAsync()
//                    .WithOkResult<TraceInfo, IErr_追溯信息_BindPIN>(cmdsucc)
//                select pi2;
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_BindPIN>> BindPinAsync(Input_TraceInfo_BindPIN input)
//        {
//            var query = from cmdArg in this.MapInputToCmdArgAsync(input)
//                        from pi in this.BindPinAsync(cmdArg)
//                        select pi;
//            return query;
//        }
//        #endregion


//        #region 增加BOM生产子项
//        public Task<FSharpResult<CmdArg_TraceInfo_AddBomItem, IErr_追溯信息_增加BOM子项>> MapInputToCmdArgAsync(Input_TraceInfo_AddBomItem input)
//        { 
//            var query = 
//                from pi in this._repo.FindAsync(input.TraceInfoId)
//                    .MapNullableToResult(() => new IErr_追溯信息_增加BOM子项.TraceInfoNotFound($"id={input.TraceInfoId}") as IErr_追溯信息_增加BOM子项 )
//                from bomRecipeItem in pi.GetBomRecipeItem(input.BomItemCode)
//                    .MapNullableToResult(() => new IErr_追溯信息_增加BOM子项.BomItemNotFound(input.BomItemCode) as IErr_追溯信息_增加BOM子项)
//                    .ToTask()
//                let consuption = input.Consumption == default ? bomRecipeItem.Quota : input.Consumption
//                select new CmdArg_TraceInfo_AddBomItem(pi, bomRecipeItem, input.SKU, consuption);
//            return query;



//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_增加BOM子项>> AddBomItemAsync(CmdArg_TraceInfo_AddBomItem cmdArg)
//        {
//            var query =
//                from cmdSucc in TraceInfoCommands.AddBomItem(cmdArg)
//                    .ToTask()
//                let pi = cmdSucc.TraceInfo
//                from fin in this._repo.SaveChangesAsync()
//                    .WithOkResult<TraceInfo, IErr_追溯信息_增加BOM子项>(pi)
//                select fin ;
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_增加BOM子项>> AddBomItemAsync(Input_TraceInfo_AddBomItem input)
//        {
//            var q =
//                from cmd in this.MapInputToCmdArgAsync(input)
//                from handled in this.AddBomItemAsync(cmd)
//                select handled;
//            return q;
//        }
//        #endregion

//        #region 删除BOM生产子项
//        public Task<FSharpResult<CmdArg_TraceInfo_RemoveBomItem, IErr_追溯信息_删除BOM子项>> MapInputToCmdArgAsync(Input_TraceInfo_RemoveBomItem input)
//        {
//            var query =
//                from pi in this._repo.FindAsync(input.TraceInfoId)
//                    .MapNullableToResult(() => new IErr_追溯信息_删除BOM子项.TraceInfoNotFound(input.TraceInfoId) as IErr_追溯信息_删除BOM子项)
//                from bomRecipeItem in GetBomRecipeItem(pi, input.BomItemId)
//                    .ToTask()
//                select new CmdArg_TraceInfo_RemoveBomItem(pi, bomRecipeItem);
//            return query;


//            FSharpResult<TraceBomItem, IErr_追溯信息_删除BOM子项> GetBomRecipeItem(TraceInfo pi, Guid bomItemId)
//            {
//                var r =  pi.BomItems?.FirstOrDefault(i => i.Id == bomItemId);
//                return r.MapNullableToResult<TraceBomItem, IErr_追溯信息_删除BOM子项>(
//                    new IErr_追溯信息_删除BOM子项.BomItemNotFound(bomItemId)
//                );
//            }
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_删除BOM子项>> RemoveBomItemAsync(CmdArg_TraceInfo_RemoveBomItem cmdArg)
//        {
//            var query =
//                from cmdSucc in TraceInfoCommands.RemoveBomItem(cmdArg)
//                    .ToTask()
//                let pi = cmdSucc.TraceInfo
//                from fin in this._repo.SaveChangesAsync()
//                    .WithOkResult<TraceInfo, IErr_追溯信息_删除BOM子项>(pi)
//                select fin;
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_删除BOM子项>> RemoveBomItemAsync(Input_TraceInfo_RemoveBomItem input)
//        {
//            var q =
//                from cmd in this.MapInputToCmdArgAsync(input)
//                from handled in this.RemoveBomItemAsync(cmd)
//                select handled;
//            return q;
//        }
//        #endregion


//        #region 增加PROC生产子项
//        public Task<FSharpResult<CmdArg_TraceInfo_AddProcItem, IErr_追溯信息_增加PROC子项>> MapInputToCmdArgAsync(Input_TraceInfo_AddProcItem input)
//        {
//            var query =
//                from pi in this._repo.FindWithPINAsync(input.PIN)
//                    .MapNullableToResult(new IErr_追溯信息_增加PROC子项.TraceInfoNotFound($"PIN={input.PIN}") as IErr_追溯信息_增加PROC子项)
//                select new CmdArg_TraceInfo_AddProcItem(pi, input.Station, input.Key, input.Value, input.DeleteExisting);
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_增加PROC子项>> AddProcItemAsync(CmdArg_TraceInfo_AddProcItem arg)
//        {
//            var query = 
//                    from cmdSucc in TraceInfoCommands.AddProcItem(arg)
//                        .ToTask()
//                    from saved in this._repo.SaveChangesAsync()
//                        .WithOkResult<TraceInfo, IErr_追溯信息_增加PROC子项>(cmdSucc.TraceInfo)
//                select saved;
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_增加PROC子项>> AddProcItemAsync(Input_TraceInfo_AddProcItem input)
//        {
//            var q =
//                from cmd in this.MapInputToCmdArgAsync(input)
//                from handled in this.AddProcItemAsync(cmd)
//                select handled;
//            return q;
//        }
//        #endregion


//        #region 删除PROC生产子项
//        public Task<FSharpResult<CmdArg_TraceInfo_RemoveProcItem, IErr_追溯信息_删除Proc子项>> MapInputToCmdArgAsync(Input_TraceInfo_RemoveProcItem input)
//        {
//            var query =
//                from pi in this._repo.FindAsync(input.TraceInfoId)
//                    .MapNullableToResult(() => new IErr_追溯信息_删除Proc子项.TraceInfoNotFound(input.TraceInfoId) as IErr_追溯信息_删除Proc子项)
//                from procItem in GetProcItem(pi, input.ProcItemId)
//                    .ToTask()
//                select new CmdArg_TraceInfo_RemoveProcItem(pi, procItem);
//            return query;


//            FSharpResult<TraceProcItem, IErr_追溯信息_删除Proc子项> GetProcItem(TraceInfo pi, Guid procItemId)
//            {
//                var r = pi.ProcItems?.FirstOrDefault(i => i.Id == procItemId);
//                return r.MapNullableToResult<TraceProcItem, IErr_追溯信息_删除Proc子项>(
//                    new IErr_追溯信息_删除Proc子项.ProcItemNotFound(procItemId)
//                );
//            }
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_删除Proc子项>> RemoveProcItemAsync(CmdArg_TraceInfo_RemoveProcItem cmdArg)
//        {
//            var query =
//                from cmdSucc in TraceInfoCommands.RemoveProcItem(cmdArg)
//                    .ToTask()
//                let pi = cmdSucc.TraceInfo
//                from fin in this._repo.SaveChangesAsync()
//                    .WithOkResult<TraceInfo, IErr_追溯信息_删除Proc子项>(pi)
//                select fin;
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_删除Proc子项>> RemoveProcItemAsync(Input_TraceInfo_RemoveProcItem input)
//        {
//            var q =
//                from cmd in this.MapInputToCmdArgAsync(input)
//                from handled in this.RemoveProcItemAsync(cmd)
//                select handled;
//            return q;
//        }
//        #endregion

//        #region 强制OK/NG
//        public async Task<FSharpResult<TraceInfo, IErr_追溯信息_强制状态>> 强制OkNgAsync(CmdArg_TraceInfo_ForceOkNg cmdArg, bool save)
//        {
//            var pi = TraceInfoCommands.强制OkNg(cmdArg);
//            if (pi.IsOk && save)
//            {
//                await this._repo.SaveChangesAsync();
//            }
//            return pi;
//        }
//        public Task<FSharpResult<CmdArg_TraceInfo_ForceOkNg, IErr_追溯信息_强制状态>> MapInputToCmdArgAsyc(Input_TraceInfo_ForceOkNg input)
//        {
//            var query =
//                from pi in this._repo.FindAsync(input.TraceInfoId)
//                    .MapNullableToResult(new IErr_追溯信息_强制状态.TraceInfoNotFound($"id={input.TraceInfoId}") as IErr_追溯信息_强制状态)
//                select new CmdArg_TraceInfo_ForceOkNg(pi, IsNg: input.IsNg, NgReason: input.NgReason);
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_强制状态>> 强制OkNgAsync(Input_TraceInfo_ForceOkNg input, bool save)
//        {
//            var qry =
//                from cmdArg in this.MapInputToCmdArgAsyc(input)
//                from x in this.强制OkNgAsync(cmdArg, save)
//                select x;
//            return qry;
//        }
//        #endregion

//        #region 强制标记是否已经被破坏
//        public async Task<FSharpResult<TraceInfo, IErr_追溯信息_强制状态>> 强制破坏Async(CmdArg_TraceInfo_ForceDestroyed cmdArg, bool save)
//        {
//            var pi = TraceInfoCommands.强制破坏(cmdArg);
//            if (pi.IsOk && save)
//            {
//                await this._repo.SaveChangesAsync();
//            }
//            return pi;
//        }
//        public Task<FSharpResult<CmdArg_TraceInfo_ForceDestroyed, IErr_追溯信息_强制状态>> MapInputToCmdArgAsyc(Input_TraceInfo_ForceDestroyed input)
//        {
//            var query =
//                from pi in this._repo.FindAsync(input.TraceInfoId)
//                    .MapNullableToResult(new IErr_追溯信息_强制状态.TraceInfoNotFound($"id={input.TraceInfoId}") as IErr_追溯信息_强制状态)
//                select new CmdArg_TraceInfo_ForceDestroyed(pi, input.Destroyed);
//            return query;
//        }

//        public Task<FSharpResult<TraceInfo, IErr_追溯信息_强制状态>> 强制破坏kNgAsync(Input_TraceInfo_ForceDestroyed input, bool save)
//        {
//            var qry =
//                from cmdArg in this.MapInputToCmdArgAsyc(input)
//                from x in this.强制破坏Async(cmdArg, save)
//                select x;
//            return qry;
//        }
//        #endregion
//    }
//}
