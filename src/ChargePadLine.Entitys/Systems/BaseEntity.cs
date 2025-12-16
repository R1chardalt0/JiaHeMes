using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Systems
{
    public class BaseEntity
    {
        /// <summary>
        /// 搜索值
        /// </summary>
        [JsonIgnore]
        [Description("搜索值")]
        public string? SearchValue { get; set; }
        /// <summary>
        /// 创建者
        /// </summary>
        [Description("创建者")]
        public string? CreateBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
        public DateTimeOffset? CreateTime { get; set; }
        /// <summary>
        /// 更新者
        /// </summary>
        [Description("更新者")]
        public string? UpdateBy { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss", ApplyFormatInEditMode = true)]
        public DateTimeOffset? UpdateTime { get; set; }
        /// <summary>
        /// 备注信息    
        /// </summary>
        [Description("备注信息")]   
        public string? Remark { get; set; }
        /// <summary>
        /// 请求参数
        /// </summary>
        [Description("请求参数")]
        [JsonExtensionData] // 如果用于 JSON 扩展数据，可加上此特性
        [NotMapped]
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
    }
}
