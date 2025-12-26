import { ActionType, ProColumns } from '@ant-design/pro-components';
import { FooterToolbar, PageContainer, ProTable,ProDescriptions } from '@ant-design/pro-components';
import { useRequest } from '@umijs/max';
import { Button, Drawer, message, Modal } from 'antd';
import React, { useRef, useState } from 'react';
import { PlusOutlined } from '@ant-design/icons';
import { getRoleList, deleteRole, createRole, updateRole } from '@/services/Api/Systems/role';
import type { RoleItem } from '@/services/Model/Systems/role';
import CreateRoleForm from './CreateRoleForm';

const RoleList: React.FC = () => {
  const actionRef = useRef<ActionType>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<RoleItem>();
  const [selectedRows, setSelectedRows] = useState<RoleItem[]>([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [checkedKeys, setCheckedKeys] = useState<{
    checked: React.Key[];
    halfChecked: React.Key[];
  }>({ checked: [], halfChecked: [] });
  const [currentSearchParams, setCurrentSearchParams] = useState<{
    current: number;
    pageSize: number;
  }>({ current: 1, pageSize: 10 });

  // 删除请求
  const { run: delRun, loading: deleteLoading } = useRequest(deleteRole, {
    manual: true,
    onSuccess: () => {
      setSelectedRows([]);
      actionRef.current?.reload();
      message.success('删除成功');
    },
    onError: (error) => {
      message.error(error.message || '删除失败');
    },
  });

  // 创建/更新统一请求
  const { run: submitRun } = useRequest(
    async (payload) => {
      return currentRow ? updateRole(payload) : createRole(payload);
    },
    {
      manual: true,
      onSuccess: () => {
        actionRef.current?.reload();
        message.success(currentRow ? '更新成功' : '新增成功');
        setModalVisible(false);
      },
      onError: (error) => {
        const errorMsg = error.message || (error as any).response?.data?.message || '操作失败';
        message.error(errorMsg);
      },
    }
  );

  const columns: ProColumns<RoleItem>[] = [
    {
      title: '角色名称',
      dataIndex: 'roleName',
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      ),
    },
    { title: '角色标识', dataIndex: 'roleKey' },
    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        '0': { text: '禁用', status: 'Error' },
        '1': { text: '启用', status: 'Success' },
      },
      hideInSearch: false,
    },
    { title: '创建时间', dataIndex: 'createTime', valueType: 'dateTime' },
    {
      title: '操作',
      valueType: 'option',
      render: (_, record) => [
        <a
          key="edit"
          onClick={() => {
            setCurrentRow(record);
            // 恢复菜单选中状态（如果已有 menuIds）
            if (record.menuIds) {
              setCheckedKeys({
                checked: record.menuIds.map(id => String(id)),
                halfChecked: [],
              });
            }
            setModalVisible(true);
          }}
        >
          编辑
        </a>,
        <a
          key="delete"
          onClick={() => {
            Modal.confirm({
              title: '确认删除',
              content: '确定删除该角色？',
              className: 'delete-confirm-modal',
              rootClassName: 'delete-confirm-modal',
              styles: {
                content: {
                  background: '#ffffff',
                  border: '1px solid #f0f0f0',
                  boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
                },
                header: {
                  background: '#ffffff',
                  borderBottom: '1px solid #f0f0f0'
                },
                body: {
                  background: '#ffffff'
                },
                mask: {
                  background: 'rgba(0,0,0,0.1)'
                }
              },
              onOk: () => delRun([record.roleId]),
            });
          }}
        >
          删除
        </a>,
      ],
    },
  ];

  const handleRemove = async () => {
    if (!selectedRows.length) {
      message.warning('请选择要删除的项');
      return;
    }
    await delRun(selectedRows.map(row => row.roleId));
  };

  // 表单提交处理（关键：类型转换）
  const handleSubmit = async (values: any) => {
    const payload = {
      ...values,
      roleSort: String(values.roleSort || 0),
      status: String(values.status),
      menuIds: checkedKeys.checked.map(key => Number(key)), // string[] → number[]
      ...(currentRow ? { roleId: currentRow.roleId } : {}),
    };

    console.log('提交角色数据:', payload); // 调试用

    await submitRun(payload);
    return true;
  };

  return (
    <PageContainer className="system-settings-page">
      <ProTable<RoleItem>
        rowKey="roleId"
        actionRef={actionRef}
        columns={columns}
        request={async (params) => {
          // 确保页码不小于1，页面大小在1-100之间
          const current = Math.max(1, params.current || 1);
          const pageSize = Math.min(100, Math.max(1, params.pageSize || 10));
          
          const res = await getRoleList({ ...params, current, pageSize });
          return {
            data: res.data || [],
            success: res.success ?? true,
            total: res.total || res.data?.length || 0,
          };
        }}
        rowSelection={{ onChange: (_, rows) => setSelectedRows(rows) }}
        pagination={{
          current: currentSearchParams.current,
          pageSize: currentSearchParams.pageSize,
          pageSizeOptions: ['10', '20', '50', '100'],
          showSizeChanger: true,
          showTotal: (total) => `共 ${total} 条记录`,
          onChange: (current, pageSize) => {
            setCurrentSearchParams({ current, pageSize });
            actionRef.current?.reload();
          },
          onShowSizeChange: (_, pageSize) => {
            setCurrentSearchParams({ current: 1, pageSize });
            actionRef.current?.reload();
          },
        }}
        toolBarRender={() => [
          <Button
            key="add"
            type="primary"
            onClick={() => {
              setCurrentRow(undefined);
              setCheckedKeys({ checked: [], halfChecked: [] });
              setModalVisible(true);
            }}
          >
            <PlusOutlined /> 新增角色
          </Button>,
        ]}
      />

      {selectedRows.length > 0 && (
        <FooterToolbar extra={`已选择 ${selectedRows.length} 项`}>
          <Button loading={deleteLoading} onClick={handleRemove}>
            批量删除
          </Button>
        </FooterToolbar>
      )}

      <Drawer
        width={600}
        open={showDetail}
        onClose={() => setShowDetail(false)}
        closable={false}
        className="role-info-drawer"
        rootClassName="role-info-drawer"
        styles={{
          content: {
            background: '#ffffff',
            borderLeft: '1px solid #f0f0f0',
            boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
          },
          header: {
            background: '#ffffff',
            borderBottom: '1px solid #f0f0f0'
          },
          body: {
            background: '#ffffff'
          },
          mask: {
            background: 'rgba(0,0,0,0.1)'
          }
        }}
      >
        {currentRow && (
          <ProDescriptions<RoleItem>
            column={2}
            title={currentRow.roleName}
            dataSource={currentRow}
            columns={columns as any}
          />
        )}
      </Drawer>

      <CreateRoleForm
        open={modalVisible}
        onOpenChange={setModalVisible}
        currentRow={currentRow}
        checkedKeys={checkedKeys}
        onCheckedChange={setCheckedKeys}
        onFinish={handleSubmit}
      />
    </PageContainer>
  );
};

export default RoleList;