using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems.Dto
{
    public class AllocateRoleMenusDto
    {
        public long RoleId { get; set; }
        public long[] MenuIds { get; set; }
    }
}
