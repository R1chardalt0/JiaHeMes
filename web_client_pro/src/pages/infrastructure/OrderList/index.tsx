// src/pages/infrastructure/OrderList/index.tsx
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { Button, Modal, message, Form } from 'antd';
import ProductSelectModal from '@/pages/infrastructure/Bom/components/ProductSelectModal';
import { ProductListDto } from '@/services/Model/Infrastructure/ProductList';
import { EyeOutlined, EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons';
import { getOrderList, getOrderById, createOrder, updateOrder, deleteOrder } from '@/services/Api/Infrastructure/OrderList';
import { OrderListDto, OrderListQueryDto, OrderListCreateDto, OrderListUpdateDto } from '@/services/Model/Infrastructure/OrderList';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';
import { getBomById } from '@/services/Api/Infrastructure/Bom/BomList';
import { getProcessRouteById } from '@/services/Api/Infrastructure/ProcessRoute/ProcessRoute';
import { getOrderListColumns } from './columns';
import OrderListFormModal from './components/OrderListFormModal';
import dayjs from 'dayjs';
import type { RequestData } from '@ant-design/pro-components';
import type { Dayjs } from 'dayjs';

// 表单专用类型，用于处理日期选择器的dayjs对象
interface OrderListFormValues {
  orderCode: string;
  orderName: string;
  productListId?: string;
  productCode?: string;
  productName?: string;
  bomId?: string;
  bomCode?: string;
  bomName?: string;
  processRouteId?: string;
  processRouteCode?: string;
  processRouteName?: string;
  orderType: number;
  planQty: number;
  completedQty: number;
  priorityLevel: number;
  planStartTime?: Dayjs | null;
  planEndTime?: Dayjs | null;
}

const OrderPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<OrderListDto | null>(null);
  const [messageApi] = message.useMessage();
  const [currentSearchParams, setCurrentSearchParams] = useState<OrderListQueryDto>({
    current: 1,
    pageSize: 50
  });
  const [isAddModalVisible, setIsAddModalVisible] = useState(false);
  const [isEditModalVisible, setIsEditModalVisible] = useState(false);
  const [editingOrder, setEditingOrder] = useState<OrderListDto | null>(null);
  const [form] = Form.useForm();
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([]);
  // 产品选择弹窗相关状态
  const [isProductModalVisible, setIsProductModalVisible] = useState(false);

  // 显示工单详情
  const handleShowDetail = useCallback(async (record: OrderListDto) => {
    try {
      let updatedRecord = { ...record };

      // 补充产品信息
      if (record.productListId) {
        try {
          const product = await getProductListById(record.productListId);
          if (product) {
            updatedRecord = {
              ...updatedRecord,
              productCode: product.productCode || '',
              productName: product.productName || ''
            };
          }
        } catch (error) {
          console.error('获取产品信息失败:', error);
        }
      }

      // 补充BOM信息
      if (record.bomId) {
        try {
          const bom = await getBomById(record.bomId);
          if (bom) {
            updatedRecord = {
              ...updatedRecord,
              bomCode: bom.bomCode || '',
              bomName: bom.bomName || ''
            };
          }
        } catch (error) {
          console.error('获取BOM信息失败:', error);
        }
      }

      // 补充工艺路线信息
      if (record.processRouteId) {
        try {
          const processRoute = await getProcessRouteById(record.processRouteId);
          if (processRoute) {
            updatedRecord = {
              ...updatedRecord,
              processRouteCode: processRoute.routeCode || '',
              processRouteName: processRoute.routeName || ''
            };
          }
        } catch (error) {
          console.error('获取工艺路线信息失败:', error);
        }
      }

      setCurrentRow(updatedRecord);
      setShowDetail(true);
    } catch (error) {
      messageApi.error('获取详情失败');
      console.error('获取详情失败:', error);
    }
  }, [messageApi]);

  // 双击行显示详情
  const handleRowDoubleClick = useCallback((record: OrderListDto) => {
    handleShowDetail(record);
  }, [handleShowDetail]);

  // 删除工单
  const handleDeleteOrder = useCallback((orderId: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '确认要删除这条工单信息吗？',
      onOk: async () => {
        try {
          await deleteOrder(orderId);
          messageApi.success('工单删除成功');
          // 刷新表格
          if (actionRef.current) {
            actionRef.current.reload();
          }
        } catch (error) {
          messageApi.error('工单删除失败');
          console.error('Delete order error:', error);
        }
      }
    });
  }, [messageApi]);

  // 打开编辑工单模态框
  const handleEditOrder = useCallback(async (order: OrderListDto) => {
    setEditingOrder(order);
    let formValues = {
      orderCode: order.orderCode,
      orderName: order.orderName,
      productListId: order.productListId,
      productCode: order.productCode,
      productName: order.productName,
      bomId: order.bomId,
      bomCode: order.bomCode,
      bomName: order.bomName,
      processRouteId: order.processRouteId,
      processRouteCode: order.processRouteCode,
      processRouteName: order.processRouteName,
      orderType: order.orderType,
      planQty: order.planQty,
      completedQty: order.completedQty,
      priorityLevel: order.priorityLevel,
      planStartTime: order.planStartTime ? dayjs(order.planStartTime) : undefined,
      planEndTime: order.planEndTime ? dayjs(order.planEndTime) : undefined,
    };

    // 补充产品信息
    if (order.productListId) {
      try {
        const product = await getProductListById(order.productListId);
        if (product) {
          formValues = {
            ...formValues,
            productCode: product.productCode || '',
            productName: product.productName || ''
          };
        }
      } catch (error) {
        console.error('获取产品信息失败:', error);
      }
    }

    // 补充BOM信息
    if (order.bomId) {
      try {
        const bom = await getBomById(order.bomId);
        if (bom) {
          formValues = {
            ...formValues,
            bomCode: bom.bomCode || '',
            bomName: bom.bomName || ''
          };
        }
      } catch (error) {
        console.error('获取BOM信息失败:', error);
      }
    }

    // 补充工艺路线信息
    if (order.processRouteId) {
      try {
        const processRoute = await getProcessRouteById(order.processRouteId);
        if (processRoute) {
          formValues = {
            ...formValues,
            processRouteCode: processRoute.routeCode || '',
            processRouteName: processRoute.routeName || ''
          };
        }
      } catch (error) {
        console.error('获取工艺路线信息失败:', error);
      }
    }

    form.setFieldsValue(formValues);
    setIsEditModalVisible(true);
  }, [form]);

  // 工单列表表格列定义
  const columns = getOrderListColumns({
    onShowDetail: handleShowDetail,
    onEdit: handleEditOrder,
    onDelete: handleDeleteOrder
  });

  // 保存工单（创建或更新）
  const handleSaveOrder = useCallback(async () => {
    try {
      const values: OrderListFormValues = await form.validateFields();

      // 确保必填字段有有效值
      const validatedValues = {
        ...values,
        productListId: values.productListId || '00000000-0000-0000-0000-000000000000',
        productCode: values.productCode || 'N/A',
        productName: values.productName || '未选择产品',
        processRouteCode: values.processRouteCode || 'N/A',
        processRouteName: values.processRouteName || '未选择工艺路线',
        planStartTime: values.planStartTime ? values.planStartTime.format('YYYY-MM-DD HH:mm:ss') : new Date().toISOString(),
        planEndTime: values.planEndTime ? values.planEndTime.format('YYYY-MM-DD HH:mm:ss') : new Date().toISOString(),
      };

      if (editingOrder) {
        // 更新工单
        const updatedOrder: OrderListUpdateDto = {
          orderListId: editingOrder.orderListId,
          orderCode: validatedValues.orderCode,
          orderName: validatedValues.orderName,
          productListId: validatedValues.productListId,
          productCode: validatedValues.productCode,
          productName: validatedValues.productName,
          bomId: validatedValues.bomId,
          bomCode: validatedValues.bomCode,
          bomName: validatedValues.bomName,
          processRouteId: validatedValues.processRouteId,
          processRouteCode: validatedValues.processRouteCode,
          processRouteName: validatedValues.processRouteName,
          orderType: validatedValues.orderType,
          planQty: validatedValues.planQty || 0,
          completedQty: validatedValues.completedQty || 0,
          priorityLevel: validatedValues.priorityLevel || 5,
          planStartTime: validatedValues.planStartTime,
          planEndTime: validatedValues.planEndTime,
        };

        // 确保URL查询参数与请求体中的ID完全一致，避免后端验证失败
        if (updatedOrder.orderListId !== editingOrder.orderListId) {
          console.error('前端检测到ID不匹配 - URL查询参数ID:', editingOrder.orderListId, '请求体ID:', updatedOrder.orderListId);
          throw new Error('工单ID不匹配，无法更新');
        }

        await updateOrder(editingOrder.orderListId, updatedOrder);
        messageApi.success('工单更新成功');
        setIsEditModalVisible(false);
        // 刷新表格
        if (actionRef.current) {
          actionRef.current.reload();
        }
      } else {
        // 创建新工单
        const newOrder: OrderListCreateDto = {
          orderCode: validatedValues.orderCode,
          orderName: validatedValues.orderName,
          productListId: validatedValues.productListId,
          productCode: validatedValues.productCode,
          productName: validatedValues.productName,
          bomId: validatedValues.bomId,
          bomCode: validatedValues.bomCode,
          bomName: validatedValues.bomName,
          processRouteId: validatedValues.processRouteId,
          processRouteCode: validatedValues.processRouteCode,
          processRouteName: validatedValues.processRouteName,
          orderType: validatedValues.orderType || 1,
          planQty: validatedValues.planQty || 0,
          completedQty: validatedValues.completedQty || 0,
          priorityLevel: validatedValues.priorityLevel || 5,
          planStartTime: validatedValues.planStartTime,
          planEndTime: validatedValues.planEndTime,
        };

        await createOrder(newOrder);
        messageApi.success('工单创建成功');
        setIsAddModalVisible(false);
        // 刷新表格
        if (actionRef.current) {
          actionRef.current.reload();
        }
      }
    } catch (error) {
      messageApi.error('保存失败，请检查输入');
      console.error('Save order error:', error);
    }
  }, [form, editingOrder, actionRef, messageApi]);

  // 打开新建工单模态框
  const handleAddOrder = useCallback(() => {
    form.resetFields();
    setEditingOrder(null);
    setIsAddModalVisible(true);
  }, [form]);

  // 批量删除工单
  const handleBatchDelete = useCallback(async () => {
    Modal.confirm({
      title: '确认批量删除',
      content: `确认要删除选中的${selectedRowKeys.length}条工单信息吗？`,
      onOk: async () => {
        try {
          await deleteOrder(selectedRowKeys);
          messageApi.success('批量删除成功');
          actionRef.current?.reload();
          setSelectedRowKeys([]);
        } catch (error) {
          messageApi.error('批量删除失败');
          console.error('Batch delete error:', error);
        }
      },
    });
  }, [selectedRowKeys, actionRef, messageApi]);

  // 打开产品选择弹窗
  const handleOpenProductModal = useCallback(() => {
    setIsProductModalVisible(true);
  }, []);

  // 处理产品选择
  const handleProductSelect = useCallback((product: ProductListDto) => {
    form.setFieldsValue({
      productListId: product.productListId,
      productCode: product.productCode,
      productName: product.productName,
      // 产品选择后自动填充BOM和工艺路线信息
      bomId: product.bomId || '',
      bomCode: product.bomCode || '',
      bomName: product.bomName || '',
      processRouteId: product.processRouteId || '',
      processRouteCode: product.processRouteCode || '',
      processRouteName: product.processRouteName || ''
    });
    setIsProductModalVisible(false);
  }, [form]);

  return (
    <PageContainer title="工单管理">
      <ProTable<OrderListDto>
        actionRef={actionRef}
        rowKey="orderListId"
        className="order-table"
        columns={columns}
        search={{
          labelWidth: 120,
          layout: 'vertical',
        }}
        request={async (
          params
        ): Promise<RequestData<OrderListDto>> => {
          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            orderCode: params.orderCode,
            orderName: params.orderName,
            orderStatus: params.orderStatus,
            orderType: params.orderType,
            priorityLevel: params.priorityLevel,
            productCode: params.productCode,
            productName: params.productName,
            bomCode: params.bomCode,
            bomName: params.bomName,
            processRouteCode: params.processRouteCode,
            processRouteName: params.processRouteName,
          });

          const queryParams: OrderListQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            orderCode: params.orderCode,
            orderName: params.orderName,
            orderStatus: params.orderStatus,
            orderType: params.orderType,
            priorityLevel: params.priorityLevel,
            productCode: params.productCode,
            productName: params.productName,
            bomCode: params.bomCode,
            bomName: params.bomName,
            processRouteCode: params.processRouteCode,
            processRouteName: params.processRouteName,
          };

          try {
            // 当搜索联合查询字段时，获取更多数据以进行本地过滤
            const searchParams = [
              'productCode', 'productName', 'bomCode', 'bomName', 'processRouteCode', 'processRouteName'
            ];
            const has联合查询 = searchParams.some(key => params[key]);

            const apiQueryParams = {
              ...queryParams,
              // 当搜索联合查询字段时，获取更多数据
              pageSize: has联合查询 ? 1000 : queryParams.pageSize
            };

            const response = await getOrderList(apiQueryParams);

            // 为每个工单补充产品、BOM和工艺路线信息
            const enhancedData = await Promise.all(
              (response.data || []).map(async (order: OrderListDto) => {
                let updatedOrder = { ...order };

                // 补充产品信息
                if (order.productListId) {
                  try {
                    const product = await getProductListById(order.productListId);
                    if (product) {
                      updatedOrder = {
                        ...updatedOrder,
                        productCode: product.productCode || '',
                        productName: product.productName || ''
                      };
                    }
                  } catch (error) {
                    console.error('获取产品信息失败:', error);
                  }
                }

                // 补充BOM信息
                if (order.bomId) {
                  try {
                    const bom = await getBomById(order.bomId);
                    if (bom) {
                      updatedOrder = {
                        ...updatedOrder,
                        bomCode: bom.bomCode || '',
                        bomName: bom.bomName || ''
                      };
                    }
                  } catch (error) {
                    console.error('获取BOM信息失败:', error);
                  }
                }

                // 补充工艺路线信息
                if (order.processRouteId) {
                  try {
                    const processRoute = await getProcessRouteById(order.processRouteId);
                    if (processRoute) {
                      updatedOrder = {
                        ...updatedOrder,
                        processRouteCode: processRoute.routeCode || '',
                        processRouteName: processRoute.routeName || ''
                      };
                    }
                  } catch (error) {
                    console.error('获取工艺路线信息失败:', error);
                  }
                }

                return updatedOrder;
              })
            );

            // 本地过滤联合查询字段
            let filteredData = enhancedData;
            if (has联合查询) {
              filteredData = enhancedData.filter(order => {
                // 检查产品编码
                if (params.productCode && !order.productCode?.includes(params.productCode)) {
                  return false;
                }
                // 检查产品名称
                if (params.productName && !order.productName?.includes(params.productName)) {
                  return false;
                }
                // 检查BOM编码
                if (params.bomCode && !order.bomCode?.includes(params.bomCode)) {
                  return false;
                }
                // 检查BOM名称
                if (params.bomName && !order.bomName?.includes(params.bomName)) {
                  return false;
                }
                // 检查工艺路线编码
                if (params.processRouteCode && !order.processRouteCode?.includes(params.processRouteCode)) {
                  return false;
                }
                // 检查工艺路线名称
                if (params.processRouteName && !order.processRouteName?.includes(params.processRouteName)) {
                  return false;
                }
                return true;
              });
            }

            return {
              data: filteredData,
              total: filteredData.length,
              success: response.success !== false,
            };
          } catch (error) {
            console.error('Order - API调用失败:', error);
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        rowSelection={{
          type: "checkbox",
          onChange: (selectedRowKeys) => setSelectedRowKeys(selectedRowKeys as string[]),
        }}
        toolBarRender={() => [
          <Button
            type="primary"
            key="add"
            icon={<PlusOutlined />}
            onClick={handleAddOrder}>

            新建工单
          </Button>,
          <Button
            danger key="batchDelete"
            icon={<DeleteOutlined />}
            onClick={handleBatchDelete}
            disabled={selectedRowKeys.length === 0}>
            批量删除
          </Button>,
        ]}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100'],
        }}
        onRow={(record) => ({
          onDoubleClick: () => handleRowDoubleClick(record),
        })}
        tableAlertRender={({ selectedRowKeys }) => (
          <div style={{ marginBottom: 8 }}>
            已选择 {selectedRowKeys?.length || 0} 项
          </div>
        )}
      />

      {showDetail && currentRow && (
        <div className="order-detail" style={{ marginTop: 20 }}>
          <ProDescriptions<OrderListDto>
            title="工单基本信息"
            dataSource={currentRow}
            column={3}
          >
            <ProDescriptions.Item label="工单ID">{currentRow.orderListId}</ProDescriptions.Item>
            <ProDescriptions.Item label="工单名称">{currentRow.orderName}</ProDescriptions.Item>
            <ProDescriptions.Item label="工单编码">{currentRow.orderCode}</ProDescriptions.Item>
            <ProDescriptions.Item label="产品ID">{currentRow.productListId || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="产品编码">{currentRow.productCode || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="产品名称">{currentRow.productName || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="BomID">{currentRow.bomId || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="Bom编码">{currentRow.bomCode || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="Bom名称">{currentRow.bomName || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="工艺路线ID">{currentRow.processRouteId || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="工艺路线编码">{currentRow.processRouteCode}</ProDescriptions.Item>
            <ProDescriptions.Item label="工艺路线名称">{currentRow.processRouteName}</ProDescriptions.Item>
            <ProDescriptions.Item label="工单类型">
              {currentRow.orderType === 1 ? '生产工单' :
                currentRow.orderType === 2 ? '返工工单' : '-'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="计划数量">{currentRow.planQty}</ProDescriptions.Item>
            <ProDescriptions.Item label="已完成数量">{currentRow.completedQty}</ProDescriptions.Item>
            <ProDescriptions.Item label="优先级">
              {currentRow.priorityLevel === 1 ? '紧急' :
                currentRow.priorityLevel === 3 ? '高' :
                  currentRow.priorityLevel === 5 ? '中' :
                    currentRow.priorityLevel === 7 ? '低' : '-'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="工单状态">
              {currentRow.orderStatus === 1 ? '新建' :
                currentRow.orderStatus === 2 ? '已排产' :
                  currentRow.orderStatus === 3 ? '生产中' :
                    currentRow.orderStatus === 4 ? '已完成' :
                      currentRow.orderStatus === 5 ? '已关闭' : '-'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="计划开始时间">{currentRow.planStartTime}</ProDescriptions.Item>
            <ProDescriptions.Item label="计划结束时间">{currentRow.planEndTime}</ProDescriptions.Item>
            <ProDescriptions.Item label="实际开始时间">{currentRow.actualStartTime || '-'}</ProDescriptions.Item>
            <ProDescriptions.Item label="实际结束时间">{currentRow.actualEndTime || '-'}</ProDescriptions.Item>
          </ProDescriptions>
        </div>
      )}

      {/* 新建/编辑工单模态框 */}
      <OrderListFormModal
        open={isAddModalVisible || isEditModalVisible}
        onCancel={() => {
          setIsAddModalVisible(false);
          setIsEditModalVisible(false);
        }}
        onOk={handleSaveOrder}
        editingOrder={editingOrder}
        form={form}
        onOpenProductModal={handleOpenProductModal}
      />

      {/* 产品选择弹窗 */}
      <ProductSelectModal
        open={isProductModalVisible}
        onCancel={() => setIsProductModalVisible(false)}
        onSelect={handleProductSelect}
      />
    </PageContainer>
  );
};

export default OrderPage;