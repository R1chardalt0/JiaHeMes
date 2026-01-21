using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Product
{
  /// <summary>
  /// 产品列表
  /// </summary>
  [Table("mes_product_list")]
  public class ProductList : BaseEntity
  {
    /// <summary>
    /// 产品ID
    /// </summary>
    [Key]
    [Column("ProductListId")]
    public Guid ProductListId { get; set; }

    /// <summary>
    /// 产品编码
    /// </summary>
    [Column("ProductCode")]
    public string ProductCode { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    [Column("ProductName")]
    public string ProductName { get; set; }

    /// <summary>
    /// BOMID
    /// </summary>
    [Column("BomId")]
    public Guid? BomId { get; set; }

    /// <summary>
    /// 工艺路线ID
    /// </summary>
    [Column("ProcessRouteId")]
    public Guid? ProcessRouteId { get; set; }

    /// <summary>
    /// 产品类别
    /// </summary>
    [Column("ProductType")]
    [MaxLength(255)]
    public string ProductType { get; set; }


    /// <summary>
    /// 最大返工次数
    /// </summary>
    [Column("MaxReworkCount")]
    public int ? MaxReworkCount { get; set;}

  }
}