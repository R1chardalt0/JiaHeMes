import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { Button, Tabs, message, Card, Row, Col, Modal, Form, Input, Select, Switch } from 'antd';
import { getProcessRouteList, getProcessRouteById, createProcessRoute, updateProcessRoute, deleteProcessRoute } from '@/services/Api/Infrastructure/ProcessRoute/ProcessRoute';
import { getProcessRouteItemsByHeadId, createProcessRouteItem, updateProcessRouteItem, deleteProcessRouteItem, getProcessRouteItemById } from '@/services/Api/Infrastructure/ProcessRoute/ProcessRouteItem';
import { ProcessRouteDto, ProcessRouteQueryDto, ProcessRouteCreateDto, ProcessRouteUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRoute';
import { ProcessRouteItem, ProcessRouteItemCreateDto, ProcessRouteItemUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItem';

import type { RequestData } from '@ant-design/pro-components';

const { TabPane } = Tabs;
const { Option } = Select;

const ProcessRoutePage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<ProcessRouteDto | null>(null);
  const [processRouteItems, setProcessRouteItems] = useState<ProcessRouteItem[]>([]);
  const [activeTabKey, setActiveTabKey] = useState('items');
  const [messageApi] = message.useMessage();
  const [currentSearchParams, setCurrentSearchParams] = useState<ProcessRouteQueryDto>({
    current: 1,
    pageSize: 50
  });
  const [isAddModalVisible, setIsAddModalVisible] = useState(false);
  const [isEditModalVisible, setIsEditModalVisible] = useState(false);
  const [isProcessRouteEditModalVisible, setIsProcessRouteEditModalVisible] = useState(false);
  const [editingProcessRouteItem, setEditingProcessRouteItem] = useState<ProcessRouteItem | null>(null);
  const [editingProcessRoute, setEditingProcessRoute] = useState<ProcessRouteDto | null>(null);
  const [form] = Form.useForm<ProcessRouteItemCreateDto>();
  const [processRouteForm] = Form.useForm();
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([]);
  const [isAddProcessRouteModalVisible, setIsAddProcessRouteModalVisible] = useState(false);

  // 工艺路线列表表格列定义
  const columns: ProColumns<ProcessRouteDto>[] = [
    {
      title: '工艺路线ID',
      dataIndex: 'id',
      key: 'id',
      width: 180,
      search: true
    },
    {
      title: '工艺路线名称',
      dataIndex: 'routeName',
      key: 'routeName',
      width: 150,
      search: true
    },
    {
      title: '工艺路线编码',
      dataIndex: 'routeCode',
      key: 'routeCode',
      width: 150,
      search: true
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      search: true,
      valueEnum: {
        0: { text: '启用', status: 'Success' },
        1: { text: '禁用', status: 'Default' }
      }
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      ellipsis: true,
      search: false
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      width: 180,
      valueType: 'dateTime',
      search: false
    },
    {
      title: '操作',
      key: 'operation',
      width: 180,
      fixed: 'right',
      search: false,
      render: (_, record) => (
        <>
          <Button
            type="primary"
            size="small"
            onClick={() => handleShowDetail(record)}
            style={{ marginRight: 8 }}
          >
            查看
          </Button>
          <Button
            type="default"
            size="small"
            onClick={() => handleEditProcessRoute(record)}
            style={{ marginRight: 8 }}
          >
            编辑
          </Button>
          <Button
            danger
            size="small"
            onClick={() => handleDeleteProcessRoute(record.id)}
          >
            删除
          </Button>
        </>
      )
    }
  ];

  // 显示工艺路线详情
  const handleShowDetail = useCallback(async (record: ProcessRouteDto) => {
    console.log('工艺路线 - 查看详情记录:', record);

    try {
      setCurrentRow(record);
      setShowDetail(true);

      // 获取工艺路线子项
      console.log('工艺路线 - 获取子项信息, headId:', record.id);
      const itemsResponse = await getProcessRouteItemsByHeadId(record.id);
      console.log('工艺路线 - 子项信息返回:', itemsResponse);
      setProcessRouteItems(itemsResponse);
    } catch (error) {
      messageApi.error('获取详情失败');
      console.error('获取详情失败:', error);
    }
  }, [messageApi]);

  // 双击行显示详情
  const handleRowDoubleClick = useCallback((record: ProcessRouteDto) => {
    handleShowDetail(record);
  }, [handleShowDetail]);

  // 删除工艺路线
  const handleDeleteProcessRoute = useCallback((processRouteId: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '确认要删除这条工艺路线信息吗？',
      onOk: async () => {
        try {
          await deleteProcessRoute(processRouteId);
          message.success('工艺路线删除成功');
          // 刷新表格
          if (actionRef.current) {
            actionRef.current.reload();
          }
        } catch (error) {
          message.error('工艺路线删除失败');
          console.error('Delete ProcessRoute error:', error);
        }
      }
    });
  }, []);

  // 打开编辑工艺路线模态框
  const handleEditProcessRoute = useCallback((processRoute: ProcessRouteDto) => {
    setEditingProcessRoute(processRoute);
    processRouteForm.setFieldsValue({
      routeName: processRoute.routeName,
      routeCode: processRoute.routeCode,
      status: processRoute.status,
      remark: processRoute.remark
    });
    setIsProcessRouteEditModalVisible(true);
  }, [processRouteForm]);

  // 保存工艺路线（创建或更新）
  const handleSaveProcessRoute = useCallback(async () => {
    try {
      const values = await processRouteForm.validateFields();

      if (editingProcessRoute) {
        // 更新工艺路线
        const updatedProcessRoute: ProcessRouteUpdateDto = {
          id: editingProcessRoute.id,
          routeName: values.routeName,
          routeCode: values.routeCode,
          status: values.status,
          remark: values.remark
        };

        await updateProcessRoute(editingProcessRoute.id, updatedProcessRoute);
        message.success('工艺路线更新成功');
        setIsProcessRouteEditModalVisible(false);
        // 刷新表格
        if (actionRef.current) {
          actionRef.current.reload();
        }
      } else {
        // 创建新工艺路线
        const newProcessRoute: ProcessRouteCreateDto = {
          routeName: values.routeName,
          routeCode: values.routeCode,
          status: values.status,
          remark: values.remark
        };

        await createProcessRoute(newProcessRoute);
        message.success('工艺路线创建成功');
        setIsAddProcessRouteModalVisible(false);
        // 刷新表格
        if (actionRef.current) {
          actionRef.current.reload();
        }
      }
    } catch (error) {
      message.error('保存失败，请检查输入');
      console.error('Save ProcessRoute error:', error);
    }
  }, [processRouteForm, editingProcessRoute, actionRef]);

  // 打开新建工艺路线模态框
  const handleAddProcessRoute = useCallback(() => {
    processRouteForm.resetFields();
    setEditingProcessRoute(null);
    setIsAddProcessRouteModalVisible(true);
  }, [processRouteForm]);

  // 批量删除工艺路线
  const handleBatchDelete = useCallback(async () => {
    Modal.confirm({
      title: '确认批量删除',
      content: `确认要删除选中的${selectedRowKeys.length}条工艺路线信息吗？`,
      onOk: async () => {
        try {
          await deleteProcessRoute(selectedRowKeys);
          message.success('批量删除成功');
          actionRef.current?.reload();
          setSelectedRowKeys([]);
        } catch (error) {
          message.error('批量删除失败');
          console.error('Batch delete error:', error);
        }
      },
    });
  }, [selectedRowKeys, actionRef]);

  // 删除工艺路线子项
  const handleDeleteProcessRouteItem = useCallback((processRouteItemId: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '确认要删除这条工艺路线子项信息吗？',
      onOk: async () => {
        try {
          await deleteProcessRouteItem(processRouteItemId);
          message.success('工艺路线子项删除成功');
          // 重新获取子项列表
          if (currentRow) {
            const itemsResponse = await getProcessRouteItemsByHeadId(currentRow.id);
            setProcessRouteItems(itemsResponse);
          }
        } catch (error) {
          message.error('工艺路线子项删除失败');
          console.error('Delete ProcessRoute item error:', error);
        }
      }
    });
  }, [currentRow]);

  // 打开添加工艺路线子项模态框
  const handleAddProcessRouteItem = useCallback(() => {
    form.resetFields();
    setEditingProcessRouteItem(null);
    setIsAddModalVisible(true);
  }, [form]);

  // 打开编辑工艺路线子项模态框
  const handleEditProcessRouteItem = useCallback(async (processRouteItem: ProcessRouteItem) => {
    setEditingProcessRouteItem(processRouteItem);
    form.setFieldsValue({
      stationCode: processRouteItem.stationCode,
      mustPassStation: processRouteItem.mustPassStation,
      checkStationList: processRouteItem.checkStationList,
      firstStation: processRouteItem.firstStation,
      checkAll: processRouteItem.checkAll
    });
    setIsEditModalVisible(true);
  }, [form]);

  // 保存工艺路线子项
  const handleSaveProcessRouteItem = useCallback(async () => {
    try {
      const values = await form.validateFields();

      if (editingProcessRouteItem) {
        // 更新工艺路线子项
        const updatedItem: ProcessRouteItemUpdateDto = {
          id: editingProcessRouteItem.id,
          headId: currentRow?.id,
          stationCode: values.stationCode,
          mustPassStation: values.mustPassStation,
          checkStationList: values.checkStationList,
          firstStation: values.firstStation,
          checkAll: values.checkAll
        };

        await updateProcessRouteItem(editingProcessRouteItem.id, updatedItem);
        message.success('工艺路线子项更新成功');
        setIsEditModalVisible(false);
      } else {
        // 创建工艺路线子项
        const newItem: ProcessRouteItemCreateDto = {
          headId: currentRow?.id,
          stationCode: values.stationCode,
          mustPassStation: values.mustPassStation,
          checkStationList: values.checkStationList,
          firstStation: values.firstStation,
          checkAll: values.checkAll
        };

        await createProcessRouteItem(newItem);
        message.success('工艺路线子项添加成功');
        setIsAddModalVisible(false);
      }

      // 重新获取子项列表
      if (currentRow) {
        const itemsResponse = await getProcessRouteItemsByHeadId(currentRow.id);
        setProcessRouteItems(itemsResponse);
      }
    } catch (error) {
      message.error('保存失败，请检查输入');
      console.error('Save ProcessRoute item error:', error);
    }
  }, [form, editingProcessRouteItem, currentRow]);

  return (
    <PageContainer title="工艺路线管理">
      <ProTable<ProcessRouteDto>
        actionRef={actionRef}
        rowKey="id"
        className="process-route-table"
        columns={columns}
        search={{
          labelWidth: 120,
          layout: 'vertical',
        }}
        request={async (
          params
        ): Promise<RequestData<ProcessRouteDto>> => {
          console.log('工艺路线 - 请求参数:', params);

          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            routeName: params.routeName,
            routeCode: params.routeCode,
            status: params.status,
          });

          const queryParams: ProcessRouteQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            routeName: params.routeName,
            routeCode: params.routeCode,
            status: params.status,
          };

          console.log('工艺路线 - API查询参数:', queryParams);

          try {
            const response = await getProcessRouteList(queryParams);
            console.log('工艺路线 - API返回数据:', response);

            return {
              data: response.data || [],
              total: response.total || 0,
              success: response.success !== false,
            };
          } catch (error) {
            console.error('工艺路线 - API调用失败:', error);
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
          <Button type="primary" key="add" onClick={handleAddProcessRoute}>
            新增工艺路线
          </Button>,
          <Button danger key="batchDelete" onClick={handleBatchDelete} disabled={selectedRowKeys.length === 0}>
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
        <div className="process-route-detail" style={{ marginTop: 20 }}>
          <ProDescriptions<ProcessRouteDto>
            title="工艺路线基本信息"
            dataSource={currentRow}
            column={3}
          >
            <ProDescriptions.Item label="工艺路线ID">{currentRow.id}</ProDescriptions.Item>
            <ProDescriptions.Item label="工艺路线名称">{currentRow.routeName}</ProDescriptions.Item>
            <ProDescriptions.Item label="工艺路线编码">{currentRow.routeCode}</ProDescriptions.Item>
            <ProDescriptions.Item label="状态">
              {currentRow.status === 0 ? '启用' : '禁用'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="备注">{currentRow.remark}</ProDescriptions.Item>
            <ProDescriptions.Item label="创建时间">{currentRow.createTime}</ProDescriptions.Item>
          </ProDescriptions>

          <Tabs activeKey={activeTabKey} onChange={setActiveTabKey} style={{ marginTop: 20 }}>
            <TabPane
              tab={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  工艺路线子项
                  <Button
                    type="primary"
                    style={{ marginLeft: 16 }}
                    onClick={handleAddProcessRouteItem}
                  >
                    添加子项
                  </Button>
                </div>
              }
              key="items"
            >
              <Row gutter={[16, 16]}>
                {processRouteItems.length > 0 ? (
                  processRouteItems.map((item) => (
                    <Col xs={24} sm={12} md={8} key={item.id}>
                      <Card
                        title={`工艺路线子项`}
                        extra={
                          <div>
                            <Button
                              type="primary"
                              size="small"
                              onClick={() => handleEditProcessRouteItem(item)}
                              style={{ marginRight: 8 }}
                            >
                              编辑
                            </Button>
                            <Button
                              danger
                              size="small"
                              onClick={() => handleDeleteProcessRouteItem(item.id)}
                            >
                              删除
                            </Button>
                          </div>
                        }
                      >
                        <div style={{ lineHeight: '1.8', fontSize: '14px' }}>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>工艺路线子项ID:</strong> {item.id}</p>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>主表ID:</strong> {item.headId}</p>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>站点编码:</strong> {item.stationCode}</p>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>是否必经站点:</strong> {item.mustPassStation ? '是' : '否'}</p>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>检查站点列表:</strong> {item.checkStationList}</p>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>是否首站点:</strong> {item.firstStation ? '是' : '否'}</p>
                          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>是否检查所有:</strong> {item.checkAll ? '是' : '否'}</p>
                        </div>
                      </Card>
                    </Col>
                  ))
                ) : (
                  <Col span={24}>
                    <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>
                      暂无工艺路线子项数据
                    </div>
                  </Col>
                )}
              </Row>
            </TabPane>
          </Tabs>
        </div>
      )}

      {/* 添加/编辑工艺路线子项模态框 */}
      <Modal
        title={editingProcessRouteItem ? "编辑工艺路线子项" : "添加工艺路线子项"}
        open={isAddModalVisible || isEditModalVisible}
        onOk={handleSaveProcessRouteItem}
        onCancel={() => {
          setIsAddModalVisible(false);
          setIsEditModalVisible(false);
        }}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
        >
          <Form.Item
            name="stationCode"
            label="站点编码"
            rules={[{ required: true, message: '请输入站点编码' }]}
          >
            <Input placeholder="请输入站点编码" />
          </Form.Item>
          <Form.Item
            name="mustPassStation"
            label="是否必经站点"
            initialValue={false}
          >
            <Switch />
          </Form.Item>
          <Form.Item
            name="checkStationList"
            label="检查站点列表"
            rules={[{ required: true, message: '请输入检查站点列表' }]}
          >
            <Input placeholder="请输入检查站点列表" />
          </Form.Item>
          <Form.Item
            name="firstStation"
            label="是否首站点"
            initialValue={false}
          >
            <Switch />
          </Form.Item>
          <Form.Item
            name="checkAll"
            label="是否检查所有"
            initialValue={false}
          >
            <Switch />
          </Form.Item>
        </Form>
      </Modal>

      {/* 编辑/新建工艺路线模态框 */}
      <Modal
        title={editingProcessRoute ? "编辑工艺路线" : "新建工艺路线"}
        open={isProcessRouteEditModalVisible || isAddProcessRouteModalVisible}
        onOk={handleSaveProcessRoute}
        onCancel={() => {
          setIsProcessRouteEditModalVisible(false);
          setIsAddProcessRouteModalVisible(false);
        }}
        width={600}
      >
        <Form
          form={processRouteForm}
          layout="vertical"
        >
          <Form.Item
            name="routeName"
            label="工艺路线名称"
            rules={[{ required: true, message: '请输入工艺路线名称' }]}
          >
            <Input placeholder="请输入工艺路线名称" />
          </Form.Item>
          <Form.Item
            name="routeCode"
            label="工艺路线编码"
            rules={[{ required: true, message: '请输入工艺路线编码' }]}
          >
            <Input placeholder="请输入工艺路线编码" />
          </Form.Item>
          <Form.Item
            name="status"
            label="状态"
            rules={[{ required: true, message: '请选择状态' }]}
          >
            <Select placeholder="请选择状态">
              <Option value={0}>启用</Option>
              <Option value={1}>禁用</Option>
            </Select>
          </Form.Item>
          <Form.Item
            name="remark"
            label="备注"
          >
            <Input.TextArea placeholder="请输入备注" rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </PageContainer>
  );
};

  export default ProcessRoutePage;