import { ProForm, ProFormSelect, ProFormText, ProFormDigit, ProFormSwitch, ProFormTreeSelect, ProFormInstance, ProFormTextArea } from '@ant-design/pro-components';
import { Button, Drawer } from 'antd';
import React, { useEffect } from 'react';
import { UserAddDto, UserUpdateDto, UserItem } from '@/services/Model/Systems/user';
import { getDeptTree } from '@/services/Api/Systems/user';
import { getPositionList } from '@/services/Api/Systems/position';
import { getRoleList } from '@/services/Api/Systems/role';

interface DeptTreeItem {
  title: string;
  value: string;
  children?: DeptTreeItem[];
}
type CreateUserFormProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  currentRow?: UserItem;
  onFinish: (values: UserAddDto | UserUpdateDto) => Promise<boolean>;
  loading?: boolean;
};

const DEFAULT_POSITION_NAME = '普通员工';
const DEFAULT_ROLE_NAME = '普通用户';

const CreateUserForm: React.FC<CreateUserFormProps> = ({ open, onOpenChange, currentRow, onFinish, loading }) => {
  const [deptTree, setDeptTree] = React.useState<any[]>([]);
  const [deptLoading, setDeptLoading] = React.useState(false);
  const [positionList, setPositionList] = React.useState<any[]>([]);
  const [positionLoading, setPositionLoading] = React.useState(false);
  const [roleList, setRoleList] = React.useState<any[]>([]);
  const [roleLoading, setRoleLoading] = React.useState(false);
  const formRef = React.useRef<ProFormInstance<any>>(null);

  // 获取部门树
  const fetchDeptTree = async () => {
    setDeptLoading(true);
    try {
      const res = await getDeptTree();
      console.log('接口返回:', res); // 调试：确认 data 存在

      if (res.code === 200 && res.data) {
        const formatted = formatDeptTree(res.data);
        console.log('格式化后:', formatted); // 调试：看是否生成了 title/value
        setDeptTree(formatted);
      } else {
        console.error('获取部门树失败:', res.msg);
      }
    } catch (error) {
      console.error('获取部门树异常:', error);
    } finally {
      setDeptLoading(false);
    }
  };

  // 格式化部门树为Select组件需要的格式
  const formatDeptTree = (data: any[]): any[] => {
    return data.map(item => ({
      title: item.deptName,
      value: String(item.deptId),
      key: String(item.deptId), // key 可选，但建议加上
      children: item.children ? formatDeptTree(item.children) : [],
    }));
  };

  // 获取岗位列表
  const fetchPositionList = async () => {
    setPositionLoading(true);
    try {
      const res = await getPositionList({ current: 1, pageSize: 1000 });
      if (res.success && res.data) {
        const formatted = res.data.map((item: any) => ({
          label: item.postName,
          value: item.postId,
        }));
        setPositionList(formatted);
      }
    } catch (error) {
      console.error('获取岗位列表失败:', error);
    } finally {
      setPositionLoading(false);
    }
  };

  // 获取角色列表
  const fetchRoleList = async () => {
    setRoleLoading(true);
    try {
      const res = await getRoleList({ current: 1, pageSize: 1000 });
      if (res.success && res.data) {
        const formatted = res.data.map((item: any) => ({
          label: item.roleName,
          value: item.roleId,
        }));
        setRoleList(formatted);
      }
    } catch (error) {
      console.error('获取角色列表失败:', error);
    } finally {
      setRoleLoading(false);
    }
  };

  useEffect(() => {
    if (open) {
      fetchDeptTree();
      fetchPositionList();
      fetchRoleList();
    }
  }, [open]);

  // 新增用户时自动选择默认岗位/角色
  useEffect(() => {
    if (!open || currentRow || !formRef.current) return;
    const defaultPosition = positionList.find((item) => item.label === DEFAULT_POSITION_NAME);
    if (defaultPosition && !formRef.current.getFieldValue('postId')) {
      formRef.current.setFieldValue('postId', defaultPosition.value);
    }
  }, [open, currentRow, positionList]);

  useEffect(() => {
    if (!open || currentRow || !formRef.current) return;
    const currentRoles = formRef.current.getFieldValue('roleIds');
    const defaultRole = roleList.find((item) => item.label === DEFAULT_ROLE_NAME);
    if (defaultRole && (!Array.isArray(currentRoles) || currentRoles.length === 0)) {
      formRef.current.setFieldValue('roleIds', [defaultRole.value]);
    }
  }, [open, currentRow, roleList]);

  // 编辑时返填；切换为新增时重置
  useEffect(() => {
    if (!formRef.current) return;
    if (open && currentRow) {
      // 确保 roleIds 是数组格式
      const rawRoleIds: any = currentRow?.roleIds ?? (currentRow as any)?.roles ?? (currentRow as any)?.RoleIds;
      let roleIdsValue: number[] = [];
      if (rawRoleIds) {
        if (Array.isArray(rawRoleIds)) {
          roleIdsValue = rawRoleIds.map(id => Number(id)).filter(id => !Number.isNaN(id));
        } else if (typeof rawRoleIds === 'string') {
          roleIdsValue = rawRoleIds.split(/[,，\s]+/).filter(Boolean).map(id => Number(id)).filter(id => !Number.isNaN(id));
        } else if (typeof rawRoleIds === 'number') {
          roleIdsValue = [rawRoleIds];
        }
      }
      
      formRef.current.setFieldsValue({
        userName: currentRow.userName,
        nickName: currentRow.nickName,
        email: currentRow.email,
        phoneNumber: currentRow.phoneNumber,
        deptId: currentRow.deptId !== undefined && currentRow.deptId !== null ? String(currentRow.deptId) : undefined,
        postId:
          currentRow.postId !== undefined && currentRow.postId !== null
            ? Number(currentRow.postId)
            : (() => {
                const postIds = (currentRow as any)?.postIds ?? (currentRow as any)?.PostIds;
                if (Array.isArray(postIds) && postIds.length > 0) {
                  const parsed = Number(postIds[0]);
                  return Number.isNaN(parsed) ? undefined : parsed;
                }
                return undefined;
              })(),
        roleIds: roleIdsValue,
        status: currentRow.status === '1',
      });
      
      // 调试日志
      if (process.env.NODE_ENV === 'development') {
        console.log('编辑用户返填数据:', {
          currentRow,
          postId: currentRow.postId,
          roleIds: roleIdsValue,
        });
      }
    } else if (open && !currentRow) {
      formRef.current.resetFields();
    }
  }, [open, currentRow]);

  return (
    <Drawer
      title={currentRow ? '编辑用户' : '新增用户'}
      width={600}
      open={open}
      onClose={() => onOpenChange(false)}
      footer={null}
      className="user-edit-drawer"
      rootClassName="user-edit-drawer"
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
      <ProForm
        onFinish={onFinish}
        layout="vertical"
        formRef={formRef}
        grid={true}
        rowProps={{
          gutter: 16,
        }}
        submitter={{
          render: (props) => (
            <div style={{ width: '100%', display: 'flex', justifyContent: 'flex-end', marginTop: 24 }}>
              <Button style={{ marginRight: 8 }} onClick={() => onOpenChange(false)}>
                取消
              </Button>
              <Button type="primary" onClick={() => props.form?.submit?.()} loading={loading}>
                确定
              </Button>
            </div>
          ),
        }}
      >
        <ProFormText
          name="nickName"
          label="姓名"
          colProps={{ span: 12 }}
          placeholder="请输入姓名"
          rules={[{ required: true, message: '请输入姓名' }, { max: 20, message: '姓名最多20个字符' }]}
        />

        <ProFormTreeSelect
          name="deptId"
          label="归属部门"
          colProps={{ span: 12 }}
          rules={[{ required: true, message: '请选择归属部门' }]}
          fieldProps={{
            treeData: deptTree,
            loading: deptLoading,
            placeholder: '请选择归属部门',
            allowClear: true,
            treeDefaultExpandAll: true,
            showSearch: true,
            popupClassName: 'user-edit-drawer-dropdown',
            dropdownStyle: {
              background: '#ffffff',
              border: '1px solid #f0f0f0',
              boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
            }
          }}
        />

        <ProFormText
          name="phoneNumber"
          label="手机号码"
          colProps={{ span: 12 }}
          placeholder={"请输入手机号码"}
          rules={[
            { pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号码（11位数字，以1开头，第二位为3-9）' },
          ]}
          extra="如填写，手机号必须唯一，不能与其他用户重复"
        />

        <ProFormText
          name="email"
          label="邮箱"
          colProps={{ span: 12 }}
          placeholder={"请输入邮箱"}
          rules={[
            { type: 'email', message: '请输入正确的邮箱格式' },
          ]}
          extra="如填写，邮箱必须唯一，不能与其他用户重复"
        />

        <ProFormText
          name="userName"
          label="工号"
          colProps={{ span: 12 }}
          placeholder={"请输入工号"}
          rules={[
            { required: true, message: '请输入工号' }, 
            { max: 20, message: '工号最多20个字符' },
            { whitespace: true, message: '工号不能为空格' }
          ]}
          extra="工号必须唯一，不能与其他用户重复"
        />

        {!currentRow && (
          <ProFormText.Password
            name="password"
            label="用户密码"
            colProps={{ span: 12 }}
            placeholder={"请输入密码"}
            rules={[
              { required: true, message: '请输入密码' },
              { min: 6, message: '密码长度不能少于6个字符' },
              { max: 20, message: '密码长度不能超过20个字符' },
            ]}
          />
        )}

        <ProFormSelect
          name="postId"
          label="岗位"
          colProps={{ span: 12 }}
          placeholder="请选择岗位"
          options={positionList}
          fieldProps={{
            loading: positionLoading,
            allowClear: true,
            popupClassName: 'user-edit-drawer-dropdown',
            dropdownStyle: {
              background: '#ffffff',
              border: '1px solid #f0f0f0',
              boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
            }
          }}
        />

        <ProFormSelect
          name="roleIds"
          label="角色"
          colProps={{ span: 12 }}
          placeholder="请选择角色"
          options={roleList}
          fieldProps={{
            loading: roleLoading,
            allowClear: true,
            mode: 'multiple',
            popupClassName: 'user-edit-drawer-dropdown',
            dropdownStyle: {
              background: '#ffffff',
              border: '1px solid #f0f0f0',
              boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
            }
          }}
        />

        <ProFormSwitch
          name="status"
          label="状态"
          colProps={{ span: 12 }}
          checkedChildren="正常"
          unCheckedChildren="停用"
          initialValue={true}
        />

        <ProFormTextArea
          name="remark"
          label="备注"
          colProps={{ span: 12 }}
          placeholder="请输入内容"
          fieldProps={{
            rows: 4,
          }}
        />

      </ProForm>
    </Drawer>
  );
};

export default CreateUserForm;