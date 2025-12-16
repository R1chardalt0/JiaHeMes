import React, { useState, useEffect, useRef, useMemo } from 'react';
import { useParams, useNavigate, useLocation } from '@umijs/max';
import { 
  Card, 
  List, 
  Input, 
  Select, 
  Row, 
  Col, 
  Descriptions, 
  Tag, 
  Spin,
  Empty,
  message,
  Pagination,
  Button,
  Space,
  Radio
} from 'antd';
import { theme } from 'antd';
import { ReloadOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-components';
import { Line } from '@ant-design/plots';
import type { DeviceInfo, DeviceInfoQueryParams } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';
import { getDeviceInfoList } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';

// 设备状态映射
const statusMap = {
  '0': { text: '禁用', status: 'Default' },
  '1': { text: '启用', status: 'Success' },
};
import { getProductionLineList } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';
import { getEquipmentTracinfosListByDeviceEnCode } from './service';
import type { EquipmentTraceData, Parameter } from './data.d';
import useStyles from './style.style';
import { inherits } from 'util';

const { Search } = Input;

const DeviceMonitorPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { styles } = useStyles();
  const { token } = theme.useToken();
  
  // 从 URL 路径中提取设备类型参数
  // URL 格式: /devicechart/monitor/催化炉
  const extractDeviceTypeFromPath = (pathname: string): string | undefined => {
    const match = pathname.match(/\/devicechart\/monitor\/(.+)$/);
    if (match && match[1]) {
      try {
        // 解码 URL 编码的设备类型
        return decodeURIComponent(match[1]);
      } catch (e) {
        console.error('解码设备类型失败:', e);
        return match[1]; // 如果解码失败，返回原始值
      }
    }
    return undefined;
  };
  
  // 获取设备类型（优先从 useParams，如果失败则从路径解析）
  const { deviceType: deviceTypeParam } = useParams<{ deviceType: string }>();
  const deviceTypeFromParams = deviceTypeParam ? decodeURIComponent(deviceTypeParam) : undefined;
  const deviceTypeFromPath = extractDeviceTypeFromPath(location.pathname);
  const deviceType = deviceTypeFromParams || deviceTypeFromPath;
  
  // 调试日志：检查设备类型获取情况
  useEffect(() => {
    console.log('🔍 设备类型参数获取:', {
      pathname: location.pathname,
      deviceTypeFromParams,
      deviceTypeFromPath,
      finalDeviceType: deviceType,
    });
  }, [location.pathname, deviceTypeFromParams, deviceTypeFromPath, deviceType]);
  
  // 设备列表相关状态
  const [deviceList, setDeviceList] = useState<DeviceInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedDevice, setSelectedDevice] = useState<DeviceInfo | null>(null);
  const [deviceNameSearch, setDeviceNameSearch] = useState<string>('');
  const [deviceCodeSearch, setDeviceCodeSearch] = useState<string>('');
  const [productionLineId, setProductionLineId] = useState<string>('');
  const [deviceStatus, setDeviceStatus] = useState<string>('');
  const [productionLines, setProductionLines] = useState<Array<{ productionLineId: string; productionLineName: string }>>([]);
  
  // 分页相关状态
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 5,
    total: 0,
  });
  
  // 产线ID到名称的映射
  const productionLineMap = useMemo(() => {
    const map = new Map<string, string>();
    productionLines.forEach(line => {
      if (line.productionLineId) {
        map.set(line.productionLineId, line.productionLineName);
      }
    });
    return map;
  }, [productionLines]);
  
  // 设备监控数据相关状态
  const [monitorData, setMonitorData] = useState<EquipmentTraceData[]>([]);
  const [monitorLoading, setMonitorLoading] = useState(false);
  const [latestParams, setLatestParams] = useState<Record<string, { value: number, unit: string }>>({});
  const [latestSendTime, setLatestSendTime] = useState<string>('');
  const [latestAlarmMessages, setLatestAlarmMessages] = useState<string>('');
  const [imageLoadError, setImageLoadError] = useState(false); // 图片加载失败状态
  const dataUpdateRef = useRef<number>(0);
  const isInitialFilterChange = useRef(true); // 用于跳过首次渲染时的 effect
  const [hiddenSeries, setHiddenSeries] = useState<string[]>([]); // 用于存储隐藏的图例项

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

  // 控制栏相关状态
  const [refreshInterval, setRefreshInterval] = useState<number>(30); // 默认30秒刷新一次
  const [isAutoRefreshing, setIsAutoRefreshing] = useState(false); // 自动刷新状态指示器
  const [dataSize, setDataSize] = useState<number>(100); // 查询数据条数，默认为100条
  const refreshTimerRef = useRef<NodeJS.Timeout | null>(null);
  const chartRef = useRef<any>(null); // 用于获取图表实例
  const applyLegendSelection = () => {
    if (!chartRef.current) return;
    // 从当前数据中收集系列名
    const names = new Set<string>();
    monitorData.forEach(td => td.parameters.forEach(p => {
      if (p.type === 0 && p.name && p.name.endsWith('温度')) names.add(p.name);
    }));
    const selected: Record<string, boolean> = {} as any;
    Array.from(names).forEach(n => selected[n] = !hiddenSeries.includes(n));
    try {
      // 通过 update 主动更新图例选中态
      chartRef.current.update({ legend: { selected } });
    } catch (e) {
      // 兜底：若 update 不生效，尝试直接访问底层 chart
      const chart = chartRef.current?.chart;
      if (chart && chart.legend) {
        chart.legend({ selected });
        chart.render(true);
      }
    }
  };

  // 加载产线列表
  useEffect(() => {
    const fetchProductionLines = async () => {
      try {
        const res = await getProductionLineList({ pageSize: 1000 });
        if (res.data) {
          setProductionLines(
            res.data
              .filter((line: any) => line.productionLineId)
              .map((line: any) => ({
                productionLineId: line.productionLineId as string,
                productionLineName: line.productionLineName as string,
              }))
          );
        }
      } catch (error) {
        console.error('获取生产线列表失败:', error);
      }
    };
    fetchProductionLines();
  }, []);

  // 页面首次加载时，加载设备列表
  // 注意：这个 effect 会在 deviceType 解析后执行，确保设备类型过滤生效
  useEffect(() => {
    // 默认选中"全部"设备状态（空字符串表示全部）
    setDeviceStatus('');
    // 重置分页到第一页
    setPagination(prev => ({ ...prev, current: 1 }));
    // 加载设备列表，deviceType 会在 fetchDeviceList 内部被使用
    fetchDeviceList(1, pagination.pageSize, {
      status: null, // 传入 null 表示查询所有状态的设备
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [deviceType]); // 依赖 deviceType，当设备类型变化时重新加载

  // 加载设备列表
  const fetchDeviceList = async (
    page: number = pagination.current, 
    pageSize: number = pagination.pageSize,
    filters?: {
      deviceName?: string | null;
      deviceEnCode?: string | null;
      productionLineId?: string | null;
      status?: string | null;
    }
  ) => {
    setLoading(true);
    try {
      // 确保设备类型参数被正确传递（如果 URL 中有设备类型参数）
      const finalDeviceType = deviceType && deviceType.trim() !== '' ? deviceType.trim() : undefined;
      
      const params: DeviceInfoQueryParams = {
        current: page,
        pageSize: pageSize,
        deviceType: finalDeviceType, // 明确传递设备类型参数
        deviceName: filters?.deviceName !== undefined 
          ? (filters.deviceName === null || filters.deviceName === '' ? undefined : filters.deviceName)
          : (deviceNameSearch || undefined),
        deviceEnCode: filters?.deviceEnCode !== undefined
          ? (filters.deviceEnCode === null || filters.deviceEnCode === '' ? undefined : filters.deviceEnCode)
          : (deviceCodeSearch || undefined),
        productionLineId: filters?.productionLineId !== undefined
          ? (filters.productionLineId === null || filters.productionLineId === '' ? undefined : filters.productionLineId)
          : (productionLineId || undefined),
        status: filters?.status !== undefined
          ? (filters.status === null || filters.status === '' ? undefined : filters.status)
          : (deviceStatus || undefined),
      };
      
      // 调试日志：检查传递给后端的参数
      console.log('🔍 监控页面 - 查询参数:', {
        deviceType: finalDeviceType,
        originalDeviceType: deviceType,
        pathname: location.pathname,
        allParams: params,
      });
      
      const result = await getDeviceInfoList(params);
      if (result.data) {
        // 前端二次过滤：确保只显示匹配设备类型的数据（作为后端过滤的兜底）
        // 注意：如果后端已经正确过滤，这里应该不会过滤掉任何数据
        let filteredData = result.data;
        let needsFrontendFiltering = false;
        
        if (finalDeviceType) {
          // 检查后端是否已经正确过滤
          const allMatchType = result.data.every((device: DeviceInfo) => {
            return device.deviceType && device.deviceType.trim() === finalDeviceType.trim();
          });
          
          if (!allMatchType) {
            // 后端没有正确过滤，进行前端过滤
            needsFrontendFiltering = true;
            filteredData = result.data.filter((device: DeviceInfo) => {
              const deviceTypeMatch = device.deviceType && 
                device.deviceType.trim() === finalDeviceType.trim();
              return deviceTypeMatch;
            });
          }
        }
        
        // 调试日志：检查返回的数据
        console.log('📥 监控页面 - 返回数据:', {
          deviceType: finalDeviceType,
          originalDataCount: result.data.length,
          filteredDataCount: filteredData.length,
          needsFrontendFiltering,
          originalDeviceTypes: result.data.map((d: DeviceInfo) => d.deviceType),
          filteredDeviceTypes: filteredData.map((d: DeviceInfo) => d.deviceType),
          backendTotal: (result as any).total || (result as any).Total,
        });
        
        // 获取后端返回的总数
        const backendTotal = (result as any).total || (result as any).Total || 0;
        
        // 如果进行了前端过滤，说明后端可能没有正确过滤
        // 在这种情况下，我们需要调整 total 以反映过滤后的实际数量
        // 但是，由于我们只获取了当前页的数据，无法知道过滤后的总数
        // 所以，我们使用一个估算值：如果当前页过滤后减少了，按比例估算总数
        let finalTotal = backendTotal;
        
        if (needsFrontendFiltering && result.data.length > 0) {
          // 计算过滤比例
          const filterRatio = filteredData.length / result.data.length;
          // 按比例调整总数（这是一个估算，可能不完全准确）
          finalTotal = Math.ceil(backendTotal * filterRatio);
          console.warn('⚠️ 后端可能没有正确按设备类型过滤，前端进行了二次过滤', {
            backendDataCount: result.data.length,
            filteredDataCount: filteredData.length,
            backendTotal,
            estimatedTotal: finalTotal,
            filterRatio,
          });
          
          // 如果过滤后当前页数据为空，但还有更多页，需要调整当前页
          if (filteredData.length === 0 && backendTotal > pageSize) {
            console.warn('⚠️ 当前页过滤后无数据，可能需要跳转到上一页');
          }
        }
        
        setDeviceList(filteredData);
        
        // 更新分页信息
        // 如果进行了前端过滤，使用估算的总数；否则使用后端返回的总数
        // 重要：确保 total 至少等于当前页的数据量，避免分页显示错误
        const minTotal = filteredData.length > 0 ? 
          Math.max(finalTotal, (page - 1) * pageSize + filteredData.length) : 
          finalTotal;
        
        setPagination(prev => ({
          ...prev,
          current: page,
          pageSize: pageSize,
          total: minTotal, // 确保 total 至少能覆盖当前页的数据
        }));
        
        // 如果还没有选中设备，且列表不为空，自动选中第一个（使用过滤后的数据）
        if (!selectedDevice && filteredData.length > 0) {
          setSelectedDevice(filteredData[0]);
        }
        // 如果当前选中的设备不在新列表中，清空选中（使用过滤后的数据）
        if (selectedDevice && !filteredData.find(d => d.deviceId === selectedDevice.deviceId)) {
          setSelectedDevice(filteredData.length > 0 ? filteredData[0] : null);
          setImageLoadError(false); // 重置图片加载错误状态
        }
      }
    } catch (error) {
      console.error('获取设备列表失败:', error);
      message.error('获取设备列表失败');
    } finally {
      setLoading(false);
    }
  };

  // 使用ref来跟踪是否是重置操作，避免重复请求
  const isResettingRef = useRef(false);
  // 使用ref来跟踪是否是手动搜索，避免重复请求
  const isManualSearchRef = useRef(false);
  // 使用ref来跟踪是否是手动改变筛选条件，避免重复请求
  const isManualFilterRef = useRef(false);

  // 注意：deviceType 变化的处理已经在首次加载的 useEffect 中处理了（依赖 deviceType）
  // 这个 useEffect 主要用于处理其他筛选条件的变化，但现在不再需要了
  // 因为首次加载的 useEffect 已经依赖 deviceType，会自动处理设备类型变化

  // 处理分页变化
  const handlePageChange = (page: number, pageSize: number) => {
    fetchDeviceList(page, pageSize);
  };

  // 重置筛选条件
  const handleReset = () => {
    // 标记为重置操作，避免useEffect重复请求
    isResettingRef.current = true;
    // 先保存当前分页大小
    const currentPageSize = pagination.pageSize;
    // 批量更新所有状态
    setDeviceNameSearch('');
    setDeviceCodeSearch('');
    setProductionLineId('');
    setDeviceStatus('');
    setPagination(prev => ({ ...prev, current: 1 }));
    // 使用重置后的值立即查询，明确传入 null 表示清空筛选条件
    fetchDeviceList(1, currentPageSize, {
      deviceName: null,
      deviceEnCode: null,
      productionLineId: null,
      status: null,
    });
  };

  // 当选中设备改变时，加载监控数据
  useEffect(() => {
    if (selectedDevice?.deviceEnCode) {
      fetchMonitorData(true); // 初始加载时，显示全屏加载
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedDevice]);

  // 当刷新间隔或数据条数变化时，自动重新加载数据
  useEffect(() => {
    // 跳过首次渲染，避免在组件加载时触发
    if (isInitialFilterChange.current) {
      isInitialFilterChange.current = false;
      return;
    }

    // 只有当有设备被选中时才执行
    if (selectedDevice) {
      fetchMonitorData(true); // 使用 true 来显示加载指示器
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshInterval, dataSize]);

  // 设置自动刷新定时器
  useEffect(() => {
    if (selectedDevice && refreshInterval > 0) {
      // 清除之前的定时器
      if (refreshTimerRef.current) {
        clearInterval(refreshTimerRef.current);
      }

      // 创建新的定时器
      refreshTimerRef.current = setInterval(() => {
        fetchMonitorData(false); // 自动刷新时，不显示全屏加载
      }, refreshInterval * 1000);
    } else if (refreshTimerRef.current) {
      // 如果不需要自动刷新，清除定时器
      clearInterval(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }

    // 组件卸载或依赖项变化时清除定时器
    return () => {
      if (refreshTimerRef.current) {
        clearInterval(refreshTimerRef.current);
        refreshTimerRef.current = null;
      }
    };
  }, [selectedDevice, refreshInterval, dataSize]);

  // 图例隐藏/显示状态变化或数据变化后，立即同步到图表
  useEffect(() => {
    applyLegendSelection();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hiddenSeries, monitorData]);

  // 加载设备监控数据
  const fetchMonitorData = async (isInitialLoad: boolean = false) => {
    if (!selectedDevice?.deviceEnCode) return;

    // 仅在初始加载时显示全屏加载状态
    if (isInitialLoad) {
      setMonitorLoading(true);
    } else {
      // 自动刷新时显示刷新状态但不显示全屏加载
      setIsAutoRefreshing(true);
    }

    try {
      const result = await getEquipmentTracinfosListByDeviceEnCode({
        deviceEnCode: selectedDevice.deviceEnCode,
        size: dataSize, // 使用设置的数据条数
      });

      if (result.success && result.data && result.data.length > 0) {
        // 按sendTime从小到大排序，以便图表正确显示时间轴
        const sortedData = [...result.data].sort((a, b) => {
          const timeA = new Date(a.sendTime || 0).getTime();
          const timeB = new Date(b.sendTime || 0).getTime();
          return timeA - timeB;
        });

        setMonitorData(sortedData);

        // 获取最新一条数据（排序后的最后一条）
        const latest = sortedData[sortedData.length - 1];
        setLatestSendTime(latest.sendTime);
        setLatestAlarmMessages(latest.alarmMessages || '');

        // 处理参数
        const params: Record<string, { value: number, unit: string }> = {};
        latest.parameters.forEach((param: Parameter) => {
          if (param.type === 0) {
            const numericValue = param.value && !isNaN(parseFloat(param.value)) ? parseFloat(param.value) : 0;
            params[param.name] = {
              value: numericValue,
              unit: param.unit || '',
            };
          }
        });
        setLatestParams(params);
        dataUpdateRef.current += 1;
      } else {
        setMonitorData([]);
        setLatestParams({});
        setLatestSendTime('');
        setLatestAlarmMessages('');
      }
    } catch (error) {
      console.error('获取监控数据失败:', error);
      message.error('获取监控数据失败');
    } finally {
      if (isInitialLoad) {
        setMonitorLoading(false);
      } else {
        // 自动刷新状态延迟清除，给用户视觉反馈
        setTimeout(() => setIsAutoRefreshing(false), 500);
      }
      // 刷新完成后，恢复用户的图例选择
      setTimeout(() => applyLegendSelection(), 0);
    }
  };

  // 处理温度图表数据
  const processChartData = () => {
    const chartData: any[] = [];
    const parseTime = (timeString: string) => {
      try {
        return new Date(timeString);
      } catch {
        return new Date();
      }
    };

    monitorData.forEach((traceData) => {
      traceData.parameters.forEach((param) => {
        if (param.type === 0 && param.name.endsWith('温度')) {
          let numericValue = param.value && !isNaN(parseFloat(param.value)) ? parseFloat(param.value) : 0;
          chartData.push({
            'Date': parseTime(traceData.sendTime),
            value: numericValue,
            series: param.name,
          });
        }
      });
    });

    chartData.sort((a, b) => {
      return new Date(a['Date'] || 0).getTime() - new Date(b['Date'] || 0).getTime();
    });

    return chartData;
  };

  // 生成图表配置
  const generateChartConfig = () => {
    const chartData = processChartData();
    return {
      theme: 'dark',
      background: 'transparent', // 设置图表背景为透明
      interaction: {
        brushFilter: true,
      },
      colorField: 'series',
      yField: 'value',
      xField: (d: { [x: string]: string | number | Date; }) => new Date(d['Date']),
      // 设置坐标轴样式
      xAxis: {
        label: {
          formatter: (text: string) => {
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
            fill: 'rgba(255, 255, 255, 0.65)', // 调暗标签颜色
            fontSize: 12,
          },
        },
        line: {
          style: {
            stroke: 'rgba(255, 255, 255, 0.2)', // 调暗轴线颜色
          },
        },
        grid: {
          line: {
            style: {
              stroke: 'rgba(255, 255, 255, 0.1)', // 网格线颜色
            },
          },
        },
        tickCount: 10,
      },
      yAxis: {
        label: {
          style: {
            fill: 'rgba(255, 255, 255, 0.65)', // 调暗标签颜色
            fontSize: 12,
          },
          formatter: (v: string) => `${v}°C`,
        },
        line: {
          style: {
            stroke: 'rgba(255, 255, 255, 0.2)', // 调暗轴线颜色
          },
        },
        grid: {
          line: {
            style: {
              stroke: 'rgba(255, 255, 255, 0.1)', // 网格线颜色
            },
          },
        },
      },
      // 设置图例样式
      legend: {
        itemName: {
          style: {
            fill: 'rgba(255, 255, 255, 0.85)', // 调亮图例文字
            fontSize: 12,
          },
        },
        // 根据 hiddenSeries 状态动态控制图例的选中状态
        selected: (() => {
          const uniqueSeries: string[] = Array.from(new Set<string>(chartData.map((d: any) => String(d.series))));
          const selection: Record<string, boolean> = {};
          uniqueSeries.forEach((s: string) => {
            selection[s] = !hiddenSeries.includes(s);
          });
          return selection;
        })(),
      },
      // 设置提示框样式
      tooltip: {
        showTitle: true,
        showMarkers: true,
        shared: true,
        showCrosshairs: true,
        crosshairs: {
          line: {
            style: {
              stroke: '#1890ff',
              opacity: 0.5,
            },
          },
        },
        domStyles: {
          'g2-tooltip': {
            background: 'rgba(0, 0, 0, 0.8)',
            color: '#fff',
            border: '1px solid rgba(255, 255, 255, 0.2)',
            borderRadius: '4px',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.3)',
            padding: '8px 12px',
          },
          'g2-tooltip-title': {
            color: '#fff',
            fontSize: '14px',
            marginBottom: '4px',
          },
          'g2-tooltip-list-item': {
            color: '#fff',
            fontSize: '13px',
            margin: '4px 0',
          },
          'g2-tooltip-marker': {
            width: '8px',
            height: '8px',
            borderRadius: '50%',
          },
        },
      },
      // 设置线条和点样式
      line: {
        size: 2,
        style: {
          lineWidth: 2,
        },
      },
      point: false, // 完全禁用默认数据点
      state: {
        active: {
          lineWidth: 2,
          lineDash: [4, 4],
        },
      },
      interactions: [
        {
          type: 'marker-active',
        },
      ],
      data: chartData,
    };
  };

  // 格式化日期时间
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
      return timeString;
    }
  };

  // 处理设备图片路径
  // 获取图片路径的辅助函数（用于头像和设备图片）
  const getImagePath = (imageName?: string): string | undefined => {
    if (!imageName) return undefined;
    
    // 如果已经是完整的 HTTP/HTTPS URL，直接返回
    if (imageName.startsWith('http://') || imageName.startsWith('https://')) {
      return imageName;
    }
    
    // 如果已经是 /images/ 开头的路径，直接返回
    if (imageName.startsWith('/images/')) {
      return imageName;
    }
    
    // 检查是否是本地文件路径（Windows 路径格式，如 D:\ 或 D:/）
    const isLocalPath = /^[A-Za-z]:[\\/]/.test(imageName) || // Windows 绝对路径 D:\ 或 D:/
                        imageName.startsWith('\\') || // Windows 网络路径 \\server\share
                        imageName.startsWith('file://'); // file:// 协议
    
    if (isLocalPath) {
      // 从本地路径提取文件名
      try {
        const normalizedPath = imageName.replace(/\\/g, '/');
        const pathParts = normalizedPath.split('/');
        const fileName = pathParts[pathParts.length - 1] || '';
        if (fileName && fileName.includes('.')) {
          // 直接使用文件名，浏览器会自动处理中文编码
          return `/images/${fileName}`;
        }
      } catch (e) {
        console.error('路径转换出错:', e);
        return undefined;
      }
    }
    
    // 检查是否包含路径分隔符（相对路径）
    const hasPathSeparator = imageName.includes('/') || imageName.includes('\\');
    
    if (hasPathSeparator && !isLocalPath) {
      // 相对路径，提取文件名
      try {
        const normalizedPath = imageName.replace(/\\/g, '/');
        const pathParts = normalizedPath.split('/');
        const fileName = pathParts[pathParts.length - 1] || '';
        if (fileName && fileName.includes('.')) {
          return `/images/${fileName}`;
        }
      } catch (e) {
        console.error('路径转换出错:', e);
        return undefined;
      }
    }
    
    // 纯文件名（如 "催化炉.png"），使用 /images/ 路径
    if (imageName.includes('.')) {
      // 直接使用文件名，浏览器会自动处理中文编码
      return `/images/${imageName}`;
    }
    
    return undefined;
  };

  // 获取设备图片 URL（用于设备详情中的大图）
  const getDeviceImageUrl = (device: DeviceInfo) => {
    // 兼容两种命名方式：PascalCase (后端) 和 camelCase (前端)
    const devicePicture = (device as any).devicePicture || (device as any).DevicePicture || '';
    return getImagePath(devicePicture) || '';
  };

  // 获取设备头像 URL（用于设备列表中的头像）
  const getDeviceAvatarUrl = (device: DeviceInfo) => {
    // 兼容两种命名方式：PascalCase (后端) 和 camelCase (前端)
    const avatar = (device as any).avatar || (device as any).Avatar || '';
    return getImagePath(avatar) || '';
  };

  return (
    <PageContainer
      className="device-monitor-page"
      title={`设备监控 - ${deviceType || '全部设备'}`}
      onBack={() => navigate('/devicemonitor/index2')}
    >
      <Row gutter={16}  style={{ marginBottom: 10 }}>
        {/* 左侧：设备列表 */}
        <Col span={9}>
          <Card 
            title="设备列表" 
            style={{ height: '100%', display: 'flex', flexDirection: 'column' }}
            bodyStyle={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden', padding: 16, minHeight: 0 }}
          >
            <div
              style={{
                marginBottom: 20,
                padding: 16,
                borderRadius: 15,
                background: 'linear-gradient(180deg, rgba(101, 96, 155, 0) 0%, rgba(101, 96, 155, 0.3) 99%)',
                flexShrink: 0,
                overflow: 'visible',
                minHeight: 'auto',
                display: 'block',
                visibility: 'visible',
                // border: '1px solid rgba(255,255,255,0.12)',
                // // boxShadow: '0 8px 24px rgba(0,0,0,0.25)',
                // backdropFilter: 'blur(6px)',
                // WebkitBackdropFilter: 'blur(6px)'
              }}
            >
              <div style={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
                <Search
                  placeholder="搜索设备名称"
                  allowClear
                  value={deviceNameSearch}
                  onChange={(e) => setDeviceNameSearch(e.target.value)}
                  onSearch={() => {
                    // 标记为手动搜索，避免useEffect重复请求
                    isManualSearchRef.current = true;
                    setPagination(prev => ({ ...prev, current: 1 }));
                    fetchDeviceList(1, pagination.pageSize);
                  }}
                  style={{ width: '100%', marginBottom: 12 }}
                />
                <Search
                  placeholder="搜索设备编码"
                  allowClear
                  value={deviceCodeSearch}
                  onChange={(e) => setDeviceCodeSearch(e.target.value)}
                  onSearch={() => {
                    // 标记为手动搜索，避免useEffect重复请求
                    isManualSearchRef.current = true;
                    setPagination(prev => ({ ...prev, current: 1 }));
                    fetchDeviceList(1, pagination.pageSize);
                  }}
                  style={{ width: '100%', marginBottom: 12 }}
                />
                <Select
                  placeholder="选择产线"
                  allowClear
                  value={productionLineId || undefined}
                  onChange={(value) => {
                    const newValue = value || '';
                    setProductionLineId(newValue);
                    // 标记为手动改变筛选条件，避免useEffect重复请求
                    isManualFilterRef.current = true;
                    setPagination(prev => ({ ...prev, current: 1 }));
                    fetchDeviceList(1, pagination.pageSize, {
                      productionLineId: newValue || undefined,
                    });
                  }}
                  style={{ width: '100%', marginBottom: 12 }}
                  options={productionLines.map(line => ({
                    value: line.productionLineId,
                    label: line.productionLineName,
                  }))}
                />
                <div style={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', justifyContent: 'space-between'}}>
                  <div style={{ fontSize: 14, fontWeight: 300, whiteSpace: 'nowrap', paddingLeft: 10}}>设备状态：
                    <Radio.Group
                      value={deviceStatus || ''}
                      onChange={(e) => {
                        const newValue = e.target.value || '';
                        setDeviceStatus(newValue);
                        // 标记为手动改变筛选条件，避免useEffect重复请求
                        isManualFilterRef.current = true;
                        setPagination(prev => ({ ...prev, current: 1 }));
                        // 当选择"全部"时（newValue 为空字符串），传入 null 明确表示清空筛选
                        // 当选择其他选项时，传入实际值
                        fetchDeviceList(1, pagination.pageSize, {
                          status: newValue === '' ? null : newValue,
                        });
                      }}
                    >
                      <Radio value="">全部</Radio>
                      <Radio value="1">启用</Radio>
                      <Radio value="0">禁用</Radio>
                    </Radio.Group>
                  </div>
                  <Button
                    icon={<ReloadOutlined />}
                    onClick={handleReset}
                    danger
                    size="small"
                  >
                    重置
                  </Button>
                </div>
              </div>
            </div>
            <div style={{ flex: '1 1 auto', minHeight: 0 }}>
              {/* style={{ flex: '1 1 auto', overflowY: 'auto', paddingRight: 12 }}自适应备用 */}
              <List
                loading={loading}
                dataSource={deviceList}
                renderItem={(item) => {
                  // 使用头像字段，如果没有则使用设备图片字段
                  const avatarUrl = getDeviceAvatarUrl(item) || getDeviceImageUrl(item);
                  const isSelected = selectedDevice?.deviceId === item.deviceId;
                  return (
                    <List.Item
                      style={{
                        cursor: 'pointer',
                        backgroundColor: isSelected ? 'rgba(50, 59, 58, 0.05)' : 'rgba(0,10,50,0.5)',
                        border: isSelected
                          ? `1px solid rgba(65,255,230,1)`
                          : `1px solid rgba(0, 0, 0, 0)`,
                        // color: isSelected ? 'rgba(240, 240, 240, 1)' : '#000',
                        padding: '15px',
                        marginBottom: 12,
                        borderRadius:isSelected ?  20 : 10,
                        // boxShadow: isSelected ? token.boxShadowSecondary : 'none',
                      }}
                      onClick={() => {
                        setSelectedDevice(item);
                        setImageLoadError(false); // 重置图片加载错误状态
                      }}
                    >
                      <List.Item.Meta
                        avatar={
                          avatarUrl ? (
                            <img
                              alt=""
                              className={styles.cardAvatar}
                              src={avatarUrl}
                              onError={(e) => {
                                // 图片加载失败时，隐藏图片并显示占位符
                                const target = e.target as HTMLImageElement;
                                target.style.display = 'none';
                                // 创建占位符
                                const parent = target.parentElement;
                                if (parent && !parent.querySelector('.avatar-placeholder')) {
                                  const placeholder = document.createElement('span');
                                  placeholder.className = `${styles.cardAvatarPlaceholder} avatar-placeholder`;
                                  parent.insertBefore(placeholder, target);
                                }
                              }}
                            />
                          ) : (
                            <span className={styles.cardAvatarPlaceholder} />
                          )
                        }
                      title={
                        <span
                          // style={{
                          //   fontSize: 16,
                          //   fontWeight: isSelected ? 600 : 500,
                          //   color: isSelected ? token.colorTextHeading : undefined,
                          // }}
                          style={{
                            fontSize: isSelected ? 14 : 12,
                            fontWeight: isSelected ? 600 : 300,
                            // color: isSelected ? 'rgba(0, 255, 153, 1)' : undefined,
                            color: isSelected ? 'rgb(255, 180, 0)' : 'rgba(255, 255, 255, 0.5)',
                            // color: isSelected ? token.colorTextHeading : undefined,
                          }}
                        >
                          {item.deviceName} 【 编码：{item.deviceEnCode} 】
                        </span>
                      }
                      description={
                        <div style={{ display: 'flex', flexDirection: 'column', fontSize: 12 }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <span>编码: {item.deviceEnCode}</span>

                            <span>
                                {item.productionLineId && productionLineMap.has(item.productionLineId) && (
                                 <div>产线: {productionLineMap.get(item.productionLineId)}</div>
                                )}
                            </span>

                            <span style={{ display: 'flex', alignItems: 'center' }}>
                              <span>设备状态：</span>
                              {(() => {
                                const deviceStatus = item.status as keyof typeof statusMap;
                                const statusInfo = statusMap[deviceStatus];
                                if (statusInfo) {
                                  return (
                                    <Tag color=
                                      {statusInfo.status === 'Success' ? 'green' : 'red'}
                                      style={{
                                        margin: 0 
                                        // background: ' linear-gradient(rgba(130, 250, 175, 1), rgba(145, 250, 240, 1))',
                                        // border: '2px solid rgba(210, 255, 230, 1)',
                                        // boxShadow: '0 0 8px rgba(65, 255, 230, 1)',
                                        // color: 'rgba(0, 10, 50, 1)',
                                       }}>
                                      {statusInfo.text}
                                    </Tag>
                                  );
                                }
                                return <Tag color="default" style={{ margin: 0 }}>未知</Tag>;
                              })()}
                            </span>
                          </div>
                          {/* {item.productionLineId && productionLineMap.has(item.productionLineId) && (
                            <div>产线: {productionLineMap.get(item.productionLineId)}</div>
                          )} */}
                        </div>
                      }
                    />
                  </List.Item>
                  );
                }}
                locale={{ emptyText: <Empty description="暂无设备数据" /> }}
              />
              {pagination.total > 0 && (
                <div style={{ marginTop: 16, textAlign: 'right' }}>
                  <Pagination
                    current={pagination.current}
                    pageSize={pagination.pageSize}
                    total={pagination.total}
                    // showSizeChanger
                    showTotal={(total) => `共 ${total} 条数据`}
                    pageSizeOptions={['5', '10', '20', '50', '100']}
                    onChange={handlePageChange}
                    onShowSizeChange={handlePageChange}
                    size='small'
                    align='center'
                  />
                </div>
              )}
            </div>
          </Card>
        </Col>

        {/* 右侧：设备详情 */}
        <Col span={15}>
          {selectedDevice ? (
            <div style={{ height: '100%', display: 'flex', flexDirection: 'column', gap: '16px' }}>
              {/* 设备图片 */}
              <Card title="设备信息" style={{ flex: '0 0 auto' }}>
                {/* 左侧图片区域 */}
                <Row gutter={16} >
                  <Col span={10}>
                    <div style={{ 
                      // width: '100%', 
                      // height: '175px', 
                      backgroundColor: 'transparent',
                      display: 'flex',
                      justifyContent: 'center',
                      alignItems: 'center',
                      border: '1px solid rgba(255,255,255,0.15)',
                      borderRadius: 8,
                    }}>
                      {getDeviceImageUrl(selectedDevice) && !imageLoadError ? (
                    <img
                      src={getDeviceImageUrl(selectedDevice)}
                      alt={selectedDevice.deviceName}
                      style={{
                        width: '100%',
                        height: '180px',
                        objectFit: 'cover',
                        borderRadius: 'inherit',
                      }}
                      onError={(e) => {
                        console.warn('设备图片加载失败', e);
                        setImageLoadError(true);
                        (e.target as HTMLImageElement).style.display = 'none';
                      }}
                      onLoad={() => {
                        setImageLoadError(false);
                      }}
                    />
                      ) : (
                        <div style={{ color: '#999' }}>暂无设备图片</div>
                      )}
                    </div>
                  </Col>
                  
                  {/* 右侧设备信息表格区域 */}
                  <Col span={14}>
                    <Descriptions column={2} size="small" bordered style={{ whiteSpace: 'nowrap' }}>
                      <Descriptions.Item label="设备名称" style={{ fontSize: '12px' }}>{selectedDevice.deviceName}</Descriptions.Item>
                      <Descriptions.Item label="设备类型" style={{ fontSize: '12px' }}>{selectedDevice.deviceType || '未知'}</Descriptions.Item>                      
                      {/* 报警信息始终显示 */}
                      <Descriptions.Item label="设备编码" style={{ fontSize: '12px' }}>{selectedDevice.deviceEnCode}</Descriptions.Item>
                      <Descriptions.Item label="产线名称" style={{ fontSize: '12px' }}>
                        {selectedDevice.productionLineId && productionLineMap.has(selectedDevice.productionLineId)
                          ? productionLineMap.get(selectedDevice.productionLineId)
                          : '未知'}
                      </Descriptions.Item>
                      <Descriptions.Item label="报警信息" style={{ fontSize: '12px' }}>
                        <div style={{ 
                          color: latestAlarmMessages ? '#ff4d4f' : 'rgba(240, 240, 240, 1)', 
                          fontSize: '12px',                          
                        }}>
                          {latestAlarmMessages || '无报警信息'}
                        </div>
                      </Descriptions.Item>
                      <Descriptions.Item label="设备状态" style={{ fontSize: '12px' }}>
                        {(() => {
                          const deviceStatus = selectedDevice.status as keyof typeof statusMap;
                          const statusInfo = statusMap[deviceStatus];
                          if (statusInfo) {
                            return (
                              <Tag color={statusInfo.status === 'Success' ? 'green' : 'red'}>
                                {statusInfo.text}
                              </Tag>
                            );
                          }
                          return <Tag color="default">未知</Tag>;
                        })()}
                      </Descriptions.Item>
                      {/*最新数据时间始终显示 */}
                      <Descriptions.Item label="最新数据时间" span={2} style={{ fontSize: '12px' }}>
                        {latestSendTime ? formatDateTime(latestSendTime) : '无数据'}
                      </Descriptions.Item>
                      <Descriptions.Item label="制造商" span={2} style={{ fontSize: '12px' }}>{selectedDevice.deviceManufacturer || '未知'}</Descriptions.Item>
                    </Descriptions>  

                      {/* {latestSendTime && (
                        <Descriptions.Item label="最新数据时间" style={{ fontSize: '12px' }} span={2}>
                          {formatDateTime(latestSendTime)}
                        </Descriptions.Item>
                      )} */}                      
                    
                      {/* {latestAlarmMessages && (
                        <Descriptions.Item label="报警信息" style={{ fontSize: '12px' }} span={2}>
                          <div style={{ color: '#ff4d4fff' }}>{latestAlarmMessages}</div>
                        </Descriptions.Item>
                      )}
                    </Descriptions> */}
                  </Col>
                </Row>              
              </Card>


              {/* 温度图表 - 已集成控制栏 */}
              <Card
                title="温度监控图表"
                style={{ flex: '1 1 auto', display: 'flex', flexDirection: 'column', backgroundColor: 'transparent', border: 'none' }}
                headStyle={{
                  // backgroundColor: 'transparent',
                  // borderBottom: '0px solid rgba(255, 255, 255, 0.15)', // 更柔和的边框
                  // color: '#fff',
                  // padding: '0 16px',
                  // minHeight: '48px',
                }}
                bodyStyle={{ padding: 0, display: 'flex', flexDirection: 'column', flex: 1 }}
              >
                <div style={{ flex: 1, minHeight: 300, backgroundColor: 'transparent', padding: '16px' }}>
                  <Spin spinning={monitorLoading}>
                    {processChartData().length > 0 ? (
                      <Line {...generateChartConfig()} onReady={(plot) => {
                        chartRef.current = plot;
                        // 记录图例点击隐藏/显示
                        plot.on('legend-item:click', (evt: any) => {
                          const name = evt?.data?.name || evt?.delegateObject?.item?.name || evt?.event?.item?.name;
                          if (!name) return;
                          setHiddenSeries((prev) => prev.includes(name) ? prev.filter(s => s !== name) : [...prev, name]);
                        });
                      }} />
                    ) : (
                      <Empty
                        description="暂无温度数据"
                        imageStyle={{ color: 'rgba(255, 255, 255, 0.3)' }}
                      />
                    )}
                  </Spin>
                </div>

                {/* 控制栏 */}
                <div
                  style={{
                    padding: '16px 16px',
                    backgroundColor: 'transparent',
                    borderTop: '1px solid rgba(255, 255, 255, 0.15)', // 更柔和的边框
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    gap: '16px',
                  }}
                >
                  <Button
                    onClick={() => fetchMonitorData(true)}
                    loading={isAutoRefreshing}
                    icon={<ReloadOutlined />}
                    ghost // 使用幽灵按钮样式，使其背景透明
                    style={{ color: 'rgb(255, 255, 255)', border: '1px solid rgba(255, 255, 255, 0.18)' }} // 设置亮色字体和边框
                  >
                    刷新数据
                  </Button>

                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ color: '#fff' }}>自动刷新:</span>
                    <Select
                      value={refreshInterval}
                      onChange={setRefreshInterval}
                      style={{ width: '120px' }} // 增加宽度以显示完整内容
                      options={refreshOptions}
                      disabled={isAutoRefreshing}
                    />
                    <span style={{ color: '#fff' }}>数据条数:</span>
                    <Select
                      value={dataSize}
                      onChange={setDataSize}
                      style={{ width: '120px' }} // 增加宽度以保持对齐
                      options={dataSizeOptions}
                      disabled={isAutoRefreshing}
                    />
                  </div>

                  {/* <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ color: '#fff' }}>数据条数:</span>
                    <Select
                      value={dataSize}
                      onChange={setDataSize}
                      style={{ width: '120px' }} // 增加宽度以保持对齐
                      options={dataSizeOptions}
                      disabled={isAutoRefreshing}
                    />
                  </div> */}
                </div>
              </Card>

              
              {/* 设备信息
              <Card title="设备信息" style={{ flex: '0 0 auto' }}>             
                  <Descriptions column={4} size="small" bordered>
                  <Descriptions.Item label="设备名称">{selectedDevice.deviceName}</Descriptions.Item>
                  <Descriptions.Item label="设备编码">{selectedDevice.deviceEnCode}</Descriptions.Item>
                  <Descriptions.Item label="设备类型">{selectedDevice.deviceType || '未知'}</Descriptions.Item>
                  <Descriptions.Item label="制造商">{selectedDevice.deviceManufacturer || '未知'}</Descriptions.Item>
                  <Descriptions.Item label="产线名称">
                    {selectedDevice.productionLineId && productionLineMap.has(selectedDevice.productionLineId)
                      ? productionLineMap.get(selectedDevice.productionLineId)
                      : '未知'}
                  </Descriptions.Item>
                  <Descriptions.Item label="设备状态">
                    {(() => {
                      const deviceStatus = selectedDevice.status as keyof typeof statusMap;
                      const statusInfo = statusMap[deviceStatus];
                      if (statusInfo) {
                        return (
                          <Tag color={statusInfo.status === 'Success' ? 'green' : 'red'}>
                            {statusInfo.text}
                          </Tag>
                        );
                      }
                      return <Tag color="default">未知</Tag>;
                    })()}
                  </Descriptions.Item>
                  {latestSendTime && (
                    <Descriptions.Item label="最新数据时间" span={2}>
                      {formatDateTime(latestSendTime)}
                    </Descriptions.Item>
                  )}
                  {latestAlarmMessages && (
                    <Descriptions.Item label="报警信息" span={2}>
                      <div style={{ color: '#ff4d4f' }}>{latestAlarmMessages}</div>
                    </Descriptions.Item>
                  )}
                </Descriptions>
              </Card> */}
            </div>
          ) : (
            <Card>
              <Empty description="请从左侧选择设备" />
            </Card>
          )}
        </Col>
      </Row>
    </PageContainer>
  );
};

export default DeviceMonitorPage;


