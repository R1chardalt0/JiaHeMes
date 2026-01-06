using ChargePadLine.Entitys.Trace.BOM;
using ChargePadLine.Service.Trace.Dto.BOM;

namespace ChargePadLine.Service.Trace
{
  /// <summary>
  /// BOM扩展方法
  /// </summary>
  public static class BomExtensions
  {
    /// <summary>
    /// 将BomItem实体转换为DTO
    /// </summary>
    /// <param name="bomItem">BomItem实体</param>
    /// <returns>BomItemDto</returns>
    public static BomItemDto ToDto(this BomItem bomItem) => new()
    {
      BomItemId = bomItem.BomItemId,
      BomId = bomItem.BomId,
      StationCode = bomItem.StationCode,
      BatchRule = bomItem.BatchRule,
      BatchQty = bomItem.BatchQty,
      BatchSNQty = bomItem.BatchSNQty,
      ProductId = bomItem.ProductId,
      CreateTime = bomItem.CreateTime,
      UpdateTime = bomItem.UpdateTime
    };

    /// <summary>
    /// 将BomList实体转换为DTO
    /// </summary>
    /// <param name="bomList">BomList实体</param>
    /// <returns>BomListDto</returns>
    public static BomListDto ToDto(this BomList bomList) => new()
    {
      BomId = bomList.BomId,
      BomName = bomList.BomName,
      BomCode = bomList.BomCode,
      Status = bomList.Status,
      CreateTime = bomList.CreateTime,
      UpdateTime = bomList.UpdateTime
    };
  }
}
