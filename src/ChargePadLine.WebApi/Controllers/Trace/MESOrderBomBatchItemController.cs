using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.Order;

namespace ChargePadLine.WebApi.Controllers.Trace
{
    /// <summary>
    /// MES工单BOM批次明细控制器
    /// </summary>
    [Route("api/trace/[controller]")]
    [ApiController]
    public class MESOrderBomBatchItemController : ControllerBase
    {
        private readonly IMESOrderBomBatchItemService _service;
        private readonly ILogger<MESOrderBomBatchItemController> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="service">MES工单BOM批次明细服务</param>
        /// <param name="logger">日志记录器</param>
        public MESOrderBomBatchItemController(IMESOrderBomBatchItemService service, ILogger<MESOrderBomBatchItemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// 根据ID查询工单BOM批次明细
        /// </summary>
        /// <param name="id">工单BOM批次明细ID</param>
        /// <returns>工单BOM批次明细数据传输对象</returns>
        [HttpGet("GetMESOrderBomBatchItemById")]
        public async Task<ActionResult<MesOrderBomBatchItemDto>> GetMESOrderBomBatchItemById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound($"未找到ID为 {id} 的工单BOM批次明细");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取工单BOM批次明细详情时发生错误，ID: {OrderBomBatchItemId}", id);
                return StatusCode(500, "获取工单BOM批次明细详情失败");
            }
        }

        /// <summary>
        /// 查询工单BOM批次明细列表
        /// </summary>
        /// <param name="queryDto">查询参数</param>
        /// <returns>工单BOM批次明细数据传输对象列表</returns>
        [HttpGet("GetMESOrderBomBatchItemList")]
        public async Task<ActionResult<List<MesOrderBomBatchItemDto>>> GetMESOrderBomBatchItemList([FromQuery] MESOrderBomBatchItemQueryDto queryDto)
        {
            try
            {
                var result = await _service.GetListAsync(queryDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取工单BOM批次明细列表时发生错误");
                return StatusCode(500, "获取工单BOM批次明细列表失败");
            }
        }

        /// <summary>
        /// 分页查询工单BOM批次明细
        /// </summary>
        /// <param name="queryDto">查询参数</param>
        /// <returns>分页结果，包含工单BOM批次明细数据传输对象列表和总记录数</returns>
        [HttpGet("GetMESOrderBomBatchItemPagedList")]
        public async Task<ActionResult<object>> GetMESOrderBomBatchItemPagedList([FromQuery] MESOrderBomBatchItemQueryDto queryDto)
        {
            try
            {
                var (data, total) = await _service.GetPagedListAsync(queryDto);
                return Ok(new { data, total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分页获取工单BOM批次明细列表时发生错误");
                return StatusCode(500, "分页获取工单BOM批次明细列表失败");
            }
        }

        /// <summary>
        /// 根据工单BOM批次ID查询明细列表
        /// </summary>
        /// <param name="orderBomBatchId">工单BOM批次ID</param>
        /// <returns>工单BOM批次明细数据传输对象列表</returns>
        [HttpGet("GetMESOrderBomBatchItemByOrderBomBatchId")]
        public async Task<ActionResult<List<MesOrderBomBatchItemDto>>> GetMESOrderBomBatchItemByOrderBomBatchId(Guid orderBomBatchId)
        {
            try
            {
                var result = await _service.GetByOrderBomBatchIdAsync(orderBomBatchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据工单BOM批次ID获取明细列表时发生错误，工单BOM批次ID: {OrderBomBatchId}", orderBomBatchId);
                return StatusCode(500, "根据工单BOM批次ID获取明细列表失败");
            }
        }

        /// <summary>
        /// 根据SN编码查询工单BOM批次明细
        /// </summary>
        /// <param name="snNumber">SN编码</param>
        /// <returns>工单BOM批次明细数据传输对象</returns>
        [HttpGet("GetMESOrderBomBatchItemBySnNumber")]
        public async Task<ActionResult<MesOrderBomBatchItemDto>> GetMESOrderBomBatchItemBySnNumber(string snNumber)
        {
            try
            {
                var result = await _service.GetBySnNumberAsync(snNumber);
                if (result == null)
                {
                    return NotFound($"未找到SN编码为 {snNumber} 的工单BOM批次明细");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据SN编码获取工单BOM批次明细时发生错误，SN编码: {SnNumber}", snNumber);
                return StatusCode(500, "根据SN编码获取工单BOM批次明细失败");
            }
        }
    }
}
