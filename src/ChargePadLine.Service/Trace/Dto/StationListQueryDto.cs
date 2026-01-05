using ChargePadLine.Entitys.Trace.WorkOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Dto
{
  /// <summary>
  /// 工单数据传输对象
  /// </summary>
  public class StationListQueryDto
    {
        public Guid StationId { get; set; }

        /// <summary>
        /// 工位名称
        /// </summary>
        public string StationName { get; set; } = "";
        /// <summary>
        /// 工位编号
        /// </summary>

        public string StationCode { get; set; } = "";

        /// <summary>
        /// 创建时间起始（包含）
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 创建时间结束（包含）
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 当前页码（最小值为1）
        /// </summary>
        public int Current { get; set; } = 1;

        /// <summary>
        /// 每页记录数（最小值为1）
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}