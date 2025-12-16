using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
    public class   RequestUpadteParams
    {
        public string deviceEnCode { get; set; }
        public DateTimeOffset sendTime { get; set; }
        public string alarmMessages { get; set; }
        public List<Iotdata> updateParams { get; set; }
    }

    public class RequestBGUpadteParams
    {
        public string deviceEnCode { get; set; }
        public DateTimeOffset sendTime { get; set; }
        public string alarmMessages { get; set; }
        public List<BGData>? updateParams { get; set; }
    }

    public class BGData
    {
        public string? Tag { get; set; }
        public double? Value { get; set; }
    }


    public class RequestParametricData
    {
      

        /// <summary>
        /// 站点(工厂或生产站点的标识)
        /// </summary>
        public string Site​​ { get; set; }

        /// <summary>
        /// 活动ID（唯一标识一个具体的生产活动、工单或任务步骤 ）       
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// 资源(执行操作所涉及的具体资源，如设备编号、产线编号、工位编号)
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// 数据采集组版本号
        /// </summary>
        public string DcGroupRevision { get; set; }

        /// <summary>
        /// 产品编码
        /// </summary>
        public string Sfc { get; set; }

        /// <summary>
        /// 是否OK
        /// </summary>
        public bool IsOK { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTimeOffset SendTime { get; set; }

        /// <summary>
        /// 产品参数组
        /// </summary>
        public List<Iotdata>? parametricDataArray { get; set; } = new List<Iotdata>();
    }
}
