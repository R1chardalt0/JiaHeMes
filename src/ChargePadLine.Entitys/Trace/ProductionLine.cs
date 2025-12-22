using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace
{
    /// <summary>
    /// 产线信息
    /// </summary>
    [Table("mes_productionLine")]
    public class ProductionLine
    {
        [Key]
        public Guid ProductionLineId { get; set; } // 生产线ID
        public string ProductionLineCode { get; set; } //产线编号
        public string Status { get; set; }
        public string ProductionLineName { get; set; } // 生产线名称
        public string Description { get; set; } // 描述
        public int? CompanyId { get; set; } // 公司ID（外键关联SysCompany表）
        public DateTime CreatedAt { get; set; } // 创建时间
        public DateTime UpdatedAt { get; set; } // 更新时间
    }
}
