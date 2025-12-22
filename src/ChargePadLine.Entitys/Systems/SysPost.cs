using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChargePadLine.Entitys.Systems
{
    /// <summary>
    /// 岗位表
    /// </summary>
    [Table("sys_post")]
    public class SysPost : BaseEntity
    {
        /// <summary>
        /// 岗位ID 
        /// </summary>
        [Description("岗位ID")]
        [Key]
        public long PostId { get; set; }
        /// <summary>
        /// 岗位编码 
        /// </summary>
        [Description("岗位编码")]
        public string? PostCode { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        [Description("岗位名称")]
        public string? PostName { get; set; }
        /// <summary>
        /// 显示顺序 
        /// </summary>
        [Description("显示顺序")]
        public string? PostSort { get; set; }
        /// <summary>
        /// 岗位状态（0正常 1停用）
        /// </summary>
        [Description("岗位状态（0正常 1停用）")]
        public string? Status { get; set; }
        /// <summary>
        /// 用户是否存在此岗位标识 默认不存在
        /// </summary>
        public bool Flag { get; set; } = false;
    }
}
