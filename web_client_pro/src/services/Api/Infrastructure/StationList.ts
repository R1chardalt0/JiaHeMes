import { request } from '@umijs/max';
import { StationListDto, StationListQueryDto } from '@/services/Model/Infrastructure/StationList';

/**
 * 分页查询站点列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getStationListList(
  params: StationListQueryDto
): Promise<any> {
  const result = await request('/api/StationList/GetStationListList', {
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
 * 根据ID获取站点详情
 * @param id 站点ID
 * @returns 站点详情
 */
export async function getStationListById(
  id: string
): Promise<StationListDto> {
  return request('/api/StationList/GetStationListById', {
    method: 'GET',
    params: { id },
  });
}

/**
 * 创建站点
 * @param data 创建站点DTO
 * @returns 创建成功的站点信息
 */
export async function createStationList(
  data: StationListDto
): Promise<StationListDto> {
  return request('/api/StationList/CreateStationList', {
    method: 'POST',
    data,
  });
}

/**
 * 更新站点
 * @param id 站点ID
 * @param data 更新站点DTO
 * @returns 更新后的站点信息
 */
export async function updateStationList(
  id: string,
  data: StationListDto
): Promise<StationListDto> {
  return request('/api/StationList/UpdateStationListById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除站点
 * @param ids 站点ID或ID数组
 * @returns 删除结果
 */
export async function deleteStationList(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/StationList/DeleteStationListByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}
