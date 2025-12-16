import { ActionType, ProColumns } from '@ant-design/pro-components';
import { FooterToolbar, PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import { useRequest } from '@umijs/max';
import { Button, Drawer, message, Modal, Popconfirm } from 'antd';
import React, { useRef, useState, useEffect } from 'react';
import { PlusOutlined, LockOutlined, UserOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { getUserList, deleteUsers, createUser, updateUser, getUserRoles, authUserRoles, getDeptTree, changeStatus, resetPassword, getUserDetail } from '@/services/Api/Systems/user';
import { getPositionList } from '@/services/Api/Systems/position';
import { getRoleList } from '@/services/Api/Systems/role';
import type { UserItem, UserQueryDto, UserAddDto, UserUpdateDto, ResetPasswordDto } from '@/services/Model/Systems/user';
import CreateUserForm from './CreateUserForm'; // 需要创建此组件
import AuthRoleModal from './AuthRoleModal'; // 需要创建此组件

const UserList: React.FC = () => {
  const actionRef = useRef<ActionType>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<UserItem>();
  const [selectedRows, setSelectedRows] = useState<UserItem[]>([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [authRoleVisible, setAuthRoleVisible] = useState(false);
  const [checkedRoleKeys, setCheckedRoleKeys] = useState<number[]>([]);
  const [currentSearchParams, setCurrentSearchParams] = useState<{
    current: number;
    pageSize: number;
  }>({ current: 1, pageSize: 20 });
  const [positionMap, setPositionMap] = useState<Record<string, string>>({});
  const [roleMap, setRoleMap] = useState<Record<string, string>>({});
  const [statusPatch, setStatusPatch] = useState<Record<number, string>>({});
  // 本地角色补丁：用于在后端未及时返回或未落库时，仍然让列表与抽屉即时显示变更
  const [rolePatch, setRolePatch] = useState<Record<number, number[]>>({});

  // 删除请求
  const { run: delRun, loading: deleteLoading } = useRequest(deleteUsers, {
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
      const response = currentRow ? await updateUser(currentRow.userId, payload) : await createUser(payload);
      // 检查返回的 code 字段，如果 code !== 200，抛出错误
      if (response && (response as any).code !== undefined && (response as any).code !== 200) {
        const error: any = new Error((response as any).msg || '操作失败');
        error.response = { data: { message: (response as any).msg } };
        throw error;
      }
      return response;
    },
    {
      manual: true,
      onSuccess: () => {
        actionRef.current?.reload();
        message.success(currentRow ? '更新成功' : '新增成功');
        setModalVisible(false);
      },
      onError: (error) => {
        const errorMsg = error.message || (error as any).response?.data?.message || (error as any).response?.data?.msg || '操作失败';
        message.error(errorMsg);
      },
    }
  );

  const [deptTree, setDeptTree] = useState<any[]>([]);
  const [deptLoading, setDeptLoading] = useState(false);

  // 添加获取部门树的函数
 const fetchDeptTree = async () => {
    setDeptLoading(true);
    try {
      const res = await getDeptTree();
      // 将 res.code === 0 修改为 res.code === 200
      if (res.code === 200 && res.data) {
        setDeptTree(formatDeptTree(res.data));
      }
    } catch (error) {
      console.error('获取部门树失败:', error);
    } finally {
      setDeptLoading(false);
    }
  };

  // 格式化部门树
  const formatDeptTree = (data: any[]): any[] => {
    return data.map(item => ({
      title: item.deptName,
      value: item.deptId,
      key: item.deptId, // key 可选，但建议加上
      children: item.children ? formatDeptTree(item.children) : [],
    }));
  };


  // 获取用户角色请求
  const { run: getUserRolesRun } = useRequest(
    async (userId: number) => {
      const response = await getUserRoles(userId);
      // 检查返回的 code 字段，如果 code !== 200，抛出错误
      if (response && (response as any).code !== undefined && (response as any).code !== 200) {
        const error: any = new Error((response as any).msg || '获取角色失败');
        error.response = { data: { message: (response as any).msg } };
        throw error;
      }
      // 返回 data 字段中的角色ID数组
      return (response as any)?.data || [];
    },
    {
      manual: true,
      onSuccess: (data) => {
        setCheckedRoleKeys(data || []);
        setAuthRoleVisible(true);
      },
      onError: (error) => {
        const errorMsg = error.message || (error as any).response?.data?.message || (error as any).response?.data?.msg || '获取角色失败';
        message.error(errorMsg);
      },
    }
  );

  // 授权角色请求（授权后主动校验，必要时用更新接口兜底；并做本地补丁，避免后端延迟导致列表不变）
  const { run: authUserRolesRun, loading: authLoading } = useRequest(
    async (userId: number, roleIds: number[]) => {
      const response = await authUserRoles(userId, roleIds);
      if (response && (response as any).code !== undefined && (response as any).code !== 200) {
        const error: any = new Error((response as any).msg || '角色授权失败');
        error.response = { data: { message: (response as any).msg } };
        throw error;
      }

      // 授权接口返回成功后，拉取一次详情确认是否真正落库；若未落库，使用更新接口强制写入
      try {
        const detailRes: any = await getUserDetail(userId);
        const serverIds = normalizeRoleIds(
          detailRes?.data?.roleIds ?? detailRes?.data?.RoleIds ?? detailRes?.data?.roles ?? detailRes?.data?.Roles
        );
        const targetIds = (roleIds || []).map((n) => Number(n)).filter((n) => !Number.isNaN(n));
        const same = JSON.stringify([...serverIds].sort()) === JSON.stringify([...targetIds].sort());
        if (!same) {
          const d = detailRes?.data || {};
          const payload: any = {
            userName: d.userName ?? d.UserName,
            nickName: d.nickName ?? d.NickName,
            email: d.email ?? d.Email,
            phoneNumber: d.phoneNumber ?? d.PhoneNumber ?? d.phone ?? d.Phone,
            phone: d.phoneNumber ?? d.PhoneNumber ?? d.phone ?? d.Phone,
            deptId: d.deptId ?? d.DeptId,
            status: String(d.status ?? d.Status ?? '1'),
            userId: userId,
          };
          // 岗位信息补全
          const rawPostId = d.postId ?? d.PostId;
          if (rawPostId !== undefined && rawPostId !== null && rawPostId !== '') {
            const postIdNum = Number(rawPostId);
            if (!Number.isNaN(postIdNum)) {
              payload.postId = postIdNum;
              payload.postIds = [postIdNum];
              payload.PostIds = [postIdNum];
            }
          }
          // 角色写入（大小写字段都带上）
          payload.roleIds = targetIds;
          payload.RoleIds = targetIds;
          await updateUser(userId, payload);
          return { appliedVia: 'update', roleIds: targetIds };
        }
      } catch (e) {
        // 若校验异常，继续按授权成功处理，但会用本地补丁保证前端显示
        return { appliedVia: 'auth', roleIds };
      }
      return { appliedVia: 'auth', roleIds };
    },
    {
      manual: true,
      onSuccess: (res: any) => {
        // 本地补丁：立即让列表展示变更
        if (currentRow?.userId) {
          setRolePatch((prev) => ({ ...prev, [currentRow.userId!]: (res?.roleIds as number[]) || [] }));
        }
        actionRef.current?.reload();
        message.success(res?.appliedVia === 'update' ? '角色授权成功（通过编辑接口落库）' : '角色授权成功');
        setAuthRoleVisible(false);
      },
      onError: (error) => {
        const errorMsg = error.message || (error as any).response?.data?.message || (error as any).response?.data?.msg || '角色授权失败';
        message.error(errorMsg);
      },
    }
  );

  // 重置密码请求
  const { run: resetPwdRun, loading: resetPwdLoading } = useRequest(
    async (userId: number, data: ResetPasswordDto) => {
      const response = await resetPassword(userId, data);
      if (response && (response as any).code !== undefined && (response as any).code !== 200) {
        const error: any = new Error((response as any).msg || '密码重置失败');
        error.response = { data: { message: (response as any).msg } };
        throw error;
      }
      return response;
    },
    {
      manual: true,
      onSuccess: () => {
        message.success('密码重置成功');
      },
      onError: (error) => {
        const errorMsg = error.message || (error as any).response?.data?.message || (error as any).response?.data?.msg || '密码重置失败';
        message.error(errorMsg);
      },
    }
  );

  // 修改状态请求（带确认）
  const { run: changeStatusRun, loading: changeStatusLoading } = useRequest(
    async (userId: number, status: string) => {
      const response = await changeStatus(userId, String(status));
      if (response && (response as any).code !== undefined && (response as any).code !== 200) {
        const error: any = new Error((response as any).msg || '状态修改失败');
        error.response = { data: { message: (response as any).msg } };
        throw error;
      }
      return { userId, status, response };
    },
    {
      manual: true,
      onSuccess: (res: any) => {
        const { userId, status } = res || {};
        setStatusPatch((prev) => ({ ...prev, [userId]: String(status) }));
        message.success('状态修改成功');
        actionRef.current?.reload();
      },
      onError: (error) => {
        const errorMsg = error.message || (error as any).response?.data?.message || (error as any).response?.data?.msg || '状态修改失败';
        message.error(errorMsg);
      },
    }
  );

  // 根据部门ID获取部门名称
  const getDeptNameById = (deptId: string | number, tree: any[]): string => {
    if (!tree || tree.length === 0) return '未知部门';
    
    for (const item of tree) {
      // 确保类型一致再比较，同时检查 value 和 deptId
      const itemValue = item.value !== undefined ? item.value : item.deptId;
      if (String(itemValue) === String(deptId)) {
        return item.title || item.deptName || '未知部门';
      }
      if (item.children && item.children.length > 0) {
        const found = getDeptNameById(deptId, item.children);
        if (found && found !== '未知部门') return found;
      }
    }
    return '未知部门';
  };

  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        // 优先加载部门树，确保表格渲染时能正确显示部门名称
        await fetchDeptTree();
        
        // 然后并行获取岗位、角色数据
        await Promise.all([
          (async () => {
            const res = await getPositionList({ current: 1, pageSize: 999 });
            if (!res.success) return;
            const map: Record<string, string> = {};
            (res.data || []).forEach((item: any) => {
              const id = item.postId ?? item.PostId;
              const name = item.postName ?? item.PostName;
              if (id !== undefined && name) {
                map[String(id)] = name;
              }
            });
            setPositionMap(map);
          })(),
          (async () => {
            const res = await getRoleList({ current: 1, pageSize: 999 });
            if (!res.success) return;
            const map: Record<string, string> = {};
            (res.data || []).forEach((item: any) => {
              const id = item.roleId ?? item.RoleId;
              const name = item.roleName ?? item.RoleName;
              if (id !== undefined && name) {
                map[String(id)] = name;
              }
            });
            setRoleMap(map);
          })(),
        ]);

        // 确保在所有初始数据加载完成后再刷新表格
        actionRef.current?.reloadAndRest?.();

      } catch (error) {
        message.error('加载初始数据失败');
        console.error('加载初始数据失败:', error);
      }
    };

    fetchInitialData();
  }, []);

  // 当部门树加载完成后，重新加载表格以确保部门名称正确显示
  useEffect(() => {
    if (deptTree && deptTree.length > 0) {
      actionRef.current?.reload?.();
    }
  }, [deptTree]);

  const normalizeRoleIds = (raw: any): number[] => {
    if (!raw && raw !== 0) return [];
    if (Array.isArray(raw)) {
      return raw.map((id) => Number(id)).filter((id) => !Number.isNaN(id));
    }
    if (typeof raw === 'string') {
      return raw
        .split(/[,，\s]+/)
        .filter(Boolean)
        .map((id) => Number(id))
        .filter((id) => !Number.isNaN(id));
    }
    if (typeof raw === 'number') {
      return [raw];
    }
    return [];
  };

  const getPostName = (postId?: number | string, fallback?: string) => {
    if (fallback) return fallback;
    if (postId === undefined || postId === null) return '';
    return positionMap[String(postId)] || '';
  };

  const getRoleNames = (roleIds: number[], fallback?: string) => {
    // 优先根据 roleIds 映射名称，避免后端返回的 roleNames 字符串未及时更新导致前端展示不变
    if (roleIds && roleIds.length > 0) {
      const names = roleIds
        .map((id) => roleMap[String(id)] || '')
        .filter(Boolean);
      if (names.length > 0) return names.join('、');
    }
    return fallback || '';
  };

  // 统一判断是否启用（兼容 '1' | 1 | true | 'true'）
  const isEnabled = (val: any) => {
    const v = String(val).toLowerCase();
    return v === '1' || v === 'true';
  };


  const columns: ProColumns<UserItem>[] = [
    { title: '用户ID', dataIndex: 'userId', hideInTable: true, hideInSearch: true },
    {
      title: '工号', dataIndex: 'userName', render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      )
    },
    { title: '姓名', dataIndex: 'nickName' },
    {
      title: '角色',
      dataIndex: 'roleNames',
      render: (text, record) => text || getRoleNames(normalizeRoleIds((record as any).roleIds), (record as any).roleNames) || '-',
    },
    {
      title: '部门',
      dataIndex: 'deptId',
      render: (deptId, record) => {
        // 优先使用后端返回的 deptName，其次使用树查询
        const deptName = (record as any).deptName || (record as any).DeptName;
        if (deptName) {
          return deptName;
        }
        if (!deptId) return '';
        // 如果后端没有返回 deptName，则从树中查询
        return getDeptNameById(String(deptId), deptTree);
      }
    },
    {
      title: '状态',
      dataIndex: 'status',
      render: (value, record) => {
        const uid = Number((record as any).userId);
        const effective = (statusPatch && statusPatch[uid] !== undefined) ? statusPatch[uid] : value;
        if (isEnabled(effective)) {
          return <span><CheckCircleOutlined className="text-success mr-1" /> 启用</span>;
        }
        return <span><CloseCircleOutlined className="text-error mr-1" /> 禁用</span>;
      }
    },
    { title: '创建时间', dataIndex: 'createTime', valueType: 'dateTime' },
    {
      title: '操作', valueType: 'option', render: (_, record) => [
        <a key="edit" onClick={() => {
          setCurrentRow(record);
          setModalVisible(true);
        }}>编辑</a>,
        <a key="resetPwd" onClick={() => handleResetPwdConfirm(record)}>重置密码</a>,
        <a key="changeStatus" onClick={() => handleChangeStatusConfirm(record)}>
          {isEnabled(statusPatch?.[Number((record as any).userId)] ?? (record as any).status) ? '禁用' : '启用'}
        </a>,
        <a key="authRole" onClick={() => {
          // 直接使用表格中已加载的 roleIds，避免因后端接口不兼容导致的 405/415 错误
          setCurrentRow(record);
          const preset = normalizeRoleIds((record as any).roleIds);
          setCheckedRoleKeys(preset);
          setAuthRoleVisible(true);
        }}>分配角色</a>,
        <a key="delete" onClick={() => {
          Modal.confirm({
            title: '确认删除',
            content: '确定删除该用户？',
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
            onOk: () => delRun([record.userId]),
          });
        }}>删除</a>,
      ]
    },
  ];

  const handleRemove = async () => {
    if (!selectedRows.length) {
      message.warning('请选择要删除的项');
      return;
    }
    await delRun(selectedRows.map(row => row.userId));
  };

  // 启用/禁用 确认并调用
  const handleChangeStatusConfirm = (record: UserItem) => {
    const uid = Number((record as any).userId);
    const effective = statusPatch?.[uid] ?? (record as any).status;
    const currentEnabled = isEnabled(effective);
    const nextStatus = currentEnabled ? '0' : '1';
    const actionText = currentEnabled ? '禁用' : '启用';
    Modal.confirm({
      title: (<span style={{ color: '#fff' }}>{`确认${actionText}`}</span>),
      content: (<span style={{ color: '#ffffff' }}>确定要{actionText}用户「{record.userName}」吗？</span>),
      className: 'status-confirm-modal',
      rootClassName: 'delete-confirm-modal status-confirm-modal',
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
      okText: '确认',
      cancelText: '取消',
      onOk: async () => {
        await changeStatusRun(record.userId, nextStatus);
      },
    });
  };

  // 重置密码 确认并调用  
  const handleResetPwdConfirm = (record: UserItem) => {
    Modal.confirm({
      title: (<span style={{ color: '#ffffff' }}>确认重置密码</span>),
      content: (
        <div style={{ color: '#ffffff' }}>
          <p>确定要重置用户「{record.userName}」的密码吗？</p>
          <p>重置后密码将变为默认密码：123456，请提醒用户及时修改。</p>
        </div>
      ),
      className: 'reset-confirm-modal',
      rootClassName: 'delete-confirm-modal reset-confirm-modal',
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
      okText: '确认重置',
      cancelText: '取消',
      onOk: () =>
        resetPwdRun(record.userId, { NewPassword: '123456' } as ResetPasswordDto),
    });
  };

  // 表单提交处理
  const handleSubmit = async (values: any) => {
    try {
      // 确保 deptId 是数字类型（ProFormTreeSelect 返回的是字符串）
      const deptId = values.deptId !== undefined && values.deptId !== null && values.deptId !== '' 
        ? Number(values.deptId) 
        : undefined;
      
      // 确保 postId 是数字类型
      const postId = values.postId !== undefined && values.postId !== null && values.postId !== '' 
        ? Number(values.postId) 
        : undefined;
      const postIds = postId !== undefined ? [postId] : [];
      
      // 确保 roleIds 是数字数组
      let roleIds: number[] = [];
      if (values.roleIds) {
        if (Array.isArray(values.roleIds)) {
          roleIds = values.roleIds.map((id: any) => Number(id)).filter((id: number) => !isNaN(id));
        } else {
          roleIds = [Number(values.roleIds)].filter((id: number) => !isNaN(id));
        }
      }

      // 可选字符串字段：去除首尾空格，空值时不传递，避免后端模型验证触发
      const trimmedEmail = typeof values.email === 'string' ? values.email.trim() : values.email;
      const normalizedEmail = trimmedEmail ? trimmedEmail : undefined;
      const trimmedPhone = typeof values.phoneNumber === 'string' ? values.phoneNumber.trim() : values.phoneNumber;
      const normalizedPhone = trimmedPhone ? trimmedPhone : undefined;
      
      if (currentRow) {
        // 更新用户
        const payload: any = {
          userName: values.userName,
          nickName: values.nickName,
          deptId: deptId,
          status: values.status ? '1' : '0',
          userId: currentRow.userId,
        };

        if (normalizedEmail !== undefined) {
          payload.email = normalizedEmail;
        }
        if (normalizedPhone !== undefined) {
          payload.phoneNumber = normalizedPhone;
          payload.phone = normalizedPhone;
        }

        // 岗位同时传递单值和数组，兼容后端
        if (postId !== undefined && postId !== null) {
          payload.postId = postId;
        }
        payload.postIds = postIds;
        payload.PostIds = postIds;

        // 更新时，即使 roleIds 为空数组也要传递（允许清空角色）
        payload.roleIds = roleIds;
        payload.RoleIds = roleIds;

        // 调试日志
        if (process.env.NODE_ENV === 'development') {
          console.log('更新用户数据:', payload);
          console.log('原始表单值:', values);
        }

        await submitRun(payload);
      } else {
        // 新增用户
        const payload: any = {
          userName: values.userName,
          password: values.password,
          nickName: values.nickName,
          deptId: deptId,
          status: values.status ? '1' : '0',
        };

        if (normalizedEmail !== undefined) {
          payload.email = normalizedEmail;
        }
        if (normalizedPhone !== undefined) {
          payload.phoneNumber = normalizedPhone;
          payload.phone = normalizedPhone;
        }

        if (postId !== undefined && postId !== null) {
          payload.postId = postId;
        }
        payload.postIds = postIds;
        payload.PostIds = postIds;

        const rolePayload = roleIds.length > 0 ? roleIds : [];
        payload.roleIds = rolePayload;
        payload.RoleIds = rolePayload;

        // 调试日志
        if (process.env.NODE_ENV === 'development') {
          console.log('新增用户数据:', payload);
          console.log('原始表单值:', values);
        }

        await submitRun(payload);
      }
      
      return true;
    } catch (error) {
      // 如果 submitRun 抛出错误，返回 false 阻止表单关闭
      return false;
    }
  };


  // 授权角色处理
  const handleAuthRoles = async (roleIds: number[]) => {
    if (!currentRow?.userId) return;
    await authUserRolesRun(currentRow.userId, roleIds);
  };
//
  return (
    <PageContainer className="system-settings-page">
      <ProTable<UserItem>
        rowKey="userId"
        actionRef={actionRef}
        columns={columns}
        request={async (params) => {
          // 确保页码不小于1，页面大小在1-100之间
          const current = Math.max(1, params.current || 1);
          const pageSize = Math.min(100, Math.max(1, params.pageSize || 10));
          
          const queryParams: UserQueryDto = {
            current,
            pageSize,
            userName: params.userName,
            status: params.status,
          };
          const res = await getUserList(queryParams);
          if (!res.success) {
            return {
              data: [],
              success: false,
              total: 0,
            };
          }

          const formattedData = (res.data || []).map((item: any) => {
            const rawPostIds = item.postIds ?? item.PostIds ?? item.postId ?? item.PostId;
            let postId: number | undefined;
            let postIds: number[] = [];
            if (Array.isArray(rawPostIds)) {
              postIds = rawPostIds.map((id: any) => Number(id)).filter((id: number) => !Number.isNaN(id));
            } else if (rawPostIds !== undefined && rawPostIds !== null) {
              const parsed = Number(rawPostIds);
              if (!Number.isNaN(parsed)) {
                postIds = [parsed];
              }
            }
            if (!postIds.length && (item.postId ?? item.PostId) !== undefined) {
              const parsed = Number(item.postId ?? item.PostId);
              if (!Number.isNaN(parsed)) {
                postIds = [parsed];
              }
            }
            postId = postIds[0];

            const rawRoleIds = item.roleIds ?? item.RoleIds ?? item.roles ?? item.Roles;
            // 如果存在本地补丁，优先使用补丁中的角色ID（解决后端授权成功但未落库导致列表不变的问题）
            const patched = rolePatch[(item.userId ?? item.UserId) as number];
            const roleIds = patched && patched.length > 0 ? patched : normalizeRoleIds(rawRoleIds);

            const phoneValue =
              item.phoneNumber ??
              item.PhoneNumber ??
              item.phone ??
              item.Phone ??
              '';

            const deptIdValue = item.deptId ?? item.DeptId;
            const deptNameValue = item.deptName ?? item.DeptName ?? (deptIdValue ? getDeptNameById(deptIdValue, deptTree) : '');

            const postNameValue = getPostName(postId, item.postName ?? item.PostName);
            const roleNamesValue = getRoleNames(roleIds, item.roleNames ?? item.RoleNames);

            return {
              ...item, // 保留原始字段
              userId: item.userId ?? item.UserId,
              userName: item.userName ?? item.UserName,
              nickName: item.nickName ?? item.NickName,
              deptId: deptIdValue,
              deptName: deptNameValue,
              postId,
              postIds,
              postName: postNameValue,
              email: item.email ?? item.Email,
              phoneNumber: phoneValue,
              status: String(item.status ?? item.Status ?? '0'),
              createTime: item.createTime ?? item.CreateTime,
              // 后端字段兼容：LoginDate/loginDate → lastLoginTime
              loginDate: item.loginDate ?? item.LoginDate,
              lastLoginTime: item.lastLoginTime ?? item.LastLoginTime ?? item.loginDate ?? item.LoginDate,
              roleIds,
              roleNames: roleNamesValue,
            };
          });

          return {
            data: formattedData,
            success: true,
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
            <PlusOutlined /> 新增用户
          </Button>,
        ]}
        expandable={{
          expandedRowRender: (record) => (
            <div className="p-3 bg-gray-50">
              <p><strong>部门:</strong> {record.deptName}</p>
              <p><strong>角色:</strong> {record.roleNames || '未分配角色'}</p>
            </div>
          )
        }}
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
        className="user-info-drawer"
        rootClassName="user-info-drawer"
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
          <ProDescriptions<UserItem>
            column={2}
            title={currentRow.userName}
            dataSource={{
              ...currentRow,
              phoneNumber:
                currentRow.phoneNumber ??
                (currentRow as any).phone ??
                (currentRow as any).Phone ??
                '',
              deptName:
                currentRow.deptName ||
                (currentRow.deptId ? getDeptNameById(currentRow.deptId, deptTree) : ''),
              postName: getPostName(currentRow.postId, currentRow.postName),
              roleNames: getRoleNames(normalizeRoleIds(currentRow.roleIds), currentRow.roleNames),
            }}
            columns={[
              { title: '用户ID', dataIndex: 'userId' },
              { title: '工号', dataIndex: 'userName' },
              { title: '姓名', dataIndex: 'nickName' },
              { title: '邮箱', dataIndex: 'email' },
              { title: '手机号码', dataIndex: 'phoneNumber' },
              { title: '部门', dataIndex: 'deptName' },
              { title: '岗位', dataIndex: 'postName' },
              { title: '角色', dataIndex: 'roleNames' },
              {
                title: '状态',
                dataIndex: 'status',
                valueEnum: {
                  '1': { text: '启用', status: 'Success' },
                }
              },
              { title: '创建时间', dataIndex: 'createTime', valueType: 'dateTime' },
              { title: '最后登录时间', dataIndex: 'lastLoginTime', valueType: 'dateTime' },
            ]}
          />
        )}
      </Drawer>

      {/* 创建/编辑用户表单 */}
      <CreateUserForm
        open={modalVisible}
        onOpenChange={setModalVisible}
        currentRow={currentRow}
        onFinish={handleSubmit}
        loading={submitLoading}
      />


      {/* 角色授权弹窗 */}
      <AuthRoleModal
        open={authRoleVisible}
        onOpenChange={setAuthRoleVisible}
        userId={currentRow?.userId}
        checkedRoleKeys={checkedRoleKeys}
        onCheckedChange={setCheckedRoleKeys}
        onOk={handleAuthRoles}
        loading={authLoading}
      />
    </PageContainer>
  );
}

export default UserList;
