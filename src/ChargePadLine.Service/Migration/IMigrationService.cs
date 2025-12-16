using System;
using System.Collections.Generic;

namespace ChargePadLine.Service.Migration
{
    /// <summary>
    /// 数据库迁移服务接口
    /// 用于将AppDbContext中的数据迁移到ReportDbContext
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>
        /// 执行全量复制操作
        /// 将SysCompanys，ProductionLines，DeviceInfos表的数据复制到ReportDbContext
        /// </summary>
        /// <returns>迁移的数据量</returns>
        Task<MigrationResult> CopyStaticTablesAsync();

        /// <summary>
        /// 执行定期转移操作
        /// 将EquipmentTracinfos，ProductTraceInfos表中超过指定时间的数据转移到ReportDbContext
        /// </summary>
        /// <param name="thresholdDate">阈值日期，早于此日期的数据将被转移</param>
        /// <returns>迁移的数据量</returns>
        Task<MigrationResult> TransferHistoryDataAsync(DateTime thresholdDate);

        /// <summary>
        /// 执行定期转移操作（使用默认时间阈值，如一个月前）
        /// </summary>
        /// <returns>迁移的数据量</returns>
        Task<MigrationResult> TransferHistoryDataAsync();
    }

    /// <summary>
    /// 迁移结果类
    /// </summary>
    public class MigrationResult
    {
        /// <summary>
        /// 成功迁移的记录数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 迁移失败的记录数
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 迁移过程中的错误信息
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 迁移是否成功
        /// </summary>
        public bool IsSuccess => FailedCount == 0 && !Errors.Any();
    }
}