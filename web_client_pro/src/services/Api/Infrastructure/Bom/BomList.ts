import { request } from '@umijs/max';
import { BomList, BomListDto, BomListQueryDto, BomListCreateDto, BomListUpdateDto } from '@/services/Model/Infrastructure/Bom/BomList';

/**
 * 分页查询BOM列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getBomList(
  params: BomListQueryDto
): Promise<any> {
  const result = await request('/api/BomList/GetBomListList', {
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

  return result;
}

/**
 * 根据ID获取BOM详情
 * @param id BOM ID
 * @returns BOM详情
 */
export async function getBomById(
  id: string
): Promise<BomList> {
  const result = await request('/api/BomList/GetBomListById', {
    method: 'GET',
    params: { id },
  });

  return result;
}

/**
 * 创建BOM
 * @param data 创建BOM DTO
 * @returns 创建成功的BOM信息
 */
export async function createBom(
  data: BomListCreateDto
): Promise<BomList> {
  return request('/api/BomList/CreateBomList', {
    method: 'POST',
    data,
  });
}

/**
 * 更新BOM
 * @param id BOM ID
 * @param data 更新BOM DTO
 * @returns 更新后的BOM信息
 */
export async function updateBom(
  id: string,
  data: BomListUpdateDto
): Promise<BomList> {
  return request('/api/BomList/UpdateBomListById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除BOM
 * @param ids BOM ID或ID数组
 * @returns 删除结果
 */
export async function deleteBom(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/BomList/DeleteBomListByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}
