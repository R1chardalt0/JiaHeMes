using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.ProcessRouting
{
    /// <summary>
    /// 路线工序
    /// </summary>
    [Table("mes_routingOperation")]
    public class RoutingOperation
    {
        public Guid OperationId { get; set; }          // 工序唯一ID
        public string StationId { get; set; }            // 工站ID
        public string Resource { get; set; }
        public bool Required { get; set; } = true;       // 是否必经
        public bool AllowSkip { get; set; } = false;     // 是否允许跳过（仅对非Required有效或特殊场景）
        public List<string> Predecessors { get; set; } = new(); // 前置工序ID列表（空表示起始节点）
        public List<string> Successors { get; set; } = new();   // 后续工序ID列表
    }
}
