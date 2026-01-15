using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Core.Pipe;
using HslCommunication.ModBus;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.Client.Helpers
{
    /// <summary>
    /// Modbus TCP连接管理类
    /// </summary>
    public class ModbusConnect : IDisposable
    {
        public readonly ILogger<ModbusConnect> _logger;

        private ModbusTcpNet _modbusClient;

        private bool _isConnected = false;

        private string _ipAddress = string.Empty;
        private int _port = 502;
        private int _connectTimeout = 5000;
        private int _receiveTimeout = 10000;

        //重连
        private bool _autoReconnectEnabled = true; // 是否启用自动重连
        private int _maxReconnectAttempts = 3;     // 最大重连次数
        private int _reconnectInterval = 1000;     // 重连间隔(毫秒)

        // 防止重入锁
        private object _reconnectLock = new object();
        private bool _isReconnecting = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ModbusConnect(ILogger<ModbusConnect> logger)
        {
            InitializeModbusClient();
            _logger = logger;
        }

        public ModbusConnect()
        {
            InitializeModbusClient();
        }

        private void InitializeModbusClient()
        {
            _modbusClient = new ModbusTcpNet()
            {
                Station = 1,
                IsStringReverse = false,
                DataFormat = DataFormat.CDAB,
            };
        }


        /// <summary>
        /// 连接到Modbus服务器
        /// </summary>
        /// <param name="ipAddress">服务器IP地址</param>
        /// <param name="port">端口号，默认502</param>
        /// <param name="connectTimeout">连接超时时间，单位毫秒，默认5000</param>
        /// <param name="receiveTimeout">接收超时时间，单位毫秒，默认10000</param>
        public OperateResult Connect(string ipAddress, int port = 502, int connectTimeout = 5000, int receiveTimeout = 10000)
        {
            try
            {
                // 保存连接参数
                _ipAddress = ipAddress;
                _port = port;
                _connectTimeout = connectTimeout;
                _receiveTimeout = receiveTimeout;

                // 如果已经连接，先断开
                if (_isConnected)
                {
                    Disconnect();
                }

                // 初始化客户端
                InitializeModbusClient();

                // 设置通信管道
                _modbusClient.CommunicationPipe = new PipeTcpNet(_ipAddress, _port)
                {
                    ConnectTimeOut = _connectTimeout,
                    ReceiveTimeOut = _receiveTimeout
                };

                // 连接服务器
                var result = _modbusClient.ConnectServer();
                if (result.IsSuccess)
                {
                    _isConnected = true;
                    _logger?.LogInformation(string.Format($"Modbus连接成功 - IP: {0}, 端口: {1}", _ipAddress, _port));
                }
                else
                {
                    _logger?.LogError(string.Format($"Modbus连接失败 - IP: {0}, 端口: {1}, 错误: {2}", _ipAddress, _port, result.Message));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus连接异常 - IP: {0}, 端口: {1}", _ipAddress, _port), ex);
                return new OperateResult(ex.Message);
            }
        }



        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_isConnected && _modbusClient != null)
                {
                    _modbusClient.ConnectClose();
                    _isConnected = false;
                    _logger?.LogInformation(string.Format($"Modbus已断开连接 - IP: {0}, 端口: {1}", _ipAddress, _port));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Modbus断开连接异常", ex);
            }
        }

        /// <summary>
        /// 自动重连
        /// </summary>
        /// <returns>重连结果</returns>
        private bool TryReconnect()
        {
            if (!_autoReconnectEnabled || string.IsNullOrEmpty(_ipAddress))
            {
                return false;
            }

            // 使用 TryEnter 避免死锁，如果无法获取锁则直接返回当前连接状态
            if (!Monitor.TryEnter(_reconnectLock))
            {
                // 如果无法获取锁，返回当前连接状态，避免阻塞
                _logger?.LogDebug("重连锁已被占用，跳过本次重连尝试");
                return _isConnected;
            }

            try
            {
                // 标记为重连中
                _isReconnecting = true;

                _logger?.LogInformation(string.Format($"开始Modbus自动重连 - IP: {0}, 端口: {1}, 最大尝试次数: {2}", _ipAddress, _port, _maxReconnectAttempts));

                // 尝试重连
                for (int attempt = 0; attempt < _maxReconnectAttempts; attempt++)
                {
                    try
                    {
                        // 断开现有连接
                        if (_isConnected)
                        {
                            Disconnect();
                        }

                        // 重新初始化客户端
                        InitializeModbusClient();

                        // 设置通信管道
                        _modbusClient.CommunicationPipe = new PipeTcpNet(_ipAddress, _port)
                        {
                            ConnectTimeOut = _connectTimeout,
                            ReceiveTimeOut = _receiveTimeout
                        };

                        // 尝试连接
                        var result = _modbusClient.ConnectServer();

                        if (result.IsSuccess)
                        {
                            _isConnected = true;
                            _logger?.LogInformation(string.Format($"Modbus自动重连成功 - IP: {0}, 端口: {1}, 尝试次数: {2}", _ipAddress, _port, attempt + 1));
                            return true;
                        }

                        _logger?.LogWarning(string.Format($"Modbus自动重连失败 - IP: {0}, 端口: {1}, 尝试次数: {2}, 错误: {3}", _ipAddress, _port, attempt + 1, result.Message));

                        // 如果不是最后一次尝试，则等待重连间隔
                        if (attempt < _maxReconnectAttempts - 1)
                        {
                            Thread.Sleep(_reconnectInterval);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(string.Format($"Modbus自动重连异常 - IP: {0}, 端口: {1}, 尝试次数: {2}, 错误: {3}", _ipAddress, _port, attempt + 1, ex.Message));
                        // 继续下一次尝试
                    }
                }

                // 所有尝试都失败，将连接状态设置为false
                _isConnected = false;
                // 所有尝试都失败
                _logger?.LogError(string.Format($"Modbus自动重连全部失败 - IP: {0}, 端口: {1}, 最大尝试次数: {2}", _ipAddress, _port, _maxReconnectAttempts));
                return false;
            }
            finally
            {
                _isReconnecting = false;
                Monitor.Exit(_reconnectLock);
            }
        }

        /// <summary>
        /// 验证连接状态并尝试重连
        /// </summary>
        private void ValidateConnection()
        {
            // 如果未连接且未在重连中
            if (!_isConnected && _autoReconnectEnabled && !_isReconnecting)
            {
                // 尝试重连
                TryReconnect();
            }
        }

        /// <summary>
        /// 写入布尔值 
        /// </summary>
        /// <param name="address">线圈地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, bool value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入布尔值异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }


        /// <summary>
        /// 读取UInt16类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<ushort> ReadUInt16(string address)
        {
            try
            {
                var result = _modbusClient.ReadUInt16(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取UInt16异常 - 地址: {0}", address), ex);
                return new OperateResult<ushort>(ex.Message);
            }
        }

        /// <summary>
        /// 读取Int16类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<short> ReadInt16(string address)
        {
            try
            {
                var result = _modbusClient.ReadInt16(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取Int16异常 - 地址: {0}", address), ex);
                return new OperateResult<short>(ex.Message);
            }
        }

        /// <summary>
        /// 读取UInt32类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<uint> ReadUInt32(string address)
        {
            try
            {
                var result = _modbusClient.ReadUInt32(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取UInt32异常 - 地址: {0}", address), ex);
                return new OperateResult<uint>(ex.Message);
            }
        }


        /// <summary>
        /// 读取Int32类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<int> ReadInt32(string address)
        {
            try
            {
                var result = _modbusClient.ReadInt32(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取Int32异常 - 地址: {0}", address), ex);
                return new OperateResult<int>(ex.Message);
            }
        }

        /// <summary>
        /// 读取UInt64类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<ulong> ReadUInt64(string address)
        {
            try
            {
                var result = _modbusClient.ReadUInt64(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取UInt64异常 - 地址: {0}", address), ex);
                return new OperateResult<ulong>(ex.Message);
            }
        }


        /// <summary>
        /// 读取Int32类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<long> ReadInt64(string address)
        {
            try
            {
                var result = _modbusClient.ReadInt64(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取Int64异常 - 地址: {0}", address), ex);
                return new OperateResult<long>(ex.Message);
            }
        }


        /// <summary>
        /// 读取Float类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<float> ReadFloat(string address)
        {
            try
            {
                var result = _modbusClient.ReadFloat(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取Float异常 - 地址: {0}", address), ex);
                return new OperateResult<float>(ex.Message);
            }
        }




        /// <summary>
        /// 读取Double类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<double> ReadDouble(string address)
        {
            try
            {
                var result = _modbusClient.ReadDouble(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取Double异常 - 地址: {0}", address), ex);
                return new OperateResult<double>(ex.Message);
            }
        }

        /// <summary>
        /// 读取字符串 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="length">字符串长度</param>
        /// <returns>读取结果</returns>
        public OperateResult<string> ReadString(string address, ushort length)
        {
            try
            {
                var result = _modbusClient.ReadString(address, length);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取字符串异常 - 地址: {0}, 长度: {1}", address, length), ex);
                return new OperateResult<string>(ex.Message);
            }
        }

        /// <summary>
        /// 读取布尔值 
        /// </summary>
        /// <param name="address">线圈地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<bool> ReadBool(string address)
        {
            try
            {
                var result = _modbusClient.ReadBool(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus读取布尔值异常 - 地址: {0}", address), ex);
                return new OperateResult<bool>(ex.Message);
            }
        }

        /// <summary>
        /// 批量读取Int16类型数据 
        /// </summary>
        /// <param name="startAddress">起始寄存器地址</param>
        /// <param name="length">读取长度</param>
        /// <returns>读取结果</returns>
        public OperateResult<short[]> ReadInt16Batch(string startAddress, ushort length)
        {
            try
            {
                var result = _modbusClient.ReadInt16(startAddress, length);
                if (!result.IsSuccess && !_isConnected && _autoReconnectEnabled && !_isReconnecting)
                {
                    _logger?.LogWarning(string.Format($"Modbus批量读取Int16失败 - 地址: {0}, 长度: {1}, 错误: {2}", startAddress, length, result.Message));
                    if (TryReconnect())
                    {
                        result = _modbusClient.ReadInt16(startAddress, length);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus批量读取Int16异常 - 地址: {0}, 长度: {1}", startAddress, length), ex);
                return new OperateResult<short[]>(ex.Message);
            }
        }

        /// <summary>
        /// 批量读取Float类型数据 
        /// </summary>
        /// <param name="startAddress">起始寄存器地址</param>
        /// <param name="length">读取长度</param>
        /// <returns>读取结果</returns>
        public OperateResult<float[]> ReadFloatBatch(string startAddress, ushort length)
        {
            try
            {
                var result = _modbusClient.ReadFloat(startAddress, length);
                if (!result.IsSuccess && !_isConnected && _autoReconnectEnabled && !_isReconnecting)
                {
                    _logger?.LogWarning(string.Format($"Modbus批量读取Float失败 - 地址: {0}, 长度: {1}, 错误: {2}", startAddress, length, result.Message));
                    if (TryReconnect())
                    {
                        result = _modbusClient.ReadFloat(startAddress, length);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus批量读取Float异常 - 地址: {0}, 长度: {1}", startAddress, length), ex);
                return new OperateResult<float[]>(ex.Message);
            }
        }

        /// <summary>
        /// 批量读取布尔值 
        /// </summary>
        /// <param name="startAddress">起始线圈地址</param>
        /// <param name="length">读取长度</param>
        /// <returns>读取结果</returns>
        public OperateResult<bool[]> ReadBoolBatch(string startAddress, ushort length)
        {
            try
            {
                var result = _modbusClient.ReadCoil(startAddress, length);
                if (!result.IsSuccess && !_isConnected && _autoReconnectEnabled && !_isReconnecting)
                {
                    _logger?.LogWarning(string.Format($"Modbus批量读取布尔值失败 - 地址: {0}, 长度: {1}, 错误: {2}", startAddress, length, result.Message));
                    if (TryReconnect())
                    {
                        result = _modbusClient.ReadCoil(startAddress, length);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus批量读取布尔值异常 - 地址: {0}, 长度: {1}", startAddress, length), ex);
                return new OperateResult<bool[]>(ex.Message);
            }
        }


        /// <summary>
        /// 写入UInt16类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, ushort value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入UInt16异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        /// <summary>
        /// 写入Int16类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, short value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入Int16异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        /// <summary>
        /// 写入UInt32类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, uint value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入UInt32异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        /// <summary>
        /// 写入Int32类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, int value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入Int32异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        /// <summary>
        /// 写入UInt64类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, ulong value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入UInt64异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        /// <summary>
        /// 写入Int64类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, long value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入Int64异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }


        /// <summary>
        /// 写入Float类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, float value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入Float异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }


        /// <summary>
        /// 写入Double类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, double value)
        {
            try
            {
                var result = _modbusClient.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入Double异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        /// <summary>
        /// 写入字符串 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的字符串</param>
        /// <returns>写入结果</returns>
        public OperateResult Write30(string address, string value)
        {
            try
            {
                var result = _modbusClient.Write(address, value, 30);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        public OperateResult Write50(string address, string value)
        {
            try
            {
                var result = _modbusClient.Write(address, value, 50);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        public OperateResult Write60(string address, string value)
        {
            try
            {
                var result = _modbusClient.Write(address, value, 60);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        public OperateResult Write100(string address, string value)
        {
            try
            {
                var result = _modbusClient.Write(address, value, 100);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"Modbus写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            if (_modbusClient != null)
            {
                _modbusClient = null;
            }
        }
    }
}

