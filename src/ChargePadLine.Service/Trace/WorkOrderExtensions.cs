using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// WorkOrder扩展方法
  /// </summary>
  public static class WorkOrderExtensions
  {
    /// <summary>
    /// 将WorkOrder实体转换为DTO
    /// </summary>
    /// <param name="workOrder">WorkOrder实体</param>
    /// <returns>WorkOrderDto</returns>
    public static WorkOrderDto ToDto(this WorkOrder workOrder) => new()
    {
      Id = workOrder.Id,
      Code = workOrder.Code.Value,
      ProductCode = workOrder.ProductCode.Value,
      BomRecipeId = workOrder.BomRecipeId,
      BomRecipeName = workOrder.BomRecipe?.BomName ?? "",
      IsInfinite = workOrder.IsInfinite,
      WorkOrderAmount = workOrder.WorkOrderAmount,
      PerTraceInfo = workOrder.PerTraceInfo,
      DocStatus = workOrder.DocStatus,
      //CreateTime = workOrder.CreatedAt
    };
  }
}