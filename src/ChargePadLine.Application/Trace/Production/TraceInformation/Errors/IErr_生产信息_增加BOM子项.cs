using ChargePadLine.Entitys.Trace.Recipes.Entities;

namespace ChargePadLine.Application.Trace.Production.TraceInformation.Errors
{
    public interface IErr_追溯信息_增加BOM子项 : IErr_TraceInfo
    {
        public record struct TraceInfoNotFound(string Condition) : IErr_追溯信息_增加BOM子项 
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到追溯信息({Condition})";
        }

        public record struct BomItemNotFound(BomItemCode BomItemCode) : IErr_追溯信息_增加BOM子项
        {
            public string Code => TraceInfo_Error_Defines.NOT_FOUND;
            public string Message => $"找不到BOM子项信息：子项编码={BomItemCode.Value}";
        }

        public record struct BomItemExceedsQuota(decimal Quota, decimal Accumulation, decimal Addition) : IErr_追溯信息_增加BOM子项
        {
            public string Code => TraceInfo_Error_Defines.BOM子项超额;

            public string Message => $"BOM子项超出配额:设定配额={Quota}; 已累计={Accumulation}; 当前增量={Addition}";
            
        }
        public record struct MiscError(string Message) : IErr_追溯信息_增加BOM子项
        {
            public string Code => TraceInfo_Error_Defines.MISC;
        }
    }
}
