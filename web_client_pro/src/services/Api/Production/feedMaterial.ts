/**
 * 物料上传接口服务
 */
import { request } from '@umijs/max';
import { RequestFeedMaterialParams, FeedMaterialResponse } from '@/services/Model/Production/feedMaterial';

/**
 * 物料上传
 * @param params 物料上传参数
 * @returns 操作结果
 */
export async function feedMaterial(params: RequestFeedMaterialParams): Promise<FeedMaterialResponse> {
  const result = await request('/api/CommonInterfase/FeedMaterial', {
    method: 'POST',
    data: params,
  });
  return result;
}
