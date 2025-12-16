import { request } from '@umijs/max';// 假设你使用 umi request
import { PagedResult, EqumentTraceinfoDto, EqumentTraceinfoQueryDto } from '@/services/Model/Trace/deviceOperation';

export async function getEqumentTraceinfoList(
  params: EqumentTraceinfoQueryDto
): Promise<PagedResult<EqumentTraceinfoDto>> {
  return request('/api/EqumentTraceinfo/GetEqumentTraceinfoList', {
    method: 'GET',
    params,
  });
}