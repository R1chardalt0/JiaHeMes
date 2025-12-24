namespace ChargePadLine.Application.Trace.Production.WorkOrders.Errors
{
    public interface IErr_工单创建 : IErrWorkOrder 
    {
        public record struct RecipeNotFound(int InputRecipeId) : IErr_工单创建
        {
            public string Code => WorkOrderErrorDefines.NOT_FOUND;

            public string Message => $"配方未找到(id={InputRecipeId})";
        }
    }
}
