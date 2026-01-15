using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeviceManage.Services; // for PaginatedList

namespace DeviceManage.Services.DeviceMagService.Impl
{
    public class LogService : ILogService
    {
        private readonly IRepository<OperationLog> _logRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<LogService> _logger;

        public LogService(IRepository<OperationLog> logRepo, AppDbContext dbContext, ILogger<LogService> logger)
        {
            _logRepo = logRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task LogAsync(OperationLog log)
        {
            await _logRepo.InsertAsync(log);
            await _dbContext.SaveChangesAsync();
        }

        public async Task LogAsync(int? userId,
                                    string? username,
                                    OperationType operationType,
                                    string module,
                                    string description,
                                    string result = "成功",
                                    string? requestParams = null,
                                    string? errorMessage = null,
                                    string? ipAddress = null)
        {
            var log = new OperationLog
            {
                UserId = userId,
                Username = username,
                OperationType = operationType,
                Module = module,
                Description = description,
                Result = result,
                RequestParams = requestParams,
                ErrorMessage = errorMessage,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };
            await LogAsync(log);
        }

        /// <summary>
        /// 分页查询操作日志
        /// </summary>
        public async Task<PaginatedList<OperationLog>> GetLogsAsync(LogSearchDto dto)
        {
            var query = _dbContext.OperationLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Module))
            {
                query = query.Where(l => l.Module != null && l.Module.Contains(dto.Module));
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                query = query.Where(l => l.Username != null && l.Username.Contains(dto.Username));
            }

            if (!string.IsNullOrWhiteSpace(dto.OperationType))
            {
                query = query.Where(l => l.OperationTypeString == dto.OperationType);
            }

            // 按时间倒序
            query = query.OrderByDescending(l => l.CreatedAt);

            return await query.RetrievePagedListAsync(dto.current, dto.pageSize);
        }
    }
}
