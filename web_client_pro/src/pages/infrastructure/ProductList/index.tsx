import type { ActionType, ProColumns } from '@ant-design/pro-components';
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

const ProductList: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [currentRow, setCurrentRow] = useState<ProductListDto>();
  const [editingRow, setEditingRow] = useState<ProductListDto>();
  const [messageApi] = message.useMessage();
  const [form] = Form.useForm<ProductListDto>();
  const [editForm] = Form.useForm<ProductListDto>();
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
      form.resetFields();
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
      editForm.resetFields();
      setEditingRow(undefined);
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('产品更新失败');
    }
  };

  // 处理删除产品
  const handleDeleteProduct = async (id: string) => {
    try {
      await deleteProductList(id);
      messageApi.success('产品删除成功');
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('产品删除失败');
    }
  };

  // 处理批量删除产品
  const handleBatchDeleteProduct = async () => {
    try {
      // 将selectedRowKeys转换为string[]类型
      const ids = selectedRowKeys.map(key => String(key));
      await deleteProductList(ids);
      messageApi.success(`成功删除${ids.length}个产品`);
      // 清空选中的行
      setSelectedRowKeys([]);
      // 刷新表格
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('批量删除产品失败');
    }
  };

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

    // 先设置表单值
    form.setFieldsValue({ bomId: bom.bomId });
    editForm.setFieldsValue({ bomId: bom.bomId });

    // 验证表单值是否正确设置
    try {
      const values = await form.getFieldsValue();
      console.log('表单值:', values);
      console.log('BOM ID字段值:', values.bomId);
    } catch (error) {
      console.error('获取表单值失败:', error);
    }

    // 延迟关闭弹窗，确保表单值更新
    setTimeout(() => {
      setIsBomModalVisible(false);
    }, 100);
  }, [form, editForm]);

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

    // 先设置表单值
    form.setFieldsValue({ processRouteId: processRoute.id });
    editForm.setFieldsValue({ processRouteId: processRoute.id });

    // 验证表单值是否正确设置
    try {
      const values = await form.getFieldsValue();
      console.log('表单值:', values);
      console.log('工艺路线 ID字段值:', values.processRouteId);
    } catch (error) {
      console.error('获取表单值失败:', error);
    }

    // 延迟关闭弹窗，确保表单值更新
    setTimeout(() => {
      setIsProcessRouteModalVisible(false);
    }, 100);
  }, [form, editForm]);

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
    editForm.setFieldsValue({
      productListId: record.productListId,
      productName: record.productName,
      productCode: record.productCode,
      bomId: record.bomId,
      processRouteId: record.processRouteId,
      productType: record.productType,
      remark: record.remark,
    });
    setShowEditModal(true);
  };

  const columns: ProColumns<ProductListDto>[] = [
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 180,
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      )
    },
    {
      title: '产品名称',
      dataIndex: 'productName',
      key: 'productName',
      width: 180,
    },
    {
      title: 'BOM ID',
      dataIndex: 'bomId',
      key: 'bomId',
      width: 180,
    },
    {
      title: '工艺路线 ID',
      dataIndex: 'processRouteId',
      key: 'processRouteId',
      width: 180,
    },
    {
      title: '产品类型',
      dataIndex: 'productType',
      key: 'productType',
      width: 180,
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      search: false,
    },
    // {
    //   title: '创建时间',
    //   dataIndex: 'createTime',
    //   key: 'createTime',
    //   width: 180,
    //   valueType: 'dateTime',
    //   search: false,
    // },
    // {
    //   title: '更新时间',
    //   dataIndex: 'updateTime',
    //   key: 'updateTime',
    //   width: 180,
    //   valueType: 'dateTime',
    //   search: false,
    // },
    {
      title: '操作',
      key: 'action',
      width: 150,
      search: false,
      render: (_, record) => (
        <div>
          <Button
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={(e) => {
              e.stopPropagation();
              openEditModal(record);
            }}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除"
            description="确定要删除该产品吗？"
            onConfirm={(e) => {
              e?.stopPropagation();
              handleDeleteProduct(record.productListId!);
            }}
            okText="确定"
            cancelText="取消"
          >
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={(e) => e.stopPropagation()}
            >
              删除
            </Button>
          </Popconfirm>
        </div>
      ),
    },
  ];

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

            const result = {
              data: response.data || [],
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

      <Modal
        title="新建产品"
        open={showCreateModal}
        onCancel={() => {
          setShowCreateModal(false);
          form.resetFields();
        }}
        onOk={() => {
          form.validateFields().then((values) => {
            handleCreateProduct(values);
          });
        }}
        width={600}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          autoComplete="off"
        >
          <Form.Item
            label="产品编码"
            name="productCode"
            rules={[
              { required: true, message: '请输入产品编码' },
            ]}
          >
            <Input placeholder="请输入产品编码" />
          </Form.Item>

          <Form.Item
            label="产品名称"
            name="productName"
            rules={[
              { required: true, message: '请输入产品名称' },
            ]}
          >
            <Input placeholder="请输入产品名称" />
          </Form.Item>

          <Form.Item label="BOM ID" rules={[{ required: true, message: '请选择BOM' }]}>
            <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
              <Form.Item name="bomId" noStyle>
                <Input placeholder="请选择BOM" readOnly style={{ flex: 1 }} />
              </Form.Item>
              <Button type="primary" icon={<SearchOutlined />} onClick={handleOpenBomModal} style={{ marginTop: 4 }}>选择</Button>
            </div>
          </Form.Item>

          <Form.Item label="工艺路线 ID" rules={[{ required: true, message: '请选择工艺路线' }]}>
            <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
              <Form.Item name="processRouteId" noStyle>
                <Input placeholder="请选择工艺路线" readOnly style={{ flex: 1 }} />
              </Form.Item>
              <Button type="primary" icon={<SearchOutlined />} onClick={handleOpenProcessRouteModal} style={{ marginTop: 4 }}>选择</Button>
            </div>
          </Form.Item>

          <Form.Item
            label="产品类型"
            name="productType"
            rules={[
              { required: true, message: '请输入产品类型' },
            ]}
          >
            <Input placeholder="请输入产品类型" />
          </Form.Item>

          <Form.Item
            label="备注"
            name="remark"
          >
            <Input.TextArea rows={4} placeholder="请输入备注" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="编辑产品"
        open={showEditModal}
        onCancel={() => {
          setShowEditModal(false);
          editForm.resetFields();
          setEditingRow(undefined);
        }}
        onOk={() => {
          editForm.validateFields().then((values) => {
            handleEditProduct(values);
          });
        }}
        width={600}
        destroyOnClose
      >
        <Form
          form={editForm}
          layout="vertical"
          autoComplete="off"
        >
          <Form.Item
            name="productListId"
            hidden
          >
            <Input />
          </Form.Item>

          <Form.Item
            label="产品编码"
            name="productCode"
            rules={[
              { required: true, message: '请输入产品编码' },
            ]}
          >
            <Input placeholder="请输入产品编码" />
          </Form.Item>

          <Form.Item
            label="产品名称"
            name="productName"
            rules={[
              { required: true, message: '请输入产品名称' },
            ]}
          >
            <Input placeholder="请输入产品名称" />
          </Form.Item>

          <Form.Item label="BOM ID" rules={[{ required: true, message: '请选择BOM' }]}>
            <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
              <Form.Item name="bomId" noStyle>
                <Input placeholder="请选择BOM" readOnly style={{ flex: 1 }} />
              </Form.Item>
              <Button type="primary" icon={<SearchOutlined />} onClick={handleOpenBomModal} style={{ marginTop: 4 }}>选择</Button>
            </div>
          </Form.Item>

          <Form.Item label="工艺路线 ID" rules={[{ required: true, message: '请选择工艺路线' }]}>
            <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
              <Form.Item name="processRouteId" noStyle>
                <Input placeholder="请选择工艺路线" readOnly style={{ flex: 1 }} />
              </Form.Item>
              <Button type="primary" icon={<SearchOutlined />} onClick={handleOpenProcessRouteModal} style={{ marginTop: 4 }}>选择</Button>
            </div>
          </Form.Item>

          <Form.Item
            label="产品类型"
            name="productType"
            rules={[
              { required: true, message: '请输入产品类型' },
            ]}
          >
            <Input placeholder="请输入产品类型" />
          </Form.Item>

          <Form.Item
            label="备注"
            name="remark"
          >
            <Input.TextArea rows={4} placeholder="请输入备注" />
          </Form.Item>
        </Form>
      </Modal>

      {/* BOM选择弹窗 */}
      <Modal
        title="选择BOM"
        open={isBomModalVisible}
        onCancel={() => setIsBomModalVisible(false)}
        footer={null}
        width={800}
      >
        <div style={{ marginBottom: 16, display: 'flex', gap: 8 }}>
          <Input
            name="bomCode"
            placeholder="BOM编码"
            value={bomSearchValues.bomCode}
            onChange={handleBomSearchInputChange}
            style={{ width: 200 }}
          />
          <Input
            name="bomName"
            placeholder="BOM名称"
            value={bomSearchValues.bomName}
            onChange={handleBomSearchInputChange}
            style={{ width: 200 }}
          />
          <Button type="primary" onClick={handleBomSearch}>搜索</Button>
        </div>
        <Table
          dataSource={boms}
          columns={[
            {
              title: 'BOM编码',
              dataIndex: 'bomCode',
              key: 'bomCode',
            },
            {
              title: 'BOM名称',
              dataIndex: 'bomName',
              key: 'bomName',
            },
            {
              title: 'BOM ID',
              dataIndex: 'bomId',
              key: 'bomId',
            },
            {
              title: '操作',
              key: 'action',
              render: (_, record) => (
                <Button type="link" onClick={() => handleBomSelect(record)}>选择</Button>
              ),
            },
          ]}
          pagination={{
            current: bomCurrent,
            pageSize: bomPageSize,
            total: bomTotal,
            onChange: handleBomPaginationChange,
          }}
        />
      </Modal>

      {/* 工艺路线选择弹窗 */}
      <Modal
        title="选择工艺路线"
        open={isProcessRouteModalVisible}
        onCancel={() => setIsProcessRouteModalVisible(false)}
        footer={null}
        width={800}
      >
        <div style={{ marginBottom: 16, display: 'flex', gap: 8 }}>
          <Input
            name="routeCode"
            placeholder="工艺路线编码"
            value={processRouteSearchValues.routeCode}
            onChange={handleProcessRouteSearchInputChange}
            style={{ width: 200 }}
          />
          <Input
            name="routeName"
            placeholder="工艺路线名称"
            value={processRouteSearchValues.routeName}
            onChange={handleProcessRouteSearchInputChange}
            style={{ width: 200 }}
          />
          <Button type="primary" onClick={handleProcessRouteSearch}>搜索</Button>
        </div>
        <Table
          dataSource={processRoutes}
          columns={[
            {
              title: '工艺路线编码',
              dataIndex: 'routeCode',
              key: 'routeCode',
            },
            {
              title: '工艺路线名称',
              dataIndex: 'routeName',
              key: 'routeName',
            },
            {
              title: '工艺路线 ID',
              dataIndex: 'id',
              key: 'id',
            },
            {
              title: '操作',
              key: 'action',
              render: (_, record) => (
                <Button type="link" onClick={() => handleProcessRouteSelect(record)}>选择</Button>
              ),
            },
          ]}
          pagination={{
            current: processRouteCurrent,
            pageSize: processRoutePageSize,
            total: processRouteTotal,
            onChange: handleProcessRoutePaginationChange,
          }}
        />
      </Modal>
    </PageContainer>
  );
};

export default ProductList;