using Microsoft.AspNetCore.Mvc;
using Microsoft.FSharp.Core;


namespace ChargePadLine.WebApi.Controllers.Systems
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BaseController : ControllerBase
    {       
        /// <summary>
        /// 从 JWT 令牌中获取当前用户 ID
        /// </summary>
        /// <returns>当前用户 ID，未登录时返回 0</returns>
        protected long GetUserId()
        {
            // 兼容大小写不同的Claim键：Id / id
            var claimVal = User.FindFirst("id")?.Value ?? User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(claimVal)) return 0;
            return long.TryParse(claimVal, out var id) ? id : 0;
        }
    }
}
