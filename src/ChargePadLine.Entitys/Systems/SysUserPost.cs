using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Systems
{
    /// <summary>
    /// 用户岗位关联表
    /// </summary>
    [Table("sys_userPost")]
    public class SysUserPost
    {
        [Key]
        public long Id { get; set; }  // ✅ 自动识别为主键
        /// <summary>
        /// 用户ID
        /// </summary>
        [Description("用户ID")]
        public long UserId { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        [Description("岗位ID")]
        public long PostId { get; set; }
    }
}
