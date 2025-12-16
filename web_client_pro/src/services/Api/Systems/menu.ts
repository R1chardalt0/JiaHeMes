// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';
import { SysMenu, MenuItem, PagedResult, MenuTreeResult } from "@/services/Model/Systems/menu";


/** 分页查询菜单 */
export async function getMenuList(params: any, options?: Record<string, any>) {
  return request<{
    total: number;
    page: number;
    success: boolean;
    data: MenuItem[];
  }>('/api/SysMenu/GetMenuPagination', {
    method: 'GET',
    params: {
      current: params.current || 1,
      pageSize: params.pageSize || 10,
      ...params
    },
    ...(options || {})
  });
}


/** 获取菜单树 */
export async function getMenuTree(options?: Record<string, any>) {
  return request<MenuTreeResult>('/api/SysMenu/GetMenuTree/tree', {
    method: 'GET',
    params: { _v: Date.now() }, // 防缓存，确保每次取最新权限
    ...(options || {})
  });
}

/** 创建菜单 */
export async function createMenu(
  data: Omit<MenuItem, 'menuId'>,
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string; data: number }>('/api/SysMenu/CreateMenu', {
    method: 'POST',
    data,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}

/** 批量删除菜单 */
export async function batchDeleteMenu(
  ids: number[],
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string }>('/api/sysMenu/batchDelete', {
    method: 'POST',
    data: { ids },
    ...(options || {})
  });
}

/** 更新菜单 */
export async function updateMenu(
  data: MenuItem,
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string }>('/api/SysMenu/UpdateMenu', {
    method: 'POST',
    data,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}

/** 获取菜单详情 */
export async function getMenuDetail(
  id: number,
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string; data: MenuItem }>(
    `/api/sysMenu/${id}`,
    {
      method: 'GET',
      ...(options || {})
    }
  );
}
export async function deleteMenu(
  ids: number[], // 修改为数组类型
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string; data: {} }>(
    '/api/sysMenu/batch', // 使用批量删除接口
    {
      method: 'DELETE',
      data: { ids }, // 发送ID数组到请求体
      ...(options || {})
    }
  );
}

