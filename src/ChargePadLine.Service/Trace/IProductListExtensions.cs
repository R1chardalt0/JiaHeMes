using ChargePadLine.Entitys.Trace.Product;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// ProductList扩展方法
  /// </summary>
  public static class ProductListExtensions
  {
    /// <summary>
    /// 转换为Dto
    /// </summary>
    /// <param name="productList"></param>
    /// <returns></returns>
    public static ProductListDto ToDto(this ProductList productList) => new()
    {
      ProductListId = productList.ProductListId,
      ProductCode = productList.ProductCode,
      ProductName = productList.ProductName,
      BomId = productList.BomId,
      ProcessRouteId = productList.ProcessRouteId,
      ProductType = productList.ProductType,
      Remark = productList.Remark,
      CreateBy = productList.CreateBy,
      CreateTime = productList.CreateTime,
      UpdateBy = productList.UpdateBy,
      UpdateTime = productList.UpdateTime
    };
  }
}
