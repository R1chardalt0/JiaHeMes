import { request } from '@umijs/max';
import { PagedResult, ProductTraceInfoDto, ProductTraceInfoQueryDto } from '@/services/Model/Trace/ProductTraceInfo';

/**
 * 获取历史产品追溯信息列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getHistoryProductTraceInfoList(
  params: ProductTraceInfoQueryDto
): Promise<PagedResult<ProductTraceInfoDto>> {
  return request('/api/HistoryProductTraceInfo/GetHistoryProductTraceInfoList', {
    method: 'GET',
    params,
  });
}

