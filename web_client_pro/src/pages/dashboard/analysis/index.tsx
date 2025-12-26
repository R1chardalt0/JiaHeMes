import React, { useState, useMemo, useEffect } from 'react';
import { Card, Row, Col, Input, DatePicker, Button } from 'antd';
import { useRequest } from '@umijs/max';
import dayjs from 'dayjs';
import { Column, Line } from '@ant-design/plots';

import { getProductionRecords, getHourlyProductionRecords, getProductionLines } from './service';

const { TextArea } = Input;
const { RangePicker } = DatePicker;

interface ProductionData {
  productionLineName: string;
  deviceName: string;
  totalCount: number;
  okCount: number;
  ngCount: number;
  yieldRate: number;
}

interface HourlyData extends ProductionData {
  hour: string;
}

const Analysis: React.FC = () => {
  // 生产线列表状态
  const [productionLines, setProductionLines] = useState<{ id: number; name: string }[]>([]);
  // 生产线选择状态
  const [selectedProductionLine, setSelectedProductionLine] = useState<string>('');
  // 设备名称状态
  const [deviceName, setDeviceName] = useState<string>('');
  // 资源状态
  const [resource, setResource] = useState<string>('');
  // 时间范围状态
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>([
    dayjs().startOf('day'),
    dayjs().endOf('day'),
  ]);

  // 处理时间范围选择：允许清空，不自动回填
  const handleDateRangeChange = (
    dates: null | [dayjs.Dayjs | null, dayjs.Dayjs | null],
  ) => {
    if (!dates) {
      setDateRange(null);
      return;
    }

    const [start, end] = dates;
    if (!start && !end) {
      setDateRange(null);
      return;
    }

    setDateRange([start ?? null, end ?? null]);
  };

  // 请求参数
  const requestParams = useMemo(() => {
    const [startDate, endDate] = dateRange ?? [null, null];
    return {
      productionLineName: selectedProductionLine,
      deviceName: deviceName.trim(),
      resource: resource.trim(),
      startTime: startDate ? startDate.toDate() : undefined,
      endTime: endDate ? endDate.toDate() : undefined,
    };
  }, [selectedProductionLine, deviceName, resource, dateRange]);

  // 获取生产记录
  const { data: productionRecords, loading, run: runProductionRecords } = useRequest(
    () => getProductionRecords(requestParams),
    { manual: true }
  );

  // 获取按小时统计的生产记录
  const { data: hourlyRecords, run: runHourlyRecords } = useRequest(
    () => getHourlyProductionRecords(requestParams),
    { manual: true }
  );

  // 初始加载 & 刷新数据
  useEffect(() => {
    refreshData();
  }, []);

  const refreshData = () => {
    runProductionRecords();
    runHourlyRecords();
  };

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

  // 处理小时产量数据
  const hourlyChartData = useMemo(() => {
    if (!hourlyRecords) return [];

    const hourlyMap: { [hour: string]: { totalCount: number; okCount: number; ngCount: number } } = {};

    hourlyRecords.forEach((record: HourlyData) => {
      const hourPart = record.hour.split(' ')[1].split(':')[0] + ':00';
      if (!hourlyMap[hourPart]) {
        hourlyMap[hourPart] = { totalCount: 0, okCount: 0, ngCount: 0 };
      }
      hourlyMap[hourPart].totalCount += safeNumber(record.totalCount);
      hourlyMap[hourPart].okCount += safeNumber(record.okCount);
      hourlyMap[hourPart].ngCount += safeNumber(record.ngCount);
    });

    return Object.entries(hourlyMap)
      .sort(([h1], [h2]) => parseInt(h1) - parseInt(h2))
      .map(([hour, value]) => ({
        hour,
        totalCount: value.totalCount,
        okCount: value.okCount,
        ngCount: value.ngCount,
      }));
  }, [hourlyRecords]);

  // 处理设备产量数据
  const deviceProductionData = useMemo(() => {
    if (!productionRecords) return [];

    const deviceMap: { [device: string]: number } = {};

    productionRecords.forEach((record: ProductionData) => {
      const deviceKey = record.deviceName || '未知设备';
      const count = safeNumber(record.totalCount);
      
      if (!deviceMap[deviceKey]) {
        deviceMap[deviceKey] = 0;
      }
      deviceMap[deviceKey] += count;
    });

    return Object.entries(deviceMap).map(([deviceName, value]) => ({
      deviceName,
      value: value || 0 // 确保不会有 null/undefined
    }));
  }, [productionRecords]);

  // 处理良率分布数据
  const yieldRateData = useMemo(() => {
    if (!hourlyRecords) return [];

    const hourlyYieldMap: { [hour: string]: { yieldSum: number; count: number } } = {};

    hourlyRecords.forEach((record: HourlyData) => {
      const hourPart = record.hour.split(' ')[1].split(':')[0] + ':00';
      const yieldRate = safeNumber(record.yieldRate);
      
      if (!hourlyYieldMap[hourPart]) {
        hourlyYieldMap[hourPart] = { yieldSum: 0, count: 0 };
      }
      hourlyYieldMap[hourPart].yieldSum += yieldRate;
      hourlyYieldMap[hourPart].count += 1;
    });

    return Object.entries(hourlyYieldMap)
      .sort(([h1], [h2]) => parseInt(h1) - parseInt(h2))
      .map(([hour, value]) => ({
        time: hour,
        yieldRate: value.count > 0 ? value.yieldSum / value.count : 0,
      }));
  }, [hourlyRecords]);

  // 小时产量统计图配置
  const hourlyChartConfig = {
    data: hourlyChartData,
    xField: 'hour',
    yField: 'totalCount',
    theme: 'dark',
    label: {
      text: (d: any) => `${safeNumber(d.totalCount)}`,
      style: {
        fill: '#fff',
        fontSize: 12,
        fontWeight: 'bold',
        textShadow: '0 1px 2px rgba(0,0,0,0.5)'
      },
      position: 'top',
    },
    xAxis: {
      label: {
        style: {
          fill: '#fff',
          fontSize: 12,
        },
      },
      line: {
        style: {
          stroke: 'rgba(255,255,255,0.2)',
        },
      },
      grid: {
        line: {
          style: {
            stroke: 'rgba(255,255,255,0.1)',
          },
        },
      },
    },
    yAxis: {
      label: {
        style: {
          fill: '#fff',
          fontSize: 12,
        },
        formatter: (v: string) => v,
      },
      line: {
        style: {
          stroke: 'rgba(255,255,255,0.2)',
        },
      },
      grid: {
        line: {
          style: {
            stroke: 'rgba(255,255,255,0.1)',
          },
        },
      },
    },
    tooltip: {
      title: '小时产量统计',
      showTitle: true,
      showMarkers: true,
      shared: true,
      showCrosshairs: true,
      crosshairs: {
        line: {
          style: {
            stroke: '#fff',
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
      formatter: (datum: any) => {
        return {
          name: '总产量',
          value: safeNumber(datum.totalCount),
        };
      },
    },
    animation: { appear: { animation: 'path-in', duration: 1000 } },
    style: { radiusTopLeft: 10, radiusTopRight: 10 },
  };

  // 设备产量统计图配置
  const deviceProductionConfig = {
    data: deviceProductionData,
    xField: 'deviceName',
    yField: 'value',
    theme: 'dark',
    columnStyle: {
      radius: [2, 2, 0, 0],
    },
    barWidth: 30,
    label: {
      position: 'top',
      style: {
        fill: '#fff',
        fontSize: 12,
        fontWeight: 'bold',
        textShadow: '0 1px 2px rgba(0,0,0,0.5)'
      },
      formatter: (datum: any) => `${safeNumber(datum.value)}`,
    },
    tooltip: {
      title: '设备产量',
      showTitle: true,
      showMarkers: true,
      shared: true,
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
      formatter: (datum: any) => {
        return {
          name: '产量',
          value: safeNumber(datum.value)
        };
      },
    },
    axis: {
      x: {
        label: {
          autoRotate: true,
          autoHide: true,
          autoEllipsis: true,
        },
      },
      y: {
        label: {
          formatter: (v: string) => v
        },
      },
    },
  };

  // 良率分布图配置
  const yieldRateConfig = {
    data: yieldRateData,
    xField: 'time',
    yField: 'yieldRate',
    theme: 'dark',
    smooth: true,
    point: { 
      size: 5,
      style: {
        fill: '#fff',
        stroke: '#1890ff',
        lineWidth: 2,
      },
    },
    line: {
      color: '#1890ff',
      size: 2,
    },
    xAxis: {
      label: {
        style: {
          fill: '#fff',
          fontSize: 12,
        },
      },
      line: {
        style: {
          stroke: 'rgba(255,255,255,0.2)',
        },
      },
      grid: {
        line: {
          style: {
            stroke: 'rgba(255,255,255,0.1)',
          },
        },
      },
    },
    yAxis: {
      label: { 
        formatter: (v: string) => `${v}%`,
        style: {
          fill: '#fff',
          fontSize: 12,
        },
      },
      line: {
        style: {
          stroke: 'rgba(255,255,255,0.2)',
        },
      },
      grid: {
        line: {
          style: {
            stroke: 'rgba(255,255,255,0.1)',
          },
        },
      },
      min: 80,
      max: 100,
    },
    tooltip: {
      title: '设备良率',
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
          backgroundColor: '#1890ff',
        },
      },
      formatter: (datum: any) => {
        return { 
          name: '良率', 
          value: `${safeNumber(datum.yieldRate).toFixed(2)}%`,
        };
      },
    },
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

  return (
    <div className="analysis-page" style={{ padding: 24, minHeight: '100vh' }}>
      {/** 统一面板样式：深色渐变 + 半透明 + 玻璃效果 */}
      {(() => {
        /* 仅用于提供样式常量，不渲染任何内容 */
        return null;
      })()}
      {/** 定义样式常量 */}
      {/** 注意：在 JSX 中定义常量 */}
      {/**/}
      {/** @ts-ignore */}
      {(() => {
        // 将样式常量挂到 window，供下方内联使用，避免重复对象创建
        // 仅在首次赋值
        const anyWin: any = window as any;
        if (!anyWin.__panelStyles) {
          anyWin.__panelStyles = {
            panelStyle: {
              background: 'linear-gradient(180deg, rgba(7,16,35,0.65) 0%, rgba(7,16,35,0.35) 100%)',
              border: '1px solid rgba(255,255,255,0.12)',
              borderRadius: 12,
              boxShadow: '0 10px 30px rgba(0,0,0,0.25)',
              backdropFilter: 'blur(6px)',
              WebkitBackdropFilter: 'blur(6px)',
              overflow: 'hidden',
            },
            headStyle: {
              background: 'transparent',
              color: '#E6F7FF',
              borderBottom: '1px solid rgba(255,255,255,0.08)',
              fontWeight: 600,
            },
            bodyStyle: {
              background: 'transparent',
              padding: 16,
              color: '#E6F7FF',
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
        }
        return null;
      })()}
      
      {/* 筛选区域 */}
      <div style={{
        marginBottom: 24,
        padding: 16,
        borderRadius: 8,
        background: '#ffffff',
        border: '1px solid #f0f0f0',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <Row gutter={16} justify="space-between" align="middle" style={{margin:'0 16'}}>
          <Col xs={24} sm={24} md={8} lg={6} xl={5} style={{ display: 'flex', alignItems: 'center', marginBottom: 0 }}>
            <span style={{ marginRight:0, color: '#000000', fontWeight: 500 }}>生产线：</span>
            <Input
              value={selectedProductionLine}
              onChange={(e) => setSelectedProductionLine(e.target.value)}
              style={{ width: '60%', ...(window as any).__panelStyles?.inputStyle }}
              placeholder="请输入"
            />
          </Col>
          <Col xs={24} sm={24} md={8} lg={6} xl={5} style={{ display: 'flex', alignItems: 'center', marginBottom: 0 }}>
            <span style={{ marginRight:0, color: '#000000', fontWeight: 500 }}>设备名称：</span>
            <Input
              value={deviceName}
              onChange={(e) => setDeviceName(e.target.value)}
              placeholder="请输入"
              style={{ width: '60%', ...(window as any).__panelStyles?.inputStyle }}
            />
          </Col>
          <Col xs={24} sm={24} md={8} lg={6} xl={5} style={{ display: 'flex', alignItems: 'center', marginBottom: 0 }}>
            <span style={{ marginRight:0, color: '#000000', fontWeight: 500 }}>资源ID：</span>
            <Input
              value={resource}
              onChange={(e) => setResource(e.target.value)}
              placeholder="请输入"
              style={{ width: '60%', ...(window as any).__panelStyles?.inputStyle }}
            />
          </Col>
          <Col xs={24} sm={24} md={24} lg={6} xl={9} style={{ display: 'flex', alignItems: 'center', marginBottom: 0  }}>
            <span style={{ marginRight:0, color: '#000000', fontWeight: 500 }}>时间：</span>
            {/* {renderTwoLineLabel('时间范围：')} */}
            <RangePicker
              value={dateRange}
              onChange={(dates) =>
                handleDateRangeChange(
                  dates as [dayjs.Dayjs | null, dayjs.Dayjs | null] | null,
                )
              }
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
              // suffixIcon={<span style={{ color: 'rgba(255, 255, 255, 0.5)' }}>📅</span>}
            />
            <Button
              type="primary"
              onClick={refreshData}
              loading={loading}
              style={{ marginLeft: 8, width: 45 }}
            >
              查询
            </Button>
          </Col>
        </Row>
      </div>

      {/* 图表区域 */}
      <Row gutter={16} style={{ marginBottom: 10 }}>
        <Col span={15}>
          <Card
            title="小时产量统计"
            style={{ ...(window as any).__panelStyles?.panelStyle, height: '100%' }}
            // style={(window as any).__panelStyles?.panelStyle}
            headStyle={(window as any).__panelStyles?.headStyle}
            bodyStyle={(window as any).__panelStyles?.bodyStyle}
          >
            {hourlyChartData.length > 0 ? (
              <Column {...hourlyChartConfig} />
            ) : (
              <div style={{ textAlign: 'center', padding: 40 }}>暂无数据</div>
            )}
          </Card>
        </Col>
            
        <Col span={9}>
          <div style={{ height: '100%', display: 'flex', flexDirection: 'column', gap: 16 }}>
            <Card
              title="设备产量统计"
              style={{ ...(window as any).__panelStyles?.panelStyle, height: '30%' }}
              headStyle={(window as any).__panelStyles?.headStyle}
              bodyStyle={(window as any).__panelStyles?.bodyStyle}
            >
              {deviceProductionData.length > 0 ? (
                <Column {...deviceProductionConfig} />
              ) : (
                <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>暂无数据</div>
              )}
            </Card>
        
            <Card
              title="设备良率分布"
              style={{ ...(window as any).__panelStyles?.panelStyle, height: '100%' }}
              headStyle={(window as any).__panelStyles?.headStyle}
              bodyStyle={(window as any).__panelStyles?.bodyStyle}
            >
              {yieldRateData.length > 0 ? (
                <Line {...yieldRateConfig} />
             ) : (
                <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>暂无数据</div>
              )}
            </Card>
           </div>
        </Col>
      </Row>
    </div>
  );
};

export default Analysis;