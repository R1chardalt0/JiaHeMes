import { ActionType, ProColumns } from '@ant-design/pro-components';
import { FooterToolbar, PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import { useRequest } from '@umijs/max';
import { Button, Drawer, message, Modal } from 'antd';
import React, { useRef, useState } from 'react';
import { PlusOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { getPositionList, deletePosition, createPosition, updatePosition } from '@/services/Api/Systems/position';
import type { PositionItem } from '@/services/Model/Systems/position';
import CreatePositionForm from './CreatePositionForm';

const PositionList: React.FC = () => {
  const actionRef = useRef<ActionType>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<PositionItem>();
  const [selectedRows, setSelectedRows] = useState<PositionItem[]>([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [currentSearchParams, setCurrentSearchParams] = useState<{
    current: number;
    pageSize: number;
  }>({ current: 1, pageSize: 10 });

  // 删除请求
  const { run: delRun, loading: deleteLoading } = useRequest(deletePosition, {
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
  const { run: submitRun, loading: submitLoading } = useRequest(
    async (payload) => {
      return currentRow ? updatePosition(payload) : createPosition(payload);
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

  const columns: ProColumns<PositionItem>[] = [
    { title: '岗位ID', dataIndex: 'postId', hideInTable: true, hideInSearch: true },
    {
      title: '岗位名称',
      dataIndex: 'postName',
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      ),
    },
    { title: '岗位编码', dataIndex: 'postCode' },
    { title: '显示顺序', dataIndex: 'postSort', hideInSearch: true },
    {
      title: '状态',
      dataIndex: 'status',
      hideInTable: true,
      valueEnum: {
        '0': { text: '启用', status: 'Success' },
        '1': { text: '停用', status: 'Error' },  
      },
      render: (value: any) => {
        const statusValue = String(value || '0').trim();
        if (statusValue === '0') {
          return <span><CheckCircleOutlined className="text-success mr-1" /> 启用</span>;
        } else if (statusValue === '1') {
          return <span><CloseCircleOutlined className="text-error mr-1" /> 停用</span>;
        }
        return <span><CheckCircleOutlined className="text-success mr-1" /> 启用</span>;
      }
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      valueType: 'dateTime',
      hideInSearch: true,
      hideInTable: true,
    },
    {
      title: '操作',
      valueType: 'option',
      render: (_, record) => [
        <a
          key="edit"
          onClick={() => {
            setCurrentRow(record);
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
              content: '确定删除该岗位？',
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
              onOk: () => delRun([record.postId]),
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
    await delRun(selectedRows.map(row => row.postId));
  };

  // 表单提交处理
  const handleSubmit = async (values: any) => {
    const statusValue = String(values.status ?? '0');
    
    const payload = {
      postName: values.postName,
      postCode: values.postCode,
      postSort: String(values.postSort || 0),
      status: statusValue,
      remark: values.remark,
      ...(currentRow ? { postId: currentRow.postId } : {}),
    };

    await submitRun(payload);
    return true;
  };

  return (
    <PageContainer className="system-settings-page">
      <ProTable<PositionItem>
        rowKey="postId"
        actionRef={actionRef}
        columns={columns}
        request={async (params) => {
          const current = Math.max(1, params.current || 1);
          const pageSize = Math.min(100, Math.max(1, params.pageSize || 10));
          
          const queryParams = {
            current,
            pageSize,
            postName: params.postName,
            postCode: params.postCode,
            status: params.status,
          };
          const res = await getPositionList(queryParams);
          
          const formattedData = (res.data || []).map((item: any) => ({
            postId: item.postId ?? item.PostId,
            postName: item.postName ?? item.PostName,
            postCode: item.postCode ?? item.PostCode,
            postSort: item.postSort ?? item.PostSort,
            status: String(item.status ?? item.Status ?? '0'),
            createTime: item.createTime ?? item.CreateTime,
            updateTime: item.updateTime ?? item.UpdateTime,
            remark: item.remark ?? item.Remark,
          }));
          
          return {
            data: formattedData,
            success: res.success ?? true,
            total: res.total || 0,
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
              setModalVisible(true);
            }}
          >
            <PlusOutlined /> 新增岗位
          </Button>,
        ]}
      />

      {selectedRows.length > 0 && (
        <FooterToolbar extra={`已选择 ${selectedRows.length} 项`}>
          <Button loading={deleteLoading} onClick={handleRemove} type="primary" danger>
            批量删除
          </Button>
        </FooterToolbar>
      )}

      {/* 详情抽屉 */}
      <Drawer
        width={600}
        open={showDetail}
        onClose={() => setShowDetail(false)}
        closable={false}
        className="position-info-drawer"
        rootClassName="position-info-drawer"
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
          <ProDescriptions<PositionItem>
            column={2}
            title={currentRow.postName}
            dataSource={{
              ...currentRow,
              status: String(currentRow.status || '')
            }}
            columns={[
              { title: '岗位ID', dataIndex: 'postId' },
              { title: '岗位名称', dataIndex: 'postName' },
              { title: '岗位编码', dataIndex: 'postCode' },
              { title: '显示顺序', dataIndex: 'postSort' },
              {
                title: '状态',
                dataIndex: 'status',
                valueEnum: {
                  '0': { text: '启用', status: 'Success' },
                  '1': { text: '停用', status: 'Error' },
                },
              },
              { title: '备注', dataIndex: 'remark' },
              {
                title: '创建时间',
                dataIndex: 'createTime',
                valueType: 'dateTime',
              },
              {
                title: '更新时间',
                dataIndex: 'updateTime',
                valueType: 'dateTime',
              },
            ]}
          />
        )}
      </Drawer>

      {/* 创建/编辑岗位表单 */}
      <CreatePositionForm
        open={modalVisible}
        onOpenChange={setModalVisible}
        currentRow={currentRow}
        onFinish={handleSubmit}
        loading={submitLoading}
      />
    </PageContainer>
  );
};

export default PositionList;

