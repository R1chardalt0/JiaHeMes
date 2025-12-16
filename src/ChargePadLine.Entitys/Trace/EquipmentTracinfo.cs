using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace
{
    public class EquipmentTracinfo
    {
        /// <summary>
        /// 设备追踪信息ID
        /// </summary>
        [Key]
        public Guid EquipmentTraceId { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        public string DeviceEnCode { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTimeOffset SendTime { get; set; }
        /// <summary>
        /// 设备报警
        /// </summary>
        public string? AlarmMessages { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.Now;
        /// <summary>
        /// 设备参数组
        /// </summary>
        public List<Iotdata>? Parameters { get; set; } = new List<Iotdata>();
    }
}
