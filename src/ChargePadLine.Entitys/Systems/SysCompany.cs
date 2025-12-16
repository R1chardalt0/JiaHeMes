using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePadLine.Entitys.Systems
{
    [Table("SysCompanys")]
    public class SysCompany : BaseEntity
    {
        /// <summary>
        /// 公司主键
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Description("公司主键")]
        public int CompanyId { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        [Description("公司名称")]
        [Required(ErrorMessage = "公司名称不能为空")]
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        /// <summary>
        /// 公司编码
        /// </summary>
        [Description("公司编码")]
        public Guid CompanyCode { get; set; }
    }
}

