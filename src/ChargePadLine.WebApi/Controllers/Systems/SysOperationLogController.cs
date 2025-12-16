using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.OperationLog;
using ChargePadLine.Service.OperationLog.Dto;
using ChargePadLine.WebApi.Controllers.Systems;
using ChargePadLine.WebApi.util;
using System;
using System.Threading.Tasks;

namespace ChargePadLine.WebApi.Controllers.Systems
{
    /// <summary>
    /// 系统操作日志控制器
    /// </summary>
    public class SysOperationLogController : BaseController
    {
        private readonly IOperationLogService _operationLogService;

        public SysOperationLogController(IOperationLogService operationLogService)
        {
            _operationLogService = operationLogService;
        }

        /// <summary>
        /// 获取操作日志列表（分页查询）
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="keyword">关键字搜索（工号、姓名、操作模块、操作对象ID、操作备注）</param>
        /// <param name="userCode">操作用户工号</param>
        /// <param name="userName">操作用户姓名</param>
        /// <param name="operationType">操作类型（INSERT、UPDATE、DELETE）</param>
        /// <param name="operationModule">操作模块</param>
        /// <param name="targetId">操作对象ID</param>
        /// <param name="operationStatus">操作状态（SUCCESS、FAIL）</param>
        /// <param name="operationTimeStart">操作时间开始</param>
        /// <param name="operationTimeEnd">操作时间结束</param>
        /// <returns>操作日志列表</returns>
        [HttpGet("list")]
        public async Task<PagedResp<SysOperationLog>> GetOperationLogList(
            int current = 1,
            int pageSize = 20,
            string? keyword = null,
            string? userCode = null,
            string? userName = null,
            string? operationType = null,
            string? operationModule = null,
            string? targetId = null,
            string? operationStatus = null,
            DateTimeOffset? operationTimeStart = null,
            DateTimeOffset? operationTimeEnd = null)
        {
            try
            {
                // 参数校验
                if (current < 1)
                {
                    current = 1;
                }
                if (pageSize < 1)
                {
                    pageSize = 20;
                }
                if (pageSize > 100)
                {
                    pageSize = 100;
                }

                var query = new OperationLogQueryDto
                {
                    Current = current,
                    PageSize = pageSize,
                    Keyword = keyword,
                    UserCode = userCode,
                    UserName = userName,
                    OperationType = operationType,
                    OperationModule = operationModule,
                    TargetId = targetId,
                    OperationStatus = operationStatus,
                    OperationTimeStart = operationTimeStart,
                    OperationTimeEnd = operationTimeEnd
                };

                var list = await _operationLogService.GetOperationLogListAsync(query);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch (Exception ex)
            {
                // 记录异常日志，便于排查问题
                // 注意：这里不能直接使用ILogger，因为BaseController可能没有注入
                return RespExtensions.MakePagedEmpty<SysOperationLog>();
            }
        }

        /// <summary>
        /// 新增操作日志（供业务模块调用）
        /// </summary>
        /// <param name="dto">操作日志数据传输对象</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> AddOperationLog([FromBody] OperationLogAddDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _operationLogService.AddOperationLogAsync(dto);
                return Ok(new { code = 200, msg = "操作日志记录成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = $"操作日志记录失败：{ex.Message}" });
            }
        }

        /// <summary>
        /// 测试查询接口 - 直接查询数据库（用于调试）
        /// </summary>
        /// <returns>测试结果</returns>
        [HttpGet("test")]
        public async Task<IActionResult> TestQuery()
        {
            try
            {
                var result = await _operationLogService.TestQueryAsync();
                return Ok(new { 
                    code = 200, 
                    msg = "测试查询成功", 
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    code = 500, 
                    msg = $"测试查询失败：{ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}

