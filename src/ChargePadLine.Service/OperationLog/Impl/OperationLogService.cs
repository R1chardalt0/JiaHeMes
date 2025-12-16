using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service;
using ChargePadLine.Service.OperationLog.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargePadLine.Service.OperationLog.Impl
{

    /// <summary>
    /// 操作日志服务实现类
    /// </summary>
    public class OperationLogService : IOperationLogService
    {
        private readonly IRepository<SysOperationLog> _operationLogRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<OperationLogService> _logger;

        public OperationLogService(
            IRepository<SysOperationLog> operationLogRepo,
            AppDbContext dbContext,
            ILogger<OperationLogService> logger)
        {
            _operationLogRepo = operationLogRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 新增操作日志
        /// 注意：此方法使用 try-catch 确保日志记录失败不影响主业务流程
        /// </summary>
        /// <param name="dto">操作日志数据传输对象</param>
        /// <returns></returns>
        public async Task AddOperationLogAsync(OperationLogAddDto dto)
        {
            try
            {
                var log = new SysOperationLog
                {
                    UserCode = dto.UserCode ?? string.Empty,
                    UserName = dto.UserName ?? string.Empty,
                    OperationType = dto.OperationType ?? string.Empty,
                    OperationModule = dto.OperationModule ?? string.Empty,
                    TargetId = dto.TargetId ?? string.Empty,
                    BeforeData = dto.BeforeData,
                    AfterData = dto.AfterData,
                    OperationIp = dto.OperationIp ?? string.Empty,
                    OperationRemark = dto.OperationRemark ?? string.Empty,
                    OperationStatus = dto.OperationStatus ?? "SUCCESS",
                    OperationTime = DateTimeOffset.Now
                };

                var result = await _operationLogRepo.InsertAsyncs(log);
                if (result > 0)
                {
                    _logger.LogInformation("操作日志保存成功：用户={UserCode}, 模块={Module}, 操作={Operation}, 对象ID={TargetId}", 
                        dto.UserCode, dto.OperationModule, dto.OperationType, dto.TargetId);
                }
                else
                {
                    _logger.LogWarning("操作日志保存失败（返回0）：用户={UserCode}, 模块={Module}, 操作={Operation}", 
                        dto.UserCode, dto.OperationModule, dto.OperationType);
                }
            }
            catch (Exception ex)
            {
                // 日志记录失败时，仅打运行日志警告，不阻断业务
                _logger.LogError(ex, "操作日志记录异常：用户={UserCode}, 模块={Module}, 操作={Operation}, 错误={Error}", 
                    dto.UserCode, dto.OperationModule, dto.OperationType, ex.Message);
            }
        }

        /// <summary>
        /// 分页查询操作日志列表
        /// </summary>
        /// <param name="dto">查询条件</param>
        /// <returns>分页操作日志列表</returns>
        public async Task<PaginatedList<SysOperationLog>> GetOperationLogListAsync(OperationLogQueryDto dto)
        {
            try
            {
                // 先检查数据库连接和表是否存在
                var totalCount = await _dbContext.SysOperationLogs.CountAsync();
                _logger.LogInformation("操作日志表总记录数：{Count}", totalCount);

                var query = _dbContext.SysOperationLogs
                    .OrderByDescending(log => log.OperationTime)
                    .AsQueryable();

                // 关键字搜索（工号、姓名、操作模块、操作对象ID、操作备注）
                if (!string.IsNullOrWhiteSpace(dto.Keyword))
                {
                    var keyword = dto.Keyword.Trim();
                    _logger.LogInformation("应用关键字搜索：{Keyword}", keyword);
                    query = query.Where(log =>
                        (log.UserCode != null && log.UserCode.Contains(keyword)) ||
                        (log.UserName != null && log.UserName.Contains(keyword)) ||
                        (log.OperationModule != null && log.OperationModule.Contains(keyword)) ||
                        (log.TargetId != null && log.TargetId.Contains(keyword)) ||
                        (log.OperationRemark != null && log.OperationRemark.Contains(keyword)));
                }

                // 操作用户工号筛选
                if (!string.IsNullOrWhiteSpace(dto.UserCode))
                {
                    var userCode = dto.UserCode.Trim();
                    _logger.LogInformation("应用用户工号筛选：{UserCode}", userCode);
                    query = query.Where(log => log.UserCode != null && log.UserCode.Contains(userCode));
                }

                // 操作用户姓名筛选
                if (!string.IsNullOrWhiteSpace(dto.UserName))
                {
                    var userName = dto.UserName.Trim();
                    _logger.LogInformation("应用用户姓名筛选：{UserName}", userName);
                    query = query.Where(log => log.UserName != null && log.UserName.Contains(userName));
                }

                // 操作类型筛选
                if (!string.IsNullOrWhiteSpace(dto.OperationType))
                {
                    _logger.LogInformation("应用操作类型筛选：{OperationType}", dto.OperationType);
                    query = query.Where(log => log.OperationType == dto.OperationType);
                }

                // 操作模块筛选
                if (!string.IsNullOrWhiteSpace(dto.OperationModule))
                {
                    var operationModule = dto.OperationModule.Trim();
                    _logger.LogInformation("应用操作模块筛选：{OperationModule}", operationModule);
                    query = query.Where(log => log.OperationModule != null && log.OperationModule.Contains(operationModule));
                }

                // 操作对象ID筛选
                if (!string.IsNullOrWhiteSpace(dto.TargetId))
                {
                    var targetId = dto.TargetId.Trim();
                    _logger.LogInformation("应用操作对象ID筛选：{TargetId}", targetId);
                    query = query.Where(log => log.TargetId != null && log.TargetId.Contains(targetId));
                }

                // 操作状态筛选
                if (!string.IsNullOrWhiteSpace(dto.OperationStatus))
                {
                    _logger.LogInformation("应用操作状态筛选：{OperationStatus}", dto.OperationStatus);
                    query = query.Where(log => log.OperationStatus == dto.OperationStatus);
                }

                // 操作时间范围筛选
                if (dto.OperationTimeStart.HasValue)
                {
                    _logger.LogInformation("应用操作时间开始筛选：{OperationTimeStart}", dto.OperationTimeStart);
                    query = query.Where(log => log.OperationTime >= dto.OperationTimeStart.Value);
                }

                if (dto.OperationTimeEnd.HasValue)
                {
                    _logger.LogInformation("应用操作时间结束筛选：{OperationTimeEnd}", dto.OperationTimeEnd);
                    query = query.Where(log => log.OperationTime <= dto.OperationTimeEnd.Value);
                }

                // 获取筛选后的总数
                var filteredCount = await query.CountAsync();
                _logger.LogInformation("筛选后记录数：{Count}", filteredCount);

                // 分页查询
                var result = await query.RetrievePagedListAsync(dto.Current, dto.PageSize);
                
                _logger.LogInformation("查询操作日志成功：当前页={Current}, 每页={PageSize}, 总数={Total}, 返回记录数={ItemsCount}", 
                    dto.Current, dto.PageSize, result.TotalCounts, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询操作日志失败：当前页={Current}, 每页={PageSize}, 错误={Error}, 堆栈={StackTrace}", 
                    dto.Current, dto.PageSize, ex.Message, ex.StackTrace);
                // 返回空列表而不是抛出异常
                return new PaginatedList<SysOperationLog>(new List<SysOperationLog>(), 0, dto.Current, dto.PageSize);
            }
        }

        /// <summary>
        /// 测试查询接口（用于调试）
        /// </summary>
        /// <returns>测试结果</returns>
        public async Task<object> TestQueryAsync()
        {
            try
            {
                // 测试1: 直接查询表总数
                var totalCount = await _dbContext.SysOperationLogs.CountAsync();
                _logger.LogInformation("测试查询 - 表总记录数：{Count}", totalCount);

                // 测试2: 查询前10条数据
                var top10 = await _dbContext.SysOperationLogs
                    .OrderByDescending(log => log.OperationTime)
                    .Take(10)
                    .ToListAsync();
                _logger.LogInformation("测试查询 - 前10条记录数：{Count}", top10.Count);

                // 测试3: 检查数据库连接
                var canConnect = await _dbContext.Database.CanConnectAsync();
                _logger.LogInformation("测试查询 - 数据库连接状态：{CanConnect}", canConnect);

                // 测试4: 获取数据库提供程序信息
                var providerName = _dbContext.Database.ProviderName;
                _logger.LogInformation("测试查询 - 数据库提供程序：{Provider}", providerName);

                return new
                {
                    totalCount,
                    top10Count = top10.Count,
                    canConnect,
                    providerName,
                    sampleData = top10.Select(log => new
                    {
                        log.LogId,
                        log.UserCode,
                        log.UserName,
                        log.OperationType,
                        log.OperationModule,
                        log.TargetId,
                        log.OperationTime
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试查询异常：{Error}, 堆栈：{StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}

