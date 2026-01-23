using ChargePadLine.Client.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc8
{
    /// <summary>
    /// PLC4 业务任务接口，例如定子检测、O 型圈装配等
    /// </summary>
    public interface IPlc8Task
    {
        /// <summary>
        /// 执行一次业务轮询 / 处理逻辑，由上层控制调用频率
        /// </summary>
        /// <param name="modbus">PLC 连接</param>
        /// <param name="cancellationToken">取消标记</param>
        Task ExecuteOnceAsync(ModbusConnect modbus, CancellationToken cancellationToken);
    }
}
