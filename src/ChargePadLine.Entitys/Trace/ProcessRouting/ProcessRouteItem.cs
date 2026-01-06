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
        /// 工路线ID
        /// </summary>
        public Guid HeadId { get; set; }

        /// <summary>
        /// 工站编号
        /// </summary>
        public string StationCode { get; set; } = "";

        /// <summary>
        /// 是否必须通过
        /// </summary>
        public bool MustPassStation { get; set; } = false;
        /// <summary>
        /// 检查站列表
        /// </summary>
        public string CheckStationList { get; set; }

        /// <summary>
        /// 状态 0-禁用 1-启用
        /// </summary>
        public int Status { get; set; } = 0;
    /// <summary>
    /// 是否首站
    /// </summary>
        public bool FirstStation { get; set; } = false;

    }
}
