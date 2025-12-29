using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Models
{
    /// <summary>
    /// 配方表
    /// </summary>
    public class Recipe
    {
        /// <summary>
        /// 配方ID
        /// </summary>
        [Key]
        public int RecipeId { get; set; }

        /// <summary>
        /// 配方名称
        /// </summary>
        [MaxLength(20)]
        public string RecipeName { get; set; }

        /// <summary>
        /// 配方描述
        /// </summary>
        [MaxLength(200)]
        public string Remarks { get; set; }

        /// <summary>
        /// 配方版本
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// 配方包含的配方项
        /// </summary>
        public ICollection<RecipeItem> Items { get; set; } = new List<RecipeItem>();
    }
}
