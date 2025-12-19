using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.TraceInfo
{
    /// <summary>
    /// 生产加工信息
    /// </summary>
    public class TraceProcItem
    {
        public Guid Id { get; set; }
        public uint Vsn { get; set; }

        #region 外键
        //public int TraceInfoId { get; set; }
        public Guid TraceInfoId { get; set; }
        public TraceInfo? TraceInfo { get; set; }
        #endregion

        /// <summary>
        /// 工位位置
        /// </summary>
        public string Station { get; set; } = null!;
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; } = null!;
        /// <summary>
        /// 值
        /// </summary>
        public JToken? Value { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }

}
