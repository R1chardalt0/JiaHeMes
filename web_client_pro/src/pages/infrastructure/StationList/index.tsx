import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { Drawer, Button, message, Modal, Form, Input, Popconfirm, Switch } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { getStationListList, createStationList, updateStationList, deleteStationList } from '@/services/Api/Infrastructure/StationList';
import { getStationTestProjectByStationId, updateStationTestProject, createStationTestProject, deleteStationTestProject } from '@/services/Api/Infrastructure/StationTest';
import { StationListDto, StationListQueryDto } from '@/services/Model/Infrastructure/StationList';
import { StationTestProjectDto } from '@/services/Model/Infrastructure/StationTest';
import type { RequestData } from '@ant-design/pro-components';

const StationList: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingRow, setEditingRow] = useState<StationListDto>();
  const [messageApi] = message.useMessage();
  const [form] = Form.useForm<StationListDto>();
  const [editForm] = Form.useForm<StationListDto>();
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [currentSearchParams, setCurrentSearchParams] = useState<StationListQueryDto>({
    pageIndex: 1,
    pageSize: 10
  });

  // 站点测试项相关状态
  const [testProjects, setTestProjects] = useState<StationTestProjectDto[]>([]);
  const [testProjectsLoading, setTestProjectsLoading] = useState(false);
  const [showTestProjects, setShowTestProjects] = useState(false);
  const [selectedStation, setSelectedStation] = useState<StationListDto | null>(null);
  const testProjectActionRef = useRef<ActionType | null>(null);

  // 测试项编辑和新增相关状态
  const [showTestProjectModal, setShowTestProjectModal] = useState(false);
  const [editingTestProject, setEditingTestProject] = useState<StationTestProjectDto>();
  const [testProjectForm] = Form.useForm<StationTestProjectDto>();

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

  /**
   * 双击行处理函数
   * @param record 当前站点记录
   */
  const handleRowDoubleClick = useCallback(async (record: StationListDto) => {
    try {
      setSelectedStation(record);
      setTestProjectsLoading(true);

      // 根据站点ID获取测试项列表
      const response = await getStationTestProjectByStationId(record.stationId!);
      setTestProjects(response);
      setShowTestProjects(true);
    } catch (error) {
      messageApi.error('获取站点测试项失败');
      console.error('获取站点测试项失败:', error);
    } finally {
      setTestProjectsLoading(false);
    }
  }, [messageApi]);

  /**
   * 打开新增测试项弹窗
   */
  const handleAddTestProject = () => {
    if (!selectedStation) return;

    setEditingTestProject(undefined);
    testProjectForm.resetFields();
    setShowTestProjectModal(true);
  };

  /**
   * 打开编辑测试项弹窗
   * @param record 测试项记录
   */
  const handleEditTestProject = (record: StationTestProjectDto) => {
    setEditingTestProject(record);
    testProjectForm.setFieldsValue({
      stationTestProjectId: record.stationTestProjectId,
      stationId: record.stationId,
      parametricKey: record.parametricKey,
      upperLimit: record.upperLimit,
      lowerLimit: record.lowerLimit,
      units: record.units,
      searchValue: record.searchValue,
      isCheck: record.isCheck,
      remark: record.remark,
    });
    setShowTestProjectModal(true);
  };

  /**
   * 保存测试项（新增或编辑）
   */
  const handleSaveTestProject = async () => {
    try {
      const values = await testProjectForm.validateFields();

      if (editingTestProject) {
        // 编辑模式
        await updateStationTestProject(editingTestProject.stationTestProjectId!, {
          stationTestProjectId: editingTestProject.stationTestProjectId!,
          stationId: editingTestProject.stationId || selectedStation?.stationId!,
          parametricKey: values.parametricKey!,
          upperLimit: values.upperLimit,
          lowerLimit: values.lowerLimit,
          units: values.units,
          searchValue: values.searchValue,
          isCheck: values.isCheck,
          remark: values.remark,
        });
        messageApi.success('更新测试项成功');
      } else {
        // 新增模式
        await createStationTestProject({
          stationId: selectedStation?.stationId!,
          parametricKey: values.parametricKey!,
          upperLimit: values.upperLimit,
          lowerLimit: values.lowerLimit,
          units: values.units,
          searchValue: values.searchValue,
          isCheck: values.isCheck,
          remark: values.remark,
        });
        messageApi.success('新增测试项成功');
      }

      // 重新获取测试项列表
      if (selectedStation) {
        const response = await getStationTestProjectByStationId(selectedStation.stationId!);
        setTestProjects(response);
        // 强制刷新测试项列表
        testProjectActionRef.current?.reload();
      }

      setShowTestProjectModal(false);
      testProjectForm.resetFields();
    } catch (error) {
      messageApi.error('保存测试项失败');
      console.error('保存测试项失败:', error);
    }
  };

  /**
   * 删除测试项
   * @param id 测试项ID
   */
  const handleDeleteTestProject = async (id: string) => {
    try {
      await deleteStationTestProject(id);
      messageApi.success('删除测试项成功');

      // 重新获取测试项列表
      if (selectedStation) {
        const response = await getStationTestProjectByStationId(selectedStation.stationId!);
        setTestProjects(response);
        // 强制刷新测试项列表
        testProjectActionRef.current?.reload();
      }
    } catch (error) {
      messageApi.error('删除测试项失败');
      console.error('删除测试项失败:', error);
    }
  };

  /**
   * 站点测试项列定义
   */
  const testProjectColumns: ProColumns<StationTestProjectDto>[] = [
    {
      title: '测试项',
      dataIndex: 'parametricKey',
      key: 'parametricKey',
      width: 180,
      search: false,
    },
    {
      title: '上限',
      dataIndex: 'upperLimit',
      key: 'upperLimit',
      width: 100,
      search: false,
    },
    {
      title: '下限',
      dataIndex: 'lowerLimit',
      key: 'lowerLimit',
      width: 100,
      search: false,
    },
    {
      title: '单位',
      dataIndex: 'units',
      key: 'units',
      width: 80,
      search: false,
    },
    {
      title: '搜索值',
      dataIndex: 'searchValue',
      key: 'searchValue',
      width: 150,
      search: false,
    },
    {
      title: '是否检查',
      dataIndex: 'isCheck',
      key: 'isCheck',
      width: 100,
      valueEnum: {
        true: { text: '是', status: 'Success' },
        false: { text: '否', status: 'Default' },
      },
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
    {
      title: '操作',
      key: 'action',
      width: 150,
      valueType: 'option',
      render: (_, record) => (
        <div>
          <Button
            key="edit"
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleEditTestProject(record)}
          >
            编辑
          </Button>
          <Popconfirm
            key="delete"
            title="确定删除此测试项吗？"
            onConfirm={() => handleDeleteTestProject(record.stationTestProjectId!)}
            okText="确定"
            cancelText="取消"
          >
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
            >
              删除
            </Button>
          </Popconfirm>
        </div>
      ),
    },
  ];

  const columns: ProColumns<StationListDto>[] = [
    {
      title: '站点编码',
      dataIndex: 'stationCode',
      key: 'stationCode',
      width: 180,
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
          console.log('ProTable params:', params);

          // 使用后端分页：将分页参数和搜索条件传递给后端
          const queryParams: StationListQueryDto = {
            pageIndex: params.current || 1,
            pageSize: params.pageSize || 10,
            sortField: 'CreateTime',
            sortOrder: 'descend',
            stationName: params.stationName,
            stationCode: params.stationCode,
          };

          // 保存当前搜索参数
          setCurrentSearchParams(queryParams);

          console.log('API queryParams:', queryParams);

          try {
            // 调用 API 获取当前页数据
            const response = await getStationListList(queryParams);

            console.log('API response:', response);

            let data: StationListDto[] = [];
            let total: number = 0;
            let success: boolean = false;

            // 处理 API 返回的数据
            if (Array.isArray(response)) {
              // 兼容旧格式：如果返回的是数组，转换为分页格式
              data = response;
              total = response.length;
              success = true;
            } else if (response.data) {
              // 如果已经是分页格式，直接使用
              data = response.data;
              total = response.total || 0;
              success = response.success || true;
            }

            const result = {
              data: data,
              total: total,
              success: success,
            };

            console.log('ProTable result:', result);

            return result;
          } catch (error) {
            console.error('Failed to get station list:', error);
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        columns={columns}
        pagination={{
          defaultPageSize: 10,
          pageSizeOptions: ['10', '20', '50'],
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
          onDoubleClick: () => {
            handleRowDoubleClick(record);
          },
        })}
      />

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

      {/* 站点测试项列表 */}
      {showTestProjects && selectedStation && (
        <div style={{ marginTop: 20, padding: 16, border: '1px solid #e8e8e8', borderRadius: 8, background: '#fafafa' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
            <h3 style={{ margin: 0 }}>
              站点测试项列表 - {selectedStation.stationCode} ({selectedStation.stationName})
            </h3>
            <div>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                style={{ marginRight: 8 }}
                onClick={handleAddTestProject}
              >
                新建测试项
              </Button>
              <Button
                type="default"
                onClick={() => setShowTestProjects(false)}
              >
                关闭
              </Button>
            </div>
          </div>

          <ProTable<StationTestProjectDto>
            actionRef={testProjectActionRef}
            rowKey="stationTestProjectId"
            columns={testProjectColumns}
            request={async (params) => {
              // 使用前端筛选：基于已获取的测试项数据进行筛选
              let filteredData = testProjects;

              // 按是否检查筛选
              if (params.isCheck !== undefined && params.isCheck !== null) {
                filteredData = filteredData.filter(item => item.isCheck === params.isCheck);
              }

              // 按测试项搜索
              if (params.parametricKey) {
                filteredData = filteredData.filter(item =>
                  item.parametricKey?.toLowerCase().includes(params.parametricKey.toLowerCase())
                );
              }

              // 按单位搜索
              if (params.units) {
                filteredData = filteredData.filter(item =>
                  item.units?.toLowerCase().includes(params.units.toLowerCase())
                );
              }

              // 在前端进行分页
              const currentPage = Math.max(1, params.current || 1);
              const pageSize = Math.min(100, Math.max(1, params.pageSize || 10));
              const startIndex = (currentPage - 1) * pageSize;
              const endIndex = startIndex + pageSize;
              const pagedData = filteredData.slice(startIndex, endIndex);

              return {
                data: pagedData,
                total: filteredData.length,
                success: true,
              };
            }}
            loading={testProjectsLoading}
            pagination={{
              defaultPageSize: 10,
              pageSizeOptions: ['10', '20', '50'],
              showSizeChanger: true,
              showTotal: (total) => `共 ${total} 条数据`,
            }}
            search={{
              labelWidth: 'auto',
            }}
            toolBarRender={false}
          />
        </div>
      )}

      {/* 测试项编辑和新增弹窗 */}
      <Modal
        title={editingTestProject ? "编辑测试项" : "新增测试项"}
        open={showTestProjectModal}
        onCancel={() => {
          setShowTestProjectModal(false);
          testProjectForm.resetFields();
        }}
        onOk={handleSaveTestProject}
        width={600}
        destroyOnClose
      >
        <Form
          form={testProjectForm}
          layout="vertical"
          autoComplete="off"
        >
          <Form.Item
            label="测试项"
            name="parametricKey"
            rules={[{ required: true, message: '请输入测试项' }]}
          >
            <Input placeholder="请输入测试项" />
          </Form.Item>

          <Form.Item
            label="上限"
            name="upperLimit"
          >
            <Input type="number" placeholder="请输入上限" />
          </Form.Item>

          <Form.Item
            label="下限"
            name="lowerLimit"
          >
            <Input type="number" placeholder="请输入下限" />
          </Form.Item>

          <Form.Item
            label="单位"
            name="units"
          >
            <Input placeholder="请输入单位" />
          </Form.Item>

          <Form.Item
            label="搜索值"
            name="searchValue"
          >
            <Input placeholder="请输入搜索值" />
          </Form.Item>

          <Form.Item
            label="是否检查"
            name="isCheck"
            valuePropName="checked"
            initialValue={false}
          >
            <Switch />
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
