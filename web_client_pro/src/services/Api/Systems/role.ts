import { request } from '@umijs/max';
import { RoleItem, AllocateRoleMenusDto ,PagedResult,BaseResponse} from '@/services/Model/Systems/role';

/** 获取角色列表 */
export async function getRoleList(
  // 将current和pageSize改为可选参数并提供默认值
  params: {
    current?: number;
    pageSize?: number;
    roleName?: string;
    roleKey?: string;
    status?: string;
    startTime?: string;
    endTime?: string;
  } = { current: 1, pageSize: 10 },
  options?: Record<string, any>
) {
  // 直接使用参数默认值，无需额外解构赋值
  return request<PagedResult<RoleItem>>('/api/SysRole/GetRoleList/list', {
    method: 'GET',
    params: {
      current: params.current,
      pageSize: params.pageSize,
      roleName: params.roleName,
      roleKey: params.roleKey,
      status: params.status,
      startTime: params.startTime,
      endTime: params.endTime
    },
    ...(options || {})
  });
}

/** 获取角色详情 */
export async function getRoleDetail(id: number, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: RoleItem
  }>(`/api/SysRole/GetRoleById/${id}`, {
    method: 'GET',
    ...(options || {})
  });
}

/** 创建角色 */
// 创建角色（自动处理类型转换）
export async function createRole(data: Omit<RoleItem, 'roleId'>) {
  const payload = {
    ...data,
    status: data.status ?? '0', // 默认启用状态
    roleSort: data.roleSort || '0'
  };
  
  return request<BaseResponse<number>>('/api/SysRole/CreateRole', {
    method: 'POST',
    data: payload,
    headers: {
      'Content-Type': 'application/json'
    }
  });
}

export interface CreateRoleFormValues extends Omit<RoleItem, 'roleId'> {
  createBy?: string;
  updateBy?: string;
  // 其他可选字段...
}

/** 更新角色 */
export async function updateRole(data: RoleItem) {
  // 确保类型匹配后端要求
  const payload = {
    ...data,
    status: String(data.status), // 显式类型转换
    roleSort: data.roleSort || '0' // 默认值处理
  };
  
  return request<BaseResponse<number>>('/api/SysRole/UpdateRole', {
    method: 'POST',
    data: payload,
    headers: {
      'Content-Type': 'application/json'
    }
  });
}

/** 删除角色 */
export async function deleteRole(roleIds: number[], options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: number
  }>('/api/SysRole/DeleteRoleByIds', {
    method: 'POST',
    data: roleIds,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}

/** 获取角色菜单权限 */
export async function getRoleMenuIds(roleId: number, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: number[]
  }>(`/api/SysRole/menu/${roleId}`, {
    method: 'GET',
    ...(options || {})
  });
}

/** 分配角色菜单权限 */
export async function allocateRoleMenus(dto: AllocateRoleMenusDto, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: boolean
  }>('/api/SysRole/allocateRoleMenus', {
    method: 'POST',
    data: dto,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}