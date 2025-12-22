using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace
{
    /// <summary>
    /// 设备信息
    /// </summary>
    [Table("mes_deviceInfo")]
    public class DeviceInfo : BaseEntity
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        [Key]
        public Guid DeviceId { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 设备图片
        /// </summary>
        public string DevicePicture { get; set; }
        /// <summary>
        /// 生产线ID
        /// </summary>
        public Guid ProductionLineId { get; set; }
        /// <summary>
        /// 生产线导航属性
        /// </summary>
        public virtual ProductionLine? ProductionLine { get; set; }
        /// <summary>
        /// 生产线名称（从生产线导航属性获取）
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ProductionLineName 
        { 
            get => ProductionLine?.ProductionLineName ?? _productionLineName;
            set => _productionLineName = value;
        }
        private string? _productionLineName;
        /// <summary>
        /// 设备名称
        /// </summary>
        public string? DeviceName { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DeviceType { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        public string DeviceEnCode { get; set; }
        /// <summary>
        /// 设备制造商
        /// </summary>
        /// 
        public string DeviceManufacturer { get; set; }
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 心跳同步时间
        /// </summary>
        public int ExpireTime { get; set; } // 心跳过期时间（秒）

    }
}
