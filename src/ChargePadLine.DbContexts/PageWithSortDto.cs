using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.DbContexts
{
    public class PageWithSortDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public string Sort { get; set; } = "Id";
        public OrderType OrderType { get; set; } = OrderType.Asc;
    }
    public enum OrderType
    {
        Asc,
        Desc,
    }
}
