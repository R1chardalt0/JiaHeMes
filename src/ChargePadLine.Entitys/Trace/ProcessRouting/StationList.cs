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
    [Table("mes_station_list")]
    public class StationList : BaseEntity
    {
        
        [Key]
        public Guid StationId { get; set; }

        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationName { get; set; } = "";
        /// <summary>
        /// 工位编号
        /// </summary>
       
        public string StationCode { get; set; } = "";


    }
}
