import { PageContainer, ProTable, ProColumns } from '@ant-design/pro-components';
import React, { useRef } from 'react';
import { getProductionRecords } from '@/services/Api/Trace/ProductionRecords';
import { ProductionRecordsDto, GetProductionRecordsParams } from '@/services/Model/Trace/ProductionRecords';
import type { RequestData } from '@ant-design/pro-components';
import dayjs from 'dayjs';

const TableList: React.FC = () => {
  const actionRef = useRef<any>(null);

  // 定义表格列
  const columns: ProColumns<ProductionRecordsDto>[] = [
    {
      title: '生产线',
      dataIndex: 'productionLineName',
      key: 'productionLineName',
      width: 150,
    },
    {
      title: '设备编码',
      dataIndex: 'resource',
      key: 'resource',
      width: 120,
    },
    {
      title: '设备名称',
      dataIndex: 'deviceName',
      key: 'deviceName',
      width: 120,
    },
    {
      title: '总产量',
      dataIndex: 'totalProduction',
      key: 'totalProduction',
      width: 100,
      search: false,
      valueType: 'digit',
    },
    {
      title: 'OK数量',
      dataIndex: 'okNum',
      key: 'okNum',
      width: 100,
      search: false,
      valueType: 'digit',
    },
    {
      title: 'NG数量',
      dataIndex: 'ngNum',
      key: 'ngNum',
      width: 100,
      search: false,
      valueType: 'digit',
    },
    {
      title: '良率',
      dataIndex: 'yield',
      key: 'yield',
      width: 100,
      search: false,
      render: (_, record: ProductionRecordsDto) => {
        const value = record.yield;
        if (value === null || value === undefined) return '-';
        return `${value}%`;
      },
    },

    {
      title: '时间范围',
      key: 'timeRange',
      dataIndex: 'timeRange',
      valueType: 'dateTimeRange',
      hideInTable: true, // 不显示在表格中
      search: {
        transform: (value: [string, string]) => ({
          // 将选择的时间范围转换为 startTime 和 endTime
          startTime: value[0],
          endTime: value[1],
        }),
      },
      fieldProps: {
        placeholder: ['开始时间', '结束时间'],
        showTime: true, // 支持选择时间（时分秒）
      },
      initialValue: [
        dayjs().startOf('day'), // 默认：今天 00:00:00
        dayjs().endOf('day'),   // 默认：今天 23:59:59
      ],
    },
  ];

  return (
    <PageContainer>
      <ProTable<ProductionRecordsDto>
        headerTitle="产量报表列表"
        actionRef={actionRef}
        rowKey={(record) => `${record.resource}_${record.deviceName}`}
        className="production-records-glass-table"
        search={{
          labelWidth: 120,
          layout: 'vertical',
        }}
        cardProps={{
          style: (window as any).__panelStyles?.panelStyle,
          headStyle: (window as any).__panelStyles?.headStyle,
          bodyStyle: (window as any).__panelStyles?.bodyStyle,
          bordered: false,
          ['data-panel-exempt']: 'true'
        } as any}
        request={async (params): Promise<RequestData<ProductionRecordsDto>> => {
          try {
            // 从 params 获取 startTime 和 endTime（由 timeRange 转换而来）
            const startTime = params.startTime;
            const endTime = params.endTime;

            const queryParams: GetProductionRecordsParams = {
              productionLineName: params.productionLineName,
              deviceName: params.deviceName,
              resource: params.resource,
              startTime: startTime || '',
              endTime: endTime || '',
            };

            const response = await getProductionRecords(queryParams);

            return {
              data: response.data || [],
              total: response.data?.length || 0,
              success: response.success ?? true,
            };
          } catch (error) {
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        columns={columns}
        pagination={false} // 关闭分页
        toolBarRender={false} // 隐藏工具栏
        rowSelection={false} // 隐藏行选择
      />
    </PageContainer>
  );
};

export default TableList;