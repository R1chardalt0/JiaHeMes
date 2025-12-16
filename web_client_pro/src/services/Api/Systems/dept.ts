// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';
import { SysDept, DeptItem, PagedResult, DeptTreeResult, DeptQueryParams } from "@/services/Model/Systems/dept";

/** 分页查询部门 */
export async function getDeptPagination(params: any, options?: Record<string, any>) {
  return request<{
    total: number;
    page: number;
    success: boolean;
    data: DeptItem[];
  }>('/api/SysDept/GetDeptPagination', {
    method: 'GET',
    params: {
      current: params.current || 1,
      pageSize: params.pageSize || 50,
      deptName: params.deptName,
      status: params.status,
      ...params  
    },
    ...(options || {})
  });
}

/** 获取部门列表 */
export async function getDeptList(params?: DeptQueryParams, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: DeptItem[];
  }>('/api/SysDept/list', {
    method: 'GET',
    params: params || {},
    ...(options || {})
  });
}

/** 获取部门详情 */
export async function getDeptById(
  deptId: number,
  options?: Record<string, any>
) {
  return request<{
    code: number;
    msg: string;
    data: DeptItem[];
  }>(`/api/SysDept/${deptId}`, {
    method: 'GET',
    ...(options || {})
  });
}

/** 获取部门树 */
export async function getDeptTree(options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: any[]; // 这里可以根据实际部门树结构定义更具体的类型
    }>('/api/SysDept/GetDeptTree/tree', {
        method: 'GET',
        ...(options || {}),
    });
}

/** 根据用户ID获取部门树 */
export async function getSelectDeptTree(options?: Record<string, any>) {
  return request<DeptTreeResult>('/api/SysDept/selectTree', {
    method: 'GET',
    ...(options || {})
  });
}

/** 创建部门 */
export async function createDept(
  data: Omit<DeptItem, 'deptId'>,
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string; data: number }>('/api/SysDept/CreateDept', {
    method: 'POST',
    data,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}

/** 更新部门 */
export async function updateDept(
  data: DeptItem,
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string; data: number }>('/api/SysDept/UpdateDept', {
    method: 'POST',
    data,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}

/** 删除部门 */
export async function deleteDept(
  deptId: number,
  options?: Record<string, any>
) {
  return request<{ code: number; msg: string; data: number }>(
    `/api/SysDept/DeleteDept/${deptId}`,
    {
      method: 'POST',
      headers: {
        'accept': '*/*'
      },
      // 根据API文档curl命令，请求体为空字符串
      data: '',
      // 不设置Content-Type，让浏览器自动处理空请求体
      ...(options || {})
    }
  );
}