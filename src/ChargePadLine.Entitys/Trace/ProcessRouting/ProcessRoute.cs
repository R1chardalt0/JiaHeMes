using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.ProcessRouting
{
  /// <summary>
  /// 工艺路线表
  /// </summary>
  [Table("mes_processRoute")]
  public class ProcessRoute : BaseEntity
  {
    /// <summary>
    /// 工艺路线ID
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 工艺路线名称
    /// </summary>
    public string RouteName { get; set; } = "";

    /// <summary>
    /// 工艺路线编码
    /// </summary>
    public string RouteCode { get; set; } = "";


    /// <summary>
    /// 状态 0-启用 1-关闭
    /// </summary>
    public int Status { get; set; } = 0;


  }
}
