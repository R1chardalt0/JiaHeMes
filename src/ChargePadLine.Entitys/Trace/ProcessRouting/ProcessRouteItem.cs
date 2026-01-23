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
  [Table("mes_processRoute_item")]
  public class ProcessRouteItem : BaseEntity
  {
    /// <summary>
    /// 工艺路线子表ID
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 主表ID
    /// </summary>
    public Guid HeadId { get; set; }

    /// <summary>
    /// 站点编码
    /// </summary>
    public string StationCode { get; set; } = "";

    /// <summary>
    /// 是否必过站
    /// </summary>
    public bool MustPassStation { get; set; } = false;
    /// <summary>
    /// 验证站点PASS记录，可多选，用逗号隔开
    /// </summary>
    public string CheckStationList { get; set; } = "";

    /// <summary>
    /// 是否首站
    /// </summary>
    public bool FirstStation { get; set; } = false;

    /// <summary>
    /// 是否校验Check StationList中的全部
    /// </summary>
    public bool CheckAll { get; set; } = false;

    /// <summary>
    /// 工艺路线序号
    /// </summary>
    public int? RouteSeq { get; set; } = 0;

    /// <summary>
    /// 最大NG次数
    /// </summary>
    public int? MaxNGCount { get; set; } = 0;

  }
}
