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
/**
 * 删除菜单
 * 后端删除接口：/api/SysMenu/DeleteMenu/{id}
 * 说明：后端为单个删除接口，这里前端支持传入数组 ids，内部逐个调用并汇总结果。
 */
export async function deleteMenu(
  ids: number[],
  options?: Record<string, any>,
) {
  const results = await Promise.all(
    ids.map((id) =>
      request<{ code: number; msg: string; data?: any }>(
        `/api/SysMenu/DeleteMenu/${id}`,
        {
          method: 'Post',
          ...(options || {}),
        },
        
      ),
      
    ),
  );

  const failed = results.find((r) => (r as any)?.code !== 200);
  if (failed) {
    return failed as any;
  }

  return { code: 200, msg: 'ok', data: {} } as any;
}


