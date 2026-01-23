using System;

namespace ChargePadLine.Service.Trace.Dto.BOM
{
  /// <summary>
  /// BOM明细更新数据传输对象
  /// </summary>
  public class BomItemUpdateDto
  {
    /// <summary>
    /// BOM明细ID
    /// </summary>
    public Guid BomItemId { get; set; }

    /// <summary>
    /// 主表ID
    /// </summary>
    public Guid? BomId { get; set; }

    /// <summary>
    /// 站点编码
    /// </summary>
    public string StationCode { get; set; }

    /// <summary>
    /// 批次规则，正则表达
    /// </summary>
    public string BatchRule { get; set; }

    /// <summary>
    /// 批次数据固定
    /// </summary>
    public bool? BatchQty { get; set; }

    /// <summary>
    /// 获取批次数量，固定位数
    /// </summary>
    public string BatchSNQty { get; set; }

    /// <summary>
    /// 物料ID
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// 日期序列
    /// </summary>
    public int? DateIndex { get; set; }

    /// <summary>
    /// 数量序列
    /// </summary>
    public int? NumberIndex { get; set; }

    /// <summary>
    /// 物料保质期（分钟）
    /// </summary>
    public int? ShelfLife { get; set; }

    /// <summary>
    /// 物料号序列
    /// </summary>
    public int? ProductIndex { get; set; }
  }
}
