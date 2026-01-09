import type { ActionType } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { Drawer, Button, message, Modal, Form, Input, Popconfirm, Table } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { getProductListList, createProductList, updateProductList, deleteProductList } from '@/services/Api/Infrastructure/ProductList';
import { getBomList } from '@/services/Api/Infrastructure/Bom/BomList';
import { getProcessRouteList } from '@/services/Api/Infrastructure/ProcessRoute/ProcessRoute';
import { ProductListDto, ProductListQueryDto } from '@/services/Model/Infrastructure/ProductList';
import { BomListQueryDto } from '@/services/Model/Infrastructure/Bom/BomList';
import { ProcessRouteQueryDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRoute';
import type { RequestData } from '@ant-design/pro-components';
import { getProductColumns } from './columns';
import ProductFormModal, { ProductFormModalRef } from './components/ProductFormModal';
import BomSelectModal from './components/BomSelectModal';
import ProcessRouteSelectModal from './components/ProcessRouteSelectModal';

const ProductList: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [currentRow, setCurrentRow] = useState<ProductListDto>();
  const [editingRow, setEditingRow] = useState<ProductListDto>();
  const [messageApi] = message.useMessage();

  // 创建ProductFormModal的ref
  const createFormRef = useRef<ProductFormModalRef>(null);
  const editFormRef = useRef<ProductFormModalRef>(null);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [currentSearchParams, setCurrentSearchParams] = useState<ProductListQueryDto>({
    current: 1,
    pageSize: 50
  });

  // BOM选择弹窗相关状态
  const [isBomModalVisible, setIsBomModalVisible] = useState(false);
  const [boms, setBoms] = useState<any[]>([]);
  const [bomTotal, setBomTotal] = useState<number>(0);
  const [bomCurrent, setBomCurrent] = useState<number>(1);
  const [bomPageSize, setBomPageSize] = useState<number>(10);
  const [bomSearchParams, setBomSearchParams] = useState<BomListQueryDto>({
    current: 1,
    pageSize: 10
  });
  const [bomSearchValues, setBomSearchValues] = useState({
    bomCode: '',
    bomName: ''
  });

  // 工艺路线选择弹窗相关状态
  const [isProcessRouteModalVisible, setIsProcessRouteModalVisible] = useState(false);
  const [processRoutes, setProcessRoutes] = useState<any[]>([]);
  const [processRouteTotal, setProcessRouteTotal] = useState<number>(0);
  const [processRouteCurrent, setProcessRouteCurrent] = useState<number>(1);
  const [processRoutePageSize, setProcessRoutePageSize] = useState<number>(10);
  const [processRouteSearchParams, setProcessRouteSearchParams] = useState<ProcessRouteQueryDto>({
    current: 1,
    pageSize: 10
  });
  const [processRouteSearchValues, setProcessRouteSearchValues] = useState({
    routeCode: '',
    routeName: ''
  });

  // 处理新建产品
  const handleCreateProduct = async (values: ProductListDto) => {
    try {
      const result = await createProductList(values);
      messageApi.success('产品创建成功');
      setShowCreateModal(false);
      // 通过ref重置表单
      if (createFormRef.current) {
        const createForm = createFormRef.current.getForm();
        createForm.resetFields();
      }
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('产品创建失败');
    }
  };

  // 处理编辑产品
  const handleEditProduct = async (values: ProductListDto) => {
    try {
      const result = await updateProductList(editingRow!.productListId!, values);
      messageApi.success('产品更新成功');
      setShowEditModal(false);
      // 通过ref重置表单
      if (editFormRef.current) {
        const editForm = editFormRef.current.getForm();
        editForm.resetFields();
      }
      setEditingRow(undefined);
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('产品更新失败');
    }
  };

  // 处理删除产品（单个或批量）
  const handleDeleteProduct = useCallback((ids: string | string[]) => {
    // 确保ids是数组
    const deleteIds = Array.isArray(ids) ? ids : [ids];

    if (deleteIds.length === 0) {
      messageApi.error('请选择要删除的产品');
      return;
    }

    Modal.confirm({
      title: '确认删除',
      content: deleteIds.length === 1 ? '确认要删除这条产品信息吗？' : `确认要删除这${deleteIds.length}条产品信息吗？`,
      onOk: async () => {
        try {
          await deleteProductList(deleteIds);
          messageApi.success(deleteIds.length === 1 ? '产品删除成功' : `成功删除${deleteIds.length}个产品`);
          // 清空选中的行
          setSelectedRowKeys([]);
          // 刷新表格
          actionRef.current?.reload();
        } catch (error) {
          messageApi.error(deleteIds.length === 1 ? '产品删除失败' : '批量删除产品失败');
          console.error('Delete product error:', error);
        }
      }
    });
  }, [messageApi]);

  // 处理批量删除产品（调用统一的删除方法）
  const handleBatchDeleteProduct = useCallback(() => {
    handleDeleteProduct(selectedRowKeys.map(key => String(key)));
  }, [handleDeleteProduct, selectedRowKeys]);

  // 获取BOM列表数据
  const fetchBoms = useCallback(async (params: BomListQueryDto) => {
    try {
      const response = await getBomList(params);
      setBoms(response.data || []);
      setBomTotal(response.total || 0);
      setBomCurrent(params.current);
      setBomPageSize(params.pageSize);
    } catch (error) {
      message.error('获取BOM列表失败');
      console.error('Fetch BOMs error:', error);
    }
  }, []);

  // 打开BOM选择弹窗
  const handleOpenBomModal = useCallback(() => {
    // 重置搜索参数并获取BOM列表
    const params: BomListQueryDto = {
      current: 1,
      pageSize: 10
    };
    setBomSearchParams(params);
    fetchBoms(params);
    setIsBomModalVisible(true);
  }, [fetchBoms]);

  // 处理BOM选择
  const handleBomSelect = useCallback(async (bom: any) => {
    // 将选中的BOM ID设置到表单中
    console.log('选择BOM:', bom);
    console.log('设置BOM ID:', bom.bomId);
    console.log('设置BOM编号:', bom.bomCode);
    console.log('设置BOM名称:', bom.bomName);

    // 先设置表单值
    if (createFormRef.current) {
      const createForm = createFormRef.current.getForm();
      createForm.setFieldsValue({
        bomId: bom.bomId,
        bomCode: bom.bomCode,
        bomName: bom.bomName
      });
    }
    if (editFormRef.current) {
      const editForm = editFormRef.current.getForm();
      editForm.setFieldsValue({
        bomId: bom.bomId,
        bomCode: bom.bomCode,
        bomName: bom.bomName
      });
    }

    // 验证表单值是否正确设置
    try {
      if (createFormRef.current) {
        const createForm = createFormRef.current.getForm();
        const values = await createForm.getFieldsValue();
        console.log('新建表单值:', values);
        console.log('BOM ID字段值:', values.bomId);
        console.log('BOM编号字段值:', values.bomCode);
        console.log('BOM名称字段值:', values.bomName);
      }
    } catch (error) {
      console.error('获取表单值失败:', error);
    }

    // 延迟关闭弹窗，确保表单值更新
    setTimeout(() => {
      setIsBomModalVisible(false);
    }, 100);
  }, []);

  // 处理BOM分页变化
  const handleBomPaginationChange = useCallback((current: number, pageSize: number) => {
    const params: BomListQueryDto = {
      ...bomSearchParams,
      current,
      pageSize
    };
    setBomSearchParams(params);
    fetchBoms(params);
  }, [bomSearchParams, fetchBoms]);

  // 处理BOM搜索
  const handleBomSearch = useCallback(() => {
    const params: BomListQueryDto = {
      current: 1,
      pageSize: bomPageSize,
      bomCode: bomSearchValues.bomCode,
      bomName: bomSearchValues.bomName
    };
    setBomSearchParams(params);
    fetchBoms(params);
  }, [bomSearchValues, bomPageSize, fetchBoms]);

  // 处理BOM搜索输入变化
  const handleBomSearchInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setBomSearchValues(prev => ({
      ...prev,
      [name]: value
    }));
  }, []);

  // 获取工艺路线列表数据
  const fetchProcessRoutes = useCallback(async (params: ProcessRouteQueryDto) => {
    try {
      const response = await getProcessRouteList(params);
      setProcessRoutes(response.data || []);
      setProcessRouteTotal(response.total || 0);
      setProcessRouteCurrent(params.current);
      setProcessRoutePageSize(params.pageSize);
    } catch (error) {
      message.error('获取工艺路线列表失败');
      console.error('Fetch process routes error:', error);
    }
  }, []);

  // 打开工艺路线选择弹窗
  const handleOpenProcessRouteModal = useCallback(() => {
    // 重置搜索参数并获取工艺路线列表
    const params: ProcessRouteQueryDto = {
      current: 1,
      pageSize: 10
    };
    setProcessRouteSearchParams(params);
    fetchProcessRoutes(params);
    setIsProcessRouteModalVisible(true);
  }, [fetchProcessRoutes]);

  // 处理工艺路线选择
  const handleProcessRouteSelect = useCallback(async (processRoute: any) => {
    // 将选中的工艺路线 ID设置到表单中
    console.log('选择工艺路线:', processRoute);
    console.log('设置工艺路线 ID:', processRoute.id);
    console.log('设置工艺路线编号:', processRoute.routeCode);
    console.log('设置工艺路线名称:', processRoute.routeName);

    // 先设置表单值
    if (createFormRef.current) {
      const createForm = createFormRef.current.getForm();
      createForm.setFieldsValue({
        processRouteId: processRoute.id,
        processRouteCode: processRoute.routeCode,
        processRouteName: processRoute.routeName
      });
    }
    if (editFormRef.current) {
      const editForm = editFormRef.current.getForm();
      editForm.setFieldsValue({
        processRouteId: processRoute.id,
        processRouteCode: processRoute.routeCode,
        processRouteName: processRoute.routeName
      });
    }

    // 验证表单值是否正确设置
    try {
      if (createFormRef.current) {
        const createForm = createFormRef.current.getForm();
        const values = await createForm.getFieldsValue();
        console.log('新建表单值:', values);
        console.log('工艺路线 ID字段值:', values.processRouteId);
        console.log('工艺路线编号字段值:', values.processRouteCode);
      }
    } catch (error) {
      console.error('获取表单值失败:', error);
    }

    // 延迟关闭弹窗，确保表单值更新
    setTimeout(() => {
      setIsProcessRouteModalVisible(false);
    }, 100);
  }, []);

  // 处理工艺路线分页变化
  const handleProcessRoutePaginationChange = useCallback((current: number, pageSize: number) => {
    const params: ProcessRouteQueryDto = {
      ...processRouteSearchParams,
      current,
      pageSize
    };
    setProcessRouteSearchParams(params);
    fetchProcessRoutes(params);
  }, [processRouteSearchParams, fetchProcessRoutes]);

  // 处理工艺路线搜索
  const handleProcessRouteSearch = useCallback(() => {
    const params: ProcessRouteQueryDto = {
      current: 1,
      pageSize: processRoutePageSize,
      routeCode: processRouteSearchValues.routeCode,
      routeName: processRouteSearchValues.routeName
    };
    setProcessRouteSearchParams(params);
    fetchProcessRoutes(params);
  }, [processRouteSearchValues, processRoutePageSize, fetchProcessRoutes]);

  // 处理工艺路线搜索输入变化
  const handleProcessRouteSearchInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setProcessRouteSearchValues(prev => ({
      ...prev,
      [name]: value
    }));
  }, []);

  // 打开编辑弹窗
  const openEditModal = (record: ProductListDto) => {
    setEditingRow(record);
    // 通过ref设置表单值
    if (editFormRef.current) {
      const editForm = editFormRef.current.getForm();
      editForm.setFieldsValue({
        productListId: record.productListId,
        productName: record.productName,
        productCode: record.productCode,
        bomId: record.bomId,
        bomCode: record.bomCode,
        bomName: record.bomName,
        processRouteId: record.processRouteId,
        processRouteCode: record.processRouteCode,
        processRouteName: record.processRouteName,
        productType: record.productType,
        remark: record.remark,
      });
    }
    setShowEditModal(true);
  };

  // 产品列表表格列定义
  const columns = getProductColumns({
    onEdit: openEditModal,
    onDelete: handleDeleteProduct
  });

  return (
    <PageContainer>
      <ProTable<ProductListDto>
        headerTitle="产品管理列表"
        actionRef={actionRef}
        rowKey="productListId"
        className="product-list-table"
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
        request={async (
          params
        ): Promise<RequestData<ProductListDto>> => {
          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            productName: params.productName,
            productCode: params.productCode,
            productType: params.productType,
          });

          const queryParams: ProductListQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            sortField: 'CreateTime',
            sortOrder: 'descend',
            productName: params.productName,
            productCode: params.productCode,
            productType: params.productType,
          };

          try {
            const response = await getProductListList(queryParams);

            // 为每个产品补充BOM和工艺路线信息
            const enhancedData = await Promise.all(
              (response.data || []).map(async (product: ProductListDto) => {
                let updatedProduct = { ...product };

                // 补充BOM信息
                if (product.bomId) {
                  try {
                    const bomList = await getBomList({ current: 1, pageSize: 1000 });
                    const bom = bomList.data.find((b: any) => b.bomId === product.bomId);

                    if (bom) {
                      updatedProduct = {
                        ...updatedProduct,
                        bomCode: bom.bomCode || '',
                        bomName: bom.bomName || ''
                      };
                    }
                  } catch (error) {
                    console.error('获取BOM信息失败:', error);
                  }
                }

                // 补充工艺路线信息
                if (product.processRouteId) {
                  try {
                    const processRouteList = await getProcessRouteList({ current: 1, pageSize: 1000 });

                    let processRoute = processRouteList.data.find((pr: any) => {
                      return pr.processRouteId === product.processRouteId ||
                        pr.id === product.processRouteId ||
                        pr.processId === product.processRouteId ||
                        pr.routeId === product.processRouteId;
                    });

                    if (processRoute) {
                      updatedProduct = {
                        ...updatedProduct,
                        processRouteCode: processRoute.processRouteCode || processRoute.code || processRoute.routeCode || '',
                        processRouteName: processRoute.processRouteName || processRoute.name || processRoute.routeName || ''
                      };
                    }
                  } catch (error) {
                    console.error('获取工艺路线信息失败:', error);
                  }
                }

                return updatedProduct;
              })
            );

            const result = {
              data: enhancedData,
              total: response.total || 0,
              success: response.success ?? true,
            };

            return result;
          } catch (error) {
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        columns={columns}
        pagination={{
          pageSize: currentSearchParams.pageSize,
          pageSizeOptions: ['10', '20', '50', '100'],
          showSizeChanger: true,
          showTotal: (total) => `共 ${total} 条数据`,
        }}
        toolBarRender={() => [
          <Button
            key="create"
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setShowCreateModal(true);
            }}
          >
            新建产品
          </Button>,
          <Popconfirm
            title="批量删除"
            description="确定要删除选中的所有产品吗？"
            onConfirm={() => {
              // 调用批量删除处理函数
              handleBatchDeleteProduct();
            }}
            okText="确定"
            cancelText="取消"
          >
            <Button
              key="batchDelete"
              danger
              disabled={!selectedRowKeys.length}
              icon={<DeleteOutlined />}
            >
              批量删除
            </Button>
          </Popconfirm>
        ]}
        rowSelection={{
          selectedRowKeys,
          onChange: (newSelectedRowKeys) => {
            setSelectedRowKeys(newSelectedRowKeys);
          },
        }}
        onRow={(record) => ({
          onClick: () => {
            setCurrentRow(record);
            setShowDetail(true);
          },
        })}
      />

      <Drawer
        width={600}
        placement="right"
        open={showDetail}
        onClose={() => setShowDetail(false)}
        closable={true}
        title="产品详情"
        className="product-info-drawer"
        rootClassName="product-info-drawer"
        styles={{
          content: {
            background: '#fff',
            borderLeft: '1px solid rgba(72,115,255,0.32)',
            boxShadow:
              '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
          },
          header: {
            background: '#fff',
            borderBottom: '1px solid rgba(72,115,255,0.22)'
          },
          body: {
            background: '#fff'
          },
          mask: {
            background: 'rgba(4,10,22,0.35)',
            backdropFilter: 'blur(2px)'
          }
        }}
      >
        {currentRow && (
          <ProDescriptions<ProductListDto>
            column={2}
            title={`产品：${currentRow.productCode}`}
            dataSource={currentRow}
            columns={[
              {
                title: '产品ID',
                dataIndex: 'productListId',
              },
              {
                title: '产品编码',
                dataIndex: 'productCode',
              },
              {
                title: '产品名称',
                dataIndex: 'productName',
              },
              {
                title: 'BOM ID',
                dataIndex: 'bomId',
              },
              {
                title: '工艺路线 ID',
                dataIndex: 'processRouteId',
              },
              {
                title: '产品类型',
                dataIndex: 'productType',
              },
              {
                title: '备注',
                dataIndex: 'remark',
              },
              {
                title: '创建时间',
                dataIndex: 'createTime',
                valueType: 'dateTime',
              },
              {
                title: '创建人',
                dataIndex: 'createBy',
              },
              {
                title: '更新时间',
                dataIndex: 'updateTime',
                valueType: 'dateTime',
              },
              {
                title: '更新人',
                dataIndex: 'updateBy',
              },
            ]}
          />
        )}
      </Drawer>

      {/* 新建产品模态框 */}
      <ProductFormModal
        ref={createFormRef}
        open={showCreateModal}
        title="新建产品"
        onCancel={() => {
          setShowCreateModal(false);
        }}
        onSave={handleCreateProduct}
        onOpenBomModal={handleOpenBomModal}
        onOpenProcessRouteModal={handleOpenProcessRouteModal}
      />

      {/* 编辑产品模态框 */}
      <ProductFormModal
        ref={editFormRef}
        open={showEditModal}
        title="编辑产品"
        initialValues={editingRow}
        onCancel={() => {
          setShowEditModal(false);
          setEditingRow(undefined);
        }}
        onSave={handleEditProduct}
        onOpenBomModal={handleOpenBomModal}
        onOpenProcessRouteModal={handleOpenProcessRouteModal}
      />

      {/* BOM选择弹窗 */}
      <BomSelectModal
        open={isBomModalVisible}
        onCancel={() => setIsBomModalVisible(false)}
        boms={boms}
        bomTotal={bomTotal}
        bomCurrent={bomCurrent}
        bomPageSize={bomPageSize}
        bomSearchValues={bomSearchValues}
        onBomSelect={handleBomSelect}
        onBomSearch={handleBomSearch}
        onBomSearchInputChange={handleBomSearchInputChange}
        onBomPaginationChange={handleBomPaginationChange}
      />

      {/* 工艺路线选择弹窗 */}
      <ProcessRouteSelectModal
        open={isProcessRouteModalVisible}
        onCancel={() => setIsProcessRouteModalVisible(false)}
        processRoutes={processRoutes}
        processRouteTotal={processRouteTotal}
        processRouteCurrent={processRouteCurrent}
        processRoutePageSize={processRoutePageSize}
        processRouteSearchValues={processRouteSearchValues}
        onProcessRouteSelect={handleProcessRouteSelect}
        onProcessRouteSearch={handleProcessRouteSearch}
        onProcessRouteSearchInputChange={handleProcessRouteSearchInputChange}
        onProcessRoutePaginationChange={handleProcessRoutePaginationChange}
      />
    </PageContainer>
  );
};

export default ProductList;