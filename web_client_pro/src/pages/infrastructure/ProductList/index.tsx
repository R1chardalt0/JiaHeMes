import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState } from 'react';
import { Drawer, Button, message, Modal, Form, Input, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { getProductListList, createProductList, updateProductList, deleteProductList } from '@/services/Api/Infrastructure/ProductList';
import { ProductListDto, ProductListQueryDto } from '@/services/Model/Infrastructure/ProductList';
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
      ellipsis: true,
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

          <Form.Item
            label="BOM ID"
            name="bomId"
            rules={[
              { required: true, message: '请输入BOM ID' },
            ]}
          >
            <Input placeholder="请输入BOM ID" />
          </Form.Item>

          <Form.Item
            label="工艺路线 ID"
            name="processRouteId"
            rules={[
              { required: true, message: '请输入工艺路线 ID' },
            ]}
          >
            <Input placeholder="请输入工艺路线 ID" />
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

          <Form.Item
            label="BOM ID"
            name="bomId"
            rules={[
              { required: true, message: '请输入BOM ID' },
            ]}
          >
            <Input placeholder="请输入BOM ID" />
          </Form.Item>

          <Form.Item
            label="工艺路线 ID"
            name="processRouteId"
            rules={[
              { required: true, message: '请输入工艺路线 ID' },
            ]}
          >
            <Input placeholder="请输入工艺路线 ID" />
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
    </PageContainer>
  );
};

export default ProductList;