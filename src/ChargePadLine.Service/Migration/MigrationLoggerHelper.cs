using Microsoft.Extensions.Logging;
using System.Text;

namespace ChargePadLine.Service.Migration
{
    /// <summary>
    /// 迁移日志辅助类
    /// 用于增强数据迁移过程中的日志记录和错误处理
    /// </summary>
    public static class MigrationLoggerHelper
    {
        /// <summary>
        /// 记录迁移操作开始
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="additionalInfo">附加信息</param>
        public static void LogMigrationStart(this ILogger logger, string operationName, string additionalInfo = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"========== {operationName} 开始 ==========");
            
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                sb.AppendLine($"附加信息: {additionalInfo}");
            }
            
            sb.AppendLine($"开始时间: {DateTime.Now}");
            sb.AppendLine($"======================================");
            
            logger.LogInformation(sb.ToString());
        }

        /// <summary>
        /// 记录迁移操作结束
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="result">迁移结果</param>
        /// <param name="startTime">开始时间</param>
        public static void LogMigrationEnd(this ILogger logger, string operationName, MigrationResult result, DateTime startTime)
        {
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            var sb = new StringBuilder();
            sb.AppendLine($"========== {operationName} 结束 ==========");
            sb.AppendLine($"结束时间: {endTime}");
            sb.AppendLine($"耗时: {duration.TotalSeconds:F2} 秒");
            sb.AppendLine($"成功记录数: {result.SuccessCount}");
            sb.AppendLine($"失败记录数: {result.FailedCount}");
            
            if (result.Errors.Any())
            {
                sb.AppendLine("错误信息:");
                foreach (var error in result.Errors)
                {
                    sb.AppendLine($"  - {error}");
                }
            }
            
            sb.AppendLine($"操作状态: {(result.IsSuccess ? "成功" : "失败")}");
            sb.AppendLine($"======================================");
            
            if (result.IsSuccess)
            {
                logger.LogInformation(sb.ToString());
            }
            else
            {
                logger.LogError(sb.ToString());
            }
        }

        /// <summary>
        /// 记录迁移错误
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="ex">异常对象</param>
        /// <param name="additionalInfo">附加信息</param>
        public static void LogMigrationError(this ILogger logger, string operationName, Exception ex, string additionalInfo = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"========== {operationName} 错误 ==========");
            sb.AppendLine($"错误时间: {DateTime.Now}");
            sb.AppendLine($"错误类型: {ex.GetType().Name}");
            sb.AppendLine($"错误消息: {ex.Message}");
            
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                sb.AppendLine($"附加信息: {additionalInfo}");
            }
            
            sb.AppendLine($"异常堆栈: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"内部异常: {ex.InnerException.Message}");
                sb.AppendLine($"内部异常堆栈: {ex.InnerException.StackTrace}");
            }
            
            sb.AppendLine($"======================================");
            
            logger.LogError(ex, sb.ToString());
        }

        /// <summary>
        /// 记录迁移警告
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="warningMessage">警告消息</param>
        public static void LogMigrationWarning(this ILogger logger, string operationName, string warningMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"========== {operationName} 警告 ==========");
            sb.AppendLine($"警告时间: {DateTime.Now}");
            sb.AppendLine($"警告消息: {warningMessage}");
            sb.AppendLine($"======================================");
            
            logger.LogWarning(sb.ToString());
        }
        
        /// <summary>
        /// 记录迁移操作结束（不包含结果的简化版本）
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="operationName">操作名称</param>
        /// <param name="startTime">开始时间</param>
        public static void LogMigrationEnd(this ILogger logger, string operationName, DateTime startTime)
        {
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            var sb = new StringBuilder();
            sb.AppendLine($"========== {operationName} 结束 ==========");
            sb.AppendLine($"结束时间: {endTime}");
            sb.AppendLine($"耗时: {duration.TotalSeconds:F2} 秒");
            sb.AppendLine($"操作状态: 已完成");
            sb.AppendLine($"======================================");
            
            logger.LogInformation(sb.ToString());
        }
    }
}