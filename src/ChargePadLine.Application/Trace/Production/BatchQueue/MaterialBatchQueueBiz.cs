//using ChargePadLine.Application.Trace.Production.Recipes;
//using ChargePadLine.Entitys.Trace.Production;
//using ChargePadLine.Entitys.Trace.Production.BatchQueue;
//using ChargePadLine.Shared;
//using Microsoft.FSharp.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChargePadLine.Application.Trace.Production.BatchQueue
//{
//    public record Input_物料批_上料(string BomItemCode, string BatchCode, int Amount, int Priority);

//    public record Input_物料批_扣料(string BomItemCode, int RequiredAmount);

//    public record Input_物料批_指定批次单一扣料(string BomItemCode, string BatchCode);

//    public record Input_物料批_调整优先级(int Id, int Priority);

//    public class MaterialBatchQueueBiz
//    {
//        private readonly IMaterialBatchQueueItemRepo _repo;
//        private readonly IBomRecipeRepository _bomRepo;

//        public MaterialBatchQueueBiz(IMaterialBatchQueueItemRepo repo, IBomRecipeRepository bomRepo)
//        {
//            this._repo = repo;
//            this._bomRepo = bomRepo;
//        }

//        #region 上料
//        public async Task<FSharpResult<BatchMaterialQueueItem, IErr_批次上料>> 上料Async(CmdArg_物料批_上料 cmdArg)
//        {
//            var q =
//                from op in MaterialBatchQueueCmds.上料(cmdArg).ToTask()
//                from s in this._repo.AddAsync(op, true)
//                    .WithOkResult<BatchMaterialQueueItem, IErr_批次上料>(op)
//                select s;
//            return await q;
//        }
//        public async Task<FSharpResult<CmdArg_物料批_上料, IErr_批次上料>> MapInputToCmdArgAsync(Input_物料批_上料 input)
//        {
//            // 校验数量是否合法  
//            if (input.Amount <= 0)
//            {
//                IErr_批次上料 err = new IErr_批次上料.Err_批次上料_数量非法(input.Amount);
//                return err.ToErrResult<CmdArg_物料批_上料, IErr_批次上料>();
//            }

//            // 校验BomItemCode合法
//            var bomItem = await this._bomRepo.FindBomItemByCodeWithNoTrackAsync(input.BomItemCode);
//            if (bomItem == null)
//            {
//                IErr_批次上料 err = new IErr_批次上料.Err_批次上料_BomItem不存在(input.BomItemCode);
//                return err.ToErrResult<CmdArg_物料批_上料, IErr_批次上料>();
//            }

//            // 校验物料批是否存在
//            var existed = await this._repo.CheckBatchExistsAsync(input.BomItemCode, input.BatchCode);
//            if (existed != null)
//            {
//                IErr_批次上料 err = new IErr_批次上料.Err_批次上料_批次重复(input.BomItemCode, input.BatchCode, existed.Id);
//                return err.ToErrResult<CmdArg_物料批_上料, IErr_批次上料>();
//            }

//            var cmdArg = new CmdArg_物料批_上料(input.BomItemCode, input.BatchCode, input.Amount, input.Priority);
//            return cmdArg.ToOkResult<CmdArg_物料批_上料, IErr_批次上料>();
//        }

//        public async Task<FSharpResult<BatchMaterialQueueItem, IErr_批次上料>> 上料Async(Input_物料批_上料 input)
//        {
//            var res =
//                from cmdArg in this.MapInputToCmdArgAsync(input)
//                from op in this.上料Async(cmdArg)
//                select op;
//            return await res;
//        }
//        #endregion


//        #region 扣料
//        public async Task<FSharpResult<List<扣料描述>, IErr_批次扣料>> 扣料Async(CmdArg_物料批_扣料 cmdArg)
//        {
//            var q =
//                from op in MaterialBatchQueueCmds.扣料(cmdArg).ToTask()
//                from s in this._repo.SaveChangesAsync()
//                    .WithOkResult<List<扣料描述>, IErr_批次扣料>(op)
//                select s;
//            return await q;
//        }

//        public async Task<FSharpResult<CmdArg_物料批_扣料, IErr_批次扣料>> MapInputToCmdArgAsync(Input_物料批_扣料 input)
//        {
//            if (input.RequiredAmount < 1)
//            {
//                return new IErr_批次扣料.Err_批次扣料_扣除量非法(input.RequiredAmount)
//                    .ToErrResult<CmdArg_物料批_扣料, IErr_批次扣料>();
//            }
//            var candicates = await this._repo.FindTopCandicatesAsync(input.BomItemCode, 4);
//            var cmdArg = new CmdArg_物料批_扣料(input.BomItemCode, candicates, input.RequiredAmount);
//            return cmdArg.ToOkResult<CmdArg_物料批_扣料, IErr_批次扣料>();
//        }

//        public async Task<FSharpResult<List<扣料描述>, IErr_批次扣料>> 扣料Async(Input_物料批_扣料 input)
//        {
//            var q =
//                from cmdArg in this.MapInputToCmdArgAsync(input)
//                from op in this.扣料Async(cmdArg)
//                select op;
//            return await q;
//        }

//        public async Task<FSharpResult<IReadOnlyList<SKU>, IErr_批次扣料>> 扣料并生成SKUAsync(Input_物料批_扣料 input)
//        {
//            var res = await this.扣料Async(input);
//            return res.Select(o => o.ExpandSKUs());
//        }
//        #endregion

//        #region 指定批次单一扣料
//        public async Task<FSharpResult<BatchMaterialQueueItem, IErr_批次扣料>> GetRequiredBatchMaterialQueueItemAsync(Input_物料批_指定批次单一扣料 input)
//        {
//            var candicates = await this._repo.FindTopCandicatesAsync(input.BomItemCode, 1);
//            if (candicates.Count == 0)
//            {
//                return new IErr_批次扣料.Err_批次扣料_超额(input.BomItemCode, 0, 1)
//                    .ToErrResult<BatchMaterialQueueItem, IErr_批次扣料>();
//            }
//            var candicate = candicates[0];
//            if (candicate.BatchCode != input.BatchCode)
//            {
//                return new IErr_批次扣料.Err_批次扣料_指定批次错误(input.BatchCode, input.BatchCode, candicate.BatchCode)
//                    .ToErrResult<BatchMaterialQueueItem, IErr_批次扣料>();
//            }

//            return candicate.ToOkResult<BatchMaterialQueueItem, IErr_批次扣料>();
//        }

//        public async Task<FSharpResult<扣料描述, IErr_批次扣料>> 扣料Async(BatchMaterialQueueItem item)
//        {
//            var q =
//                from op in MaterialBatchQueueCmds.单扣料(item, 1).ToTask()
//                from s in this._repo.SaveChangesAsync()
//                    .WithOkResult<扣料描述, IErr_批次扣料>(op)
//                select s;
//            return await q;
//        }

//        public async Task<FSharpResult<扣料描述, IErr_批次扣料>> 扣料Async(Input_物料批_指定批次单一扣料 input)
//        {
//            var q =
//                from item in this.GetRequiredBatchMaterialQueueItemAsync(input)
//                from s in this.扣料Async(item)
//                select s;
//            return await q;
//        }
//        #endregion


//        #region 调整优先级
//        public async Task<FSharpResult<BatchMaterialQueueItem, IErr>> 调整优先级Async(CmdArg_物料批_调整优先级 cmdArg)
//        {
//            var q =
//                from op in MaterialBatchQueueCmds.调整优先级(cmdArg).ToTask()
//                from s in this._repo.SaveChangesAsync()
//                    .WithOkResult<BatchMaterialQueueItem, IErr>(op)
//                select s;
//            return await q;
//        }

//        public async Task<FSharpResult<CmdArg_物料批_调整优先级, IErr>> MapInputToCmdArgAsync(Input_物料批_调整优先级 input)
//        {

//            var q =
//                from candicate in this._repo.FindAsync(input.Id)
//                    .MapNullableToResult<BatchMaterialQueueItem, IErr>(new Err_物料批_Misc("未找到物料批", $"未找到id={input.Id}的物料批"))
//                let cmdArg = new CmdArg_物料批_调整优先级(candicate, input.Priority)
//                select cmdArg;
//            return await q;
//        }

//        public async Task<FSharpResult<BatchMaterialQueueItem, IErr>> 调整优先级Async(Input_物料批_调整优先级 input)
//        {
//            var q =
//                from cmdArg in this.MapInputToCmdArgAsync(input)
//                from op in this.调整优先级Async(cmdArg)
//                select op;
//            return await q;
//        }
//        #endregion
//    }
//}