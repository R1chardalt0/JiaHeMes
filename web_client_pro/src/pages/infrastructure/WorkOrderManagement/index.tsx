import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState } from 'react';
import { Drawer, Button, message, Modal, Form, Input, InputNumber, Switch, Select, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { getWorkOrderList, createWorkOrder, updateWorkOrder, deleteWorkOrder } from '@/services/Api/Infrastructure/WorkOrder';
import { WorkOrderDto, WorkOrderQueryDto, WorkOrderStatusText, WorkOrderStatusColor, CreateWorkOrderDto, UpdateWorkOrderDto } from '@/services/Model/Infrastructure/WorkOrder';
import type { RequestData } from '@ant-design/pro-components';

const WorkOrderManagement: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [currentRow, setCurrentRow] = useState<WorkOrderDto>();
  const [editingRow, setEditingRow] = useState<WorkOrderDto>();
  const [messageApi] = message.useMessage();
  const [form] = Form.useForm<CreateWorkOrderDto>();
  const [editForm] = Form.useForm<UpdateWorkOrderDto>();
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [currentSearchParams, setCurrentSearchParams] = useState<WorkOrderQueryDto>({
    current: 1,
    pageSize: 50
  });

  // 处理新建工单
  const handleCreateWorkOrder = async (values: CreateWorkOrderDto) => {
    try {
      const result = await createWorkOrder(values);
      messageApi.success('工单创建成功');
      setShowCreateModal(false);
      form.resetFields();
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('工单创建失败');
    }
  };

  // 处理编辑工单
  const handleEditWorkOrder = async (values: UpdateWorkOrderDto) => {
    try {
      const result = await updateWorkOrder(editingRow!.id!, values);
      messageApi.success('工单更新成功');
      setShowEditModal(false);
      editForm.resetFields();
      setEditingRow(undefined);
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('工单更新失败');
    }
  };

  // 处理删除工单
  const handleDeleteWorkOrder = async (id: number) => {
    try {
      await deleteWorkOrder([id]);
      messageApi.success('工单删除成功');
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('工单删除失败');
    }
  };

  // 处理批量删除工单
  const handleBatchDeleteWorkOrder = async () => {
    try {
      // 将selectedRowKeys转换为number[]类型
      const ids = selectedRowKeys.map(key => Number(key));
      await deleteWorkOrder(ids);
      messageApi.success(`成功删除${ids.length}个工单`);
      // 清空选中的行
      setSelectedRowKeys([]);
      // 刷新表格
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('批量删除工单失败');
    }
  };

  // 打开编辑弹窗
  const openEditModal = (record: WorkOrderDto) => {
    setEditingRow(record);
    editForm.setFieldsValue({
      id: record.id,
      code: record.code,
      productCode: record.productCode,
      bomRecipeId: record.bomRecipeId,
      isInfinite: record.isInfinite,
      workOrderAmount: record.workOrderAmount,
      perTraceInfo: record.perTraceInfo,
      docStatus: record.docStatus,
    });
    setShowEditModal(true);
  };

  const columns: ProColumns<WorkOrderDto>[] = [
    {
      title: '工单编号',
      dataIndex: 'code',
      key: 'code',
      width: 180,
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      )
    },
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 150,
    },
    {
      title: 'BOM配方',
      dataIndex: 'bomRecipeId',
      key: 'bomRecipeId',
      width: 180,
      search: false,
    },
    {
      title: '是否无限生产',
      dataIndex: 'isInfinite',
      key: 'isInfinite',
      width: 120,
      search: false,
      render: (val) => val ? '是' : '否',
    },
    {
      title: '生产数量',
      dataIndex: 'workOrderAmount',
      key: 'workOrderAmount',
      width: 120,
      search: false,
      render: (val, record) => record.isInfinite ? '无限' : (val?.toLocaleString() || 0),
    },
    {
      title: '追踪增量',
      dataIndex: 'perTraceInfo',
      key: 'perTraceInfo',
      width: 120,
      search: false,
      render: (val) => val?.toLocaleString() || 0,
    },
    {
      title: '工单状态',
      dataIndex: 'docStatus',
      key: 'docStatus',
      width: 100,
      valueType: 'select',
      valueEnum: {
        0: { text: '草稿', status: 'Drafting' },
        1: { text: '已提交', status: 'Commited' },
        2: { text: '已拒绝', status: 'Rejected' },
        3: { text: '已通过', status: 'Approved' },
      },
    },
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
            description="确定要删除该工单吗？"
            onConfirm={(e) => {
              e?.stopPropagation();
              handleDeleteWorkOrder(record.id!);
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
      <ProTable<WorkOrderDto>
        headerTitle="工单管理列表"
        actionRef={actionRef}
        rowKey="id"
        className="work-order-glass-table"
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
        ): Promise<RequestData<WorkOrderDto>> => {
          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            code: params.code,
            productCode: params.productCode,
            bomRecipeId: params.bomRecipeId,
            docStatus: params.docStatus,
          });

          const queryParams: WorkOrderQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            sortField: 'id',
            sortOrder: 'ascend',
            code: params.code,
            productCode: params.productCode,
            bomRecipeId: params.bomRecipeId,
            docStatus: params.docStatus,
          };

          try {
            const response = await getWorkOrderList(queryParams);

            const result = {
              data: (response.data || []).sort((a: WorkOrderDto, b: WorkOrderDto) => {
                return (a.id || 0) - (b.id || 0);
              }),
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
            新建工单
          </Button>,
          <Popconfirm
            title="批量删除"
            description="确定要删除选中的所有工单吗？"
            onConfirm={() => {
              // 调用批量删除处理函数
              handleBatchDeleteWorkOrder();
            }}
            okText="确定"
            cancelText="取消"
          >
            <Button
              key="batchDelete"
              danger
              disabled={selectedRowKeys.length === 0}
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
        title="工单详情"
        className="work-order-info-drawer"
        rootClassName="work-order-info-drawer"
        styles={{
          content: {
            background:
              'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
            backdropFilter: 'blur(14px) saturate(115%)',
            WebkitBackdropFilter: 'blur(14px) saturate(115%)',
            borderLeft: '1px solid rgba(72,115,255,0.32)',
            boxShadow:
              '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
          },
          header: {
            background: 'transparent',
            borderBottom: '1px solid rgba(72,115,255,0.22)'
          },
          body: {
            background: 'transparent'
          },
          mask: {
            background: 'rgba(4,10,22,0.35)',
            backdropFilter: 'blur(2px)'
          }
        }}
      >
        {currentRow && (
          <ProDescriptions<WorkOrderDto>
            column={2}
            title={`工单：${currentRow.code}`}
            dataSource={currentRow}
            columns={[
              {
                title: '工单编号',
                dataIndex: 'code',
              },
              {
                title: '产品编码',
                dataIndex: 'productCode',
              },
              {
                title: 'BOM配方ID',
                dataIndex: 'bomRecipeId',
              },
              {
                title: '是否无限生产',
                dataIndex: 'isInfinite',
                render: (val) => val ? '是' : '否',
              },
              {
                title: '生产数量',
                dataIndex: 'workOrderAmount',
                render: (val, record) => record.isInfinite ? '无限' : (val?.toLocaleString() || 0),
              },
              {
                title: '追踪增量',
                dataIndex: 'perTraceInfo',
                render: (val) => val?.toLocaleString() || 0,
              },
              {
                title: '工单状态',
                dataIndex: 'docStatus',
                render: (status) => (
                  <span style={{
                    color: WorkOrderStatusColor[status as number] || '#000',
                    fontWeight: 500
                  }}>
                    {WorkOrderStatusText[status as number] || '未知'}
                  </span>
                )
              },
            ]}
          />
        )}
      </Drawer>

      <Modal
        title="新建工单"
        open={showCreateModal}
        onCancel={() => {
          setShowCreateModal(false);
          form.resetFields();
        }}
        onOk={() => {
          form.validateFields().then((values) => {
            handleCreateWorkOrder(values);
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
            label="工单编号"
            name="code"
            rules={[
              { required: true, message: '请输入工单编号' },
            ]}
          >
            <Input placeholder="请输入工单编号" />
          </Form.Item>

          {/* <Form.Item
            label="产品编码"
            name="productCode"
            rules={[
              { required: true, message: '请输入产品编码' },
            ]}
          >
            <Input placeholder="请输入产品编码" />
          </Form.Item> */}

          <Form.Item
            label="BOM配方ID"
            name="bomRecipeId"
            rules={[
              { required: true, message: '请输入BOM配方ID' },
            ]}
          >
            <InputNumber
              placeholder="请输入BOM配方ID"
              style={{ width: '100%' }}
              min={1}
            />
          </Form.Item>

          <Form.Item
            label="是否无限生产"
            name="isInfinite"
            valuePropName="checked"
            initialValue={false}
          >
            <Switch />
          </Form.Item>

          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.isInfinite !== currentValues.isInfinite}
          >
            {({ getFieldValue }) =>
              !getFieldValue('isInfinite') ? (
                <Form.Item
                  label="生产数量"
                  name="workOrderAmount"
                  rules={[
                    { required: true, message: '请输入生产数量' },
                    { type: 'number', min: 1, message: '生产数量必须大于0' },
                  ]}
                >
                  <InputNumber
                    placeholder="请输入生产数量"
                    style={{ width: '100%' }}
                    min={1}
                  />
                </Form.Item>
              ) : null
            }
          </Form.Item>

          <Form.Item
            label="追踪增量"
            name="perTraceInfo"
          >
            <InputNumber
              placeholder="请输入追踪增量"
              style={{ width: '100%' }}
              min={1}
            />
          </Form.Item>

          <Form.Item
            label="工单状态"
            name="docStatus"
            initialValue={0}
          >
            <Select
              placeholder="请选择工单状态"
              options={[
                { label: '草稿', value: 0 },
                { label: '已提交', value: 1 },
                { label: '已拒绝', value: 2 },
                { label: '已通过', value: 3 },
              ]}
            />
          </Form.Item>
        </Form>
      </Modal>
      <Modal
        title="编辑工单"
        open={showEditModal}
        onCancel={() => {
          setShowEditModal(false);
          editForm.resetFields();
          setEditingRow(undefined);
        }}
        onOk={() => {
          editForm.validateFields().then((values) => {
            handleEditWorkOrder(values);
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
            name="id"
            hidden
          >
            <Input />
          </Form.Item>

          <Form.Item
            label="工单编号"
            name="code"
            rules={[
              { required: true, message: '请输入工单编号' },
            ]}
          >
            <Input placeholder="请输入工单编号" />
          </Form.Item>

          {/* <Form.Item
            label="产品编码"
            name="productCode"
            rules={[
              { required: true, message: '请输入产品编码' },
            ]}
          >
            <Input placeholder="请输入产品编码" />
          </Form.Item> */}

          <Form.Item
            label="BOM配方ID"
            name="bomRecipeId"
            rules={[
              { required: true, message: '请输入BOM配方ID' },
            ]}
          >
            <InputNumber
              placeholder="请输入BOM配方ID"
              style={{ width: '100%' }}
              min={1}
            />
          </Form.Item>

          <Form.Item
            label="是否无限生产"
            name="isInfinite"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>

          <Form.Item
            noStyle
            shouldUpdate={(prevValues, currentValues) => prevValues.isInfinite !== currentValues.isInfinite}
          >
            {({ getFieldValue }) =>
              !getFieldValue('isInfinite') ? (
                <Form.Item
                  label="生产数量"
                  name="workOrderAmount"
                  rules={[
                    { required: true, message: '请输入生产数量' },
                    { type: 'number', min: 1, message: '生产数量必须大于0' },
                  ]}
                >
                  <InputNumber
                    placeholder="请输入生产数量"
                    style={{ width: '100%' }}
                    min={1}
                  />
                </Form.Item>
              ) : null
            }
          </Form.Item>

          <Form.Item
            label="追踪增量"
            name="perTraceInfo"
          >
            <InputNumber
              placeholder="请输入追踪增量"
              style={{ width: '100%' }}
              min={1}
            />
          </Form.Item>

          <Form.Item
            label="工单状态"
            name="docStatus"
          >
            <Select
              placeholder="请选择工单状态"
              options={[
                { label: '草稿', value: 0 },
                { label: '已提交', value: 1 },
                { label: '已拒绝', value: 2 },
                { label: '已通过', value: 3 },
              ]}
            />
          </Form.Item>
        </Form>
      </Modal>
    </PageContainer>
  );
};

export default WorkOrderManagement;
