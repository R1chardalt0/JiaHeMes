// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';
import {
  CompanyItem,
  CompanyPagedResp,
  CompanyQueryParams,
  SysCompany,
} from '@/services/Model/Systems/company';

/** 分页查询公司 */
export async function getCompanyPagination(
  params: CompanyQueryParams = { current: 1, pageSize: 10 },
  options?: Record<string, any>,
) {
  return request<CompanyPagedResp>('/api/SysCompany/GetCompanyPagination', {
    method: 'GET',
    params: {
      current: params.current || 1,
      pageSize: params.pageSize || 10,
      companyName: params.companyName,
      ...params,
    },
    ...(options || {}),
  });
}

/** 获取公司详情 */
export async function getCompanyById(
  companyId: number,
  options?: Record<string, any>,
) {
  return request<{
    code: number;
    msg: string;
    data?: CompanyItem;
  }>(`/api/SysCompany/${companyId}`, {
    method: 'GET',
    ...(options || {}),
  });
}

/** 新增公司 */
export async function createCompany(
  data: Pick<SysCompany, 'companyName' | 'remark'>,
  options?: Record<string, any>,
) {
  return request<{ code: number; msg: string; data?: number }>(
    '/api/SysCompany/CreateCompany',
    {
      method: 'POST',
      data,
      headers: {
        'Content-Type': 'application/json',
      },
      ...(options || {}),
    },
  );
}

/** 更新公司 */
export async function updateCompany(
  data: Partial<SysCompany> & { companyId: number },
  options?: Record<string, any>,
) {
  return request<{ code: number; msg: string; data?: number }>(
    '/api/SysCompany/UpdateCompany',
    {
      method: 'POST',
      data,
      headers: {
        'Content-Type': 'application/json',
      },
      ...(options || {}),
    },
  );
}

/** 删除公司 */
export async function deleteCompany(
  companyId: number,
  options?: Record<string, any>,
) {
  return request<{ code: number; msg: string; data?: number }>(
    `/api/SysCompany/DeleteCompany/${companyId}`,
    {
      method: 'POST',
      ...(options || {}),
    },
  );
}

/** 获取所有公司列表（用于下拉选择） */
export async function getAllCompanies(
  options?: Record<string, any>,
) {
  // 使用分页查询，设置较大的pageSize来获取所有公司
  return request<CompanyPagedResp>(
    '/api/SysCompany/GetCompanyPagination',
    {
      method: 'GET',
      params: {
        current: 1,
        pageSize: 1000, // 设置较大的pageSize以获取所有公司
      },
      ...(options || {}),
    },
  );
}

