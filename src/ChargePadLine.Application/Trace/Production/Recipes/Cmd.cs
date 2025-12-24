using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Shared;
using Microsoft.FSharp.Core;
using System;
using System.Reflection.Emit;

namespace ChargePadLine.Application.Trace.Production.Recipes
{
    public record CmdArg_BomRecipe_AddMain(
        BomCode BomCode,
        ProductCode ProductCode,
        string BomName,
        string? Description
    );
    public record CmdArg_BomRecipe_AddItem(
        BomRecipe Recipe,
        BomItemCode BomItemCode,
        Material Material,
        decimal Quota,
        string? Description
        );

    public record CmdSucc_BomRecipe(BomRecipe BomRecipe);

    public static class BomRecipeCommands
    {
        public static FSharpResult<CmdSucc_BomRecipe, IErr_BomRecipe> AddBomRecipe(CmdArg_BomRecipe_AddMain arg)
        {
            var recipe = BomRecipe.MakeNew(
                bomCode: arg.BomCode.Value, 
                bomName: arg.BomName, 
                prodCode: arg.ProductCode, 
                description: arg.Description ?? ""
                );
            var succ = new CmdSucc_BomRecipe(recipe);
            return succ.ToOkResult<CmdSucc_BomRecipe, IErr_BomRecipe>();
        }


        public static FSharpResult<BomRecipeItem, IErr_BomRecipe> AddBomRecipeItem(CmdArg_BomRecipe_AddItem arg)
        {
            var recipe = arg.Recipe;
            var itemCode = arg.BomItemCode.Value;

            var exists = recipe.Items.Any(i => i.BomItemCode.Value == itemCode && !i.IsDeleted);
            if (exists)
            {
                return new IErr_BomRecipe.子项已存在(recipe.Id, arg.BomItemCode)
                    .ToErrResult<BomRecipeItem, IErr_BomRecipe>();
            }

            var item = recipe.AddItem(itemCode, arg.Material, arg.Quota, arg.Description ?? "");
            return item.ToOkResult<BomRecipeItem, IErr_BomRecipe>();
        }


    }
}

