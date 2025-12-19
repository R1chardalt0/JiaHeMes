using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.WorkOrders
{
    /// <summary>
    /// 工单物资
    /// </summary>
    public class WorkOrderGoods
    {
        public int Id { get; set; }

        #region 所属工单
        public WorkOrder? WorkOrder { get; set; }
        public int WorkOrderId { get; set; }
        #endregion

        public MaterialCode MaterialCode { get; set; } = "";
        public SKU SKU { get; set; } = "";
        public decimal Amount { get; set; }

        public string Note = "";
    }
}
