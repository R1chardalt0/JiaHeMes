using HslCommunication;
using HslCommunication.LogNet;
using HslCommunication.ModBus;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NJCH_Station
{
    public class PLC_Conn : IDisposable
    {
        private readonly object _syncLock = new object();
        private bool _isDisposed = false;
        private const int DefaultTimeout = 3000; // 3秒超时

        public bool IsConnect { get; private set; }
        public ModbusTcpNet BusTcpNet { get; } = new ModbusTcpNet();

        public async Task<bool> ConnectAsync(string ip, int port, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentNullException(nameof(ip));

            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port));

            // 重置连接状态
            lock (_syncLock)
            {
                IsConnect = false;
            }

            // 1. 首先检查网络连通性
            if (!await CheckNetworkAvailabilityAsync(ip, cancellationToken))
            {
                LogMessage(HslMessageDegree.ERROR, $"PLC网络不可达 - {ip}");
                return false;
            }

            // 2. 配置并尝试建立连接
            try
            {
                ConfigureTcpClient(ip, port);

                var connectResult = await AttemptConnectionAsync(cancellationToken);

                // 3. 验证实际通信能力
                if (connectResult.IsSuccess && !await VerifyCommunicationAsync(cancellationToken))
                {
                    connectResult.IsSuccess = false;
                    connectResult.Message = "测试通信失败";
                }

                UpdateConnectionStatus(connectResult.IsSuccess);
                LogConnectionResult(connectResult, ip, port);

                return IsConnect;
            }
            catch (OperationCanceledException)
            {
                LogMessage(HslMessageDegree.WARN, "连接操作已取消");
                return false;
            }
            catch (Exception ex)
            {
                LogMessage(HslMessageDegree.FATAL, $"连接异常: {ex.Message}");
                throw new PLCConnectionException($"连接PLC失败: {ip}:{port}", ex);
            }
        }

        private void ConfigureTcpClient(string ip, int port)
        {
            lock (_syncLock)
            {
                BusTcpNet.IpAddress = ip;
                BusTcpNet.Port = port;
                BusTcpNet.ConnectTimeOut = DefaultTimeout;
                BusTcpNet.ReceiveTimeOut = DefaultTimeout;
                BusTcpNet.IsStringReverse = true;
            }
        }

        private async Task<bool> CheckNetworkAvailabilityAsync(string ip, CancellationToken cancellationToken)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ip, DefaultTimeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private Task<OperateResult> AttemptConnectionAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => BusTcpNet.ConnectServer(), cancellationToken);
        }

        private async Task<bool> VerifyCommunicationAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 改为读取 0 号寄存器（常见最小可用地址），并明确读取 1 个 UInt16
                var testResult = await Task.Run(() => 
                    BusTcpNet.ReadUInt16("1400"), // 若 0 号不可用，尝试联系 PLC 厂商确认有效地址
                    cancellationToken
                );
                return testResult.IsSuccess;
            }
            catch (Exception ex)
            {
                // 记录具体异常信息，包含越界细节（如地址/长度）
                LogMessage(HslMessageDegree.ERROR, $"通信验证失败: {ex.Message}");
                return false;
            }
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            lock (_syncLock)
            {
                IsConnect = isConnected;
            }
        }

        private void LogConnectionResult(OperateResult result, string ip, int port)
        {
            var status = result.IsSuccess ? "成功" : "失败";
            var message = $"PLC连接{status} - {ip}:{port}";

            if (!string.IsNullOrEmpty(result.Message))
            {
                message += $" 原因: {result.Message}";
            }

            LogMessage(
                result.IsSuccess ? HslMessageDegree.INFO : HslMessageDegree.ERROR,
                message);
        }
 
        public void Disconnect()
        {
            lock (_syncLock)
            {
                if (IsConnect) // 双重检查
                {
                    try
                    {
                        BusTcpNet.ConnectClose();
                        IsConnect = false;
                        LogMessage(HslMessageDegree.INFO, "PLC已断开连接");
                    }
                    catch (Exception ex)
                    {
                        LogMessage(HslMessageDegree.ERROR, $"断开连接异常: {ex.Message}");
                        IsConnect = false; // 强制更新状态
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    Disconnect();
                    // 显式关闭底层 Socket（HslCommunication 可能需要额外清理）
                    BusTcpNet?.ConnectClose(); 
                    BusTcpNet?.Dispose();
                    // 重置关键状态，确保新实例无残留
                    lock (_syncLock)
                    {
                        IsConnect = false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(HslMessageDegree.WARN, $"释放资源时出错: {ex.Message}");
                }
                _isDisposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Disconnect();
                    BusTcpNet.Dispose();
                }
                _isDisposed = true;
            }
        }

        private void LogMessage(HslMessageDegree degree, string message)
        {
            Manager.LogNet?.RecordMessage(degree, nameof(PLC_Conn), message);
        }

        ~PLC_Conn()
        {
            Dispose(false);
        }
    }

    public class PLCConnectionException : Exception
    {
        public PLCConnectionException(string message) : base(message) { }
        public PLCConnectionException(string message, Exception inner) : base(message, inner) { }
    }

    public static class Manager
    {
        private static readonly object _logLock = new object();
        private static ILogNet _logNet;

        public static PLC_Conn[] PLCConnections { get; set; }

        public static ILogNet LogNet
        {
            get
            {
                if (_logNet == null)
                {
                    lock (_logLock)
                    {
                        if (_logNet == null)
                        {
                            string logPath = Path.Combine(Application.StartupPath, "Logs");
                            Directory.CreateDirectory(logPath); // 确保日志目录存在
                            _logNet = new LogNetDateTime(logPath, GenerateMode.ByEveryDay);
                        }
                    }
                }
                return _logNet;
            }
            set
            {
                lock (_logLock)
                {
                    _logNet = value;
                }
            }
        }
    }
}