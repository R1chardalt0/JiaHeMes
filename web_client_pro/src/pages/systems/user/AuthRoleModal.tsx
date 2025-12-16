import { ProTable } from '@ant-design/pro-components';
import { Button, Modal, message } from 'antd';
import React, { useState, useEffect } from 'react';
import { getRoleList } from '@/services/Api/Systems/role';
import type { RoleItem } from '@/services/Model/Systems/role';

type AuthRoleModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  userId?: number;
  checkedRoleKeys: number[];
  onCheckedChange: (keys: number[]) => void;
  onOk: (roleIds: number[]) => Promise<void>;
  loading?: boolean;
};

const AuthRoleModal: React.FC<AuthRoleModalProps> = ({ open, onOpenChange, userId, checkedRoleKeys, onCheckedChange, onOk, loading }) => {
  const [roleList, setRoleList] = useState<RoleItem[]>([]);
  const [roleLoading, setRoleLoading] = useState(false);
  const [selectedRowKeys, setSelectedRowKeys] = useState<number[]>(checkedRoleKeys);

  // 获取角色列表
  const fetchRoleList = async () => {
    setRoleLoading(true);
    try {
      const res = await getRoleList({ current: 1, pageSize: 100 });
      if (res.success && res.data) {
        setRoleList(res.data);
      }
    } catch (error) {
      message.error('获取角色列表失败');
      console.error('获取角色列表失败:', error);
    } finally {
      setRoleLoading(false);
    }
  };

  useEffect(() => {
    if (open) {
      fetchRoleList();
      setSelectedRowKeys(checkedRoleKeys);
    }
  }, [open, checkedRoleKeys]);

  useEffect(() => {
    onCheckedChange(selectedRowKeys);
  }, [selectedRowKeys, onCheckedChange]);

  const handleOk = async () => {
    if (!userId) {
      message.warning('用户ID不存在');
      return;
    }
    await onOk(selectedRowKeys);
  };

  const columns = [
    { title: '角色名称', dataIndex: 'roleName' },
    { title: '角色标识', dataIndex: 'roleKey' },
    { title: '角色描述', dataIndex: 'description' },
    { title: '状态', dataIndex: 'status', valueEnum: {
      '0': { text: '禁用', status: 'Error' },
      '1': { text: '启用', status: 'Success' },
    }},
  ];

  return (
    <Modal
      title={`为用户分配角色`}
      width={700}
      open={open}
      onCancel={() => onOpenChange(false)}
      className="auth-role-modal"
      rootClassName="auth-role-modal"
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
      footer={[
        <Button key="cancel" onClick={() => onOpenChange(false)}>取消</Button>,
        <Button key="confirm" type="primary" loading={loading} onClick={handleOk}>确认分配</Button>,
      ]}
    >
      <ProTable<RoleItem>
        rowKey="roleId"
        columns={columns}
        dataSource={roleList}
        loading={roleLoading}
        rowSelection={{ 
          type: 'checkbox', 
          selectedRowKeys, 
          onChange: (selectedRowKeys: React.Key[]) => {
            setSelectedRowKeys(selectedRowKeys.map(key => Number(key)));
          }
        }}
        pagination={false}
        options={false}
        toolBarRender={false}
      />
    </Modal>
  );
};

export default AuthRoleModal;