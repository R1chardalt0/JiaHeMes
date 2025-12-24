using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.ProcessRouting
{
    /// <summary>
    /// 工艺路线表
    /// </summary>
    [Table("mes_processRoute")]
    public class ProcessRoute : BaseEntity
    {
        /// <summary>
        /// 工艺路线ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 工路线名称
        /// </summary>
        public string RouteName { get; set; } = "";

        /// <summary>
        /// 工艺版本
        /// </summary>
        public string ProcessVersion { get; set; } = "";

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; } = "";

        /// <summary>
        /// 状态 0-禁用 1-启用
        /// </summary>
        public int Status { get; set; } = 0;
        public Dictionary<Guid, RoutingOperation> Nodes { get; set; } = new(); // Key: OperationId
        public List<Guid> StartNodes => Nodes.Values.Where(n => !n.Predecessors.Any()).Select(n => n.OperationId).ToList();

    }
}
