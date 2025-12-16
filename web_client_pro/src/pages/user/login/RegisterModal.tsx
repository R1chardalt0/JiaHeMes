import React, { useEffect, useMemo, useRef, useState } from 'react';
import { App, Button, Form, Input, Modal, TreeSelect } from 'antd';
import type { ModalProps } from 'antd';
import { createStyles } from 'antd-style';
import type { TreeSelectProps } from 'antd';
import { createUser, getDeptTree, checkUserNameExists, checkEmailExists } from '@/services/Api/Systems/user';
import { getRoleList } from '@/services/Api/Systems/role';
import { getPositionList } from '@/services/Api/Systems/position';
import type { RoleItem } from '@/services/Model/Systems/role';
import type { PositionItem } from '@/services/Model/Systems/position';

type RegisterSuccessPayload = {
  userName: string;
  password: string;
};

type RegisterModalProps = {
  open: boolean;
  onClose: () => void;
  onRegistered: (payload: RegisterSuccessPayload) => void;
};

type DeptTreeNode = {
  title: string;
  value: string;
  key: string;
  pathLabel: string;
  children?: DeptTreeNode[];
};

const DEFAULT_ROLE_KEY = 'common_user';
const DEFAULT_ROLE_NAME = '普通用户';
const DEFAULT_POSITION_CODE = 'ordinary_staff';
const DEFAULT_POSITION_NAME = '普通员工';

const useRegisterStyles = createStyles(() => ({
  glassForm: {
    '.ant-form-item-label > label': {
      color: '#f7f9ff',
      fontSize: 14,
      display: 'inline-flex',
      alignItems: 'center',
      gap: 4,
    },
    '.ant-form-item': {
      marginBottom: 16,
    },
  },
  labelWrapper: {
    display: 'inline-flex',
    alignItems: 'center',
    gap: 4,
  },
  requiredStar: {
    color: '#ff4d4f',
    fontWeight: 700,
    fontSize: 18,
    lineHeight: 1,
  },
  optionalText: {
    color: 'rgba(247, 249, 255, 0.55)',
    fontSize: 12,
  },
  // 输入框样式
  textInput: {
    height: 44,
    backgroundColor: 'rgba(7,16,35,0.65) !important',
    borderColor: 'rgba(255,255,255,0.32)',
    color: '#ffffff !important',
    caretColor: '#ffffff',
    boxShadow: 'none',
    transition: 'border-color 0.25s ease, transform 0.25s ease',
    '&:hover, &:focus-within, &:focus': {
      borderColor: 'rgba(65,255,235,1)',
      // boxShadow: '0 0 12px rgba(104,182,255,0.25)',
      backgroundColor: 'rgba(7,16,35,0.65) !important',
    },
    '&::selection': {
      backgroundColor: 'rgba(104,182,255,0.35)',
    },
    '&::placeholder': {
      color: 'rgba(255,255,255,0.65)',
    },
    '&.ant-input': {
      backgroundColor: 'rgba(7,16,35,0.65) !important',
      boxShadow: '0 4px 10px rgba(0,0,0,0.25) inset !important',
      color: '#ffffff !important',
      // // 输入状态底色
      // '&:focus': {
      //   backgroundColor: 'rgba(255,255,255,0.12) !important',
      // },
      '&::placeholder': {
        color: 'rgba(255,255,255,0.65)',
      },
      '&:-webkit-autofill': {
        WebkitTextFillColor: '#ffffff',
        WebkitBoxShadow: '0 0 0px 1000px rgba(255,255,255,0.08) inset',
        transition: 'background-color 50000s ease-in-out 0s',
      },
    },
    '&:-webkit-autofill': {
      WebkitTextFillColor: '#ffffff',
      WebkitBoxShadow: '0 0 0px 1000px rgba(7,16,35,0.65) inset',
      transition: 'background-color 50000s ease-in-out 0s',
    },
  },
  passwordInput: {
    height: 44,
    backgroundColor: 'rgba(7,16,35,0.65) !important',
    borderColor: 'rgba(255,255,255,0.32)',
    color: '#ffffff',
    boxShadow: 'none',
    transition: 'border-color 0.25s ease, transform 0.25s ease',
    // 去掉内部浅灰色内框（来自内部 input 的背景和边框）
    '& .ant-input': {
      background: 'transparent !important',
      border: 'none !important',
      boxShadow: 'none !important',
      color: '#ffffff',
      caretColor: '#ffffff',
      '&:focus': {
        background: 'transparent !important',
      },
      '&::placeholder': {
        color: 'rgba(255,255,255,0.65)',
      },
      '&:-webkit-autofill': {
        WebkitTextFillColor: '#ffffff',
        WebkitBoxShadow: '0 0 0px 1000px rgba(7,16,35,0.65) inset',
        transition: 'background-color 50000s ease-in-out 0s',
      },
    },
    // hover/focus 高亮只作用于外层，不改变内部背景
    '&:hover, &:focus, &.ant-input-affix-wrapper-focused': {
      borderColor: 'rgba(65,255,235,1)',
      boxShadow: '0 4px 10px rgba(0,0,0,0.25) inset !important',
      backgroundColor: 'rgba(7,16,35,0.65) !important',
    },
    '& .ant-input-password-icon': {
      color: 'rgba(255,255,255,0.75) !important',
    },
  },
  treeSelect: {
    '& .ant-select-selector': {
      height: 44,
      backgroundColor: 'rgba(7,16,35,0.65) !important',
      borderColor: 'rgba(255,255,255,0.32) !important',
      color: '#ffffff',
      display: 'flex',
      alignItems: 'center',
      transition: 'border-color 0.25s ease, background 0.25s ease',
    },
    '&:hover .ant-select-selector, &.ant-select-focused .ant-select-selector': {
      borderColor: 'rgba(65,255,235,1) !important',
      backgroundColor: 'rgba(7,16,35,0.65) !important',
      boxShadow: '0 4px 10px rgba(0,0,0,0.25) inset !important',
    },
    '& .ant-select-selection-placeholder': {
      color: 'rgba(255,255,255,0.65) !important',
    },
    '& .ant-select-selection-item': {
      color: '#ffffff',
    },
    '& .ant-select-arrow': {
      color: 'rgba(255,255,255,0.8)',
    },
  },
  treeDropdown: {
    background:
      'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
    backdropFilter: 'blur(12px) saturate(115%)',
    WebkitBackdropFilter: 'blur(12px) saturate(115%)',
    color: '#ffffff',
    // border: '1px solid rgba(72,115,255,0.28)',
    boxShadow:
      '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)',
    overflow: 'hidden',
    '.ant-select-tree': {
      color: '#ffffff',
      background: 'transparent',
    },
    '.ant-select-tree-switcher': {
      color: 'rgba(255,255,255,0.75)'
    },
    '.ant-select-tree-title': {
      color: 'rgba(255,255,255,0.92)',
    },
    '.ant-select-tree-node-content-wrapper': {
      borderLeft: '2px solid transparent',
      transition: 'background 0.2s ease, border-color 0.2s ease',
    },
    '.ant-select-tree-node-content-wrapper:hover': {
      backgroundColor: 'rgba(24, 144, 255,1) !important',
      // borderLeftColor: 'rgba(104,182,255,0.6)'
    },
    // '.ant-select-tree-treenode-selected .ant-select-tree-node-content-wrapper': {
    //   backgroundColor: 'rgba(70,130,255,0.28) !important',
    //   borderLeftColor: '#68b6ff',
    // },
    '.ant-select-tree-treenode-selected .ant-select-tree-title': {
      color: '#060000ff'
    },
    '.ant-select-tree-list-holder-inner': {
      padding: '6px 4px'
    },
    // 滚动条样式
    // '& ::-webkit-scrollbar': { width: 8, height: 8 },
    // '& ::-webkit-scrollbar-thumb': {
    //   background: 'rgba(104,182,255,0.35)',
    //   borderRadius: 8,
    // },
    // '& ::-webkit-scrollbar-track': {
    //   background: 'rgba(255,255,255,0.06)'
    // },
  },
}));

const RegisterModal: React.FC<RegisterModalProps> = ({ open, onClose, onRegistered }) => {
  const [form] = Form.useForm();
  const { message } = App.useApp();
  const { styles: registerStyles } = useRegisterStyles();

  const [deptTree, setDeptTree] = useState<DeptTreeNode[]>([]);
  const [deptLoading, setDeptLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [roleId, setRoleId] = useState<number | undefined>(undefined);
  const [postId, setPostId] = useState<number | undefined>(undefined);
  const [leafDeptMap, setLeafDeptMap] = useState<Record<string, boolean>>({});
  const hasInitRef = useRef(false);
  const accountCheckCache = useRef<{ value: string; exists: boolean }>({
    value: '',
    exists: false,
  });

  const formatDeptTree = (data: any[], parentPath: string[] = [], leafMap: Record<string, boolean> = {}): DeptTreeNode[] => {
    return data.map((item) => {
      const currentPath = [...parentPath, item.deptName].filter(Boolean);
      const node: DeptTreeNode = {
        title: item.deptName,
        value: String(item.deptId),
        key: String(item.deptId),
        pathLabel: currentPath.join(' → '),
      };
      if (item.children && item.children.length > 0) {
        node.children = formatDeptTree(item.children, currentPath, leafMap);
      } else {
        leafMap[node.value] = true;
      }
      return node;
    });
  };

  const fetchDeptTree = async () => {
    setDeptLoading(true);
    try {
      const res = await getDeptTree();
      const successFlag = (res as any)?.success;
      const success = successFlag !== false && (res?.code === undefined || res.code === 200);
      if (success && Array.isArray(res?.data)) {
        const nextLeafMap: Record<string, boolean> = {};
        const formatted = formatDeptTree(res.data, [], nextLeafMap);
        setDeptTree(formatted);
        setLeafDeptMap(nextLeafMap);
      } else {
        message.error(res?.msg || '获取部门数据失败');
      }
    } catch (error: any) {
      console.error('获取部门数据失败', error);
      message.error(error?.message || '获取部门数据失败');
    } finally {
      setDeptLoading(false);
    }
  };

  const fetchFixedRoleAndPost = async () => {
    try {
      const [roleRes, postRes] = await Promise.all([
        getRoleList({ current: 1, pageSize: 999 }),
        getPositionList({ current: 1, pageSize: 999 }),
      ]);

      if (roleRes?.data) {
        const matchRole = (roleRes.data as RoleItem[]).find(
          (item) => item.roleKey === DEFAULT_ROLE_KEY || item.roleName === DEFAULT_ROLE_NAME,
        );
        if (matchRole) {
          setRoleId(matchRole.roleId);
        }
      }

      if (postRes?.data) {
        const matchPost = (postRes.data as PositionItem[]).find(
          (item) => item.postCode === DEFAULT_POSITION_CODE || item.postName === DEFAULT_POSITION_NAME,
        );
        if (matchPost) {
          setPostId(matchPost.postId);
        }
      }
    } catch (error) {
      console.warn('获取默认角色/岗位失败，仅发送固定字段', error);
    }
  };

  useEffect(() => {
    if (open) {
      form.resetFields();
      fetchDeptTree();
      if (!hasInitRef.current) {
        fetchFixedRoleAndPost();
        hasInitRef.current = true;
      }
    }
  }, [open]);

  const isLeafDept = (value?: string) => {
    if (!value) return false;
    return Boolean(leafDeptMap[value]);
  };

  const checkAccountExists = async (userName: string) => {
    const trimmed = (userName || '').trim();
    if (!trimmed) return false;
    if (accountCheckCache.current.value === trimmed) {
      return accountCheckCache.current.exists;
    }

    try {
      const res = await checkUserNameExists(trimmed, undefined, { skipErrorHandler: true });
      const exists =
        res?.data === true ||
        ((res as any)?.success === false) ||
        (res?.code !== undefined && res.code !== 200);
      accountCheckCache.current = { value: trimmed, exists: Boolean(exists) };
      return Boolean(exists);
    } catch (error: any) {
      // 若接口不存在/返回404，则跳过账号唯一性预校验
      const status = error?.response?.status || error?.status || error?.data?.code;
      if (status === 404) {
        console.warn('checkUserNameExists API 404，跳过账号预校验');
        return false;
      }
      console.warn('校验账号唯一性失败，忽略并继续提交流程', error);
      return false;
    }
  };

  const isEmailTaken = async (email: string) => {
    const trimmed = (email || '').trim();
    if (!trimmed) return false;
    try {
      const res = await checkEmailExists(trimmed, undefined, { skipErrorHandler: true });
      const exists =
        res?.data === true ||
        ((res as any)?.success === false) ||
        (res?.code !== undefined && res.code !== 200);
      return Boolean(exists);
    } catch (error: any) {
      const status = error?.response?.status || error?.status || error?.data?.code;
      if (status === 404) {
        console.warn('checkEmailExists API 404，跳过邮箱预校验');
        return false;
      }
      console.warn('校验邮箱唯一性失败，忽略并继续提交流程', error);
      return false;
    }
  };


  const passwordRules = [
    { required: true, message: '请输入密码' },
    {
      validator: (_: any, value: string) => {
        if (!value) return Promise.reject();
        if (value.length < 6) {
          return Promise.reject(new Error('密码至少6位'));
        }
        const pattern = /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d\W_]{6,}$/;
        if (!pattern.test(value)) {
          return Promise.reject(new Error('密码需包含字母和数字'));
        }
        return Promise.resolve();
      },
    },
  ];

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      if (!isLeafDept(values.deptId)) {
        message.error('请选择最末级部门节点');
        return;
      }
      setSubmitting(true);

      const userName = values.userName.trim();
      const nickName = values.nickName?.trim();
      const passwordValue = values.password;

      // 后端可能不支持预校验接口，直接提交，由后端返回重复信息


      const payload: any = {
        userName,
        password: values.password,
        nickName: nickName || userName,
        deptId: Number(values.deptId),
        status: '1',
        roleIds: roleId ? [roleId] : [],
        RoleIds: roleId ? [roleId] : [],
        postId: postId,
        postIds: postId ? [postId] : [],
        PostIds: postId ? [postId] : [],
        roleKey: DEFAULT_ROLE_KEY,
        role: DEFAULT_ROLE_KEY,
        roleCode: DEFAULT_ROLE_KEY,
        position: DEFAULT_POSITION_CODE,
        positionCode: DEFAULT_POSITION_CODE,
        positionKey: DEFAULT_POSITION_CODE,
      };

      const res = await createUser(payload, { skipErrorHandler: true });
      const success = res?.success !== false && (res?.code === undefined || res.code === 200);
      if (success) {
        message.success('注册成功');
        onRegistered({
          userName,
          password: passwordValue,
        });
        onClose();
      } else {
        const msg = (res?.msg || '').toString();
        const fullMsg = msg.toLowerCase();
        // 针对后端返回的重复信息进行定向提示与表单高亮
        if ((/账号|用户名|user|username/.test(msg) && (/已存在/.test(msg) || /exist/.test(fullMsg))) || res?.code === 40901) {
          form.setFields([{ name: 'userName', errors: ['账号已存在，请重新输入'] }]);
          message.error('账号已存在，请重新输入');
        } else if ((/邮箱|email/.test(msg) && (/已存在/.test(msg) || /exist/.test(fullMsg))) || res?.code === 40903) {
          form.setFields([{ name: 'email', errors: ['邮箱已存在，请重新输入'] }]);
          message.error('邮箱已存在，请重新输入');
        } else {
          message.error(msg || '注册失败，请稍后重试');
        }
      }
    } catch (error: any) {
      if (error?.errorFields) {
        return;
      }
      console.error('注册失败', error);
      const status = error?.response?.status;
      const respMsg = error?.response?.data?.msg || error?.message || '';
      if (status === 404) {
        message.error('注册接口不存在（404），请联系管理员检查后端路由或代理配置');
      } else if (/账号|用户名|user|username/.test(respMsg) && (/已存在/.test(respMsg) || /exist/i.test(respMsg))) {
        form.setFields([{ name: 'userName', errors: ['账号已存在，请重新输入'] }]);
        message.error('账号已存在，请重新输入');
      } else if (/邮箱|email/i.test(respMsg) && (/已存在/.test(respMsg) || /exist/i.test(respMsg))) {
        form.setFields([{ name: 'email', errors: ['邮箱已存在，请重新输入'] }]);
        message.error('邮箱已存在，请重新输入');
      } else {
        message.error(respMsg || '注册失败，请稍后再试');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const treeSelectProps: TreeSelectProps = useMemo(
    () => ({
      treeData: deptTree,
      placeholder: '请选择所属部门',
      showSearch: true,
      allowClear: true,
      treeDefaultExpandAll: true,
      treeLine: { showLeafIcon: false },
      treeNodeFilterProp: 'pathLabel',
      treeNodeLabelProp: 'pathLabel',
      dropdownStyle: { maxHeight: 320, overflow: 'auto' },
      filterTreeNode: (input, node) =>
        ((node as any)?.pathLabel || (node as any)?.title || '')
          .toLowerCase()
          .includes(input.toLowerCase()),
      loading: deptLoading,
    }),
    [deptTree, deptLoading],
  );

  const modalStyles: ModalProps['styles'] = {
    content: {
      background:
        'radial-gradient(140% 140% at 0% 0%, rgba(64, 116, 255, 0.12) 0%, rgba(12, 18, 40, 0.9) 65%)',
      border: '2px solid rgba(72, 115, 255, 0.35)',
      boxShadow: '0 20px 60px rgba(4, 9, 20, 0.65)',
      borderRadius: 25,
      overflow: 'hidden',
      backdropFilter: 'blur(18px)',
      maxHeight: '90vh',
    },
    header: {
      background: 'transparent',
      paddingBottom: 8,
      color: '#ffffff',
    },
    body: {
      background: 'transparent',
       maxHeight: '70vh',
       overflowY: 'auto',
       paddingRight: 6,
    },
    footer: {
      background: 'transparent',
      paddingBottom: 15,
    },
    mask: {
      backdropFilter: 'blur(2px)',
    },
  };

  return (
    <Modal
      title={<span style={{ color: '#ffffff' }}>用户注册</span>}
      closeIcon={<span style={{ color: '#ffffff', fontSize: 18 }}>×</span>}
      open={open}
      onCancel={onClose}
      maskClosable
      destroyOnClose
      width={520}
      styles={modalStyles}
      centered
      footer={
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
          <Button onClick={onClose}>取消</Button>
          <Button type="primary" loading={submitting} onClick={handleSubmit}>
            注册
          </Button>
        </div>
      }
    >
      <Form
        layout="vertical"
        form={form}
        requiredMark={(label, { required }) => (
          <span className={registerStyles.labelWrapper}>
            {required && <span className={registerStyles.requiredStar}>*</span>}
            <span>{label}</span>
            {!required && <span className={registerStyles.optionalText}>（选填）</span>}
          </span>
        )}
        style={{ marginTop: 8 }}
        className={registerStyles.glassForm}
      >
        <Form.Item
          label="工号"
          name="userName"
          rules={[
            { required: true, message: '请输入工号' },
          ]}
        >
          <Input
            placeholder="请输入工号"
            className={registerStyles.textInput}
          />
        </Form.Item>

        <Form.Item
          label="密码"
          name="password"
          rules={passwordRules}
        >
          <Input.Password
            placeholder="请输入密码"
            maxLength={32}
            className={registerStyles.passwordInput}
          />
        </Form.Item>

        <Form.Item
          label="确认密码"
          name="confirmPassword"
          dependencies={['password']}
          rules={[
            { required: true, message: '请再次输入密码' },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value) {
                  return Promise.reject(new Error('请再次输入密码'));
                }
                if (value !== getFieldValue('password')) {
                  return Promise.reject(new Error('两次密码输入不一致'));
                }
                return Promise.resolve();
              },
            }),
          ]}
        >
          <Input.Password
            placeholder="请再次输入密码"
            maxLength={32}
            className={registerStyles.passwordInput}
          />
        </Form.Item>

        <Form.Item
          label="姓名"
          name="nickName"
          rules={[
            { required: true, message: '请输入姓名' },
            { max: 20, message: '姓名最多20个字符' },
          ]}
        >
          <Input
            placeholder="请输入姓名"
            maxLength={20}
            className={registerStyles.textInput}
          />
        </Form.Item>

        <Form.Item
          label="所属部门"
          name="deptId"
          rules={[
            { required: true, message: '请选择所属部门' },
            () => ({
              validator(_, value) {
                if (!value) return Promise.resolve();
                if (!isLeafDept(value)) {
                  return Promise.reject(new Error('请选择最末级子部门'));
                }
                return Promise.resolve();
              },
            }),
          ]}
        >
          <TreeSelect
            {...treeSelectProps}
            className={registerStyles.treeSelect}
            dropdownClassName={[registerStyles.treeDropdown, 'glass-dropdown', 'user-edit-drawer-dropdown'].join(' ')}
          />
        </Form.Item>

        {/* 姓名字段已去除 */}
      </Form>
    </Modal>
  );
};

export default RegisterModal;

