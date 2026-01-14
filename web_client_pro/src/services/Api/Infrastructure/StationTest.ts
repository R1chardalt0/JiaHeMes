import { request } from '@umijs/max';
import {
  StationTestProjectDto,
  StationTestProjectCreateDto,
  StationTestProjectUpdateDto,
  StationTestProjectQueryDto,
  PaginatedList
} from '@/services/Model/Infrastructure/StationTest';

/**
 * 分页查询站点测试项列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getStationTestProjectList(
  params: StationTestProjectQueryDto
): Promise<PaginatedList<StationTestProjectDto>> {
  const result = await request('/api/StationTestProject/GetStationTestProjectList', {
    method: 'GET',
    params,
  });

  // 处理后端返回的数据格式
  // 如果返回的是数组，转换为分页格式
  if (Array.isArray(result)) {
    return {
      total: result.length,
      items: result
    };
  }

  // 如果已经是分页格式，直接返回
  return result;
}

/**
 * 根据ID获取站点测试项详情
 * @param id 站点测试项ID
 * @returns 站点测试项详情
 */
export async function getStationTestProjectById(
  id: string
): Promise<StationTestProjectDto> {
  return request('/api/StationTestProject/GetStationTestProjectById', {
    method: 'GET',
    params: { id },
  });
}

/**
 * 根据站点ID获取测试项列表
 * @param stationId 站点ID
 * @returns 测试项列表
 */
export async function getStationTestProjectByStationId(
  stationId: string
): Promise<StationTestProjectDto[]> {
  return request('/api/StationTestProject/GetStationTestProjectByStationId', {
    method: 'GET',
    params: { stationId },
  });
}

/**
 * 创建站点测试项
 * @param data 创建站点测试项DTO
 * @returns 创建成功的站点测试项信息
 */
export async function createStationTestProject(
  data: StationTestProjectCreateDto
): Promise<StationTestProjectDto> {
  return request('/api/StationTestProject/CreateStationTestProject', {
    method: 'POST',
    data,
  });
}

/**
 * 更新站点测试项
 * @param id 站点测试项ID
 * @param data 更新站点测试项DTO
 * @returns 更新后的站点测试项信息
 */
export async function updateStationTestProject(
  id: string,
  data: StationTestProjectUpdateDto
): Promise<StationTestProjectDto> {
  return request('/api/StationTestProject/UpdateStationTestProjectById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除站点测试项（支持单个和批量删除）
 * @param ids 站点测试项ID或ID数组
 * @returns 删除结果
 */
export async function deleteStationTestProject(
  ids: string | string[]
): Promise<{ deletedCount: number; message: string }> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/StationTestProject/DeleteStationTestProjectByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}
