using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChargePadLine.Client.ViewModels;


namespace ChargePadLine.Client.Services.PlcService
{
    public interface ILogService
    {
        /// <summary>
        /// 记录系统日志
        /// </summary>
        Task RecordLogAsync(LogLevel logLevel, string message);

        /// <summary>
        /// 记录操作日志
        /// </summary>
        Task RecordOperationLogAsync(string operation, string details = "", string userName = "");

        /// <summary>
        /// 记录用户操作日志（带用户名）
        /// </summary>
        Task RecordUserOperationAsync(string userName, string operation, string details = "");
    }
}
