/**
 * 返工接口服务
 */
import { request } from '@umijs/max';
import { ReWorkParams, ReWorkResponse } from '@/services/Model/Production/reWork';

/**
 * 返工操作
 * @param params 返工参数
 * @returns 操作结果
 */
export async function reWork(params: ReWorkParams): Promise<ReWorkResponse> {
  const result = await request('/api/CommonInterfase/ReWork', {
    method: 'POST',
    data: params,
  });
  return result;
}