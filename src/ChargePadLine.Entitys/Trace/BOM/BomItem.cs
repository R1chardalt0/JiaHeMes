using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChargePadLine.Entitys.Systems;

namespace ChargePadLine.Entitys.Trace.BOM
{
  /// <summary>
  /// 工单
  /// </summary>|
  [Table("mes_bom_item")]
  public class BomItem : BaseEntity
  {
    /// <summary>
    /// BOM明细ID
    /// </summary>
    [Key]
    [Column("BomItemId")]
    public Guid BomItemId { get; set; }

    /// <summary>
    /// 主表ID
    /// </summary>
    [Column("BomId")]
    public Guid? BomId { get; set; }

    /// <summary>
    /// 站点编码
    /// </summary>
    [Column("StationCode")]
    public string StationCode { get; set; }

    /// <summary>
    /// 批次规则，正则表达
    /// </summary>
    [Column("BatchRule")]
    public string BatchRule { get; set; }

    /// <summary>
    /// 批次数据固定
    /// </summary>
    [Column("BatchQty")]
    public bool? BatchQty { get; set; }

    /// <summary>
    /// 获取批次数量，固定位数
    /// </summary>
    [Column("BatchSNQty")]
    public string BatchSNQty { get; set; }

    /// <summary>
    /// 物料ID
    /// </summary>
    [Column("ProductId")]
    public Guid? ProductId { get; set; }

    /// <summary>
    /// 日期序列
    /// </summary>
    [Column("DateIndex")]
    public int? DateIndex { get; set; }

    /// <summary>
    /// 数量序列
    /// </summary>
    [Column("NumberIndex")]
    public int? NumberIndex { get; set; }

    /// <summary>
    /// 物料保质期（分钟）
    /// </summary>
    [Column("ShelfLife")]
    public int? ShelfLife { get; set; }

    /// <summary>
    /// 物料号序列
    /// </summary>
    [Column("ProductIndex")]
    public int? ProductIndex { get; set; }

    /// <summary>
    /// BOM主表（多对一关联）
    /// </summary>
    [ForeignKey("BomId")]
    public virtual BomList Bom { get; set; }
  }

}
