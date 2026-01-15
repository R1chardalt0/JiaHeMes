using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService.Dto
{
    /// <summary>
    /// 用户搜索DTO
    /// </summary>
    public class UserSearchDto
    {
        public int current { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public string Username { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool? IsEnabled { get; set; }
    }
}

