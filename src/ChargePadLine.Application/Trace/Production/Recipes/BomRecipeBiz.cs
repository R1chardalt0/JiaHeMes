using ChargePadLine.Application.Trace.Production.TraceInformation;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Shared;
using Microsoft.FSharp.Core;

namespace ChargePadLine.Application.Trace.Production.Recipes
{
    public record Input_BomRecipe_AddMain(
        string ProductCode,
        string BomName,
        string? Description
    );

    public record Input_BomRecipe_AddItem(
        int BomRecipeId,
        string BomRecipeItemCode,
        string MaterialCode,
        decimal Quota,
        string? Description
    );

    public class BomRecipeBiz
    {
        private readonly IBomRecipeRepository _bomRepo;
        private readonly IMaterialRepository _matRepo;
        private readonly ICtrlVsnsService _vsnSvc;

        public BomRecipeBiz(IBomRecipeRepository bomRepo, IMaterialRepository matRepo, ICtrlVsnsService vsnSvc)
        {
            this._bomRepo = bomRepo;
            this._matRepo = matRepo;
            this._vsnSvc = vsnSvc;
        }

        #region 增加主表
        public Task<FSharpResult<CmdArg_BomRecipe_AddMain, IErr_BomRecipe>> MapInputToCmdArgAsync(Input_BomRecipe_AddMain input)
        {
           var vsnTask = this._vsnSvc.TryGetVsnAsync(input.ProductCode);
           var vsnResult = vsnTask.MapNullableToResult<CtrlVsn, IErr_BomRecipe>(() => new IErr_BomRecipe.产品编码错误(ProductCode: input.ProductCode));
           
           var query = from vsn in vsnResult
                       let bomCode = Guid.NewGuid().ToString()
                       select new CmdArg_BomRecipe_AddMain(bomCode, input.ProductCode, input.BomName, input.Description);
           return query;
        }

        public Task<FSharpResult<BomRecipe, IErr_BomRecipe>> AddBomRecipeAsync(CmdArg_BomRecipe_AddMain arg)
        {
           var query = from r in BomRecipeCommands.AddBomRecipe(arg).ToTask()
                       from s in this._bomRepo.AddAsync(r.BomRecipe, true)
                           .WithOkResult<BomRecipe, IErr_BomRecipe>(r.BomRecipe)
                       select s;
           return query;
        }

        public Task<FSharpResult<BomRecipe, IErr_BomRecipe>> AddBomRecipeAsync(Input_BomRecipe_AddMain input)
        {
           var query = from arg in this.MapInputToCmdArgAsync(input)
                       from cmdsucc in this.AddBomRecipeAsync(arg)
                       select cmdsucc;
           return query;
        }
        #endregion

        #region 增加子表
        public async Task<FSharpResult<CmdArg_BomRecipe_AddItem, IErr_BomRecipe>> MapInputToCmdArgAsync(Input_BomRecipe_AddItem input)
        {
            var bomRecipe = await this._bomRepo.FindAsync(input.BomRecipeId);
            if (bomRecipe == null)
            {
                return new IErr_BomRecipe.BomNotFound(input.BomRecipeId)
                    .ToErrResult<CmdArg_BomRecipe_AddItem, IErr_BomRecipe>();
            }

            var material = await this._matRepo.FindWithMaterialCodeAsync(input.MaterialCode);
            if (material == null)
            {
                return new IErr_BomRecipe.MaterialNotFound(input.MaterialCode)
                    .ToErrResult<CmdArg_BomRecipe_AddItem, IErr_BomRecipe>();
            }

            return new CmdArg_BomRecipe_AddItem(bomRecipe, input.BomRecipeItemCode, material, input.Quota, input.Description)
                .ToOkResult<CmdArg_BomRecipe_AddItem, IErr_BomRecipe>();
        }


        public Task<FSharpResult<BomRecipeItem, IErr_BomRecipe>> AddBomRecipeItemAsync(CmdArg_BomRecipe_AddItem arg)
        {
            var query = from r in BomRecipeCommands.AddBomRecipeItem(arg).ToTask()
                        from s in this._bomRepo.SaveChangesAsync()
                          .ToOkResult<BomRecipeItem, IErr_BomRecipe>(r)
                        select s;
            return query;
        }

        public Task<FSharpResult<BomRecipeItem, IErr_BomRecipe>> AddBomRecipeItemAsync(Input_BomRecipe_AddItem input)
        {
            var query = from cmd in this.MapInputToCmdArgAsync(input)
                        from succ in this.AddBomRecipeItemAsync(cmd)
                        select succ;
            return query;
        }
        #endregion
    }
}

