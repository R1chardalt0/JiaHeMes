import { request } from '@umijs/max';
import { ProcessRouteItem, ProcessRouteItemQueryDto, ProcessRouteItemCreateDto, ProcessRouteItemUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItem';

/**
 * 分页查询工艺路线子项列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getProcessRouteItemList(
  params: ProcessRouteItemQueryDto
): Promise<any> {
  const result = await request('/api/ProcessRouteItem/GetProcessRouteItemList', {
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
 * 根据ID获取工艺路线子项详情
 * @param id 工艺路线子项ID
 * @returns 工艺路线子项详情
 */
export async function getProcessRouteItemById(
  id: string
): Promise<ProcessRouteItem> {
  return request('/api/ProcessRouteItem/GetProcessRouteItemById', {
    method: 'GET',
    params: { id },
  });
}

/**
 * 根据HeadId获取工艺路线子项详情
 * @param headId 工艺路线ID
 * @returns 工艺路线子项列表
 */
export async function getProcessRouteItemsByHeadId(
  headId: string
): Promise<ProcessRouteItem[]> {
  return request('/api/ProcessRouteItem/GetProcessRouteItemsByHeadId', {
    method: 'GET',
    params: { headId },
  });
}

/**
 * 创建工艺路线子项
 * @param data 创建工艺路线子项DTO
 * @returns 创建成功的工艺路线子项信息
 */
export async function createProcessRouteItem(
  data: ProcessRouteItemCreateDto
): Promise<ProcessRouteItem> {
  return request('/api/ProcessRouteItem/CreateProcessRouteItem', {
    method: 'POST',
    data,
  });
}

/**
 * 更新工艺路线子项
 * @param id 工艺路线子项ID
 * @param data 更新工艺路线子项DTO
 * @returns 更新后的工艺路线子项信息
 */
export async function updateProcessRouteItem(
  id: string,
  data: ProcessRouteItemUpdateDto
): Promise<ProcessRouteItem> {
  return request('/api/ProcessRouteItem/UpdateProcessRouteItemById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除工艺路线子项
 * @param ids 工艺路线子项ID或ID数组
 * @returns 删除结果
 */
export async function deleteProcessRouteItem(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/ProcessRouteItem/DeleteProcessRouteItemByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}