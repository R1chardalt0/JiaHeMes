using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    [ApiController]
    [Route("api/[action]")]
    public class DeviceInfoCollectionController : ControllerBase
    {
        public IDeviceInfoCollectionService _collectionService { get; set; }
        public DeviceInfoCollectionController(IDeviceInfoCollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        /// <summary>
        /// 上报设备运行数据
        /// </summary>
        /// <param name="request">设备数据采集参数</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<IActionResult> DeviceDataCollectionExAsync(RequestUpadteParams request)
        {
            try
            {
                // 参数验证
                if (string.IsNullOrWhiteSpace(request.deviceEnCode))
                {
                    return BadRequest(new { code = 400, message = "设备编码不能为空" });
                }

                // 调用服务层方法并获取结果
                var result = await _collectionService.DeviceDataCollectionExAsync(
                    request.deviceEnCode,
                    request.sendTime,
                    request.alarmMessages,
                    request.updateParams
                );

                // 处理结果并返回相应的HTTP响应
                if (result.IsOk)
                {
                    return Ok(new { code = 200, message = "设备信息收集成功" });
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

        /// <summary>
        /// 通过网关上传设备运行数据
        /// </summary>
        /// <param name="request">设备数据采集参数</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<IActionResult> DeviceDataCollectionByBGExAsync(RequestBGUpadteParams request)
        {
            try
            {
                // 参数验证
                if (string.IsNullOrWhiteSpace(request.deviceEnCode))
                {
                    return BadRequest(new { code = 400, message = "设备编码不能为空" });
                }

                // 转换参数
                var updateParams = request.updateParams?.Select(x => new Iotdata
                {
                    Name = x.Tag ?? string.Empty,
                    Type = 0,
                    Value = x.Value.ToString() ?? string.Empty,
                    Unit = ""
                }).ToList();

                // 调用服务层方法并获取结果
                var result = await _collectionService.DeviceDataCollectionExAsync(
                    request.deviceEnCode,
                    request.sendTime,
                    request.alarmMessages,
                    updateParams ?? []
                );

                // 处理结果并返回相应的HTTP响应
                if (result.IsOk)
                {
                    return Ok(new { code = 200, message = "设备信息收集成功" });
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
