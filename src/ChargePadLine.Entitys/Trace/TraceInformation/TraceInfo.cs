using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Entitys.Trace.BOM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.TraceInformation
{
  /// <summary>
  /// 追溯信息
  /// </summary>
  [Table("mes_traceinfo")]
  public class TraceInfo
  {
    // public int Id { get; set; }
    public Guid Id { get; set; }

    /// <summary>
    /// 产线
    /// </summary>
    public string ProductLine { get; set; } = "";

    public uint Vsn { get; set; }

    #region 工单信息
    public ProductCode ProductCode { get; set; } = "";
    public WorkOrder? WorkOrder { get; set; }
    public int WorkOrderId { get; set; }
    #endregion

    #region BOM信息
    public Guid BomRecipeId { get; set; }
    public BomList? BomList { get; set; }
    #endregion

    /// <summary>
    /// 产品识别码
    /// </summary>
    public SKU PIN { get; set; } = "";

    public IList<TraceBomItem> BomItems { get; set; } = new List<TraceBomItem>();

    public IList<TraceProcItem> ProcItems { get; set; } = new List<TraceProcItem>();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    #region NG
    /// <summary>
    /// 是否NG？
    /// </summary>
    public bool IsNG { get; set; }

    /// <summary>
    /// NG原因
    /// </summary>
    public string NGReason { get; set; } = "";

    /// <summary>
    /// NG时间
    /// </summary>
    public DateTimeOffset NgedAt { get; set; }
    #endregion

    #region 破坏？
    /// <summary>
    /// 是否被破坏？
    /// </summary>
    public bool Destroyed { get; set; }
    /// <summary>
    /// 破坏时间
    /// </summary>
    public DateTimeOffset DestroyedAt { get; set; }
    #endregion

    public string? Note { get; set; }
  }


  public static class TraceInfoExtensions_GetProcItem
  {
    // public static BomItem? GetBomRecipeItem(this TraceInfo pi, string bomItemCode)
    // {
    //   var r = pi.BomList?
    //       .Items?
    //       .FirstOrDefault(i => i.BomItemCode.Value == bomItemCode);
    //   return r;
    // }

    public static TraceBomItem? GetBomItem(this TraceInfo pi, BomItemCode bomItemCode)
    {
      var item = pi.BomItems.FirstOrDefault(i => i.BomItemCode == bomItemCode.Value);
      return item;
    }

    #region

    /// <summary>
    /// see https://stackoverflow.com/q/75830460/10091607
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="pi"></param>
    /// <param name="station"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static TValue? GetProcItemStruct<TValue>(this TraceInfo pi, string station, string key)
        where TValue : struct
    {
      var item = pi.ProcItems.FirstOrDefault(i => i.Station == station && i.Key == key && !i.IsDeleted);
      var jtoken = item?.Value;
      if (jtoken == null)
      {
        return null;
      }
      TValue? x = jtoken.ToObject<TValue>();
      return x;
    }


    public static TValue? GetProcItem<TValue>(this TraceInfo pi, string station, string key)
        where TValue : class
    {
      var item = pi.ProcItems.FirstOrDefault(i => i.Station == station && i.Key == key && !i.IsDeleted);
      var jtoken = item?.Value;
      if (jtoken == null)
      {
        return null;
      }
      TValue? x = jtoken.ToObject<TValue>();
      return x;
    }
    #endregion
  }


  public class TraceInfoEntityTypeConfiguration : IEntityTypeConfiguration<TraceInfo>
  {
    public void Configure(EntityTypeBuilder<TraceInfo> builder)
    {
      builder.HasIndex(e => e.CreatedAt);
      builder.HasIndex(e => new { e.Vsn, e.ProductLine }).IsUnique();
      builder.OwnsOne(e => e.PIN, nb =>
      {
        nb.HasIndex(x => x.Value);
      });
    }
  }
}
