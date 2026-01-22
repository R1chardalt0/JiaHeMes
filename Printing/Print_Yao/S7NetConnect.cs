using HslCommunication;
using HslCommunication.Core.Pipe;
using HslCommunication.Profinet.Siemens;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JY_Print
{
    public class S7NetConnect : IDisposable
    {
        public readonly ILogger<S7NetConnect> _logger;

        private SiemensS7Net _s7net;

        private bool _isConnected = false;

        private string _ipAddress = string.Empty;
        private int _port = 102;
        private int _connectTimeout = 5000;
        private int _receiveTimeout = 10000;

        //重连
        private bool _autoReconnectEnabled = true; // 是否启用自动重连
        private int _maxReconnectAttempts = 3;     // 最大重连次数
        private int _reconnectInterval = 1000;     // 重连间隔(毫秒)

        // 防止重入锁
        private object _reconnectLock = new object();
        private bool _isReconnecting = false;

        // 获取连接状态
        public bool IsConnect => _isConnected;

        /// <summary>
        /// 构造函数
        /// </summary>
        public S7NetConnect(ILogger<S7NetConnect> logger)
        {
            InitializeS7NetClient();
            _logger = logger;
        }

        public S7NetConnect()
        {
            InitializeS7NetClient();
        }

        private void InitializeS7NetClient()
        {
            _s7net = new SiemensS7Net(SiemensPLCS.S1200)
            {
                Rack = 0,
                Slot = 0
            };
        }

        /// <summary>
        /// 连接到S7Net服务器
        /// </summary>
        /// <param name="ipAddress">服务器IP地址</param>
        /// <param name="port">端口号，默认102</param>
        /// <param name="connectTimeout">连接超时时间，单位毫秒，默认5000</param>
        /// <param name="receiveTimeout">接收超时时间，单位毫秒，默认10000</param>
        public OperateResult Connect(string ipAddress, int port = 102, int connectTimeout = 5000, int receiveTimeout = 10000)
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
                InitializeS7NetClient();

                // 设置通信管道
                _s7net.CommunicationPipe = new PipeTcpNet(_ipAddress, _port)
                {
                    ConnectTimeOut = _connectTimeout,
                    ReceiveTimeOut = _receiveTimeout
                };

                // 连接服务器
                var result = _s7net.ConnectServer();
                if (result.IsSuccess)
                {
                    _isConnected = true;
                    _logger?.LogInformation(string.Format($"S7Net连接成功 - IP: {0}, 端口: {1}", _ipAddress, _port));
                }
                else
                {
                    _logger?.LogError(string.Format($"S7Net连接失败 - IP: {0}, 端口: {1}, 错误: {2}", _ipAddress, _port, result.Message));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net连接异常 - IP: {0}, 端口: {1}", _ipAddress, _port), ex);
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
                if (_isConnected && _s7net != null)
                {
                    _s7net.ConnectClose();
                    _isConnected = false;
                    _logger?.LogInformation(string.Format($"S7Net已断开连接 - IP: {0}, 端口: {1}", _ipAddress, _port));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"S7Net断开连接异常", ex);
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

                _logger?.LogInformation(string.Format($"开始S7Net自动重连 - IP: {0}, 端口: {1}, 最大尝试次数: {2}", _ipAddress, _port, _maxReconnectAttempts));

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
                        InitializeS7NetClient();

                        // 设置通信管道
                        _s7net.CommunicationPipe = new PipeTcpNet(_ipAddress, _port)
                        {
                            ConnectTimeOut = _connectTimeout,
                            ReceiveTimeOut = _receiveTimeout
                        };

                        // 尝试连接
                        var result = _s7net.ConnectServer();

                        if (result.IsSuccess)
                        {
                            _isConnected = true;
                            _logger?.LogInformation(string.Format($"S7Net自动重连成功 - IP: {0}, 端口: {1}, 尝试次数: {2}", _ipAddress, _port, attempt + 1));
                            return true;
                        }

                        _logger?.LogWarning(string.Format($"S7Net自动重连失败 - IP: {0}, 端口: {1}, 尝试次数: {2}, 错误: {3}", _ipAddress, _port, attempt + 1, result.Message));

                        // 如果不是最后一次尝试，则等待重连间隔
                        if (attempt < _maxReconnectAttempts - 1)
                        {
                            Thread.Sleep(_reconnectInterval);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(string.Format($"S7Net自动重连异常 - IP: {0}, 端口: {1}, 尝试次数: {2}, 错误: {3}", _ipAddress, _port, attempt + 1, ex.Message));
                        // 继续下一次尝试
                    }
                }

                // 所有尝试都失败，将连接状态设置为false
                _isConnected = false;
                // 所有尝试都失败
                _logger?.LogError(string.Format($"S7Net自动重连全部失败 - IP: {0}, 端口: {1}, 最大尝试次数: {2}", _ipAddress, _port, _maxReconnectAttempts));
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入布尔值异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }


        /// <summary>
        /// 读取byte类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <returns>读取结果</returns>
        public OperateResult<byte> ReadByte(string address)
        {
            try
            {
                var result = _s7net.ReadByte(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取byte异常 - 地址: {0}", address), ex);
                return new OperateResult<byte>(ex.Message);
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
                var result = _s7net.ReadUInt16(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取UInt16异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadInt16(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取Int16异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadUInt32(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取UInt32异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadInt32(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取Int32异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadUInt64(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取UInt64异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadInt64(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取Int64异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadFloat(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取Float异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadDouble(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取Double异常 - 地址: {0}", address), ex);
                return new OperateResult<double>(ex.Message);
            }
        }

        /// <summary>
        /// 读取字符串 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="length">字符串长度</param>
        /// <returns>读取结果</returns>
        //public OperateResult<string> ReadString(string address, ushort length)
        //{
        //    try
        //    {
        //        var result = _s7net.ReadString(address, length, Encoding.ASCII);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(string.Format($"S7Net读取字符串异常 - 地址: {0}, 长度: {1}", address, length), ex);
        //        return new OperateResult<string>(ex.Message);
        //    }
        //}
        public string ReadString(string address, ushort length)
        {
            try
            {
                var value = _s7net.ReadString(address, length).Content;
                string cleanedValue = Regex.Replace(value, @"[\u0000-\u001F]", "");
                cleanedValue = Regex.Unescape(cleanedValue);
                cleanedValue = cleanedValue.Trim();
                return cleanedValue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取字符串异常 - 地址: {0}, 长度: {1}", address, length), ex);
                return $"{ex.Message}";
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
                var result = _s7net.ReadBool(address);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net读取布尔值异常 - 地址: {0}", address), ex);
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
                var result = _s7net.ReadInt16(startAddress, length);
                if (!result.IsSuccess && !_isConnected && _autoReconnectEnabled && !_isReconnecting)
                {
                    _logger?.LogWarning(string.Format($"S7Net批量读取Int16失败 - 地址: {0}, 长度: {1}, 错误: {2}", startAddress, length, result.Message));
                    if (TryReconnect())
                    {
                        result = _s7net.ReadInt16(startAddress, length);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net批量读取Int16异常 - 地址: {0}, 长度: {1}", startAddress, length), ex);
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
                var result = _s7net.ReadFloat(startAddress, length);
                if (!result.IsSuccess && !_isConnected && _autoReconnectEnabled && !_isReconnecting)
                {
                    _logger?.LogWarning(string.Format($"S7Net批量读取Float失败 - 地址: {0}, 长度: {1}, 错误: {2}", startAddress, length, result.Message));
                    if (TryReconnect())
                    {
                        result = _s7net.ReadFloat(startAddress, length);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net批量读取Float异常 - 地址: {0}, 长度: {1}", startAddress, length), ex);
                return new OperateResult<float[]>(ex.Message);
            }
        }


        /// <summary>
        /// 写入byte类型数据 
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入结果</returns>
        public OperateResult Write(string address, byte value)
        {
            try
            {
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入byte异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入UInt16异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入Int16异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入UInt32异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入Int32异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入UInt64异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入Int64异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入Float异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入Double异常 - 地址: {0}, 值: {1}", address, value), ex);
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
                var result = _s7net.Write(address, value, 30);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        public OperateResult Write50(string address, string value)
        {
            try
            {
                var result = _s7net.Write(address, value, 50);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        public OperateResult Write60(string address, string value)
        {
            try
            {
                var result = _s7net.Write(address, value, 60);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }

        public OperateResult Write100(string address, string value)
        {
            try
            {
                var result = _s7net.Write(address, value, 60);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(string.Format($"S7Net写入字符串异常 - 地址: {0}, 值: {1}", address, value), ex);
                return new OperateResult(ex.Message);
            }
        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            if (_s7net != null)
            {
                _s7net = null;
            }
        }
    }
}
