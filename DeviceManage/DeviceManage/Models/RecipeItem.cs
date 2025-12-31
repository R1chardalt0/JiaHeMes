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
    [Table("dm_recipeItem")]
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

        /// <summary>
        /// 点位标识
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TagKey { get; set; } = string.Empty;

        /// <summary>
        /// 目标值（以字符串存储，保存/下发时按点位 DataType 解析校验）
        /// </summary>
        [Required]
        public string TargetValue { get; set; } = string.Empty;

        // 导航属性
        [ForeignKey(nameof(RecipeId))]
        public Recipe Recipe { get; set; } = null!;

        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; } = null!;
    }
}
