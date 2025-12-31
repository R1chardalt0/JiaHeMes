using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService.Dto
{
    public class TagSearchDto
    {
        public int current { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public string RecipeName { get; set; } = string.Empty;
        public string PLCName { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
    }
}
