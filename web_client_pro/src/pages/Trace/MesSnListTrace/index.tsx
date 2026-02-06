import type { ActionType, ProColumns, RequestData } from '@ant-design/pro-components';
import { PageContainer, ProTable } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback, useMemo } from 'react';
import { Button, message } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import { getMesSnListCurrentList } from '@/services/Api/Trace/MesSnListCurrent';
import { getMesSnListHistoryList } from '@/services/Api/Trace/MesSnListHistory';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';
import { getOrderById } from '@/services/Api/Infrastructure/OrderList';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';
import { getProductionLineById } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';
import { getDeviceInfoById } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import type { MesSnListCurrentDto, MesSnListCurrentQueryDto } from '@/services/Model/Trace/MesSnListCurrent';
import type { MesSnListHistoryDto, MesSnListHistoryQueryDto } from '@/services/Model/Trace/MesSnListHistory';
import type { ProductListDto } from '@/services/Model/Infrastructure/ProductList';
import type { OrderList } from '@/services/Model/Infrastructure/OrderList';
import type { StationListDto } from '@/services/Model/Infrastructure/StationList';
import type { productionLine } from '@/services/Model/Trace/ProductionEquipment‌/productionLineInfo';
import type { DeviceInfo } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';
import ExcelExportButton from './components/excel';
import { exportToExcel, convertObjectsToRows, createInfoRows } from './components/excel';

/**
 * Unicode转中文的辅助函数
 * 将形如 "\u7EDD\u7F18\u7535\u963B" 的Unicode编码转换为中文字符
 */
const decodeUnicode = (str: string): string => {
  try {
    // 使用正则表达式匹配Unicode转义序列并替换
    return str.replace(/\\u([0-9a-fA-F]{4})/g, (_, code) =>
      String.fromCharCode(parseInt(code, 16))
    );
  } catch (error) {
    console.error('Unicode解码错误:', error);
    return str; // 解码失败时返回原字符串
  }
};

/**
 * 解析并格式化测试数据，将其中的Unicode编码转换为中文
 */
const formatTestData = (testData: any): string => {
  if (!testData) return '-';
  
  try {
    // 如果是字符串，尝试解析为JSON
    let parsedData;
    if (typeof testData === 'string') {
      // 首先解码字符串中的Unicode
      const decodedString = decodeUnicode(testData);
      parsedData = JSON.parse(decodedString);
    } else {
      parsedData = testData;
    }
    
    // 如果是数组，格式化为更易读的形式
    if (Array.isArray(parsedData)) {
      return parsedData.map(item => {
        // 对对象中的每个属性值进行Unicode解码
        const decodedItem: Record<string, any> = {};
        Object.keys(item).forEach(key => {
          const value = item[key];
          if (typeof value === 'string') {
            decodedItem[key] = decodeUnicode(value);
          } else {
            decodedItem[key] = value;
          }
        });
        
        // 返回关键信息，如参数名称和测试结果
        const parametricKey = decodedItem.ParametricKey || '未知参数';
        const testResult = decodedItem.TestResult || '无结果';
        const testValue = decodedItem.TestValue ? `${decodedItem.TestValue}${decodedItem.Units || ''}` : '';
        
        return `${parametricKey}: ${testValue} (${testResult})`;
      }).join('; ');
    } else if (typeof parsedData === 'object' && parsedData !== null) {
      // 对于单个对象，也进行Unicode解码
      const decodedObj: Record<string, any> = {};
      Object.keys(parsedData).forEach(key => {
        const value = parsedData[key];
        if (typeof value === 'string') {
          decodedObj[key] = decodeUnicode(value);
        } else {
          decodedObj[key] = value;
        }
      });
      
      // 检查是否为简单的键值对对象还是复杂对象，如果是简单对象则格式化输出
      if (decodedObj.ParametricKey) {
        // 这是一个测试项目对象
        const parametricKey = decodedObj.ParametricKey || '未知参数';
        const testResult = decodedObj.TestResult || '无结果';
        const testValue = decodedObj.TestValue ? `${decodedObj.TestValue}${decodedObj.Units || ''}` : '';
        
        return `${parametricKey}: ${testValue} (${testResult})`;
      } else {
        // 返回JSON字符串，但确保不出现[object Object]
        return JSON.stringify(decodedObj);
      }
    }
    
    // 处理基本类型数据
    if (typeof parsedData === 'string') {
      return decodeUnicode(parsedData);
    }
    
    return String(parsedData);
  } catch (error) {
    console.error('解析测试数据错误:', error);
    // 如果解析失败，尝试仅进行Unicode解码
    if (typeof testData === 'string') {
      return decodeUnicode(testData);
    }
    // 确保不返回[object Object]，而是返回有意义的字符串
    if (typeof testData === 'object' && testData !== null) {
      // 如果是对象，尝试转换为字符串
      try {
        return JSON.stringify(testData);
      } catch (e) {
        // 如果JSON.stringify也失败，返回对象的类型信息
        return `[${Object.keys(testData).length}个属性的对象]`;
      }
    }
    return String(testData);
  }
};

/**
 * SN表追溯页面
 * 功能：查询当前SN表数据，双击查看历史SN表数据
 */
const MesSnListTracePage: React.FC = () => {
  // 表格操作引用
  const actionRef = useRef<ActionType | null>(null);
  // 消息提示
  const [messageApi, contextHolder] = message.useMessage();
  // 当前选中行
  const [currentRow, setCurrentRow] = useState<MesSnListCurrentDto | null>(null);
  // 历史数据列表
  const [historyData, setHistoryData] = useState<MesSnListHistoryDto[]>([]);
  // 历史数据加载状态
  const [historyLoading, setHistoryLoading] = useState(false);
  // 是否显示历史记录详情
  const [showDetail, setShowDetail] = useState(false);

  // 保存所有数据用于导出
  const [allData, setAllData] = useState<MesSnListCurrentDto[]>([]);
  // 保存筛选后的数据用于导出
  const [filteredData, setFilteredData] = useState<MesSnListCurrentDto[]>([]);

  // 保存当前页面的搜索参数
  const [currentSearchParams, setCurrentSearchParams] = useState<MesSnListCurrentQueryDto>({});

  // 缓存编码信息，避免重复调用API
  const [codeCache, setCodeCache] = useState<{
    product: Record<string, string>;
    device: Record<string, string>;
  }>({
    product: {},
    device: {},
  });

  /**
   * 显示历史记录
   * @param record 当前SN记录
   */
  const handleShowHistory = useCallback(async (record: MesSnListCurrentDto) => {
    try {
      setCurrentRow(record);
      setHistoryLoading(true);
      // 根据SN号查询历史记录，设置一个足够大的pageSize以获取所有数据
      const response = await getMesSnListHistoryList({
        snNumber: record.snNumber,
        pageIndex: 1,
        pageSize: 10000,
      });

      // 为每个历史记录获取产品编码、工单编码、站点编码和产线编码
      const enhancedHistoryData = await Promise.all(
        (response.data || []).map(async (historyRecord: MesSnListHistoryDto) => {
          let updatedRecord = { ...historyRecord };

          // 获取产品编码
          if (historyRecord.productListId) {
            try {
              const product = await getProductListById(historyRecord.productListId);
              updatedRecord = {
                ...updatedRecord,
                productCode: product.productCode || ''
              };
            } catch (error) {
              console.error('获取产品编码失败:', error);
            }
          }

          // 获取工单编码
          if (historyRecord.orderListId) {
            try {
              const order = await getOrderById(historyRecord.orderListId);
              updatedRecord = {
                ...updatedRecord,
                orderCode: order.orderCode || ''
              };
            } catch (error) {
              console.error('获取工单编码失败:', error);
            }
          }

          // 获取站点编码
          if (historyRecord.currentStationListId) {
            try {
              const station = await getStationListById(historyRecord.currentStationListId);
              updatedRecord = {
                ...updatedRecord,
                stationCode: station.stationCode || ''
              };
            } catch (error) {
              console.error('获取站点编码失败:', error);
            }
          }

          // 获取产线编码
          if (historyRecord.productionLineId) {
            try {
              const response = await getProductionLineById(historyRecord.productionLineId);
              const line = response.data;
              updatedRecord = {
                ...updatedRecord,
                productionLineCode: line.productionLineCode || ''
              };
            } catch (error) {
              console.error('获取产线编码失败:', error);
            }
          }

          // 获取设备编码
          if (historyRecord.resourceId) {
            try {
              const response = await getDeviceInfoById(historyRecord.resourceId);
              const device = response.data;
              updatedRecord = {
                ...updatedRecord,
                deviceEnCode: device.resource || ''
              };
            } catch (error) {
              console.error('获取设备编码失败:', error);
            }
          }

          return updatedRecord;
        })
      );

      setHistoryData(enhancedHistoryData);
      setShowDetail(true);
    } catch (error) {
      messageApi.error('获取历史记录失败');
      console.error('获取历史记录失败:', error);
    } finally {
      setHistoryLoading(false);
    }
  }, [messageApi]);

  /**
   * 双击行处理函数
   * @param record 当前SN记录
   */
  const handleRowDoubleClick = useCallback((record: MesSnListCurrentDto) => {
    handleShowHistory(record);
  }, [handleShowHistory]);

  /**
   * 当前SN表列定义
   */
  const currentColumns: ProColumns<MesSnListCurrentDto>[] = [
    {
      title: 'SN号',
      dataIndex: 'snNumber',
      key: 'snNumber',
      width: 180,
      search: true,
    },
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 150,
      search: true,
      render: (productCode) => productCode || '-',
    },
    {
      title: '工单编码',
      dataIndex: 'orderCode',
      key: 'orderCode',
      width: 150,
      search: true,
      render: (orderCode) => orderCode || '-',
    },
    {
      title: '当前状态',
      dataIndex: 'stationStatus',
      key: 'stationStatus',
      width: 120,
      valueEnum: {
        1: { text: '合格', status: 'Success' },
        2: { text: '不合格', status: 'Error' },
        3: { text: '已包装', status: 'Default' },
        4: { text: '已入库', status: 'Default' },
        5: { text: '跳站', status: 'Default' },
      },
      search: true,
    },
    {
      title: '当前站点编码',
      dataIndex: 'stationCode',
      key: 'stationCode',
      width: 150,
      search: true,
      render: (stationCode) => stationCode || '-',
    },
    {
      title: '产线编码',
      dataIndex: 'productionLineCode',
      key: 'productionLineCode',
      width: 120,
      search: true,
      render: (productionLineCode) => productionLineCode || '-',
    },
    {
      title: '设备编码',
      dataIndex: 'deviceEnCode',
      key: 'deviceEnCode',
      width: 150,
      search: true,
      render: (deviceEnCode) => deviceEnCode || '-',
    },
    {
      title: '是否异常',
      dataIndex: 'isAbnormal',
      key: 'isAbnormal',
      width: 100,
      valueEnum: {
        true: { text: '是', status: 'Error' },
        false: { text: '否', status: 'Success' },
      },
      search: {
        transform: (value) => value,
      },
    },
    {
      title: '异常代码',
      dataIndex: 'abnormalCode',
      key: 'abnormalCode',
      width: 120,
      search: false
    },
    {
      title: '异常描述',
      dataIndex: 'abnormalDescription',
      key: 'abnormalDescription',
      width: 200,
      search: false,
    },
    {
      title: '是否锁定',
      dataIndex: 'isLocked',
      key: 'isLocked',
      width: 100,
      valueEnum: {
        true: { text: '是', status: 'Default' },
        false: { text: '否', status: 'Success' },
      },
      search: {
        transform: (value) => value,
      },
    },
    {
      title: '返工次数',
      dataIndex: 'reworkCount',
      key: 'reworkCount',
      width: 100,
      search: false,
    },
    {
      title: '是否正在返工',
      dataIndex: 'isReworking',
      key: 'isReworking',
      width: 120,
      valueEnum: {
        true: { text: '是', status: 'Default' },
        false: { text: '否', status: 'Success' },
      },
      search: {
        transform: (value) => value,
      },
    },
    {
      title: '返工原因',
      dataIndex: 'reworkReason',
      key: 'reworkReason',
      width: 200,
      search: false,
    },
    {
      title: '返工时间',
      key: 'reworkTimeRange',
      dataIndex: 'reworkTime',
      valueType: 'dateTimeRange',
      fieldProps: {
        format: 'YYYY-MM-DD HH:mm:ss',
        placeholder: ['开始时间', '结束时间'],
        showTime: true,
      },
      hideInTable: true, // 不显示在表格中
      search: {
        transform: (value: [string, string]) => ({
          // 将选择的返工时间范围转换为 reworkStartTime 和 reworkEndTime
          reworkStartTime: value[0],
          reworkEndTime: value[1],
        }),
      },
    },
    {
      title: '返工时间',
      dataIndex: 'reworkTime',
      key: 'reworkTimeActual',
      width: 180,
      search: false,
      valueType: 'dateTime',
      fieldProps: {
        format: 'YYYY-MM-DD HH:mm:ss',
      },
      render: (reworkTime) => reworkTime ? reworkTime : '-',
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      search: false,
    },
    {
      title: '创建时间',
      key: 'createTimeRange',
      dataIndex: 'createTime',
      valueType: 'dateTimeRange',
      fieldProps: {
        format: 'YYYY-MM-DD HH:mm:ss',
        placeholder: ['开始时间', '结束时间'],
        showTime: true,
      },
      hideInTable: true, // 不显示在表格中
      search: {
        transform: (value: [string, string]) => ({
          // 将选择的创建时间范围转换为 startTime 和 endTime
          startTime: value[0],
          endTime: value[1],
        }),
      },
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTimeActual',
      width: 180,
      search: false,
      valueType: 'dateTime',
      fieldProps: {
        format: 'YYYY-MM-DD HH:mm:ss',
      },
      render: (createTime) => createTime ? createTime : '-',
    },
  ];

  /**
   * 历史SN表列定义
   */
  const historyColumns: ProColumns<MesSnListHistoryDto>[] = [
    {
      title: 'SN号',
      dataIndex: 'snNumber',
      key: 'snNumber',
      width: 180,
      search: false,
    },
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 150,
      search: false,
      render: (productCode) => productCode || '-',
    },
    {
      title: '工单编码',
      dataIndex: 'orderCode',
      key: 'orderCode',
      width: 150,
      search: false,
      render: (orderCode) => orderCode || '-',
    },
    {
      title: '当前状态',
      dataIndex: 'stationStatus',
      key: 'stationStatus',
      width: 120,
      valueEnum: {
        1: { text: '合格', status: 'Success' },
        2: { text: '不合格', status: 'Error' },
        3: { text: '已包装', status: 'Default' },
        4: { text: '已入库', status: 'Default' },
        5: { text: '跳站', status: 'Default' },
      },
    },
    {
      title: '当前站点编码',
      dataIndex: 'stationCode',
      key: 'stationCode',
      width: 150,
      search: false,
      render: (stationCode) => stationCode || '-',
    },
    {
      title: '产线编码',
      dataIndex: 'productionLineCode',
      key: 'productionLineCode',
      width: 120,
      search: false,
      render: (productionLineCode) => productionLineCode || '-',
    },
    {
      title: '设备编码',
      dataIndex: 'deviceEnCode',
      key: 'deviceEnCode',
      width: 150,
      search: false,
      render: (deviceEnCode) => deviceEnCode || '-',
    },
    {
      title: '测试数据',
      dataIndex: 'testData',
      key: 'testData',
      width: 150,
      search: false,
      ellipsis: true,
      render: (testData) => formatTestData(testData),
    },
    {
      title: '批次数据',
      dataIndex: 'batchResults',
      key: 'batchResults',
      width: 150,
      search: false,
      ellipsis: true,
      render: (batchResults) => formatTestData(batchResults),
    },
    {
      title: '是否异常',
      dataIndex: 'isAbnormal',
      key: 'isAbnormal',
      width: 100,
      valueEnum: {
        true: { text: '是', status: 'Error' },
        false: { text: '否', status: 'Success' },
      },
      search: {
        transform: (value) => value,
      },
    },
    {
      title: '异常代码',
      dataIndex: 'abnormalCode',
      key: 'abnormalCode',
      width: 120,
      search: false,
    },
    {
      title: '异常描述',
      dataIndex: 'abnormalDescription',
      key: 'abnormalDescription',
      width: 200,
      search: false,
    },
    {
      title: '是否锁定',
      dataIndex: 'isLocked',
      key: 'isLocked',
      width: 100,
      valueEnum: {
        true: { text: '是', status: 'Default' },
        false: { text: '否', status: 'Success' },
      },
      search: {
        transform: (value) => value,
      },
    },
    {
      title: '返工次数',
      dataIndex: 'reworkCount',
      key: 'reworkCount',
      width: 100,
      search: false,
    },
    {
      title: '是否正在返工',
      dataIndex: 'isReworking',
      key: 'isReworking',
      width: 120,
      valueEnum: {
        true: { text: '是', status: 'Default' },
        false: { text: '否', status: 'Success' },
      },
      search: {
        transform: (value) => value,
      },
    },
    {
      title: '返工原因',
      dataIndex: 'reworkReason',
      key: 'reworkReason',
      width: 200,
      search: false,
    },
    {
      title: '返工时间',
      dataIndex: 'reworkTime',
      key: 'reworkTimeActual',
      width: 180,
      search: false,
      valueType: 'dateTime',
      fieldProps: {
        format: 'YYYY-MM-DD HH:mm:ss',
      },
      render: (reworkTime) => reworkTime ? reworkTime : '-',
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      search: false,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      width: 180,
      search: false,
      valueType: 'dateTime',
      fieldProps: {
        format: 'YYYY-MM-DD HH:mm:ss',
      },
      render: (createTime) => createTime ? createTime : '-',
    },
  ];

  /**
   * 导出历史SN数据为Excel
   */
  // 全量导出当前SN数据
  const handleExportCurrentData = useCallback(async () => {
    try {
      messageApi.loading('正在导出数据...');

      // 分页获取所有数据
      const pageSize = 100; // 每页大小，与服务层限制保持一致
      let currentPage = 1;
      let totalCount = 0;
      let allData: MesSnListCurrentDto[] = [];

      // 创建基础查询参数
      const baseQueryParams: MesSnListCurrentQueryDto = {
        ...currentSearchParams,
        pageSize: pageSize
      };

      // 第一次请求获取总数和第一页数据
      const firstResponse = await getMesSnListCurrentList({
        ...baseQueryParams,
        pageIndex: currentPage
      });

      if (firstResponse.success && firstResponse.data) {
        allData = [...firstResponse.data];
        totalCount = firstResponse.total || 0;

        // 使用后端返回的实际PageSize（如果有的话），否则使用前端设定值
        const actualPageSize = firstResponse.pageSize || pageSize;

        // 计算总页数
        const totalPages = Math.ceil(totalCount / actualPageSize);

        // 如果有更多页，继续获取
        if (totalPages > 1) {
          const remainingPages = Array.from({ length: totalPages - 1 }, (_, i) => i + 2);

          // 并发请求剩余页数据
          const pagePromises = remainingPages.map(page =>
            getMesSnListCurrentList({ ...baseQueryParams, pageIndex: page })
          );

          const responses = await Promise.all(pagePromises);

          // 合并所有数据
          responses.forEach(response => {
            if (response.success && response.data) {
              allData = [...allData, ...response.data];
            }
          });
        }
      }

      // 为每个记录获取产品编码、工单编码、站点编码、产线编码和设备编码
      const enhancedData = await Promise.all(
        allData.map(async (record: MesSnListCurrentDto) => {
          let updatedRecord = { ...record };

          // 获取产品编码
          if (record.productListId) {
            try {
              // 使用缓存
              if (codeCache.product[record.productListId]) {
                updatedRecord.productCode = codeCache.product[record.productListId];
              } else {
                const product = await getProductListById(record.productListId);
                updatedRecord.productCode = product.productCode || '';
                setCodeCache(prev => ({
                  ...prev,
                  product: {
                    ...prev.product,
                    [record.productListId]: product.productCode || ''
                  }
                }));
              }
            } catch (error) {
              console.error('获取产品编码失败:', error);
            }
          }

          // 获取工单编码
          if (record.orderListId) {
            try {
              const order = await getOrderById(record.orderListId);
              updatedRecord.orderCode = order.orderCode || '';
            } catch (error) {
              console.error('获取工单编码失败:', error);
            }
          }

          // 获取站点编码
          if (record.currentStationListId) {
            try {
              const station = await getStationListById(record.currentStationListId);
              updatedRecord.stationCode = station.stationCode || '';
            } catch (error) {
              console.error('获取站点编码失败:', error);
            }
          }

          // 获取产线编码
          if (record.productionLineId) {
            try {
              const response = await getProductionLineById(record.productionLineId);
              const line = response.data;
              updatedRecord.productionLineCode = line.productionLineCode || '';
            } catch (error) {
              console.error('获取产线编码失败:', error);
            }
          }

          // 获取设备编码
          if (record.resourceId) {
            try {
              // 使用缓存
              if (codeCache.device[record.resourceId]) {
                updatedRecord.deviceEnCode = codeCache.device[record.resourceId];
              } else {
                const response = await getDeviceInfoById(record.resourceId);
                const device = response.data;
                updatedRecord.deviceEnCode = device.resource || '';
                setCodeCache(prev => ({
                  ...prev,
                  device: {
                    ...prev.device,
                    [record.resourceId]: device.resource || ''
                  }
                }));
              }
            } catch (error) {
              console.error('获取设备编码失败:', error);
            }
          }

          return updatedRecord;
        })
      );

      // 过滤掉需要导出的列（排除隐藏列、没有dataIndex的列和操作列）
      const exportColumns = currentColumns
        .filter(col => col.dataIndex && col.title && col.key !== 'operation' && col.hideInTable !== true)
        .map(col => ({
          key: col.dataIndex as string,
          title: col.title as string,
          formatter: col.render as ((value: any) => string) | undefined
        }));

      // 转换数据
      const dataRows = convertObjectsToRows(enhancedData, exportColumns);

      // 创建信息行
      const exportTime = new Date().toLocaleString();
      const infoRows = createInfoRows(exportTime, currentSearchParams, enhancedData.length);

      // 导出配置
      const config = {
        infoRows,
        headers: exportColumns.map(col => col.title),
        dataRows,
        filename: `SN表追溯_${exportTime.replace(/[\s:]/g, '-')}`
      };

      // 执行导出
      exportToExcel(config);

      messageApi.success('数据导出成功');
    } catch (error) {
      messageApi.error('数据导出失败');
      console.error('导出数据失败:', error);
    }
  }, [currentSearchParams, currentColumns, messageApi]);

  const handleExportHistory = useCallback(async () => {
    try {
      messageApi.loading('正在导出历史数据...');

      // 创建历史数据查询参数
      const historyQueryParams: MesSnListHistoryQueryDto = {
        pageIndex: 1,
        pageSize: 1000, // 设置一个很大的值，确保获取所有数据
      };

      // 应用当前页面的创建时间范围筛选
      if (currentSearchParams.startTime) {
        historyQueryParams.createTimeStart = currentSearchParams.startTime;
      }
      if (currentSearchParams.endTime) {
        historyQueryParams.createTimeEnd = currentSearchParams.endTime;
      }

      // 调用API获取历史数据
      const response = await getMesSnListHistoryList(historyQueryParams);
      const historyData = response.data || [];

      // 为每个历史记录获取产品编码、工单编码、站点编码和产线编码
      const enhancedHistoryData = await Promise.all(
        historyData.map(async (historyRecord: MesSnListHistoryDto) => {
          let updatedRecord = { ...historyRecord };

          // 获取产品编码
          if (historyRecord.productListId) {
            try {
              const product = await getProductListById(historyRecord.productListId);
              updatedRecord = {
                ...updatedRecord,
                productCode: product.productCode || ''
              };
            } catch (error) {
              console.error('获取产品编码失败:', error);
            }
          }

          // 获取工单编码
          if (historyRecord.orderListId) {
            try {
              const order = await getOrderById(historyRecord.orderListId);
              updatedRecord = {
                ...updatedRecord,
                orderCode: order.orderCode || ''
              };
            } catch (error) {
              console.error('获取工单编码失败:', error);
            }
          }

          // 获取站点编码
          if (historyRecord.currentStationListId) {
            try {
              const station = await getStationListById(historyRecord.currentStationListId);
              updatedRecord = {
                ...updatedRecord,
                stationCode: station.stationCode || ''
              };
            } catch (error) {
              console.error('获取站点编码失败:', error);
            }
          }

          // 获取产线编码
          if (historyRecord.productionLineId) {
            try {
              const response = await getProductionLineById(historyRecord.productionLineId);
              const line = response.data;
              updatedRecord = {
                ...updatedRecord,
                productionLineCode: line.productionLineCode || ''
              };
            } catch (error) {
              console.error('获取产线编码失败:', error);
            }
          }

          // 获取设备编码
          if (historyRecord.resourceId) {
            try {
              const response = await getDeviceInfoById(historyRecord.resourceId);
              const device = response.data;
              updatedRecord = {
                ...updatedRecord,
                deviceEnCode: device.resource || ''
              };
            } catch (error) {
              console.error('获取设备编码失败:', error);
            }
          }

          return updatedRecord;
        })
      );

      // 使用直接导入的导出函数来实现导出
      const exportButtonProps = {
        data: enhancedHistoryData,
        columns: historyColumns,
        filename: 'SN历史记录',
        searchParams: {},
        buttonText: '导出历史SN数据为Excel'
      };

      // 过滤掉需要导出的列（排除隐藏列、没有dataIndex的列和操作列）
      const exportColumns = historyColumns
        .filter(col => col.dataIndex && col.title && col.key !== 'operation' && col.hideInTable !== true)
        .map(col => ({
          key: col.dataIndex as string, // 明确转换为string类型
          title: col.title as string, // 明确转换为string类型
          formatter: col.render as ((value: any) => string) | undefined // 明确转换类型
        }));

      // 转换数据
      const dataRows = convertObjectsToRows(enhancedHistoryData, exportColumns);

      // 创建信息行
      const exportTime = new Date().toLocaleString();
      const infoRows = createInfoRows(exportTime, historyQueryParams, enhancedHistoryData.length);

      // 导出配置
      const config = {
        infoRows,
        headers: exportColumns.map(col => col.title),
        dataRows,
        filename: `${exportButtonProps.filename}_${exportTime.replace(/[\s:]/g, '-')}`
      };

      // 执行导出
      exportToExcel(config);

      messageApi.success('历史数据导出成功');
    } catch (error) {
      messageApi.error('历史数据导出失败');
      console.error('导出历史数据失败:', error);
    }
  }, [currentSearchParams, historyColumns, messageApi]);

  return (
    <>
      {contextHolder}
      <PageContainer
        title="SN表追溯"
        extra={
          <>
            <Button
              type="primary"
              icon={<DownloadOutlined />}
              onClick={handleExportCurrentData}
              style={{ marginBottom: 16, marginRight: 10 }}
            >
              导出当前SN数据为Excel
            </Button>
            <Button
              type="primary"
              icon={<DownloadOutlined />}
              onClick={handleExportHistory}
              style={{ marginBottom: 16 }}
            >
              导出历史SN数据为Excel
            </Button>
          </>
        }
      >
        <ProTable<MesSnListCurrentDto>
          actionRef={actionRef}
          rowKey="snListCurrentId"
          columns={currentColumns}
          search={{
            labelWidth: 120,
            layout: 'vertical',
          }}
          request={async (
            params
          ): Promise<RequestData<MesSnListCurrentDto>> => {
            // 使用后端分页：将分页参数和搜索条件传递给后端
            // ✅ 直接使用 ProTable 传入的分页参数
            const queryParams: MesSnListCurrentQueryDto = {
              pageIndex: params.current || 1,
              pageSize: params.pageSize || 10,
              snNumber: params.snNumber,
              stationStatus: params.stationStatus,
              isAbnormal: params.isAbnormal,
              isLocked: params.isLocked,
              isReworking: params.isReworking,
            };

            // 处理返工时间范围参数
            if (params.reworkTime && Array.isArray(params.reworkTime)) {
              queryParams.reworkStartTime = params.reworkTime[0];
              queryParams.reworkEndTime = params.reworkTime[1];
            }

            // 处理创建时间范围参数
            if (params.startTime) {
              queryParams.startTime = params.startTime;
            }
            if (params.endTime) {
              queryParams.endTime = params.endTime;
            }

            // 保存当前搜索参数
            setCurrentSearchParams(queryParams);

            try {
              // 调用 API 获取当前页数据
              const response = await getMesSnListCurrentList(queryParams);

              // 如果请求成功且有数据
              if (response.success && response.data) {
                // 为每个记录获取产品编码、工单编码、站点编码、产线编码和设备编码
                const allData = await Promise.all(
                  response.data.map(async (record: MesSnListCurrentDto) => {
                    let updatedRecord = { ...record };

                    // 获取产品编码
                    if (record.productListId) {
                      try {
                        // 使用缓存
                        if (codeCache.product[record.productListId]) {
                          updatedRecord.productCode = codeCache.product[record.productListId];
                        } else {
                          const product = await getProductListById(record.productListId);
                          updatedRecord.productCode = product.productCode || '';
                          setCodeCache(prev => ({
                            ...prev,
                            product: {
                              ...prev.product,
                              [record.productListId]: product.productCode || ''
                            }
                          }));
                        }
                      } catch (error) {
                        console.error('获取产品编码失败:', error);
                      }
                    }

                    // 获取工单编码
                    if (record.orderListId) {
                      try {
                        const order = await getOrderById(record.orderListId);
                        updatedRecord.orderCode = order.orderCode || '';
                      } catch (error) {
                        console.error('获取工单编码失败:', error);
                      }
                    }

                    // 获取站点编码
                    if (record.currentStationListId) {
                      try {
                        const station = await getStationListById(record.currentStationListId);
                        updatedRecord.stationCode = station.stationCode || '';
                      } catch (error) {
                        console.error('获取站点编码失败:', error);
                      }
                    }

                    // 获取产线编码
                    if (record.productionLineId) {
                      try {
                        const response = await getProductionLineById(record.productionLineId);
                        const line = response.data;
                        updatedRecord.productionLineCode = line.productionLineCode || '';
                      } catch (error) {
                        console.error('获取产线编码失败:', error);
                      }
                    }

                    // 获取设备编码
                    if (record.resourceId) {
                      try {
                        // 使用缓存
                        if (codeCache.device[record.resourceId]) {
                          updatedRecord.deviceEnCode = codeCache.device[record.resourceId];
                        } else {
                          const response = await getDeviceInfoById(record.resourceId);
                          const device = response.data;
                          updatedRecord.deviceEnCode = device.resource || '';
                          setCodeCache(prev => ({
                            ...prev,
                            device: {
                              ...prev.device,
                              [record.resourceId]: device.resource || ''
                            }
                          }));
                        }
                      } catch (error) {
                        console.error('获取设备编码失败:', error);
                      }
                    }

                    return updatedRecord;
                  })
                );

                // 保存当前页数据用于导出
                setFilteredData(allData);

                // 返回后端分页数据
                return {
                  data: allData,
                  total: response.total || 0,
                  success: true,
                };
              }

              // 如果请求失败
              return {
                data: [],
                total: 0,
                success: false,
              };
            } catch (error) {
              messageApi.error('获取数据失败');
              console.error('获取数据失败:', error);
              return {
                data: [],
                total: 0,
                success: false,
              };
            }
          }}
          pagination={{
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50'],
            defaultPageSize: 10, // 关键：用 defaultPageSize 而不是 pageSize
          }}
          onRow={(record) => ({
            onDoubleClick: () => handleRowDoubleClick(record),
          })}
        />

        {/* 历史记录详情 */}
        {showDetail && currentRow && (
          <div className="sn-history-detail" style={{ marginTop: 20, padding: 20, borderTop: '1px solid #e8e8e8' }}>
            <h3 style={{ marginBottom: 16 }}>SN号: {currentRow.snNumber} 的历史记录</h3>
            <ProTable<MesSnListHistoryDto>
              rowKey="snListHistoryId"
              columns={historyColumns}
              scroll={{ x: 'max-content', y: 600 }} // 设置固定高度，超出部分滚动
              request={async (params) => {
                // 使用前端筛选：基于已获取的历史数据进行筛选
                let filteredData = historyData;

                // SN号搜索
                if (params.snNumber) {
                  filteredData = filteredData.filter(item =>
                    item.snNumber?.toLowerCase().includes(params.snNumber.toLowerCase())
                  );
                }

                // 产品编码搜索
                if (params.productCode) {
                  filteredData = filteredData.filter(item =>
                    item.productCode?.toLowerCase().includes(params.productCode.toLowerCase())
                  );
                }

                // 工单编码搜索
                if (params.orderCode) {
                  filteredData = filteredData.filter(item =>
                    item.orderCode?.toLowerCase().includes(params.orderCode.toLowerCase())
                  );
                }

                // 当前状态搜索
                if (params.stationStatus) {
                  filteredData = filteredData.filter(item =>
                    item.stationStatus === params.stationStatus
                  );
                }

                // 站点编码搜索
                if (params.stationCode) {
                  filteredData = filteredData.filter(item =>
                    item.stationCode?.toLowerCase().includes(params.stationCode.toLowerCase())
                  );
                }

                // 产线编码搜索
                if (params.productionLineCode) {
                  filteredData = filteredData.filter(item =>
                    item.productionLineCode?.toLowerCase().includes(params.productionLineCode.toLowerCase())
                  );
                }

                // 设备编码搜索
                if (params.deviceEnCode) {
                  filteredData = filteredData.filter(item =>
                    item.deviceEnCode?.toLowerCase().includes(params.deviceEnCode.toLowerCase())
                  );
                }

                // 是否异常搜索
                if (params.isAbnormal !== undefined) {
                  filteredData = filteredData.filter(item =>
                    item.isAbnormal === params.isAbnormal
                  );
                }

                // 是否锁定搜索
                if (params.isLocked !== undefined) {
                  filteredData = filteredData.filter(item =>
                    item.isLocked === params.isLocked
                  );
                }

                // 是否正在返工搜索
                if (params.isReworking !== undefined) {
                  filteredData = filteredData.filter(item =>
                    item.isReworking === params.isReworking
                  );
                }

                // 不分页，返回所有数据
                return {
                  data: filteredData,
                  total: filteredData.length,
                  success: true,
                };
              }}
              loading={historyLoading}
              pagination={false} // 禁用分页
              tableAlertRender={() => (
                <div style={{ marginBottom: 8 }}>
                  共 {historyData.length} 条历史记录
                </div>
              )}
            />
          </div>
        )}
      </PageContainer>
    </>
  );
};

export default MesSnListTracePage;