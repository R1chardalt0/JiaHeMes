using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChargePadLine.Common.Config;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Service.Migration;
using System.Data;

namespace ChargePadLine.Service.Migration.Impl
{
    /// <summary>
    /// 数据库迁移服务实现类
    /// </summary>
    public class MigrationServiceImpl : IMigrationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<MigrationServiceImpl> _logger;
        //private readonly int _retentionDays;
        //private readonly bool _isEnabled;
        private readonly MigrationConfig _config;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="appDbContext">应用数据库上下文</param>
        /// <param name="reportDbContext">报表数据库上下文</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="configuration">配置对象</param>
        public MigrationServiceImpl(AppDbContext appDbContext, ReportDbContext reportDbContext, ILogger<MigrationServiceImpl> logger, IOptions<MigrationConfig> config)
        {
            _appDbContext = appDbContext;
            _reportDbContext = reportDbContext;
            _logger = logger;
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            // 从配置中读取数据保留天数，默认为30天
            //_retentionDays = configuration.GetValue<int>("Migration:DataRetentionDays", 30);
            //_isEnabled = configuration.GetValue<bool>("Migration:IsEnabled", true);
            
            _logger.LogInformation("数据迁移服务初始化完成：数据保留天数 = {RetentionDays}, 迁移功能状态 = {IsEnabled}", _config.DataRetentionDays, _config.IsEnabled);
        }

        /// <summary>
        /// 执行全量复制操作
        /// </summary>
        /// <returns>迁移结果</returns>
        public async Task<MigrationResult> CopyStaticTablesAsync()
        {
            var result = new MigrationResult();
            var startTime = DateTime.Now;
            
            try
            {
                _logger.LogMigrationStart("静态表数据复制操作");

                // 复制SysCompanys表
                await CopySysCompanysAsync(result);
                
                // 复制ProductionLines表
                await CopyProductionLinesAsync(result);
                
                // 复制DeviceInfos表
                await CopyDeviceInfosAsync(result);

                _logger.LogMigrationEnd("静态表数据复制操作", result, startTime);
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("静态表数据复制操作", ex);
                result.Errors.Add(ex.Message);
                result.FailedCount++;
                _logger.LogMigrationEnd("静态表数据复制操作", result, startTime);
            }

            return result;
        }

        /// <summary>
        /// 执行定期转移操作
        /// </summary>
        /// <param name="thresholdDate">阈值日期</param>
        /// <returns>迁移结果</returns>
        public async Task<MigrationResult> TransferHistoryDataAsync(DateTime thresholdDate)
        {
            var result = new MigrationResult();
            var startTime = DateTime.Now;
            
            try
            {
                _logger.LogMigrationStart("历史数据转移操作", $"阈值日期：{thresholdDate}");

                // 使用事务确保数据一致性
                using var appTransaction = await _appDbContext.Database.BeginTransactionAsync();
                using var reportTransaction = await _reportDbContext.Database.BeginTransactionAsync();
                
                try
                {
                    // 转移EquipmentTracinfos表
                    await TransferEquipmentTracinfosAsync(thresholdDate, result, reportTransaction, appTransaction);
                    
                    // 转移ProductTraceInfos表
                    await TransferProductTraceInfosAsync(thresholdDate, result, reportTransaction, appTransaction);

                    // 如果没有错误，提交事务
                    if (result.IsSuccess)
                    {
                        await reportTransaction.CommitAsync();
                        await appTransaction.CommitAsync();
                        _logger.LogMigrationEnd("历史数据转移操作", result, startTime);
                    }
                    else
                    {
                        await reportTransaction.RollbackAsync();
                        await appTransaction.RollbackAsync();
                        _logger.LogMigrationWarning("历史数据转移操作", "出现错误，已回滚事务");
                        _logger.LogMigrationEnd("历史数据转移操作", result, startTime);
                    }
                }
                catch (Exception ex)
                {
                    await reportTransaction.RollbackAsync();
                    await appTransaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("历史数据转移操作", ex);
                result.Errors.Add(ex.Message);
                result.FailedCount++;
                _logger.LogMigrationEnd("历史数据转移操作", result, startTime);
            }

            return result;
        }

        /// <summary>
        /// 执行定期转移操作（使用配置的时间阈值）
        /// </summary>
        /// <returns>迁移结果</returns>
        public async Task<MigrationResult> TransferHistoryDataAsync()
        {
            // 如果迁移功能未启用，直接返回成功结果
            if (!_config.IsEnabled)
            {
                _logger.LogInformation("数据迁移功能已禁用，跳过迁移操作");
                return new MigrationResult();
            }
            
            var thresholdDate = DateTimeOffset.Now.AddDays(-_config.DataRetentionDays);
            _logger.LogInformation("使用配置的数据保留天数：{RetentionDays}天，计算阈值日期：{ThresholdDate}",
                _config.DataRetentionDays, thresholdDate.DateTime);
                
            return await TransferHistoryDataAsync(thresholdDate.DateTime);
        }

        #region 私有方法

        /// <summary>
        /// 复制SysCompanys表数据
        /// </summary>
        private async Task CopySysCompanysAsync(MigrationResult result)
        {
            try
            {
                var companies = await _appDbContext.SysCompanys.ToListAsync();
                var existingCompanyIds = await _reportDbContext.SysCompanys.Select(c => c.CompanyId).ToListAsync();
                
                int addedCount = 0;
                foreach (var company in companies)
                {
                    if (!existingCompanyIds.Contains(company.CompanyId))
                    {
                        await _reportDbContext.SysCompanys.AddAsync(company);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _reportDbContext.SaveChangesAsync();
                    result.SuccessCount += addedCount;
                    _logger.LogInformation($"成功复制SysCompanys表数据：{addedCount} 条");
                }
                else
                {
                    _logger.LogInformation("SysCompanys表无需复制数据，所有记录已存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("复制SysCompanys表", ex);
                result.Errors.Add($"复制SysCompanys表失败：{ex.Message}");
                result.FailedCount++;
            }
        }

        /// <summary>
        /// 复制ProductionLines表数据
        /// </summary>
        private async Task CopyProductionLinesAsync(MigrationResult result)
        {
            try
            {
                var productionLines = await _appDbContext.ProductionLines.ToListAsync();
                var existingLineIds = await _reportDbContext.ProductionLines.Select(p => p.ProductionLineId).ToListAsync();
                
                int addedCount = 0;
                foreach (var line in productionLines)
                {
                    if (!existingLineIds.Contains(line.ProductionLineId))
                    {
                        await _reportDbContext.ProductionLines.AddAsync(line);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _reportDbContext.SaveChangesAsync();
                    result.SuccessCount += addedCount;
                    _logger.LogInformation($"成功复制ProductionLines表数据：{addedCount} 条");
                }
                else
                {
                    _logger.LogInformation("ProductionLines表无需复制数据，所有记录已存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("复制ProductionLines表", ex);
                result.Errors.Add($"复制ProductionLines表失败：{ex.Message}");
                result.FailedCount++;
            }
        }

        /// <summary>
        /// 复制DeviceInfos表数据
        /// </summary>
        private async Task CopyDeviceInfosAsync(MigrationResult result)
        {
            try
            {
                var devices = await _appDbContext.DeviceInfos.ToListAsync();
                var existingDeviceIds = await _reportDbContext.DeviceInfos.Select(d => d.DeviceId).ToListAsync();
                
                int addedCount = 0;
                foreach (var device in devices)
                {
                    if (!existingDeviceIds.Contains(device.DeviceId))
                    {
                        await _reportDbContext.DeviceInfos.AddAsync(device);
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    await _reportDbContext.SaveChangesAsync();
                    result.SuccessCount += addedCount;
                    _logger.LogInformation($"成功复制DeviceInfos表数据：{addedCount} 条");
                }
                else
                {
                    _logger.LogInformation("DeviceInfos表无需复制数据，所有记录已存在");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("复制DeviceInfos表", ex);
                result.Errors.Add($"复制DeviceInfos表失败：{ex.Message}");
                result.FailedCount++;
            }
        }

        /// <summary>
        /// 转移EquipmentTracinfos表数据
        /// </summary>
        private async Task TransferEquipmentTracinfosAsync(DateTime thresholdDate, MigrationResult result, IDbContextTransaction reportTransaction, IDbContextTransaction appTransaction)
        {
            try
            {
                // 查询需要转移的数据
                var equipmentTraces = await _appDbContext.EquipmentTracinfos
                    .Where(e => e.CreateTime <= new DateTimeOffset(thresholdDate))
                    .ToListAsync();

                if (equipmentTraces.Any())
                {
                    _logger.LogInformation($"开始转移EquipmentTracinfos表数据，共 {equipmentTraces.Count} 条记录");
                    
                    // 添加到报表数据库
                    await _reportDbContext.EquipmentTracinfos.AddRangeAsync(equipmentTraces);
                    int reportRowsAffected = await _reportDbContext.SaveChangesAsync();
                    
                    if (reportRowsAffected == equipmentTraces.Count)
                    {
                        // 从应用数据库删除
                        _appDbContext.EquipmentTracinfos.RemoveRange(equipmentTraces);
                        int appRowsAffected = await _appDbContext.SaveChangesAsync();
                        
                        if (appRowsAffected == equipmentTraces.Count)
                        {
                            result.SuccessCount += equipmentTraces.Count;
                            _logger.LogInformation($"成功转移EquipmentTracinfos表数据：{equipmentTraces.Count} 条");
                        }
                        else
                        {
                            throw new InvalidOperationException($"删除应用数据库EquipmentTracinfos表数据不匹配，预期删除 {equipmentTraces.Count} 条，实际删除 {appRowsAffected} 条");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"插入报表数据库EquipmentTracinfos表数据不匹配，预期插入 {equipmentTraces.Count} 条，实际插入 {reportRowsAffected} 条");
                    }
                }
                else
                {
                    _logger.LogInformation("EquipmentTracinfos表无需转移数据，没有符合条件的记录");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("转移EquipmentTracinfos表", ex);
                result.Errors.Add($"转移EquipmentTracinfos表失败：{ex.Message}");
                result.FailedCount++;
                throw;
            }
        }

        /// <summary>
        /// 转移ProductTraceInfos表数据
        /// </summary>
        private async Task TransferProductTraceInfosAsync(DateTime thresholdDate, MigrationResult result, IDbContextTransaction reportTransaction, IDbContextTransaction appTransaction)
        {
            try
            {
                // 查询需要转移的数据
                var productTraces = await _appDbContext.ProductTraceInfos
                    .Where(p => p.CreateTime <= new DateTimeOffset(thresholdDate))
                    .ToListAsync();

                if (productTraces.Any())
                {
                    _logger.LogInformation($"开始转移ProductTraceInfos表数据，共 {productTraces.Count} 条记录");
                    
                    // 添加到报表数据库
                    await _reportDbContext.ProductTraceInfos.AddRangeAsync(productTraces);
                    int reportRowsAffected = await _reportDbContext.SaveChangesAsync();
                    
                    if (reportRowsAffected == productTraces.Count)
                    {
                        // 从应用数据库删除
                        _appDbContext.ProductTraceInfos.RemoveRange(productTraces);
                        int appRowsAffected = await _appDbContext.SaveChangesAsync();
                        
                        if (appRowsAffected == productTraces.Count)
                        {
                            result.SuccessCount += productTraces.Count;
                            _logger.LogInformation($"成功转移ProductTraceInfos表数据：{productTraces.Count} 条");
                        }
                        else
                        {
                            throw new InvalidOperationException($"删除应用数据库ProductTraceInfos表数据不匹配，预期删除 {productTraces.Count} 条，实际删除 {appRowsAffected} 条");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"插入报表数据库ProductTraceInfos表数据不匹配，预期插入 {productTraces.Count} 条，实际插入 {reportRowsAffected} 条");
                    }
                }
                else
                {
                    _logger.LogInformation("ProductTraceInfos表无需转移数据，没有符合条件的记录");
                }
            }
            catch (Exception ex)
            {
                _logger.LogMigrationError("转移ProductTraceInfos表", ex);
                result.Errors.Add($"转移ProductTraceInfos表失败：{ex.Message}");
                result.FailedCount++;
                throw;
            }
        }

        #endregion
    }
}