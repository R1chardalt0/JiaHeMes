import { request } from '@umijs/max';
import { ProcessRoute, ProcessRouteDto, ProcessRouteQueryDto, ProcessRouteCreateDto, ProcessRouteUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRoute';

/**
 * 分页查询工艺路线列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getProcessRouteList(
  params: ProcessRouteQueryDto
): Promise<any> {
  const result = await request('/api/ProcessRoute/GetProcessRouteList', {
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
 * 根据ID获取工艺路线详情
 * @param id 工艺路线ID
 * @returns 工艺路线详情
 */
export async function getProcessRouteById(
  id: string
): Promise<ProcessRoute> {
  const result = await request('/api/ProcessRoute/GetProcessRouteById', {
    method: 'GET',
    params: { id },
  });

  return result;
}

/**
 * 创建工艺路线
 * @param data 创建工艺路线DTO
 * @returns 创建成功的工艺路线信息
 */
export async function createProcessRoute(
  data: ProcessRouteCreateDto
): Promise<ProcessRoute> {
  return request('/api/ProcessRoute/CreateProcessRoute', {
    method: 'POST',
    data,
  });
}

/**
 * 更新工艺路线
 * @param id 工艺路线ID
 * @param data 更新工艺路线DTO
 * @returns 更新后的工艺路线信息
 */
export async function updateProcessRoute(
  id: string,
  data: ProcessRouteUpdateDto
): Promise<ProcessRoute> {
  return request('/api/ProcessRoute/UpdateProcessRouteById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除工艺路线
 * @param ids 工艺路线ID或ID数组
 * @returns 删除结果
 */
export async function deleteProcessRoute(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/ProcessRoute/DeleteProcessRouteByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}