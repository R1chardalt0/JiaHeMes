using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Common.Config
{
    public class MigrationConfig
    {
        public int DataRetentionDays { get; set; } = 30;
        public bool IsEnabled { get; set; } = false;
    }
}
