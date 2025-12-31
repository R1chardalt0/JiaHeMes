using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Models
{
    [Table("dm_plcDevice")]
    public class PlcDevice
    {
        /// <summary>
        /// plc设备ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        ///  PLC名称
        /// </summary>
        [MaxLength(20)]
        public string PLCName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 通信协议
        ///  Modbus TCP、S7
        /// </summary>
        [MaxLength(20)]
        public string Protocolc { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [MaxLength(20)]
        public string Model { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        [MaxLength(200)]
        public string Remarks { get; set; }
    }
}
