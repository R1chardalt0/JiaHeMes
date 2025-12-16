using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
    public class ProductionRecordsDto
    {
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductionLineName { get; set; }
        /// <summary>
        /// 资源号
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 总产量
        /// </summary>
        public long TotalProduction { get; set; }
        /// <summary>
        /// OK总数
        /// </summary>
        public long OKNum { get; set; }
        /// <summary>
        /// NG总数
        /// </summary>
        public long NGNum { get; set; }
        /// <summary>
        /// 良率
        /// </summary>
        public double Yield { get; set; }
    }

    /// <summary>
    /// 按小时统计的产量数据DTO
    /// </summary>
    public class HourlyProductionRecordsDto
    {
        /// <summary>
        /// 产线名称
        /// </summary>
        public string ProductionLineName { get; set; }
        /// <summary>
        /// 资源号
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 统计小时（格式：yyyy-MM-dd HH:00）
        /// </summary>
        public string Hour { get; set; }
        /// <summary>
        /// 该小时内的总产量
        /// </summary>
        public long TotalProduction { get; set; }
        /// <summary>
        /// 该小时内的OK总数
        /// </summary>
        public long OKNum { get; set; }
        /// <summary>
        /// 该小时内的NG总数
        /// </summary>
        public long NGNum { get; set; }
        /// <summary>
        /// 该小时内的良率
        /// </summary>
        public double Yield { get; set; }
    }
}
