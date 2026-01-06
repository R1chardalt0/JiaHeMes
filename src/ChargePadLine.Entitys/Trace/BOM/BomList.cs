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

namespace ChargePadLine.Entitys.Trace.BOM
{
    /// <summary>
    /// 工单
    /// </summary>|
    [Table("mes_bom")]
    public class BomList
    {
        /// <summary>
        /// BOM ID
        /// </summary>
        [Key]
        [Column("BomId")]
        public Guid BomId { get; set; }

        /// <summary>
        /// BOM名称
        /// </summary>
        [Required]
        [Column("BomName")]
        public string BomName { get; set; } = "";

        /// <summary>
        /// BOM编码
        /// </summary>
        [Required]
        [Column("BomCode")]
        public string BomCode { get; set; } = "";

        /// <summary>
        /// 状态
        /// </summary>
        [Required]
        [Column("Status")]
        public int Status { get; set; }

        /// <summary>
        /// BOM明细项集合（一对多关联）
        /// </summary>
        [ForeignKey("BomId")]
        public virtual ICollection<BomItem> BomItems { get; set; } = new List<BomItem>();
    }

}
