using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.Production
{
    /// <summary>
    /// 库存码
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record SKU(string Value)
    {
        private SKU() : this("") { }

        public static implicit operator SKU(string code)
        {
            // todo: 校验规则
            return new SKU(code);
        }
    }

    /// <summary>
    /// 工单码
    /// </summary>
    /// <param name="Value"></param>
    [Owned]
    public record WorkOrderCode(string Value)
    {
        private WorkOrderCode() : this("") { }

        public static implicit operator WorkOrderCode(string code)
        {
            return new WorkOrderCode(code);
        }
    }
}
