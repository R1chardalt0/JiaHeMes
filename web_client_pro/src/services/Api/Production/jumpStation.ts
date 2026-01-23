/**
 * 跳站接口服务
 */
import { request } from '@umijs/max';
import { JumpStationParams, JumpStationResponse } from '@/services/Model/Production/jumpStation';

/**
 * 跳站操作
 * @param params 跳站参数
 * @returns 操作结果
 */
export async function jumpStation(params: JumpStationParams): Promise<JumpStationResponse> {
  const result = await request('/api/CommonInterfase/JumpStation', {
    method: 'POST',
    data: params,
  });
  return result;
}