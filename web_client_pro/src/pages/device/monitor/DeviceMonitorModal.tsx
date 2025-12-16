import React, { useEffect, useState, useRef } from 'react';
import { Button, Modal, Spin, Card as AntCard, Row, Col, Statistic, Descriptions, Tag, Select, message } from 'antd';
import type { DeviceDetailType, EquipmentTraceData, Parameter } from '../../../services/Model/Device/device_monitor_data';
import { Card } from 'antd';
import { Line } from '@ant-design/plots';
import { getEquipmentTracinfosListByDeviceEnCode } from '../../../services/Api/Device/Monitor/deviceMonitorService';
import { PlusOutlined, MinusOutlined, ReloadOutlined, InfoCircleOutlined, DragOutlined } from '@ant-design/icons';

interface DeviceMonitorModalProps {
  visible: boolean;
  device: DeviceDetailType | undefined;
  onClose: () => void;
}

/**
 * 设备监控数据弹窗组件
 * 用于显示指定设备的详细监控数据
 */
const DeviceMonitorModal: React.FC<DeviceMonitorModalProps> = ({
  visible,
  device,
  onClose
}) => {
  const [data, setData] = useState<EquipmentTraceData[]>([]);
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(false); // 仅用于初始加载
  const [latestParams, setLatestParams] = useState<Record<string, { value: number, unit: string }>>({});
  const [tempParams, setTempParams] = useState<string[]>([]);
  const [latestSendTime, setLatestSendTime] = useState<string>('');
  const [latestAlarmMessages, setLatestAlarmMessages] = useState<string>('');
  const [refreshInterval, setRefreshInterval] = useState<number>(30); // 默认30秒刷新一次
  const [isAutoRefreshing, setIsAutoRefreshing] = useState(false); // 自动刷新状态指示器
  const [dataSize, setDataSize] = useState<number>(100); // 查询数据条数，默认为100条
  const refreshTimerRef = useRef<NodeJS.Timeout | null>(null);
  const dataUpdateRef = useRef<number>(0); // 用于触发图表更新但不引起整个组件重渲染
  const [showDeviceInfo, setShowDeviceInfo] = useState(false); // 控制设备详情弹窗显示
  const [activePoint, setActivePoint] = useState<number | null>(null); // 当前选中的设备关键点
  // 图片操作相关状态
  const [zoomLevel, setZoomLevel] = useState(1); // 缩放级别，默认为1
  const [panOffset, setPanOffset] = useState({ x: 0, y: 0 }); // 平移偏移量
  const [isDragging, setIsDragging] = useState(false); // 是否正在拖拽
  const [isPanning, setIsPanning] = useState(false); // 是否处于移动模式
  const [startPos, setStartPos] = useState({ x: 0, y: 0 }); // 拖拽开始位置
  const [imageFit, setImageFit] = useState<'fill' | 'contain' | 'cover' | 'none' | 'scale-down'>('contain'); // 图片填充模式
  const [imageLoadError, setImageLoadError] = useState(false); // 图片加载失败状态

  // 处理图片拖动开始
  const handleMouseDown = (e: React.MouseEvent) => {
    if (!isPanning) return;
    e.preventDefault();
    setIsDragging(true);
    setStartPos({ x: e.clientX - panOffset.x, y: e.clientY - panOffset.y });
  };

  // 处理图片拖动中
  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging) return;
    setPanOffset({ x: e.clientX - startPos.x, y: e.clientY - startPos.y });
  };

  // 处理图片拖动结束
  const handleMouseUp = () => {
    setIsDragging(false);
  };

  // 处理图片拖动离开
  const handleMouseLeave = () => {
    setIsDragging(false);
  };

  // 刷新时间选项
  const refreshOptions = [
    { value: 5, label: '5秒' },
    { value: 10, label: '10秒' },
    { value: 30, label: '30秒' },
    { value: 60, label: '1分钟' },
    { value: 300, label: '5分钟' },
    { value: 1800, label: '30分钟' },
    { value: 0, label: '关闭自动刷新' },
  ];

  // 数据条数选项
  const dataSizeOptions = [
    { value: 50, label: '50条' },
    { value: 100, label: '100条' },
    { value: 200, label: '200条' },
    { value: 500, label: '500条' },
    { value: 1000, label: '1000条' },
  ];

  useEffect(() => {
    if (visible && device) {
      setInitialLoading(true);
      setImageLoadError(false); // 重置图片加载错误状态
      fetchEquipmentData(true).finally(() => {
        setInitialLoading(false);
      });
    }
  }, [visible, device]);

  // 设置自动刷新定时器
  useEffect(() => {
    if (visible && device && refreshInterval > 0) {
      // 清除之前的定时器
      if (refreshTimerRef.current) {
        clearInterval(refreshTimerRef.current);
      }

      // 创建新的定时器
      refreshTimerRef.current = setInterval(() => {
        fetchEquipmentData(false);
      }, refreshInterval * 1000);
    } else if (refreshTimerRef.current) {
      // 如果不需要自动刷新，清除定时器
      clearInterval(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }

    // 组件卸载时清除定时器
    return () => {
      if (refreshTimerRef.current) {
        clearInterval(refreshTimerRef.current);
        refreshTimerRef.current = null;
      }
    };
  }, [visible, device, refreshInterval, dataSize]);

  useEffect(() => {
    if (data.length > 0) {
      // 获取最新的参数值
      const latestData = data[data.length - 1];
      const paramsRecord: Record<string, { value: number, unit: string }> = {};
      const tempParamsList: string[] = [];

      latestData.parameters.forEach(param => {
        if (param.type === 0 && param.value && !isNaN(parseFloat(param.value))) {
          paramsRecord[param.name] = {
            value: parseFloat(param.value),
            unit: param.unit || ''
          };

          // 收集以温度结尾的参数名称
          if (param.name.endsWith('温度')) {
            tempParamsList.push(param.name);
          }
        }
      });

      setLatestParams(paramsRecord);
      setTempParams(tempParamsList);
      setLatestSendTime(latestData.sendTime || '');
      setLatestAlarmMessages(latestData.alarmMessages || '');
    }
  }, [data]);

  const fetchEquipmentData = async (isInitialLoad: boolean = false) => {
    if (!device) return;

    // 仅在初始加载时显示加载状态
    if (isInitialLoad) {
      setLoading(true);
    } else {
      // 自动刷新时显示刷新状态但不显示全屏加载
      setIsAutoRefreshing(true);
    }

    try {
      const result = await getEquipmentTracinfosListByDeviceEnCode({
        deviceEnCode: device.deviceEnCode,
        size: dataSize, // 使用设置的数据条数
      });

      if (result.success && result.data) {
        // 按sendTime从小到大排序
        const sortedData = [...result.data].sort((a, b) => {
          const timeA = new Date(a.sendTime || 0).getTime();
          const timeB = new Date(b.sendTime || 0).getTime();
          return timeA - timeB;
        });

        // 优化：如果是自动刷新且数据变化不大，可以考虑增量更新而不是完全替换
        // 这里先简单处理，但保留了优化空间
        setData(prevData => {
          // 如果是第一次加载或是数据量有显著变化，则完全替换
          if (isInitialLoad || Math.abs(prevData.length - sortedData.length) > 10) {
            return sortedData;
          }

          // 否则，只更新最新的数据点（假设数据是按时间顺序返回的）
          const latestTimeInPrev = prevData.length > 0 ?
            new Date(prevData[prevData.length - 1].sendTime || 0).getTime() : 0;

          const newDataPoints = sortedData.filter(item =>
            new Date(item.sendTime || 0).getTime() > latestTimeInPrev
          );

          if (newDataPoints.length > 0) {
            // 添加新数据点并保持最大数据量为设置的值
            const updatedData = [...prevData, ...newDataPoints];
            return updatedData.slice(-dataSize);
          }

          return prevData; // 没有新数据，保持原数据不变
        });

        // 触发数据更新标记，用于强制图表刷新
        dataUpdateRef.current += 1;
      } else {
        console.error('获取设备数据失败:', result.errorMessage || '未知错误');
      }
    } catch (error) {
      console.error('获取设备数据异常:', error);
    } finally {
      if (isInitialLoad) {
        setLoading(false);
      } else {
        // 自动刷新状态延迟清除，给用户视觉反馈但不影响体验
        setTimeout(() => setIsAutoRefreshing(false), 500);
      }
    }
  };

  // 处理数据，将多参数转换为@ant-design/plots Line组件支持的格式
  const processChartData = () => {
    const chartData: any[] = [];

    // 时间格式化函数 - 将时间字符串转换为Date对象
    const parseTime = (timeString: string) => {
      try {
        return new Date(timeString);
      } catch {
        return new Date(); // 如果时间解析失败，返回当前时间
      }
    };

    data.forEach((traceData) => {
      traceData.parameters.forEach((param) => {
        // 只处理数字类型的参数，且是温度参数
        if (param.type === 0 && param.name.endsWith('温度')) {
          // 修复数据值为null的问题：添加验证和默认值处理
          let numericValue = param.value && !isNaN(parseFloat(param.value)) ? parseFloat(param.value) : 0;

          chartData.push({
            'Date': parseTime(traceData.sendTime),
            value: numericValue,
            series: param.name,
          });
        }
      });
    });

    // 按原始时间从小到大排序
    chartData.sort((a, b) => {
      return new Date(a['Date'] || 0).getTime() - new Date(b['Date'] || 0).getTime();
    });

    return chartData;
  };

  // 格式化日期时间 - 显示年月日时分秒
  const formatDateTime = (timeString: string) => {
    try {
      const date = new Date(timeString);
      const year = date.getFullYear();
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const day = date.getDate().toString().padStart(2, '0');
      const hours = date.getHours().toString().padStart(2, '0');
      const minutes = date.getMinutes().toString().padStart(2, '0');
      const seconds = date.getSeconds().toString().padStart(2, '0');
      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    } catch {
      return timeString; // 如果时间解析失败，返回原始字符串
    }
  };

  // 生成图表配置
  const generateChartConfig = () => {
    const chartData = processChartData();

    return {
      interaction: {
        brushFilter: true,
      },
      colorField: 'series',
      yField: 'value',
      xField: (d: { [x: string]: string | number | Date; }) => new Date(d['Date']),
      axis: {
        x: {
          labelAutoRotate: false,
          label: {
            formatter: (text: string) => {
              // 优化时间显示格式，只显示时分
              try {
                const date = new Date(text);
                const hours = date.getHours().toString().padStart(2, '0');
                const minutes = date.getMinutes().toString().padStart(2, '0');
                return `${hours}:${minutes}`;
              } catch {
                return text;
              }
            },
            style: {
              fontSize: 12,
            },
          },
          // 设置tickCount来控制显示的标签数量
          tickCount: 10,
          // 允许标签溢出容器，避免被截断
          allowLabelOverlap: false,
        },
      },
      data: chartData,
    };
  };

  // 渲染参数监控区域
  const renderParameterMonitor = () => {
    if (initialLoading || data.length === 0) return null;

    // 按类型分组参数
    const paramGroups: {
      drying: Array<{ name: string, value: number, unit: string }>;
      degreasing: Array<{ name: string, value: number, unit: string }>;
      preheating: Array<{ name: string, value: number, unit: string }>;
      soldering: Array<{ name: string, value: number, unit: string }>;
      other: Array<{ name: string, value: number, unit: string }>;
    } = {
      drying: [],
      degreasing: [],
      preheating: [],
      soldering: [],
      other: [],
    };

    Object.entries(latestParams).forEach(([name, { value, unit }]) => {
      if (name.includes('烘干')) {
        paramGroups.drying.push({ name, value, unit });
      } else if (name.includes('脱脂')) {
        paramGroups.degreasing.push({ name, value, unit });
      } else if (name.includes('预热')) {
        paramGroups.preheating.push({ name, value, unit });
      } else if (name.includes('钎焊')) {
        paramGroups.soldering.push({ name, value, unit });
      } else {
        paramGroups.other.push({ name, value, unit });
      }
    });

    // 计算需要显示的组数
    const groupsToShow = Object.entries(paramGroups)
      .filter(([_, params]) => params.length > 0)
      .map(([key, params]) => ({ key, params }));

    if (groupsToShow.length === 0) return null;

    return (
      <div style={{ marginBottom: '20px' }}>
        {latestSendTime && (
          <div style={{ marginBottom: '12px', color: '#666', fontSize: '14px', display: 'flex', alignItems: 'center' }}>
            最新数据时间: {formatDateTime(latestSendTime)}
            {isAutoRefreshing && (
              <span style={{ marginLeft: '10px', color: '#1890FF', fontSize: '12px' }}>（自动刷新中...）</span>
            )}
          </div>
        )}
        {groupsToShow.map((group, idx) => (
          <AntCard key={idx} style={{ marginBottom: '12px', padding: '8px' }}>
            <Row gutter={16}>
              {group.params.map((param, paramIdx) => (
                <Col key={paramIdx} span={6}>
                  <Statistic
                    title={param.name}
                    value={param.value}
                    precision={1}
                    suffix={param.unit}
                    valueStyle={{ color: param.name.includes('温度') ? '#1890FF' : '#30BF78' }}
                  />
                </Col>
              ))}
            </Row>
          </AntCard>
        ))}
      </div>
    );
  };

  return (
    <Modal
      title="设备监控数据"
      open={visible}
      onCancel={onClose}
      footer={[
        <Button key="refresh" onClick={() => fetchEquipmentData(false)} loading={isAutoRefreshing}>
          {isAutoRefreshing ? '刷新中...' : '刷新数据'}
        </Button>,
        <span key="interval" style={{ marginRight: '10px', lineHeight: '32px', padding: '0 10px' }}>
          自动刷新:
        </span>,
        <Select
          key="select"
          value={refreshInterval}
          onChange={setRefreshInterval}
          style={{ width: 120, marginRight: '10px' }}
          options={refreshOptions}
          disabled={isAutoRefreshing}
        />,
        <span key="dataSizeLabel" style={{ marginRight: '10px', lineHeight: '32px', padding: '0 10px' }}>
          数据条数:
        </span>,
        <Select
          key="dataSizeSelect"
          value={dataSize}
          onChange={setDataSize}
          style={{ width: 120, marginRight: '10px' }}
          options={dataSizeOptions}
          disabled={isAutoRefreshing}
        />,
        <Button key="close" onClick={onClose}>
          关闭
        </Button>,
      ]}
      width={2000}
    >
      {device && (
        <div style={{ padding: '20px' }}>
          <h3 hidden>设备ID: {device.deviceId}</h3>
          <Descriptions style={{ padding: '20px' }} title="设备信息" column={3}>
            <Descriptions.Item label="设备名称">{device.deviceName}</Descriptions.Item>
            <Descriptions.Item label="设备编码">{device.deviceEnCode}</Descriptions.Item>
            <Descriptions.Item label="设备状态">
              {latestAlarmMessages ? (
                <Tag color="red">异常</Tag>
              ) : (
                <Tag color="green">正常运行</Tag>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="设备类型">{device.deviceType || '未知'}</Descriptions.Item>
            <Descriptions.Item label="制造商">{device.deviceManufacturer || '未知'}</Descriptions.Item>
            <Descriptions.Item label="创建时间">{device.createTime ? formatDateTime(device.createTime) : '未知'}</Descriptions.Item>
            <Descriptions.Item label="报警信息" span={3}>
              {latestAlarmMessages ? (
                <div style={{ color: '#ff4d4f' }}>
                  {latestAlarmMessages}
                </div>
              ) : (
                '无报警信息'
              )}
            </Descriptions.Item>
          </Descriptions>

          {/* 二维图显示区域 */}
          <Card style={{ marginTop: '20px', marginBottom: '20px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
              <h3 style={{ margin: 0 }}>设备二维图</h3>
              <div style={{ display: device.devicePicture ? 'block' : 'none' }}>
                <Button
                  icon={<PlusOutlined />}
                  size="small"
                  style={{ marginRight: '8px' }}
                  onClick={() => {
                    setZoomLevel(prev => Math.min(prev + 0.1, 3));
                  }}
                >
                  放大
                </Button>
                <Button
                  icon={<MinusOutlined />}
                  size="small"
                  style={{ marginRight: '8px' }}
                  onClick={() => {
                    setZoomLevel(prev => Math.max(prev - 0.1, 0.5));
                  }}
                >
                  缩小
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  size="small"
                  style={{ marginRight: '8px' }}
                  onClick={() => {
                    setZoomLevel(1);
                    setPanOffset({ x: 0, y: 0 });
                    setIsDragging(false);
                    setIsPanning(false);
                  }}
                >
                  重置
                </Button>
                <Button
                  icon={<DragOutlined />}
                  size="small"
                  style={{ marginRight: '8px' }}
                  type={isPanning ? 'primary' : 'default'}
                  onClick={() => {
                    setIsPanning(!isPanning);
                  }}
                >
                  移动模式
                </Button>
                <Select
                  size="small"
                  value={imageFit}
                  onChange={setImageFit}
                  style={{ width: 100, marginRight: '8px' }}
                  options={[
                    { value: 'contain', label: '适应' },
                    { value: 'cover', label: '覆盖' },
                    { value: 'fill', label: '填充' },
                    { value: 'none', label: '原样' },
                    { value: 'scale-down', label: '缩放适应' }
                  ]}
                />
                <Button
                  icon={<InfoCircleOutlined />}
                  size="small"
                  type="primary"
                  onClick={() => {
                    setShowDeviceInfo(true);
                  }}
                >
                  设备详情
                </Button>
              </div>
            </div>

            {/* 二维图容器 */}
            <div
              style={{
                width: '100%',
                height: '300px',
                backgroundColor: '#f9f9f9',
                border: '1px solid #d9d9d9',
                borderRadius: '4px',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              {/* 使用设备实际图片 */}
              {device.devicePicture && !imageLoadError ? (
                <img
                  src={device.devicePicture}
                  alt={`${device.deviceName}二维图`}
                  style={{
                    width: imageFit === 'none' || imageFit === 'scale-down' ? 'auto' : '100%',
                    height: imageFit === 'none' || imageFit === 'scale-down' ? 'auto' : '100%',
                    objectFit: imageFit,
                    transform: `translate(${panOffset.x}px, ${panOffset.y}px) scale(${zoomLevel})`,
                    cursor: isPanning ? 'grab' : 'default',
                    transition: isDragging ? 'none' : 'transform 0.2s ease',
                    maxWidth: 'none',
                    maxHeight: 'none'
                  }}
                  onMouseDown={handleMouseDown}
                  onMouseMove={handleMouseMove}
                  onMouseUp={handleMouseUp}
                  onMouseLeave={handleMouseLeave}
                  onError={(e) => {
                    console.warn('设备图片加载失败，使用默认示意图', e);
                    // 如果图片加载失败，设置错误状态并显示默认示意图
                    setImageLoadError(true);
                    const target = e.target as HTMLImageElement;
                    if (target) {
                      target.style.display = 'none';
                    }
                  }}
                  onLoad={() => {
                    // 图片加载成功时，清除错误状态
                    setImageLoadError(false);
                  }}
                />
              ) : null}
              {/* 设备关键点 - 这些点可以点击显示信息 */}
              {/* {[1, 2, 3, 4].map((point) => (
                <div
                  key={point}
                  style={{
                    position: 'absolute',
                    width: '20px',
                    height: '20px',
                    backgroundColor: '#ff4d4f',
                    borderRadius: '50%',
                    cursor: 'pointer',
                    left: `${25 + (point - 1) * 16}%`,
                    top: point % 2 === 0 ? '80%' : '20%',
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    color: 'white',
                    fontSize: '12px',
                    boxShadow: '0 0 10px rgba(255, 77, 79, 0.5)'
                  }}
                  onClick={() => setActivePoint(point)}
                >
                  {point}
                </div>
              ))} */}
              {/* 默认示意图（当没有图片或图片加载失败时显示） */}
              <div style={{
                width: '100%',
                height: '100%',
                display: (device.devicePicture && !imageLoadError) ? 'none' : 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                position: 'absolute',
                top: 0,
                left: 0,
                backgroundColor: '#f9f9f9'
              }}>
                <div style={{
                  width: '80%',
                  height: '80%',
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  position: 'relative'
                }}>
                  {/* 设备主体 */}
                  <div style={{
                    width: '200px',
                    height: '150px',
                    backgroundColor: '#e6f7ff',
                    border: '2px solid #1890ff',
                    borderRadius: '8px',
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    fontSize: '16px',
                    fontWeight: 'bold',
                    color: '#1890ff'
                  }}>
                    {device.deviceName || '设备示意图'}

                    {/* 设备关键点 - 这些点可以点击显示信息 */}
                    {[1, 2, 3, 4].map((point) => (
                      <div
                        key={point}
                        style={{
                          position: 'absolute',
                          width: '20px',
                          height: '20px',
                          backgroundColor: '#ff4d4f',
                          borderRadius: '50%',
                          cursor: 'pointer',
                          left: `${25 + (point - 1) * 16}%`,
                          top: point % 2 === 0 ? '80%' : '20%',
                          display: 'flex',
                          justifyContent: 'center',
                          alignItems: 'center',
                          color: 'white',
                          fontSize: '12px',
                          boxShadow: '0 0 10px rgba(255, 77, 79, 0.5)'
                        }}
                        onClick={() => setActivePoint(point)}
                      >
                        {point}
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          </Card>


          {/* 参数监控区域 */}
          {renderParameterMonitor()}

          <Card style={{ marginTop: '20px', padding: '16px', backgroundColor: '#f0f2f5', borderRadius: '4px' }}>
            {initialLoading ? (
              <div style={{ textAlign: 'center', padding: '50px 0' }}>
                <Spin size="large" tip="加载中..." />
              </div>
            ) : data.length > 0 ? (
              <Line key={dataUpdateRef.current} {...generateChartConfig()} />
            ) : (
              <div style={{ textAlign: 'center', padding: '50px 0', color: '#999' }}>
                暂无数据
              </div>
            )}
          </Card>

          {/* 设备详细信息弹窗 */}
          <Modal
            title="设备详细信息"
            open={showDeviceInfo}
            onCancel={() => setShowDeviceInfo(false)}
            footer={[
              <Button key="close" onClick={() => setShowDeviceInfo(false)}>
                关闭
              </Button>
            ]}
            width={600}
          >
            {device ? (
              <Descriptions column={2}>
                <Descriptions.Item label="设备ID">{device.deviceId || '未知'}</Descriptions.Item>
                <Descriptions.Item label="生产线ID">{device.productionLineId || '未知'}</Descriptions.Item>
                <Descriptions.Item label="设备名称">{device.deviceName || '未知'}</Descriptions.Item>
                <Descriptions.Item label="设备编码">{device.deviceEnCode || '未知'}</Descriptions.Item>
                <Descriptions.Item label="设备类型">{device.deviceType || '未知'}</Descriptions.Item>
                <Descriptions.Item label="制造商">{device.deviceManufacturer || '未知'}</Descriptions.Item>
                <Descriptions.Item label="设备状态">
                  {latestAlarmMessages ? (
                    <Tag color="red">异常</Tag>
                  ) : (
                    <Tag color="green">正常运行</Tag>
                  )}
                </Descriptions.Item>
                <Descriptions.Item label="最新数据时间">
                  {latestSendTime ? formatDateTime(latestSendTime) : '暂无数据'}
                </Descriptions.Item>
                <Descriptions.Item label="创建时间">{device.createTime ? formatDateTime(device.createTime) : '未知'}</Descriptions.Item>
                <Descriptions.Item label="更新时间">{device.updateTime ? formatDateTime(device.updateTime) : '未知'}</Descriptions.Item>
                <Descriptions.Item label="参数数量" span={2}>
                  {Object.keys(latestParams).length} 个
                </Descriptions.Item>
                <Descriptions.Item label="备注" span={2}>
                  {device.description || '无备注信息'}
                </Descriptions.Item>
              </Descriptions>
            ) : (
              <div style={{ textAlign: 'center', padding: '20px 0', color: '#999' }}>
                设备信息加载中...
              </div>
            )}
          </Modal>

          {/* 点信息显示框 */}
          {activePoint && (
            <Modal
              title={`设备关键点 #${activePoint} 信息`}
              open={!!activePoint}
              onCancel={() => setActivePoint(null)}
              footer={[
                <Button key="close" onClick={() => setActivePoint(null)}>
                  关闭
                </Button>
              ]}
              width={400}
            >
              <div style={{ lineHeight: '1.8' }}>
                <p><strong>关键点编号：</strong>{activePoint}</p>
                <p><strong>设备名称：</strong>{device.deviceName}</p>
                <p><strong>设备编码：</strong>{device.deviceEnCode}</p>
                <p><strong>当前状态：</strong>
                  {latestAlarmMessages ? (
                    <span style={{ color: '#ff4d4f' }}>异常</span>
                  ) : (
                    <span style={{ color: '#52c41a' }}>正常</span>
                  )}
                </p>
                <p><strong>相关参数：</strong>
                  {activePoint === 1 ? '烘干温度、湿度' :
                    activePoint === 2 ? '脱脂时间、浓度' :
                      activePoint === 3 ? '预热温度、速度' :
                        activePoint === 4 ? '钎焊温度、压力' : '未知参数'}
                </p>
              </div>
            </Modal>
          )}
        </div>
      )}
    </Modal>
  );
};

export default DeviceMonitorModal;