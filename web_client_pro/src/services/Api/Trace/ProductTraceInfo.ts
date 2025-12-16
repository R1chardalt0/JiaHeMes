import { request } from '@umijs/max';// 假设你使用 umi request
import { PagedResult, ProductTraceInfoDto, ProductTraceInfoQueryDto } from '@/services/Model/Trace/ProductTraceInfo';

export async function getProductTraceInfoList(
  params: ProductTraceInfoQueryDto
): Promise<PagedResult<ProductTraceInfoDto>> {
  return request('/api/ProductTraceInfo/GetProductTraceInfoList', {
    method: 'GET',
    params,
  });
}