using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JY_Print.model
{
    public class RespDto
    {
        public bool Success { get; set; }
        public string Messages { get; set; }
        public BoxCodeCache Data { get; set; }
    }

    /// <summary>
    /// 缓存箱体码
    /// </summary>
    public class BoxCodeCache
    {
        /// <summary>
        /// 缓存ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 缓存箱体码
        /// </summary>
        public string CacheBoxCode { get; set; }
        /// <summary>
        /// 客户零件图号
        /// </summary>
        public string CustomerPartNumber { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 生产日期
        /// </summary>
        public string DatePrefix { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 班别
        /// </summary>
        public string Shift { get; set; }
        /// <summary>
        /// 发往地
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }
    }
}
