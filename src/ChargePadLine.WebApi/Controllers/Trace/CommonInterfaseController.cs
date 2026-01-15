using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Trace;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChargePadLine.Service.Trace.Impl;
using ChargePadLine.Service.Trace.Dto;
using ChargePadLine.Service;
using ChargePadLine.Entitys.Trace.Product;
using Microsoft.FSharp.Core;
using static ChargePadLine.Service.Trace.Impl.CommonInterfaseService;

namespace ChargePadLine.WebApi.Controllers.Trace
{
  [ApiController]
  [Route("api/[controller]")]
  public class CommonInterfaseController : BaseController
  {
    private readonly ICommonInterfaseService _iCommonInterfaseService;
    public CommonInterfaseController(ICommonInterfaseService iCommonInterfaseService)
    {
      _iCommonInterfaseService = iCommonInterfaseService;
    }
       /// <summary>
       /// 获取NS信息
       /// </summary>
       /// <param name="sn"></param>
       /// <returns></returns>
        [HttpPost("TraceSN")]
        public async Task<FSharpResult<SnTraceDto, (int, string)>> TraceSN(string sn)
        {
             
                var result = await _iCommonInterfaseService.TraceSN(sn);

                // 处理结果并返回相应的HTTP响应
                return result;
           
        }

    /// <summary>
    /// 物料上传接口
    /// </summary>
    /// <param name="request">设备数据采集参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("FeedMaterial")]
    public async Task<IActionResult> FeedMaterial(RequestFeedMaterialParams request)
    {
      try
      {

        var result = await _iCommonInterfaseService.FeedMaterial(request);

        // 处理结果并返回相应的HTTP响应
        if (result.ErrorValue.Item1.ToString() == "0")
        {
          return Ok(new { code = 200, message = result.ErrorValue.Item2.ToString() });
        }
        else
        {
          var errorResult = result.ErrorValue;
          return Ok(new { code = errorResult.Item1, message = errorResult.Item2 });
        }
      }
      catch (Exception ex)
      {
        // 记录异常并返回500错误
        return StatusCode(500, new { code = 500, message = $"服务器内部错误: {ex.Message}" });
      }
    }

    /// <summary>
    /// 数据上传检查接口
    /// </summary>
    /// <param name="request">设备数据采集参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("UploadCheck")]
    public async Task<IActionResult> UploadCheck(RequestUploadCheckParams request)
    {
      try
      {
        //// 参数验证
        //if (string.IsNullOrWhiteSpace(request.Resource))
        //{
        //    return BadRequest(new { code = 400, message = "设备编码不能为空" });
        //}

        var result = await _iCommonInterfaseService.UploadCheck(request);

        // 处理结果并返回相应的HTTP响应
        if (result.ErrorValue.Item1.ToString() == "0")
        {
          return Ok(new { code = 200, message = result.ErrorValue.Item2.ToString() });
        }
        else
        {
          var errorResult = result.ErrorValue;
          return Ok(new { code = errorResult.Item1, message = errorResult.Item2 });
        }
      }
      catch (Exception ex)
      {
        // 记录异常并返回500错误
        return StatusCode(500, new { code = 500, message = $"服务器内部错误: {ex.Message}" });
      }
    }
    /// <summary>
    /// 数据上传检查接口
    /// </summary>
    /// <param name="request">设备数据采集参数</param>
    /// <returns>操作结果</returns>
    [HttpPost("UploadData")]
    public async Task<IActionResult> UploadData(RequestUploadCheckParams request)
    {
      try
      {
        //// 参数验证
        //if (string.IsNullOrWhiteSpace(request.Resource))
        //{
        //    return BadRequest(new { code = 400, message = "设备编码不能为空" });
        //}

        var result = await _iCommonInterfaseService.UploadData(request);

        // 处理结果并返回相应的HTTP响应
        if (result.ErrorValue.Item1.ToString() == "0")
        {
          return Ok(new { code = 200, message = "数据上传成功" });
        }
        else
        {
          var errorResult = result.ErrorValue;
          return Ok(new { code = errorResult.Item1, message = errorResult.Item2 });
        }
      }
      catch (Exception ex)
      {
        // 记录异常并返回500错误
        return StatusCode(500, new { code = 500, message = $"服务器内部错误: {ex.Message}" });
      }
    }



    /// <summary>
    /// 上料接口
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MaterialLoaded(RequestMaterialLoadedParams request)
    {
      try
      {
        // 参数验证
        if (string.IsNullOrWhiteSpace(request.Resource))
        {
          return BadRequest(new { code = 400, message = "设备编码不能为空" });
        }



        // 处理结果并返回相应的HTTP响应
        if (true)
        {
          return Ok(new { code = 200, message = "设备信息收集成功" });
        }
        else
        {

          // return BadRequest(new { code = errorResult.Item1, message = errorResult.Item2 });
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

