import React, { useState, useMemo, useEffect } from 'react';
import { Card, Row, Col, DatePicker, Button, Select, Statistic } from 'antd';
import { useRequest } from '@umijs/max';
import dayjs from 'dayjs';
import { Column, Line, Pie } from '@ant-design/plots';


import {
  getHourlyOutput,
  getStationNGStatistics,
  calculateFirstPassYield,
  calculateQualityRate,
  getTopDefects
} from './service';

// 导入生产线API
import { getProductionLineList } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';
import type { ProductionLineQueryParams, productionLine, PagedResult } from '@/services/Model/Trace/ProductionEquipment‌/productionLineInfo';

const { RangePicker } = DatePicker;
const { Option } = Select;

const Analysis: React.FC = () => {
  // 时间范围状态
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null]>([
    dayjs().startOf('day'),
    dayjs().endOf('day'),
  ]);
  // 生产线选择状态
  const [selectedProductionLine, setSelectedProductionLine] = useState<string>('');
  // 生产线列表数据
  const [productionLines, setProductionLines] = useState<productionLine[]>([]);


  // 处理时间范围选择
  const handleDateRangeChange = (
    dates: (dayjs.Dayjs | null)[] | (dayjs.Dayjs | null) | null,
  ) => {
    if (dates && Array.isArray(dates) && dates.length === 2) {
      setDateRange([dates[0], dates[1]]);
    } else if (dates && !Array.isArray(dates)) {
      // 单个日期选择的情况
      setDateRange([dates, null]);
    } else {
      setDateRange([null, null]);
    }
  };

  // 时间格式化为字符串
  const formatDateTime = (date: dayjs.Dayjs | null): string => {
    return date ? date.format('YYYY-MM-DD HH:mm:ss') : '';
  };

  // 获取当日每小时产出统计
  const {
    data: hourlyOutputData,
    loading: hourlyOutputLoading,
    run: runHourlyOutput
  } = useRequest(
    () => getHourlyOutput(selectedProductionLine, undefined, formatDateTime(dateRange[0]), formatDateTime(dateRange[1])),
    { manual: true, refreshDeps: [dateRange, selectedProductionLine] }
  );



  // 计算一次通过率
  const {
    data: firstPassYieldData,
    loading: firstPassYieldLoading,
    run: runFirstPassYield
  } = useRequest(
    () => calculateFirstPassYield(
      formatDateTime(dateRange[0]),
      formatDateTime(dateRange[1]),
      selectedProductionLine
    ),
    { manual: true, refreshDeps: [dateRange, selectedProductionLine] }
  );

  // 计算合格率/不良率
  const {
    data: qualityRateData,
    loading: qualityRateLoading,
    run: runQualityRate
  } = useRequest(
    () => calculateQualityRate(
      formatDateTime(dateRange[0]),
      formatDateTime(dateRange[1]),
      selectedProductionLine
    ),
    { manual: true, refreshDeps: [dateRange, selectedProductionLine] }
  );

  // 获取各站NG件统计
  const {
    data: stationNGData,
    loading: stationNGLoading,
    run: runStationNGStatistics
  } = useRequest(
    () => getStationNGStatistics(
      formatDateTime(dateRange[0]),
      formatDateTime(dateRange[1]),
      selectedProductionLine
    ),
    { manual: true, refreshDeps: [dateRange, selectedProductionLine] }
  );

  // 获取生产线列表
  const {
    data: productionLineData,
    loading: productionLineLoading,
    run: runGetProductionLines
  } = useRequest(
    () => getProductionLineList({ current: 1, pageSize: 100 } as ProductionLineQueryParams),
    { manual: true }
  );

  // 初始加载 & 刷新数据
  useEffect(() => {
    refreshData();
  }, []);

  const refreshData = () => {
    // 先获取生产线列表，再获取其他数据
    runGetProductionLines();
    runHourlyOutput();
    runFirstPassYield();
    runQualityRate();
    runStationNGStatistics();
  };

  // 监听生产线数据变化，更新生产线列表
  useEffect(() => {
    if (productionLineData) {
      setProductionLines(productionLineData);
    }
  }, [productionLineData]);

  // 监听生产线选择变化，自动刷新数据
  useEffect(() => {
    refreshData();
  }, [selectedProductionLine, dateRange]);

  // 仅用于“时间范围：”标签的两行展示（自动每两字换行）
  const renderTwoLineLabel = (label: string) => {
    const arr = Array.from(label);
    const first = arr.slice(0, 2).join('');
    const second = arr.slice(2, 4).join('');
    return (
      <span
        style={{
          display: 'inline-flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          width: 48,
          color: '#E6F7FF',
          lineHeight: 1.1,
          marginRight: 8,
          whiteSpace: 'pre-wrap',
        }}
      >
        <span>{first}</span>
        <span>{second}</span>
      </span>
    );
  };

  // 数据处理辅助函数：安全获取数值
  const safeNumber = (value: any): number => {
    if (value === null || value === undefined) return 0;
    if (typeof value === 'string') {
      if (value.toLowerCase() === 'null' || value.toLowerCase() === 'undefined') return 0;
      return Number(value) || 0;
    }
    if (typeof value === 'number') return value;
    return 0;
  };

  // 处理小时产量数据，添加时间格式验证和无效数据过滤
  const hourlyChartData = useMemo(() => {
    if (!hourlyOutputData) return [];

    return hourlyOutputData
      .map((item: any) => {
        if (item.hour === undefined || item.hour === null) {
          console.warn('缺失Hour字段:', item);
          return null;
        }
        const hour = String(item.hour).padStart(2, '0');
        return {
          timeLabel: `${hour}:00`, // ← 字符串作为 xField
          totalCount: safeNumber(item.outputQuantity),
          okCount: safeNumber(item.passQuantity),
          ngCount: safeNumber(item.failQuantity),
        };
      })
      .filter(Boolean); // 过滤无效数据
  }, [hourlyOutputData]);



  // 处理良率分布数据，添加时间格式验证和无效数据过滤
  const yieldRateData = useMemo(() => {
    if (!hourlyOutputData) return [];

    return hourlyOutputData
      .map((item: any) => {
        if (item.hour === undefined || item.hour === null) {
          console.warn('缺失Hour字段:', item);
          return null;
        }
        const hour = String(item.hour).padStart(2, '0');
        const total = safeNumber(item.outputQuantity);
        const pass = safeNumber(item.passQuantity);
        return {
          timeLabel: `${hour}:00`, // ← 字符串作为 xField
          yieldRate: total > 0 ? (pass / total) * 100 : 0,
        };
      })
      .filter(Boolean); // 过滤无效数据
  }, [hourlyOutputData]);

  // 处理站点NG统计数据，用于饼状图
  const stationNGChartData = useMemo(() => {
    if (!stationNGData) return [];

    // 先过滤出有效数据
    const validData = stationNGData
      .map((item: any) => {
        if (!item.stationName || item.ngCount === undefined || item.ngCount === null || item.ngRate === undefined) {
          console.warn('缺失站点NG数据:', item);
          return null;
        }
        return {
          station: item.stationName,
          value: safeNumber(item.ngCount),
          ngRate: safeNumber(item.ngRate), // 直接使用后端返回的NG率
          rate: safeNumber(item.ngRate) * 100, // 转换为百分比
        };
      })
      .filter(Boolean) as Array<{ station: string; value: number; ngRate: number; rate: number }>;

    // 计算总NG数量
    const totalNGCount = validData.reduce((sum, item) => sum + item.value, 0);

    // 添加百分比字段
    return validData.map(item => ({
      ...item,
      percentage: totalNGCount > 0 ? (item.value / totalNGCount) * 100 : 0
    }));
  }, [stationNGData]);



  // 小时产量统计图配置
  const hourlyChartConfig = {
    data: hourlyChartData,
    xField: 'timeLabel', // ← 字符串作为 xField
    yField: 'totalCount',
    axis: {
      x: {
        visible: true,
        title: {
          visible: true,
          text: '时间',
          style: { fill: '#1890ff', fontSize: 12, fontWeight: 'bold' },
        },
        label: {
          visible: true,
          style: { fill: '#1890ff', fontSize: 12 },
        },
        line: { style: { stroke: '#1890ff' } },
        grid: null,
      },
      y: {
        visible: true,
        title: {
          visible: true,
          text: '产品数量',
          style: { fill: '#1890ff', fontSize: 12, fontWeight: 'bold' },
        },
        label: {
          visible: true,
          style: { fill: '#1890ff', fontSize: 12 },
        },
        line: { style: { stroke: '#1890ff' } },
        grid: { line: { style: { stroke: 'rgba(24,144,255,0.1)' } } },
        min: 0,
      },
    },

    label: {
      visible: true,
    },
    tooltip: {
      title: '小时产量统计',
      showTitle: true,
      shared: true,
      showCrosshairs: true,
      domStyles: {
        'g2-tooltip': {
          background: '#ffffff',
          color: '#1890ff',
          border: '1px solid #1890ff',
          borderRadius: '4px',
          padding: '8px 12px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        },
      },
      formatter: (datum: any) => ({
        name: '总产量',
        value: safeNumber(datum.totalCount),
      }),
    },
    animation: { appear: { animation: 'path-in', duration: 1000 } },
    style: { radiusTopLeft: 10, radiusTopRight: 10 },
  };





  // 良率分布图配置
  const yieldRateConfig = {
    data: yieldRateData,
    xField: 'timeLabel', // ← 字符串作为 xField
    yField: 'yieldRate',
    theme: 'light',
    smooth: true,
    point: {
      size: 5,
      style: {
        fill: '#1890ff',
        stroke: '#1890ff',
        lineWidth: 2,
      },
    },
    line: {
      color: '#1890ff',
      size: 2,
    },

    // ✅ 正确：使用 axis，不是 xAxis/yAxis
    axis: {
      x: {
        visible: true,
        title: {
          visible: true,
          text: '时间',
          style: { fill: '#1890ff', fontSize: 12, fontWeight: 'bold' },
        },
        label: {
          visible: true,
          style: { fill: '#1890ff', fontSize: 12 },
        },
        line: { style: { stroke: '#1890ff' } },
        grid: null,
      },
      y: {
        visible: true,
        title: {
          visible: true,
          text: '良率 (%)',
          style: { fill: '#1890ff', fontSize: 12, fontWeight: 'bold' },
          offset: 60, // 增加偏移量，确保标题不被遮挡
          textAlign: 'center', // 确保标题居中显示
        },
        label: {
          visible: true,
          style: { fill: '#1890ff', fontSize: 12 },
          formatter: (v: string) => `${v}%`,
        },
        line: { style: { stroke: '#1890ff' } },
        grid: { line: { style: { stroke: 'rgba(24,144,255,0.1)' } } },
        min: 80,
        max: 100,
      },
    },

    tooltip: {
      title: '设备良率',
      showTitle: true,
      shared: true,
      showCrosshairs: true,
      domStyles: {
        'g2-tooltip': {
          background: '#ffffff',
          color: '#1890ff',
          border: '1px solid #1890ff',
          borderRadius: '4px',
          padding: '8px 12px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        },
      },
      formatter: (datum: any) => ({
        name: '良率',
        value: `${safeNumber(datum.yieldRate).toFixed(2)}%`,
      }),
    },
  };

  // 计算总NG数
  const totalNGCount = useMemo(() => {
    if (!stationNGChartData || stationNGChartData.length === 0) return 0;
    return stationNGChartData.reduce((sum, item) => sum + item.value, 0);
  }, [stationNGChartData]);

  // 站点NG统计饼状图配置
  const stationNGChartConfig = {
    data: stationNGChartData,
    angleField: 'value',
    colorField: 'station',
    radius: 0.8,
    label: {
      type: 'outer',
      content: '{station}: {value} ({percentage}%)',
      style: {
        fill: '#1890ff',
        fontSize: 12,
      },
    },
    tooltip: {
      title: '站点NG统计',
      formatter: (datum: any) => {
        return [
          { name: '站点', value: datum.station },
          { name: 'NG数量', value: datum.value },
          { name: 'NG率', value: `${datum.rate.toFixed(2)}%` },
          { name: '占比', value: `${datum.percentage.toFixed(2)}%` },
        ];
      },
    },
    legend: {
      position: 'right',
      orient: 'vertical',
      itemName: {
        formatter: (datum: any) => {
          const item = stationNGChartData.find(item => item.station === datum);
          if (item) {
            const yieldRate = (100 - item.rate).toFixed(2); // 使用后端返回的ngRate计算良率
            return `${datum}: ${item.value} (良率: ${yieldRate}%)`;
          }
          return datum;
        },
        style: {
          fill: '#1890ff',
          fontSize: 12,
        },
      },
    },
    // 自动生成不同颜色
    color: [
      '#1890ff', '#52c41a', '#faad14', '#f5222d',
      '#722ed1', '#13c2c2', '#eb2f96', '#fa8c16',
      '#a0d911', '#2f54eb', '#fa541c', '#1890ff'
    ],
  };

  // 添加全局样式
  useEffect(() => {
    // 动态添加样式
    const style = document.createElement('style');
    style.textContent = `
      .custom-range-picker-dropdown .ant-picker-panel {
        background: transparent !important;
        border: none !important;
      }
      .custom-range-picker-dropdown .ant-picker-date-panel,
      .custom-range-picker-dropdown .ant-picker-time-panel,
      .custom-range-picker-dropdown .ant-picker-footer {
        background: #ffffff !important;
        border: 1px solid #f0f0f0 !important;
        border-radius: 8px !important;
      }
      .custom-range-picker-dropdown .ant-picker-header,
      .custom-range-picker-dropdown .ant-picker-time-panel-column,
      .custom-range-picker-dropdown .ant-picker-time-panel-column > li {
        background: transparent !important;
        color: #000000 !important;
        border-color: #f0f0f0 !important;
      }
      .custom-range-picker-dropdown .ant-picker-cell {
        color: rgba(0, 0, 0, 0.65) !important;
      }
      .custom-range-picker-dropdown .ant-picker-cell-in-view {
        color: rgba(0, 0, 0, 0.85) !important;
      }
      .custom-range-picker-dropdown .ant-picker-cell:hover:not(.ant-picker-cell-in-view) {
        color: rgba(0, 0, 0, 0.3) !important;
      }
      .custom-range-picker-dropdown .ant-picker-cell-in-view.ant-picker-cell-today .ant-picker-cell-inner::before {
        border-color: #1890ff !important;
      }
      .custom-range-picker-dropdown .ant-picker-cell-in-view.ant-picker-cell-in-range::before,
      .custom-range-picker-dropdown .ant-picker-cell-in-view.ant-picker-cell-range-hover::before {
        background: rgba(24, 144, 255, 0.1) !important;
      }
      .custom-range-picker-dropdown .ant-picker-cell-in-view.ant-picker-cell-selected .ant-picker-cell-inner,
      .custom-range-picker-dropdown .ant-picker-cell-in-view.ant-picker-cell-range-start .ant-picker-cell-inner,
      .custom-range-picker-dropdown .ant-picker-cell-in-view.ant-picker-cell-range-end .ant-picker-cell-inner {
        background: #1890ff !important;
        color: #ffffff !important;
      }
      .custom-range-picker-dropdown .ant-picker-time-panel-cell .ant-picker-time-panel-cell-inner {
        color: rgba(0, 0, 0, 0.85) !important;
      }
      .custom-range-picker-dropdown .ant-picker-time-panel-cell-selected .ant-picker-time-panel-cell-inner {
        background: #1890ff !important;
        color: #ffffff !important;
      }
      .custom-range-picker-dropdown .ant-picker-now-btn,
      .custom-range-picker-dropdown .ant-picker-ok .ant-btn {
        color: #000000 !important;
        border-color: #d9d9d9 !important;
      }
      .custom-range-picker-dropdown .ant-picker-now-btn:hover,
      .custom-range-picker-dropdown .ant-picker-ok .ant-btn:hover {
        color: #1890ff !important;
        border-color: #1890ff !important;
      }
      .custom-range-picker-dropdown .ant-picker-time-panel-cell-inner:hover {
        background: rgba(0, 0, 0, 0.05) !important;
      }
      .custom-range-picker-dropdown .ant-picker-header-super-prev-btn,
      .custom-range-picker-dropdown .ant-picker-header-prev-btn,
      .custom-range-picker-dropdown .ant-picker-header-next-btn,
      .custom-range-picker-dropdown .ant-picker-header-super-next-btn {
        color: rgba(0, 0, 0, 0.45) !important;
      }
      .custom-range-picker-dropdown .ant-picker-header-super-prev-btn:hover,
      .custom-range-picker-dropdown .ant-picker-header-prev-btn:hover,
      .custom-range-picker-dropdown .ant-picker-header-next-btn:hover,
      .custom-range-picker-dropdown .ant-picker-header-super-next-btn:hover {
        color: #1890ff !important;
      }
    `;
    document.head.appendChild(style);

    return () => {
      // 组件卸载时移除样式
      document.head.removeChild(style);
    };
  }, []);

  // 样式常量定义
  const panelStyles = {
    panelStyle: {
      background: '#ffffff',
      border: '1px solid #d9d9d9',
      borderRadius: 12,
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
      overflow: 'hidden',
    },
    headStyle: {
      background: '#fafafa',
      color: '#333333',
      borderBottom: '1px solid #d9d9d9',
      fontWeight: 600,
    },
    bodyStyle: {
      background: '#ffffff',
      padding: 16,
      color: '#333333',
      height: 400,
    },
    inputStyle: {
      background: '#ffffff',
      border: '1px solid #d9d9d9',
      color: '#000000',
      borderRadius: 6,
      height: 32,
      boxShadow: 'none',
    },
    pickerStyle: {
      background: '#ffffff',
      border: '1px solid #d9d9d9',
      color: '#000000',
      borderRadius: 6,
      height: 32,
      boxShadow: 'none',
      width: 'calc(100% - 80px)'
    }
  };

  return (
    <div className="analysis-page" style={{ padding: 24, minHeight: '100vh' }}>

      {/* 筛选区域 */}
      <div style={{
        marginBottom: 24,
        padding: 16,
        borderRadius: 8,
        background: '#ffffff',
        border: '1px solid #f0f0f0',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <Row gutter={16} justify="space-between" align="middle" style={{ margin: '0 16' }}>
          <Col xs={24} sm={24} md={8} lg={6} xl={5} style={{ display: 'flex', alignItems: 'center', marginBottom: 0 }}>
            <span style={{ marginRight: 0, color: '#000000', fontWeight: 500 }}>生产线：</span>
            <Select
              value={selectedProductionLine}
              onChange={(value) => setSelectedProductionLine(value)}
              style={{ width: '60%', height: 32 }}
              placeholder="请选择生产线"
              allowClear
              loading={productionLineLoading}
            >
              <Option value="">全部</Option>
              {productionLines.map((line) => (
                <Option key={line.productionLineId} value={line.productionLineId}>{line.productionLineName}</Option>
              ))}
            </Select>
          </Col>

          <Col xs={24} sm={24} md={24} lg={6} xl={14} style={{ display: 'flex', alignItems: 'center', marginBottom: 0 }}>
            <span style={{ marginRight: 0, color: '#000000', fontWeight: 500 }}>时间：</span>
            <RangePicker
              value={dateRange}
              onChange={(dates) => {
                if (dates && Array.isArray(dates)) {
                  setDateRange([dates[0], dates[1]]);
                } else {
                  setDateRange([null, null]);
                }
              }}
              showTime
              format="YYYY-MM-DD HH:mm"
              style={{
                flex: 1,
                background: '#ffffff',
                border: '1px solid #d9d9d9',
                borderRadius: 6,
                color: '#000000',
                height: 32,
                boxShadow: 'none'
              }}
              popupStyle={{
                background: '#ffffff',
                border: '1px solid #f0f0f0',
                borderRadius: 8,
                padding: 12,
                color: '#000000',
                boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
              }}
              dropdownClassName="custom-range-picker-dropdown"
              placeholder={['开始时间', '结束时间']}
              allowClear
            />
            <Button
              type="primary"
              onClick={refreshData}
              loading={hourlyOutputLoading || firstPassYieldLoading || qualityRateLoading || stationNGLoading}
              style={{ marginLeft: 8, width: 60 }}
            >
              查询
            </Button>
            <Button
              onClick={() => {
                // 重置时间范围为当天
                setDateRange([dayjs().startOf('day'), dayjs().endOf('day')]);
                // 重置生产线选择
                setSelectedProductionLine('');
                // 刷新数据
                refreshData();
              }}
              style={{ marginLeft: 8, width: 60 }}
            >
              重置
            </Button>
          </Col>
        </Row>
      </div>

      {/* 统计卡片区域 */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={6}>
          <Card style={panelStyles.panelStyle}>
            <div style={{ padding: 24 }}>
              <Statistic
                title="总产出"
                value={hourlyOutputData?.reduce((sum: number, item: any) => sum + safeNumber(item.outputQuantity), 0) || 0}
                valueStyle={{ color: '#3f8600', fontSize: 24 }}
                suffix="件"
              />
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card style={panelStyles.panelStyle}>
            <div style={{ padding: 24 }}>
              <Statistic
                title="一次通过率"
                value={(firstPassYieldData?.firstPassYield || 0) * 100}
                precision={2}
                valueStyle={{ color: '#1890ff', fontSize: 24 }}
                suffix="%"
              />
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card style={panelStyles.panelStyle}>
            <div style={{ padding: 24 }}>
              <Statistic
                title="合格率"
                value={(qualityRateData?.passRate || 0) * 100}
                precision={2}
                valueStyle={{ color: '#13c2c2', fontSize: 24 }}
                suffix="%"
              />
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card style={panelStyles.panelStyle}>
            <div style={{ padding: 24 }}>
              <Statistic
                title="不良率"
                value={(qualityRateData?.failRate || 0) * 100}
                precision={2}
                valueStyle={{ color: '#ff4d4f', fontSize: 24 }}
                suffix="%"
              />
            </div>
          </Card>
        </Col>
      </Row>

      {/* 图表区域 */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        {/* 小时产量统计 */}
        <Col span={14}>
          <Card
            title="小时产量统计"
            style={{
              ...panelStyles.panelStyle,
              height: '100%'
            }}
          >
            <div style={{ height: 400 }}>
              {hourlyChartData.length > 0 ? (
                <Column {...hourlyChartConfig} />
              ) : (
                <div style={{ textAlign: 'center', padding: 40 }}>暂无数据</div>
              )}
            </div>
          </Card>
        </Col>

        {/* 良率分布趋势 */}
        <Col span={10}>
          <Card
            title="良率分布趋势"
            style={{
              ...panelStyles.panelStyle,
              height: '100%'
            }}
          >
            <div style={{ height: 400 }}>
              {yieldRateData.length > 0 ? (
                <Line {...yieldRateConfig} />
              ) : (
                <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>暂无数据</div>
              )}
            </div>
          </Card>
        </Col>
      </Row>

      {/* 站点NG统计区域 */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={12}>
          <Card
            title={
              <div>
                <span>各站NG件统计</span>
                <span style={{ marginLeft: 20, fontSize: 14, color: '#ff4d4f' }}>
                  总NG数: {totalNGCount}
                </span>
              </div>
            }
            style={{
              ...panelStyles.panelStyle,
              height: '100%'
            }}
          >
            <div style={{ height: 400 }}>
              {stationNGChartData.length > 0 ? (
                <Pie {...stationNGChartConfig} />
              ) : (
                <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>暂无数据</div>
              )}
            </div>
          </Card>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginBottom: 16 }}>



      </Row>
    </div>
  );
};

export default Analysis;