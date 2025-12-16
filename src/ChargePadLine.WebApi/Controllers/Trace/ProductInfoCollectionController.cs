using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    [ApiController]
    [Route("api/[action]")]
    public class ProductInfoCollectionController : ControllerBase
    {
        public IProductTraceInfoCollectionService _collectionService { get; set; }
        public ProductInfoCollectionController(IProductTraceInfoCollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        /// <summary>
        /// 上报产品采集数据
        /// </summary>
        /// <param name="request">采集参数</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<IActionResult> DataCollectForSfcExAsync([FromBody] RequestParametricData request)
        {
            try
            {
                // 参数验证
                if (string.IsNullOrWhiteSpace(request.Sfc))
                {
                    return BadRequest(new { code = 400, message = "产品编码不能为空" });
                }
                if (string.IsNullOrWhiteSpace(request.Site​​))
                {
                    return BadRequest(new { code = 400, message = "站点不能为空" });
                }
                if (string.IsNullOrWhiteSpace(request.ActivityId))
                {
                    return BadRequest(new { code = 400, message = "活动ID不能为空" });
                }
                if (string.IsNullOrWhiteSpace(request.Resource))
                {
                    return BadRequest(new { code = 400, message = "资源不能为空" });
                }
                if (string.IsNullOrWhiteSpace(request.DcGroupRevision))
                {
                    return BadRequest(new { code = 400, message = "数据采集组版本号不能为空" });
                }
                // 调用服务层方法并获取结果
                var result = await _collectionService.DataCollectForSfcExAsync(request);
                // 处理结果并返回相应的HTTP响应
                if (result.IsOk)
                {
                    return Ok(new { code = 200, message = "产品信息收集成功" });
                }
                else
                {
                    var errorResult = result.ErrorValue;
                    return BadRequest(new { code = errorResult.Item1, message = errorResult.Item2 });
                }
            }
            catch (Exception ex)
            {
                // 记录异常并返回500错误
                return StatusCode(500, new { code = 500, message = $"服务器内部错误: {ex.Message}" });
            }
        }
    }
}

