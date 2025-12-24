using ChargePadLine.Application;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Shared;

namespace ChargePadLine.Application.Trace.Production.Recipes
{
    public static class ERROR_CODE_DEFINES_BOMRECIPE
    {
        private static string PREFIX = "BomRecipe_";
        public static string BOM不存在 = $"{PREFIX}BOM不存在";
        public static string 物料不存在 = $"{PREFIX}物料不存在";
        public static string 子项已存在 = $"{PREFIX}子项已存在";
        public static string 产品编码错误= $"{PREFIX}产品编码错误";
    }

    public interface IErr_BomRecipe : IErr 
    { 
        public record BomNotFound(int BomId) : IErr_BomRecipe
        {
            public string Code => ERROR_CODE_DEFINES_BOMRECIPE.BOM不存在;

            public string Message => $"BOM不存在(id={BomId})";
        }

        public record MaterialNotFound(MaterialCode MaterialCode) : IErr_BomRecipe
        { 
            public string Code => ERROR_CODE_DEFINES_BOMRECIPE.物料不存在;

            public string Message => $"物料不存在(物料编码={MaterialCode.Value})";
        }

        public record 产品编码错误(ProductCode ProductCode) : IErr_BomRecipe
        {
            public string Code => ERROR_CODE_DEFINES_BOMRECIPE.子项已存在;

            public string Message => $"产品编码({ProductCode.Value})对应的VSN不存在";
        }

        public record 子项已存在(int BomId,BomItemCode BomItemCode) : IErr_BomRecipe
        {
            public string Code => ERROR_CODE_DEFINES_BOMRECIPE.子项已存在;

            public string Message => $"指定子项已经存在，不可重复添加(子项编码={BomItemCode}；BomId={BomId})";
        }
    }

}

