using ChargePadLine.Application.Trace.Production.TraceInformation;
using ChargePadLine.Application.Trace.Production.TraceInformation.Errors;
using ChargePadLine.Application.Trace.Production.WorkOrders.Errors;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Shared;
using Microsoft.FSharp.Core;
using Newtonsoft.Json.Linq;

namespace ChargePadLine.Application.Trace.Production.Traceinfo
{
    public record CmdArg_TraceInfo_AddMain(WorkOrderExecution WorkOrderExecution, string ProductLineCode, CtrlVsn CtrlVsn);

    public record CmdArg_TraceInfo_AddBomItem(TraceInfo TraceInfo, BomRecipeItem Recipe, SKU SKU, decimal Consumption );

    public record CmdArg_TraceInfo_RemoveBomItem(TraceInfo TraceInfo ,TraceBomItem ProdBomItem);

    public record CmdArg_TraceInfo_AddProcItem(TraceInfo TraceInfo, string Station, string Key, JToken Value, bool DeleteExisting);
    public record CmdArg_TraceInfo_RemoveProcItem(TraceInfo TraceInfo ,TraceProcItem ProdProcItem);

    /// <summary>
    /// 绑定产品识别码
    /// </summary>
    /// <param name="TraceInfo"></param>
    /// <param name="PIN"></param>
    public record CmdArg_TraceInfo_BindPIN(TraceInfo TraceInfo, SKU PIN);

    /// <summary>
    /// 强制Ok/Ng状态
    /// </summary>
    public record CmdArg_TraceInfo_ForceOkNg(TraceInfo TraceInfo, bool IsNg, string NgReason);

    /// <summary>
    /// 强制已破坏状态
    /// </summary>
    public record CmdArg_TraceInfo_ForceDestroyed(TraceInfo TraceInfo, bool Destroyed);

    public record CmdSucc_TraceInfo(TraceInfo TraceInfo, IReadOnlyList<ITraceInfoEvent> Events);

    public static class TraceInfoCommands
    {
        public static FSharpResult<CmdSucc_TraceInfo, IErr_工单生产> AddMain(CmdArg_TraceInfo_AddMain arg)
        {
            var woe = arg.WorkOrderExecution;
            var wo = woe.WorkOrder;
            if (woe.HasFinished)
            {
                return new IErr_工单生产.WorkOrderFinished()
                    .ToErrResult<CmdSucc_TraceInfo, IErr_工单生产>();
            }

            if (wo.DocStatus is not WorkOrderDocStatus.Approved)
            {
                return new IErr_工单生产.WorkOrderNotReady(wo.DocStatus)
                    .ToErrResult<CmdSucc_TraceInfo, IErr_工单生产>();
            }

            var amount = wo.GetWorkOrderAmount();

            var pi = new TraceInfo
            {
                Vsn = arg.CtrlVsn.Current,
                ProductLine = arg.ProductLineCode,
                ProductCode = wo.ProductCode.Value,
                WorkOrderId = wo.Id,
                BomRecipeId = wo.BomRecipeId,
                PIN = "",
                CreatedAt = DateTimeOffset.UtcNow
            };

            return amount.Match(
                inf => {
                    return new CmdSucc_TraceInfo(pi, new List<ITraceInfoEvent>()).ToOkResult<CmdSucc_TraceInfo, IErr_工单生产>();
                },
                quo => {
                    if (woe.Accumulation > quo.Amount)
                    {
                        return new IErr_工单生产.WorkOrderQuotaExceeds(quo.Amount, woe.Accumulation, wo.PerTraceInfo)
                            .ToErrResult<CmdSucc_TraceInfo, IErr_工单生产>();
                    }
                    return new CmdSucc_TraceInfo(pi, new List<ITraceInfoEvent>()).ToOkResult<CmdSucc_TraceInfo, IErr_工单生产>();
                }
            );

        }

        public static FSharpResult<TraceInfo, IErr_追溯信息_BindPIN> BindPIN(CmdArg_TraceInfo_BindPIN arg)
        {
            var pi = arg.TraceInfo;

            if (!string.IsNullOrEmpty(pi.PIN?.Value))
            {
                return new IErr_追溯信息_BindPIN.AlreadyBound(pi.Id)
                    .ToErrResult<TraceInfo, IErr_追溯信息_BindPIN>();
            }
            pi.PIN = arg.PIN.Value;
            return pi.ToOkResult<TraceInfo, IErr_追溯信息_BindPIN>();
        }



        public static FSharpResult<CmdSucc_TraceInfo, IErr_追溯信息_增加BOM子项> AddBomItem(TraceInfo tr, BomItemCode bomItemCode, SKU sku, decimal consumption)
        {
            var recipe = tr.GetBomRecipeItem(bomItemCode.Value);
            if (recipe == null)
            {
                return new IErr_追溯信息_增加BOM子项.BomItemNotFound(bomItemCode.Value).ToErrResult<CmdSucc_TraceInfo, IErr_追溯信息_增加BOM子项>();
            }
            var cmdArg = new CmdArg_TraceInfo_AddBomItem(tr, recipe, sku, consumption);
            return AddBomItem(cmdArg);
        }


        public static FSharpResult<CmdSucc_TraceInfo, IErr_追溯信息_增加BOM子项> AddBomItem(CmdArg_TraceInfo_AddBomItem arg)
        {
            var pi = arg.TraceInfo;
            var recipe = arg.Recipe;

            // 已经存在的数量是否会超过定额
            var exists = pi.BomItems.Where(i => !i.IsDeleted && i.BomItemCode.Value == recipe.BomItemCode.Value).ToList();
            var acc = exists.Sum(i => i.Consumption);
            if (acc + arg.Consumption> recipe.Quota)
            {
                return new IErr_追溯信息_增加BOM子项.BomItemExceedsQuota(recipe.Quota, acc, arg.Consumption)
                    .ToErrResult<CmdSucc_TraceInfo, IErr_追溯信息_增加BOM子项>();
            }

            var item = new TraceBomItem
            {
                MaterialCode = recipe.MaterialCode.Value,
                MeasureUnit = recipe.MeasureUnit.Value,
                MaterialName = recipe.MaterialName,
                BomItemCode = recipe.BomItemCode.Value,
                Description = recipe.Description,
                BomId = recipe.BomId,
                Quota = recipe.Quota,
                SKU = arg.SKU,
                Vsn = pi.Vsn,
                TraceInfoId = pi.Id,
                TraceInfo = pi,
                Consumption = arg.Consumption,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            pi.BomItems.Add(item);

            return 
                 new CmdSucc_TraceInfo(pi, new List<ITraceInfoEvent>())
                 .ToOkResult<CmdSucc_TraceInfo, IErr_追溯信息_增加BOM子项>();
        }

        public static FSharpResult<CmdSucc_TraceInfo, IErr_追溯信息_删除BOM子项> RemoveBomItem(CmdArg_TraceInfo_RemoveBomItem arg)
        {
            var pbi = arg.ProdBomItem;
            if (pbi.IsDeleted)
            {
                return new IErr_追溯信息_删除BOM子项.AlreadyDeleted(pbi.Id)
                    .ToErrResult<CmdSucc_TraceInfo, IErr_追溯信息_删除BOM子项>();
            }
            pbi.IsDeleted = true;
            pbi.DeletedAt = DateTimeOffset.UtcNow;
            return new CmdSucc_TraceInfo(arg.TraceInfo, new List<ITraceInfoEvent>())
                .ToOkResult<CmdSucc_TraceInfo, IErr_追溯信息_删除BOM子项>();
        }

        public static FSharpResult<CmdSucc_TraceInfo, IErr_追溯信息_增加PROC子项> AddProcItem( CmdArg_TraceInfo_AddProcItem arg)
        {
            var pi = arg.TraceInfo;
            var station = arg.Station;
            var key = arg.Key; 
            var exists = pi.ProcItems.Where(i => !i.IsDeleted && i.Station == station && i.Key == key).ToList();

            // 如果已经存在记录
            if (exists.Count > 0)
            {
                // 倘若不需要强制删除老数据，则返回重复加工的信息
                if (!arg.DeleteExisting)
                {
                    return new IErr_追溯信息_增加PROC子项.AlreadyExists(station, key)
                        .ToErrResult<CmdSucc_TraceInfo, IErr_追溯信息_增加PROC子项>();
                }

                // 运行至此，说明一定有未删除的历史记录，且需要删除老数据。直接标记老数据无效覆盖
                foreach (var ex in exists)
                {
                    ex.IsDeleted = true;
                    ex.DeletedAt = DateTimeOffset.UtcNow;
                }
            }

            // 增加加工记录
            var item = new TraceProcItem
            {
                TraceInfo = pi,
                TraceInfoId = pi.Id,
                Vsn = pi.Vsn,

                Station = arg.Station,
                Key = arg.Key,
                Value = arg.Value,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            pi.ProcItems.Add(item);

            return new CmdSucc_TraceInfo(pi, new List<ITraceInfoEvent>() { })
                .ToOkResult<CmdSucc_TraceInfo, IErr_追溯信息_增加PROC子项>();
        }


        public static FSharpResult<CmdSucc_TraceInfo, IErr_追溯信息_删除Proc子项> RemoveProcItem(CmdArg_TraceInfo_RemoveProcItem arg)
        {
            var pbi = arg.ProdProcItem;
            if (pbi.IsDeleted)
            {
                return new IErr_追溯信息_删除Proc子项.AlreadyDeleted(pbi.Id)
                    .ToErrResult<CmdSucc_TraceInfo, IErr_追溯信息_删除Proc子项>();
            }
            pbi.IsDeleted = true;
            pbi.DeletedAt = DateTimeOffset.UtcNow;
            return new CmdSucc_TraceInfo(arg.TraceInfo, new List<ITraceInfoEvent>())
                .ToOkResult<CmdSucc_TraceInfo, IErr_追溯信息_删除Proc子项>();
        }


        public static FSharpResult<TraceInfo, IErr_追溯信息_强制状态> 强制OkNg(CmdArg_TraceInfo_ForceOkNg cmdArg)
        { 
            var pi = cmdArg.TraceInfo;
            pi.IsNG = cmdArg.IsNg;
            pi.NGReason = cmdArg.NgReason;
            return pi.ToOkResult<TraceInfo, IErr_追溯信息_强制状态>();
        }


        public static FSharpResult<TraceInfo, IErr_追溯信息_强制状态> 强制破坏(CmdArg_TraceInfo_ForceDestroyed cmdArg)
        {
            var pi = cmdArg.TraceInfo;
            pi.Destroyed = cmdArg.Destroyed;
            if (cmdArg.Destroyed)
            { 
                pi.DestroyedAt =  DateTimeOffset.UtcNow;
            }
            return pi.ToOkResult<TraceInfo, IErr_追溯信息_强制状态>();
        }
    }

}
