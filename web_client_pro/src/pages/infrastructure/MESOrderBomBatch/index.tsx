import type { ActionType, ProColumns, RequestData } from '@ant-design/pro-components';
import { PageContainer, ProTable } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback, useMemo } from 'react';
import { message } from 'antd';
import {
  getMESOrderBomBatchById,
  getMESOrderBomBatchList,
  getMESOrderBomBatchPagedList,
  getMESOrderBomBatchByOrderListId,
  getMESOrderBomBatchByProductListId,
} from '@/services/Api/Infrastructure/MESOrderBomBatch/MESOrderBomBatch';
import {
  getMESOrderBomBatchItemList,
  getMESOrderBomBatchItemByOrderBomBatchId,
} from '@/services/Api/Infrastructure/MESOrderBomBatch/MESOrderBomBatchItem';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';
import { getOrderById } from '@/services/Api/Infrastructure/OrderList';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';
import { getDeviceInfoById } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import type { MESOrderBomBatch, MESOrderBomBatchQueryDto } from '@/services/Model/Infrastructure/MESOrderBomBatch/MESOrderBomBatch';
import type { MesOrderBomBatchItem } from '@/services/Model/Infrastructure/MESOrderBomBatch/MESOrderBomBatchItem';

/**
 * MES工单BOM批次页面
 * 功能：展示工单BOM批次列表，双击查看明细项
 */
const MESOrderBomBatchPage: React.FC = () => {
  // 表格操作引用
  const actionRef = useRef<ActionType | null>(null);
  // 消息提示
  const [messageApi, contextHolder] = message.useMessage();
  // 当前选中行
  const [currentRow, setCurrentRow] = useState<MESOrderBomBatch | null>(null);
  // 明细数据列表
  const [detailData, setDetailData] = useState<MesOrderBomBatchItem[]>([]);
  // 明细数据加载状态
  const [detailLoading, setDetailLoading] = useState(false);
  // 是否显示明细列表
  const [showDetail, setShowDetail] = useState(false);
  // 当前搜索参数
  const [currentSearchParams, setCurrentSearchParams] = useState<MESOrderBomBatchQueryDto>({
    pageIndex: 1,
    pageSize: 10
  });

  /**
   * 显示明细列表
   * @param record 当前工单BOM批次记录
   */
  const handleShowDetail = useCallback(async (record: MESOrderBomBatch) => {
    try {
      setCurrentRow(record);
      setDetailLoading(true);
      // 根据工单BOM批次ID查询明细记录
      const response = await getMESOrderBomBatchItemByOrderBomBatchId(record.orderBomBatchId);

      setDetailData(response.data || []);
      setShowDetail(true);
    } catch (error) {
      messageApi.error('获取明细记录失败');
      console.error('获取明细记录失败:', error);
    } finally {
      setDetailLoading(false);
    }
  }, [messageApi]);

  /**
   * 双击行处理函数
   * @param record 当前工单BOM批次记录
   */
  const handleRowDoubleClick = useCallback((record: MESOrderBomBatch) => {
    handleShowDetail(record);
  }, [handleShowDetail]);

  /**
   * 工单BOM批次列定义
   */
  const columns: ProColumns<MESOrderBomBatch>[] = [
    {
      title: '工单BOM批次ID',
      dataIndex: 'orderBomBatchId',
      key: 'orderBomBatchId',
      width: 200,
      search: false,
    },
    {
      title: '批次编码',
      dataIndex: 'batchCode',
      key: 'batchCode',
      width: 150,
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
      title: '站点编码',
      dataIndex: 'stationCode',
      key: 'stationCode',
      width: 150,
      search: true,
      render: (stationCode) => stationCode || '-',
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
      title: '状态',
      dataIndex: 'orderBomBatchStatus',
      key: 'orderBomBatchStatus',
      width: 120,
      search: true,
      valueEnum: {
        1: { text: '正常', status: 'Success' },
        2: { text: '已使用完', status: 'Default' },
        3: { text: '已下料', status: 'Default' },
      },
    },
    {
      title: '批次数量',
      dataIndex: 'batchQty',
      key: 'batchQty',
      width: 120,
      search: false,
      render: (batchQty) => batchQty || '-',
    },
    {
      title: '已使用数量',
      dataIndex: 'completedQty',
      key: 'completedQty',
      width: 120,
      search: false,
      render: (completedQty) => completedQty || '-',
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
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
      render: (remark) => remark || '-',
    },
  ];

  /**
   * 明细项列定义
   */
  const detailColumns: ProColumns<MesOrderBomBatchItem>[] = [
    {
      title: '工单BOM批次明细ID',
      dataIndex: 'orderBomBatchItemId',
      key: 'orderBomBatchItemId',
      width: 200,
      search: false,
    },
    {
      title: 'SN编码',
      dataIndex: 'snNumber',
      key: 'snNumber',
      width: 180,
      search: true,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
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
      render: (remark) => remark || '-',
    },
  ];

  return (
    <>
      {contextHolder}
      <PageContainer title="MES工单BOM批次管理">
        <ProTable<MESOrderBomBatch>
          actionRef={actionRef}
          rowKey="orderBomBatchId"
          columns={columns}
          search={{
            labelWidth: 120,
            layout: 'vertical',
          }}
          request={async (
            params
          ): Promise<RequestData<MESOrderBomBatch>> => {
            // 设置当前搜索参数，包括分页参数验证
            setCurrentSearchParams({
              pageIndex: Math.max(1, params.current || 1),
              pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
              orderBomBatchId: params.orderBomBatchId,
              batchCode: params.batchCode,
              orderBomBatchStatus: params.orderBomBatchStatus,
              sortField: params.sortField,
              sortOrder: params.sortOrder,
            });

            // 构建查询参数
            const queryParams: MESOrderBomBatchQueryDto = {
              pageIndex: Math.max(1, params.current || 1),
              pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
              orderBomBatchId: params.orderBomBatchId,
              batchCode: params.batchCode,
              orderBomBatchStatus: params.orderBomBatchStatus,
              sortField: params.sortField,
              sortOrder: params.sortOrder,
            };

            try {
              // 调用 API 获取分页数据
              const response = await getMESOrderBomBatchPagedList(queryParams);

              // 为每个记录获取产品编码、工单编码、站点编码和设备编码
              const enhancedData = await Promise.all(
                (response.data || []).map(async (record: MESOrderBomBatch) => {
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
                  if (record.stationListId) {
                    try {
                      const station = await getStationListById(record.stationListId);
                      updatedRecord = {
                        ...updatedRecord,
                        stationCode: station.stationCode || ''
                      };
                    } catch (error) {
                      console.error('获取站点编码失败:', error);
                    }
                  }

                  // 获取设备编码
                  if (record.resourceId) {
                    try {
                      const response = await getDeviceInfoById(record.resourceId);
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

              return {
                data: enhancedData,
                total: response.total || 0,
                success: true,
              };
            } catch (error) {
              messageApi.error('获取工单BOM批次列表失败');
              console.error('获取工单BOM批次列表失败:', error);
              return {
                data: [],
                total: 0,
                success: false,
              };
            }
          }}
          onRow={(record) => ({
            onDoubleClick: () => handleRowDoubleClick(record),
          })}
          pagination={{
            defaultPageSize: 10,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50'],
            showTotal: (total) => `共 ${total} 条记录`,
          }}
        />

        {/* 明细项列表 */}
        {showDetail && currentRow && (
          <div style={{ marginTop: 20, padding: 16, backgroundColor: '#f5f5f5', borderRadius: 8 }}>
            <h3 style={{ marginBottom: 16 }}>
              工单BOM批次明细 - {currentRow.batchCode}
            </h3>
            <ProTable<MesOrderBomBatchItem>
              rowKey="orderBomBatchItemId"
              columns={detailColumns}
              loading={detailLoading}
              dataSource={detailData}
              pagination={{
                defaultPageSize: 10,
                showSizeChanger: true,
                pageSizeOptions: ['10', '20', '50'],
                showQuickJumper: true,
                showTotal: (total) => `共 ${total} 条记录`,
              }}
              search={{
                labelWidth: 120,
                layout: 'vertical',
              }}
              scroll={{
                y: 600,
              }}
            />
          </div>
        )}
      </PageContainer>
    </>
  );
};

export default MESOrderBomBatchPage;