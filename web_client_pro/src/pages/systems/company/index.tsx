import React, { useRef, useState } from 'react';
import { Button, Drawer, Form, Input, Modal, message } from 'antd';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProDescriptions, ProTable } from '@ant-design/pro-components';
import { useModel } from '@umijs/max';
import {
  createCompany,
  deleteCompany,
  getCompanyPagination,
  updateCompany,
} from '@/services/Api/Systems/company';
import type { CompanyItem } from '@/services/Model/Systems/company';

const CompanyManagement: React.FC = () => {
  const actionRef = useRef<ActionType>(null);
  const [form] = Form.useForm<{ companyName: string; remark?: string }>();
  const [modalVisible, setModalVisible] = useState(false);
  const [confirmLoading, setConfirmLoading] = useState(false);
  const [currentCompany, setCurrentCompany] = useState<CompanyItem | null>(null);
  const [detailVisible, setDetailVisible] = useState(false);
  const [detailCompany, setDetailCompany] = useState<CompanyItem | null>(null);
  const [currentSearchParams, setCurrentSearchParams] = useState<{
    current: number;
    pageSize: number;
  }>({ current: 1, pageSize: 10 });
  const { setInitialState } = useModel('@@initialState');

  const refreshCompanyMenu = React.useCallback(async () => {
    if (!setInitialState) return;
    await setInitialState((prev: any) => ({
      ...prev,
      companyMenuVersion: (prev?.companyMenuVersion || 0) + 1,
    }));
  }, [setInitialState]);

  const openCreateModal = () => {
    setCurrentCompany(null);
    form.resetFields();
    setModalVisible(true);
  };

  const openEditModal = (record: CompanyItem) => {
    setCurrentCompany(record);
    form.setFieldsValue({
      companyName: record.companyName,
      remark: record.remark,
    });
    setModalVisible(true);
  };

  const openDetailDrawer = (record: CompanyItem) => {
    setDetailCompany(record);
    setDetailVisible(true);
  };

  const closeDetailDrawer = () => {
    setDetailVisible(false);
    setDetailCompany(null);
  };

  const handleModalCancel = () => {
    setModalVisible(false);
    setCurrentCompany(null);
    form.resetFields();
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      const trimmedName = values.companyName.trim();
      const trimmedRemark = values.remark?.trim();

      if (!trimmedName) {
        message.error('公司名称不能为空');
        return;
      }

      const payload = {
        companyName: trimmedName,
        remark: trimmedRemark || undefined,
      };

      setConfirmLoading(true);

      if (currentCompany) {
        const response = await updateCompany({
          companyId: currentCompany.companyId,
          companyName: payload.companyName,
          remark: payload.remark,
        });

        if (response.code === 200) {
          message.success('更新成功');
          setModalVisible(false);
          actionRef.current?.reload();
          await refreshCompanyMenu();
        } else {
          message.error(response.msg || '更新失败');
        }
      } else {
        const response = await createCompany(payload);
        if (response.code === 200) {
          message.success('创建成功');
          setModalVisible(false);
          actionRef.current?.reload();
          await refreshCompanyMenu();
        } else {
          message.error(response.msg || '创建失败');
        }
      }
    } catch (error: any) {
      if (error?.errorFields) {
        return;
      }
      message.error(error?.message || '操作失败');
    } finally {
      setConfirmLoading(false);
    }
  };

  const handleDelete = (record: CompanyItem) => {
    Modal.confirm({
      title: `确定删除公司【${record.companyName}】吗？`,
      okText: '确认',
      cancelText: '取消',
      className: 'delete-confirm-modal',
      rootClassName: 'delete-confirm-modal',
      styles: {
        content: {
          background:
            'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
          backdropFilter: 'blur(14px) saturate(115%)',
          WebkitBackdropFilter: 'blur(14px) saturate(115%)',
          border: '1px solid rgba(72,115,255,0.28)',
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
      },
      onOk: async () => {
        try {
          const response = await deleteCompany(record.companyId);
          if (response.code === 200) {
            message.success('删除成功');
            actionRef.current?.reload();
            await refreshCompanyMenu();
          } else {
            message.error(response.msg || '删除失败');
          }
        } catch (error: any) {
          message.error(error?.message || '删除失败');
        }
      },
    });
  };

  const columns: ProColumns<CompanyItem>[] = [
    {
      title: '公司名称',
      dataIndex: 'companyName',
      ellipsis: true,
      width: 200,
      render: (_, record) => (
        <a
          onClick={() => openDetailDrawer(record)}
          style={{ color: '#1677ff' }}
        >
          {record.companyName}
        </a>
      ),
      formItemProps: {
        rules: [
          {
            required: false,
          },
        ],
      },
    },
    {
      title: '公司编码',
      dataIndex: 'companyCode',
      ellipsis: true,
      width: 260,
      copyable: true,
      search: false,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      valueType: 'dateTime',
      width: 180,
      search: false,
    },
    {
      title: '更新时间',
      dataIndex: 'updateTime',
      valueType: 'dateTime',
      width: 180,
      search: false,
    },
    {
      title: '操作',
      valueType: 'option',
      width: 160,
      render: (_, record) => [
        <a key="edit" onClick={() => openEditModal(record)}>
          编辑
        </a>,
        <a key="delete" onClick={() => handleDelete(record)}>
          删除
        </a>,
      ],
    },
  ];

  return (
    <PageContainer
      className="system-settings-page"
      header={{
        title: '公司管理',
      }}
    >
      <ProTable<CompanyItem>
        rowKey="companyId"
        actionRef={actionRef}
        columns={columns}
        search={{
          labelWidth: 120,
        }}
        pagination={{
          current: currentSearchParams.current,
          pageSize: currentSearchParams.pageSize,
          pageSizeOptions: ['10', '20', '50', '100'],
          showSizeChanger: true,
          showTotal: (total) => `共 ${total} 条记录`,
          showQuickJumper: true,
          onChange: (current, pageSize) => {
            setCurrentSearchParams({ current, pageSize });
            actionRef.current?.reload();
          },
          onShowSizeChange: (_, pageSize) => {
            setCurrentSearchParams({ current: 1, pageSize });
            actionRef.current?.reload();
          },
        }}
        request={async (params) => {
          try {
            // 确保页码不小于1，页面大小在1-100之间
            const current = Math.max(1, params.current || 1);
            const pageSize = Math.min(100, Math.max(1, params.pageSize || 10));
            
            const response = await getCompanyPagination({
              current,
              pageSize,
              companyName: params.companyName,
            });

            if (response.success) {
              return {
                data: response.data || [],
                total: response.total || 0,
                success: true,
              };
            }

            message.error('获取公司列表失败');
            return {
              data: [],
              total: 0,
              success: false,
            };
          } catch (error: any) {
            message.error(error?.message || '获取公司列表失败');
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        toolBarRender={() => [
          <Button key="create" type="primary" onClick={openCreateModal}>
            新增公司
          </Button>,
        ]}
        options={{
          density: true,
          fullScreen: true,
          reload: () => {
            actionRef.current?.reload();
          },
          setting: true,
        }}
      />

      <Drawer
        width={520}
        title={detailCompany?.companyName}
        placement="right"
        open={detailVisible}
        onClose={closeDetailDrawer}
        destroyOnClose
        className="company-info-drawer"
        rootClassName="company-info-drawer"
        extra={detailCompany ? (
          <div style={{ display: 'flex', gap: 12 }}>
            <Button
              type="link"
              icon={<EditOutlined />}
              onClick={() => openEditModal(detailCompany)}
            >
              编辑
            </Button>
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
              onClick={() => handleDelete(detailCompany)}
            >
              删除
            </Button>
          </div>
        ) : null}
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
        {detailCompany && (
          <ProDescriptions<CompanyItem>
            column={1}
            dataSource={detailCompany}
            columns={[
              { title: '公司名称', dataIndex: 'companyName' },
              { title: '公司编码', dataIndex: 'companyCode' },
              { title: '创建时间', dataIndex: 'createTime', valueType: 'dateTime' },
              { title: '更新时间', dataIndex: 'updateTime', valueType: 'dateTime' },
              {
                title: '备注',
                dataIndex: 'remark',
                renderText: (text) => text || '-',
              },
            ]}
          />
        )}
      </Drawer>

      <Modal
        title={currentCompany ? '编辑公司' : '新增公司'}
        open={modalVisible}
        onOk={handleSubmit}
        onCancel={handleModalCancel}
        destroyOnClose
        confirmLoading={confirmLoading}
        className="company-edit-modal"
        rootClassName="company-edit-modal"
        styles={{
          content: {
            background:
              'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
            backdropFilter: 'blur(14px) saturate(115%)',
            WebkitBackdropFilter: 'blur(14px) saturate(115%)',
            border: '1px solid rgba(72,115,255,0.28)',
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
        <Form form={form} layout="vertical" autoComplete="off">
          <Form.Item
            label="公司名称"
            name="companyName"
            rules={[{ required: true, message: '请输入公司名称' }]}
          >
            <Input placeholder="请输入公司名称" maxLength={200} allowClear autoComplete="off" name="companyName_no_autofill" spellCheck={false} />
          </Form.Item>

          {currentCompany && (
            <Form.Item label="公司编码">
              <Input value={currentCompany.companyCode} disabled />
            </Form.Item>
          )}

          <Form.Item label="备注" name="remark">
            <Input.TextArea rows={3} maxLength={500} allowClear autoComplete="off" name="remark_no_autofill" spellCheck={false} />
          </Form.Item>
        </Form>
      </Modal>
    </PageContainer>
  );
};

export default CompanyManagement;

