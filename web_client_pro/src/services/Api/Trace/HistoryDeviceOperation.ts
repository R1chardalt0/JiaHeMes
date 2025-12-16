import { request } from '@umijs/max';
import { PagedResult, EqumentTraceinfoDto, EqumentTraceinfoQueryDto } from '@/services/Model/Trace/deviceOperation';

/**
 * 获取历史设备追溯信息列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getHistoryEqumentTraceinfoList(
  params: EqumentTraceinfoQueryDto
): Promise<PagedResult<EqumentTraceinfoDto>> {
  return request('/api/HistoryEqumentTraceinfo/GetHistoryEqumentTraceinfoList', {
    method: 'GET',
    params,
  });
}

