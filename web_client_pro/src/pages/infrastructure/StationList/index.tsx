import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState } from 'react';
import { Drawer, Button, message, Modal, Form, Input, Popconfirm } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { getStationListList, createStationList, updateStationList, deleteStationList } from '@/services/Api/Infrastructure/StationList';
import { StationListDto, StationListQueryDto } from '@/services/Model/Infrastructure/StationList';
import type { RequestData } from '@ant-design/pro-components';

const StationList: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [currentRow, setCurrentRow] = useState<StationListDto>();
  const [editingRow, setEditingRow] = useState<StationListDto>();
  const [messageApi] = message.useMessage();
  const [form] = Form.useForm<StationListDto>();
  const [editForm] = Form.useForm<StationListDto>();
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [currentSearchParams, setCurrentSearchParams] = useState<StationListQueryDto>({
    current: 1,
    pageSize: 50
  });

  // 处理新建站点
  const handleCreateStation = async (values: StationListDto) => {
    try {
      const result = await createStationList(values);
      messageApi.success('站点创建成功');
      setShowCreateModal(false);
      form.resetFields();
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('站点创建失败');
    }
  };

  // 处理编辑站点
  const handleEditStation = async (values: StationListDto) => {
    try {
      const result = await updateStationList(editingRow!.stationId!, values);
      messageApi.success('站点更新成功');
      setShowEditModal(false);
      editForm.resetFields();
      setEditingRow(undefined);
      actionRef.current?.reload();
    } catch (error) {
      messageApi.error('站点更新失败');
    }
  };

  // 处理删除站点（单个或批量）
  const handleDeleteStation = (ids: string | string[]) => {
    // 确保ids是数组
    const deleteIds = Array.isArray(ids) ? ids : [ids];

    if (deleteIds.length === 0) {
      messageApi.error('请选择要删除的站点');
      return;
    }

    Modal.confirm({
      title: '确认删除',
      content: deleteIds.length === 1 ? '确认要删除这条站点信息吗？' : `确认要删除这${deleteIds.length}条站点信息吗？`,
      onOk: async () => {
        try {
          await deleteStationList(deleteIds);
          messageApi.success(deleteIds.length === 1 ? '站点删除成功' : `成功删除${deleteIds.length}个站点`);
          // 清空选中的行
          setSelectedRowKeys([]);
          // 刷新表格
          actionRef.current?.reload();
        } catch (error) {
          messageApi.error(deleteIds.length === 1 ? '站点删除失败' : '批量删除站点失败');
          console.error('Delete station error:', error);
        }
      }
    });
  };

  // 打开编辑弹窗
  const openEditModal = (record: StationListDto) => {
    setEditingRow(record);
    editForm.setFieldsValue({
      stationId: record.stationId,
      stationName: record.stationName,
      stationCode: record.stationCode,
      remark: record.remark,
    });
    setShowEditModal(true);
  };

  const columns: ProColumns<StationListDto>[] = [
    {
      title: '站点编码',
      dataIndex: 'stationCode',
      key: 'stationCode',
      width: 180,
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      )
    },
    {
      title: '站点名称',
      dataIndex: 'stationName',
      key: 'stationName',
      width: 180,
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      ellipsis: true,
      search: false,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      width: 180,
      valueType: 'dateTime',
      search: false,
    },
    // {
    //   title: '创建人',
    //   dataIndex: 'createBy',
    //   key: 'createBy',
    //   width: 120,
    //   search: false,
    // },
    {
      title: '更新时间',
      dataIndex: 'updateTime',
      key: 'updateTime',
      width: 180,
      valueType: 'dateTime',
      search: false,
    },
    // {
    //   title: '更新人',
    //   dataIndex: 'updateBy',
    //   key: 'updateBy',
    //   width: 120,
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
          <Button
            type="link"
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={(e) => {
              e.stopPropagation();
              handleDeleteStation(record.stationId!);
            }}
          >
            删除
          </Button>
        </div>
      ),
    },
  ];

  return (
    <PageContainer>
      <ProTable<StationListDto>
        headerTitle="站点管理列表"
        actionRef={actionRef}
        rowKey="stationId"
        className="station-list-table"
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
        ): Promise<RequestData<StationListDto>> => {
          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            stationName: params.stationName,
            stationCode: params.stationCode,
          });

          const queryParams: StationListQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            sortField: 'CreateTime',
            sortOrder: 'descend',
            stationName: params.stationName,
            stationCode: params.stationCode,
          };

          try {
            const response = await getStationListList(queryParams);

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
            新建站点
          </Button>,
          <Button
            key="batchDelete"
            danger
            disabled={selectedRowKeys.length === 0}
            icon={<DeleteOutlined />}
            onClick={() => {
              // 调用删除处理函数，传递选中的ID数组
              const ids = selectedRowKeys.map(key => String(key));
              handleDeleteStation(ids);
            }}
          >
            批量删除
          </Button>
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
        title="站点详情"
        className="station-info-drawer"
        rootClassName="station-info-drawer"
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
          <ProDescriptions<StationListDto>
            column={2}
            title={`站点：${currentRow.stationCode}`}
            dataSource={currentRow}
            columns={[
              {
                title: '站点ID',
                dataIndex: 'stationId',
              },
              {
                title: '站点编码',
                dataIndex: 'stationCode',
              },
              {
                title: '站点名称',
                dataIndex: 'stationName',
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
        title="新建站点"
        open={showCreateModal}
        onCancel={() => {
          setShowCreateModal(false);
          form.resetFields();
        }}
        onOk={() => {
          form.validateFields().then((values) => {
            handleCreateStation(values);
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
            label="站点编码"
            name="stationCode"
            rules={[
              { required: true, message: '请输入站点编码' },
            ]}
          >
            <Input placeholder="请输入站点编码" />
          </Form.Item>

          <Form.Item
            label="站点名称"
            name="stationName"
            rules={[
              { required: true, message: '请输入站点名称' },
            ]}
          >
            <Input placeholder="请输入站点名称" />
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
        title="编辑站点"
        open={showEditModal}
        onCancel={() => {
          setShowEditModal(false);
          editForm.resetFields();
          setEditingRow(undefined);
        }}
        onOk={() => {
          editForm.validateFields().then((values) => {
            handleEditStation(values);
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
            name="stationId"
            hidden
          >
            <Input />
          </Form.Item>

          <Form.Item
            label="站点编码"
            name="stationCode"
            rules={[
              { required: true, message: '请输入站点编码' },
            ]}
          >
            <Input placeholder="请输入站点编码" />
          </Form.Item>

          <Form.Item
            label="站点名称"
            name="stationName"
            rules={[
              { required: true, message: '请输入站点名称' },
            ]}
          >
            <Input placeholder="请输入站点名称" />
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

export default StationList;
