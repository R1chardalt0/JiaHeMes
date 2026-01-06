import { request } from '@umijs/max';
import { BomItem, BomItemQueryDto, BomItemCreateDto, BomItemUpdateDto } from '@/services/Model/Infrastructure/Bom/BomItem';

/**
 * 分页查询BOM子项列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getBomItemList(
  params: BomItemQueryDto
): Promise<any> {
  const result = await request('/api/BomItem/GetBomItemList', {
    method: 'GET',
    params,
  });

  // 处理后端返回的数据格式
  // 如果返回的是数组，转换为分页格式
  if (Array.isArray(result)) {
    return {
      data: result,
      total: result.length,
      success: true
    };
  }

  // 如果已经是分页格式，直接返回
  return result;
}

/**
 * 根据ID获取BOM子项详情
 * @param id BOM子项ID
 * @returns BOM子项详情
 */
export async function getBomItemById(
  id: string
): Promise<BomItem> {
  return request('/api/BomItem/GetBomItemById', {
    method: 'GET',
    params: { id },
  });
}

/**
 * 根据BomId获取BOM子项详情
 * @param bomId BOM ID
 * @returns BOM子项列表
 */
export async function getBomItemsByBomId(
  bomId: string
): Promise<BomItem[]> {
  return request('/api/BomItem/GetBomItemsByBomId', {
    method: 'GET',
    params: { bomId },
  });
}

/**
 * 创建BOM子项
 * @param data 创建BOM子项DTO
 * @returns 创建成功的BOM子项信息
 */
export async function createBomItem(
  data: BomItemCreateDto
): Promise<BomItem> {
  return request('/api/BomItem/CreateBomItem', {
    method: 'POST',
    data,
  });
}

/**
 * 更新BOM子项
 * @param id BOM子项ID
 * @param data 更新BOM子项DTO
 * @returns 更新后的BOM子项信息
 */
export async function updateBomItem(
  id: string,
  data: BomItemUpdateDto
): Promise<BomItem> {
  return request('/api/BomItem/UpdateBomItemById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除BOM子项
 * @param ids BOM子项ID或ID数组
 * @returns 删除结果
 */
export async function deleteBomItem(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/BomItem/DeleteBomItemByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}
