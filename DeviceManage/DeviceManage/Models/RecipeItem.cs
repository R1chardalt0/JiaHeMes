using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Models
{
    public class RecipeItem
    {
        /// <summary>
        /// 配方项ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 配方ID
        /// </summary>
        [Required]
        public int RecipeId { get; set; }

        /// <summary>
        /// 点位ID
        /// </summary>
        [Required]
        public int TagId { get; set; }

        [Required]
        public string TargetValue { get; set; } = string.Empty; // 存储为字符串，运行时按 DataType 解析
        // 导航属性
        [ForeignKey(nameof(RecipeId))]
        public Recipe Recipe { get; set; } = null!;

        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; } = null!;
    }
}
