using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Entitys.Trace.TraceInformation
{
    [Table("mes_ctrl_vsn")]
    public class CtrlVsn
    {
        public int Id { get; set; }

        public string ProductCode { get; set; } = "";

        public uint Current { get; set; }

        public string Note { get; set; } = "";

    }
}
