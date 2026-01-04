using DeviceManage.Helpers;
using DeviceManage.Models;
using HslCommunication;
using HslCommunication.ModBus;
using HslCommunication.Profinet.LSIS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService.Impl
{
    public class SwitchRecipeService : ISwitchRecipeService
    {
        public readonly ILogger<SwitchRecipeService> _logger;

        public SwitchRecipeService(ILogger<SwitchRecipeService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SwitchRecipeTagAsync(Tag tag)
        {
            if (tag != null)
            {
                if (tag.PlcDevice.Protocolc.Contains("S7"))
                {
                    using (var s7Net = new S7NetConnect())
                    {
                        var connectResult = s7Net.Connect(tag.PlcDevice.IPAddress, tag.PlcDevice.Port);
                        if (!connectResult.IsSuccess)
                        {
                            _logger.LogError($"S7Net连接失败 - IP: {tag.PlcDevice.IPAddress}, 端口: {tag.PlcDevice.Port}, 错误: {connectResult.Message}");
                            return false;
                        }

                        try
                        {
                            // 遍历TagDetailDataArray，按照DataType写入PLC
                            if (tag.TagDetailDataArray != null)
                            {
                                foreach (var tagDetail in tag.TagDetailDataArray)
                                {
                                    var writeResult = WriteValueToPLC(s7Net, tagDetail.Address, tagDetail.Value, tagDetail.DataType);
                                    if (!writeResult.IsSuccess)
                                    {
                                        _logger.LogError($"写入PLC失败 - 地址: {tagDetail.Address}, 数据类型: {tagDetail.DataType}, 值: {tagDetail.Value}, 错误: {writeResult.Message}");
                                        return false;
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"成功写入PLC - 地址: {tagDetail.Address}, 数据类型: {tagDetail.DataType}, 值: {tagDetail.Value}");
                                    }
                                }
                            }
                        }
                        finally
                        {
                            s7Net.Disconnect();
                        }
                    }
                }
                else if (tag.PlcDevice.Protocolc.Contains("Modbus"))
                {
                    using (var modbusNet = new ModbusConnect())
                    {
                        var connectResult = modbusNet.Connect(tag.PlcDevice.IPAddress, tag.PlcDevice.Port);
                        if (!connectResult.IsSuccess)
                        {
                            _logger.LogError($"ModbusTcpNet连接失败 - IP: {tag.PlcDevice.IPAddress}, 端口: {tag.PlcDevice.Port}, 错误: {connectResult.Message}");
                            return false;
                        }

                        try
                        {
                            // 遍历TagDetailDataArray，按照DataType写入PLC
                            if (tag.TagDetailDataArray != null)
                            {
                                foreach (var tagDetail in tag.TagDetailDataArray)
                                {
                                    var writeResult = WriteValueToPLC(modbusNet, tagDetail.Address, tagDetail.Value, tagDetail.DataType);
                                    if (!writeResult.IsSuccess)
                                    {
                                        _logger.LogError($"写入PLC失败 - 地址: {tagDetail.Address}, 数据类型: {tagDetail.DataType}, 值: {tagDetail.Value}, 错误: {writeResult.Message}");
                                        return false;
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"成功写入PLC - 地址: {tagDetail.Address}, 数据类型: {tagDetail.DataType}, 值: {tagDetail.Value}");
                                    }
                                }
                            }
                        }
                        finally
                        {
                            modbusNet.Disconnect();
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"未实现{tag.PlcDevice.Protocolc}协议的配方切换功能 - IP: {tag.PlcDevice.IPAddress}, 端口: {tag.PlcDevice.Port}");
                }
            }
            else
            {
                _logger.LogWarning("传入的Tag为空，无法执行配方切换");
            }
            return true;
        }

        /// <summary>
        /// 根据数据类型将值写入PLC
        /// </summary>
        /// <param name="s7Net">S7Net连接对象</param>
        /// <param name="address">PLC地址</param>
        /// <param name="value">要写入的值</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>写入结果</returns>
        private OperateResult WriteValueToPLC(S7NetConnect s7Net, string address, string value, DataType dataType)
        {
            try
            {
                return dataType switch
                {
                    DataType.Bool => s7Net.Write(address, Convert.ToBoolean(value)),
                    DataType.Char => s7Net.Write(address, Convert.ToChar(value)),
                    DataType.Byte => s7Net.Write(address, Convert.ToByte(value)),
                    DataType.SByte => s7Net.Write(address, Convert.ToSByte(value)),
                    DataType.Int16 => s7Net.Write(address, Convert.ToInt16(value)),
                    DataType.UInt16 => s7Net.Write(address, Convert.ToUInt16(value)),
                    DataType.Int32 => s7Net.Write(address, Convert.ToInt32(value)),
                    DataType.UInt32 => s7Net.Write(address, Convert.ToUInt32(value)),
                    DataType.Int64 => s7Net.Write(address, Convert.ToInt64(value)),
                    DataType.UInt64 => s7Net.Write(address, Convert.ToUInt64(value)),
                    DataType.Float => s7Net.Write(address, Convert.ToSingle(value)),
                    DataType.Double => s7Net.Write(address, Convert.ToDouble(value)),
                    DataType.String30 => s7Net.Write30(address, value),
                    DataType.String50 => s7Net.Write50(address, value),
                    DataType.String60 => s7Net.Write60(address, value),
                    DataType.String100 => s7Net.Write100(address, value),
                    _ => new OperateResult($"不支持的数据类型: {dataType}")
                };
            }
            catch (Exception ex)
            {
                return new OperateResult($"转换或写入数据时发生异常: {ex.Message}");
            }
        }

        private OperateResult WriteValueToPLC(ModbusConnect modbus, string address, string value, DataType dataType)
        {
            try
            {
                return dataType switch
                {
                    DataType.Bool => modbus.Write(address, Convert.ToBoolean(value)),
                    DataType.Int16 => modbus.Write(address, Convert.ToInt16(value)),
                    DataType.UInt16 => modbus.Write(address, Convert.ToUInt16(value)),
                    DataType.Int32 => modbus.Write(address, Convert.ToInt32(value)),
                    DataType.UInt32 => modbus.Write(address, Convert.ToUInt32(value)),
                    DataType.Int64 => modbus.Write(address, Convert.ToInt64(value)),
                    DataType.UInt64 => modbus.Write(address, Convert.ToUInt64(value)),
                    DataType.Float => modbus.Write(address, Convert.ToSingle(value)),
                    DataType.Double => modbus.Write(address, Convert.ToDouble(value)),
                    DataType.String30 => modbus.Write30(address, value),
                    DataType.String50 => modbus.Write50(address, value),
                    DataType.String60 => modbus.Write60(address, value),
                    DataType.String100 => modbus.Write100(address, value),
                    _ => new OperateResult($"不支持的数据类型: {dataType}")
                };
            }
            catch (Exception ex)
            {
                return new OperateResult($"转换或写入数据时发生异常: {ex.Message}");
            }
        }
    }
}
