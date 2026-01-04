using DeviceManage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService
{
    public interface ISwitchRecipeService
    {
        Task<bool> SwitchRecipeTagAsync(Tag tag);
    }
}
