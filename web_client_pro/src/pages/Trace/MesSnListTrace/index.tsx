import type { ActionType, ProColumns, RequestData } from '@ant-design/pro-components';
import { PageContainer, ProTable } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback, useMemo } from 'react';
import { message } from 'antd';
import { getMesSnListCurrentList } from '@/services/Api/Trace/MesSnListCurrent';
import { getMesSnListHistoryList } from '@/services/Api/Trace/MesSnListHistory';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';
import { getOrderById } from '@/services/Api/Infrastructure/OrderList';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';
import { getProductionLineById } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';
import { getDeviceInfoById } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import type { MesSnListCurrentDto, MesSnListCurrentQueryDto } from '@/services/Model/Trace/MesSnListCurrent';
import type { MesSnListHistoryDto } from '@/services/Model/Trace/MesSnListHistory';
import type { ProductListDto } from '@/services/Model/Infrastructure/ProductList';
import type { OrderList } from '@/services/Model/Infrastructure/OrderList';
import type { StationListDto } from '@/services/Model/Infrastructure/StationList';
import type { productionLine } from '@/services/Model/Trace/ProductionEquipment‌/productionLineInfo';
import type { DeviceInfo } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';

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
      // 根据SN号查询历史记录
      const response = await getMesSnListHistoryList({
        snNumber: record.snNumber,
        pageIndex: 1,
        pageSize: 1000,
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
                deviceEnCode: device.deviceEnCode || ''
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
    },
    {
      title: '异常代码',
      dataIndex: 'abnormalCode',
      key: 'abnormalCode',
      width: 120,
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
      key: 'reworkTime',
      width: 180,
      search: false,
      valueType: 'dateTime',
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      search: false,
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
    },
    {
      title: '批次数据',
      dataIndex: 'batchResults',
      key: 'batchResults',
      width: 150,
      search: false,
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
    },
    {
      title: '异常代码',
      dataIndex: 'abnormalCode',
      key: 'abnormalCode',
      width: 120,
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
      key: 'reworkTime',
      width: 180,
      search: false,
      valueType: 'dateTime',
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      search: false,
    },
  ];

  return (
    <>
      {contextHolder}
      <PageContainer title="SN表追溯">
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
            const queryParams: MesSnListCurrentQueryDto = {
              pageIndex: Math.max(1, params.current || 1),
              pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
              snNumber: params.snNumber,
              stationStatus: params.stationStatus,
            };

            try {
              const response = await getMesSnListCurrentList(queryParams);

              // 为每个记录获取产品编码、工单编码、站点编码、产线编码和设备编码
              const enhancedData = await Promise.all(
                (response.data || []).map(async (record: MesSnListCurrentDto) => {
                  let updatedRecord = { ...record };

                  // 获取产品编码
                  if (record.productListId) {
                    try {
                      const product = await getProductListById(record.productListId);
                      updatedRecord = {
                        ...updatedRecord,
                        productCode: product.productCode || ''
                      };
                    } catch (error) {
                      console.error('获取产品编码失败:', error);
                    }
                  }

                  // 获取工单编码
                  if (record.orderListId) {
                    try {
                      const order = await getOrderById(record.orderListId);
                      updatedRecord = {
                        ...updatedRecord,
                        orderCode: order.orderCode || ''
                      };
                    } catch (error) {
                      console.error('获取工单编码失败:', error);
                    }
                  }

                  // 获取站点编码
                  if (record.currentStationListId) {
                    try {
                      const station = await getStationListById(record.currentStationListId);
                      updatedRecord = {
                        ...updatedRecord,
                        stationCode: station.stationCode || ''
                      };
                    } catch (error) {
                      console.error('获取站点编码失败:', error);
                    }
                  }

                  // 获取产线编码
                  if (record.productionLineId) {
                    try {
                      const response = await getProductionLineById(record.productionLineId);
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
                  if (record.resourceId) {
                    try {
                      const response = await getDeviceInfoById(record.resourceId);
                      const device = response.data;
                      updatedRecord = {
                        ...updatedRecord,
                        deviceEnCode: device.deviceEnCode || ''
                      };
                    } catch (error) {
                      console.error('获取设备编码失败:', error);
                    }
                  }

                  return updatedRecord;
                })
              );

              return {
                data: enhancedData,
                total: response.total || 0,
                success: response.success !== false,
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
            pageSize: 10,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50', '100'],
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
              dataSource={historyData}
              loading={historyLoading}
              pagination={{
                pageSize: 10,
                showSizeChanger: true,
                pageSizeOptions: ['10', '20', '50', '100'],
              }}
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