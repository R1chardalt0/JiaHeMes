using ChargePadLine.Client.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace ChargePadLine.Client.Services.PlcService.Plc1
{
    /// <summary>
    /// PLC1 业务任务接口，例如定子检测、O 型圈装配等
    /// </summary>
    public interface IPlc1Task
    {
        /// <summary>
        /// 执行一次业务轮询 / 处理逻辑，由上层控制调用频率
        /// </summary>
        /// <param name="s7Net">PLC 连接</param>
        /// <param name="cancellationToken">取消标记</param>
        Task ExecuteOnceAsync(S7NetConnect s7Net, CancellationToken cancellationToken);
    }
}


