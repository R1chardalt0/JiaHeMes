import { request } from '@umijs/max';
import { ProcessRouteItemTest, ProcessRouteItemTestQueryDto, ProcessRouteItemTestCreateDto, ProcessRouteItemTestUpdateDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItemTest';

/**
 * 分页查询工艺路线工位测试列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getProcessRouteItemTestList(
  params: ProcessRouteItemTestQueryDto
): Promise<any> {
  const result = await request('/api/ProcessRouteItemTest/GetProcessRouteItemTestList', {
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
 * 根据ID获取工艺路线工位测试详情
 * @param id 工艺路线工位测试ID
 * @returns 工艺路线工位测试详情
 */
export async function getProcessRouteItemTestById(
  id: string
): Promise<ProcessRouteItemTest> {
  return request('/api/ProcessRouteItemTest/GetProcessRouteItemTestById', {
    method: 'GET',
    params: { id },
  });
}

/**
 * 根据ProcessRouteItemId获取工艺路线工位测试详情
 * @param processRouteItemId 工艺路线明细ID
 * @returns 工艺路线工位测试列表
 */
export async function getProcessRouteItemTestByProcessRouteItemId(
  processRouteItemId: string
): Promise<ProcessRouteItemTest[]> {
  return request('/api/ProcessRouteItemTest/GetProcessRouteItemTestByProcessRouteItemId', {
    method: 'GET',
    params: { processRouteItemId },
  });
}

/**
 * 创建工艺路线工位测试
 * @param data 创建工艺路线工位测试DTO
 * @returns 创建成功的工艺路线工位测试信息
 */
export async function createProcessRouteItemTest(
  data: ProcessRouteItemTestCreateDto
): Promise<ProcessRouteItemTest> {
  return request('/api/ProcessRouteItemTest/CreateProcessRouteItemTest', {
    method: 'POST',
    data,
  });
}

/**
 * 更新工艺路线工位测试
 * @param id 工艺路线工位测试ID
 * @param data 更新工艺路线工位测试DTO
 * @returns 更新后的工艺路线工位测试信息
 */
export async function updateProcessRouteItemTest(
  id: string,
  data: ProcessRouteItemTestUpdateDto
): Promise<ProcessRouteItemTest> {
  return request('/api/ProcessRouteItemTest/UpdateProcessRouteItemTestById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除工艺路线工位测试
 * @param ids 工艺路线工位测试ID或ID数组
 * @returns 删除结果
 */
export async function deleteProcessRouteItemTest(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/ProcessRouteItemTest/DeleteProcessRouteItemTestByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}