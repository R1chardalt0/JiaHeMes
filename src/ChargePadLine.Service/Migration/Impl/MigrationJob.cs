using Quartz;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ChargePadLine.Service.Migration;

namespace ChargePadLine.Service.Migration.Impl
{
    /// <summary>
    /// 数据迁移定时作业
    /// </summary>
    [DisallowConcurrentExecution]
    public class MigrationJob : IJob
    {
        private readonly IMigrationService _migrationService;
        private readonly ILogger<MigrationJob> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="migrationService">迁移服务</param>
        /// <param name="logger">日志记录器</param>
        public MigrationJob(IMigrationService migrationService, ILogger<MigrationJob> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        /// <summary>
        /// 执行定时作业
        /// </summary>
        /// <param name="context">作业执行上下文</param>
        public async Task Execute(IJobExecutionContext context)
        {
            var startTime = DateTime.Now;
            _logger.LogMigrationStart("数据迁移定时任务");

            try
            {
                // 执行静态表复制
                var copyResult = await _migrationService.CopyStaticTablesAsync();
                
                // 执行历史数据转移
                var transferResult = await _migrationService.TransferHistoryDataAsync();

                _logger.LogMigrationEnd("数据迁移定时任务", startTime);
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("数据迁移定时任务", ex);
                _logger.LogMigrationEnd("数据迁移定时任务（失败）", startTime);
                throw;
            }
        }
    }
}