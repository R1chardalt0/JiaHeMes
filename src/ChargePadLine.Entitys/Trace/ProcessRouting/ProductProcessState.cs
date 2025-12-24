using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.ProcessRouting
{
    /// <summary>
    /// 工艺流程中，某个产品的处理状态
    /// </summary>
    [Table("mes_productProcessState")]
    public class ProductProcessState
    {
        public string ProductId { get; set; }
        public HashSet<Guid> CompletedOperations { get; set; } = new(); // 已完成的 OperationId
        public HashSet<Guid> SkippedOperations { get; set; } = new();   // 明确跳过的（可选记录）
    }
}
