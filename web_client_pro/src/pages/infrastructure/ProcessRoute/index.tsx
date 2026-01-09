import type { ActionType } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback, useEffect } from 'react';
import { Button, Tabs, message, Modal, Form, Select, Switch } from 'antd';
import { EyeOutlined, EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons';
import { getProcessRouteList, getProcessRouteById, createProcessRoute, updateProcessRoute, deleteProcessRoute } from '@/services/Api/Infrastructure/ProcessRoute/ProcessRoute';
import { getProcessRouteItemsByHeadId, createProcessRouteItem, updateProcessRouteItem, deleteProcessRouteItem, getProcessRouteItemById } from '@/services/Api/Infrastructure/ProcessRoute/ProcessRouteItem';
import { getStationListList } from '@/services/Api/Infrastructure/StationList';
import { ProcessRouteDto, ProcessRouteQueryDto, ProcessRouteCreateDto, ProcessRouteUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRoute';
import { ProcessRouteItem, ProcessRouteItemCreateDto, ProcessRouteItemUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItem';
import { StationListDto } from '@/services/Model/Infrastructure/StationList';

import type { RequestData } from '@ant-design/pro-components';

// 导入拆分后的组件
import ProcessRouteItemList from './components/ProcessRouteItemList';
import ProcessRouteItemForm from './components/ProcessRouteItemForm';
import ProcessRouteForm from './components/ProcessRouteForm';
import { getProcessRouteColumns } from './columns';

const { TabPane } = Tabs;

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
  // 站点列表状态
  const [stations, setStations] = useState<StationListDto[]>([]);
  const [stationLoading, setStationLoading] = useState(false);

  // 获取工位列表
  useEffect(() => {
    const fetchStations = async () => {
      if (isAddModalVisible || isEditModalVisible) {
        try {
          setStationLoading(true);
          const res = await getStationListList({ current: 1, pageSize: 1000 });
          if (res.data) {
            setStations(res.data);
          }
        } catch (error) {
          message.error('获取工位列表失败');
          console.error('获取工位列表失败:', error);
        } finally {
          setStationLoading(false);
        }
      }
    };
    fetchStations();
  }, [isAddModalVisible, isEditModalVisible]);

  // 显示工艺路线详情
  const handleShowDetail = useCallback(async (record: ProcessRouteDto) => {

    try {
      setCurrentRow(record);
      setShowDetail(true);

      // 获取工艺路线子项
      const itemsResponse = await getProcessRouteItemsByHeadId(record.id);
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

  // 获取工艺路线列表表格列定义
  const columns = getProcessRouteColumns(
    handleShowDetail,
    handleEditProcessRoute,
    handleDeleteProcessRoute
  );

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

      // 确保 checkStationList 是逗号分隔的字符串
      const checkStationListValue = Array.isArray(values.checkStationList)
        ? values.checkStationList.join(',')
        : values.checkStationList;

      if (editingProcessRouteItem) {
        // 更新工艺路线子项
        const updatedItem: ProcessRouteItemUpdateDto = {
          id: editingProcessRouteItem.id,
          headId: currentRow?.id,
          stationCode: values.stationCode,
          mustPassStation: values.mustPassStation,
          checkStationList: checkStationListValue,
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
          checkStationList: checkStationListValue,
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

          try {
            const response = await getProcessRouteList(queryParams);

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
          <Button
            type="primary"
            key="add"
            icon={<PlusOutlined />}
            onClick={handleAddProcessRoute}>
            新建工艺路线
          </Button>,
          <Button
            danger
            key="batchDelete"
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
                    icon={<PlusOutlined />}
                    onClick={handleAddProcessRouteItem}
                  >
                    添加子项
                  </Button>
                </div>
              }
              key="items"
            >
              <ProcessRouteItemList
                processRouteItems={processRouteItems}
                onEdit={handleEditProcessRouteItem}
                onDelete={handleDeleteProcessRouteItem}
              />
            </TabPane>
          </Tabs>
        </div>
      )
      }

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
        <ProcessRouteItemForm
          form={form}
          stations={stations}
          stationLoading={stationLoading}
        />
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
        <ProcessRouteForm
          form={processRouteForm}
        />
      </Modal>
    </PageContainer >
  );
};

export default ProcessRoutePage;