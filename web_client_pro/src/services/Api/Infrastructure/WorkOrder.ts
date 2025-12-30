import { request } from '@umijs/max';
import { WorkOrderDto, WorkOrderQueryDto, CreateWorkOrderDto, UpdateWorkOrderDto } from '@/services/Model/Infrastructure/WorkOrder';

/**
 * 分页查询工单列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getWorkOrderList(
  params: WorkOrderQueryDto
): Promise<any> {
  console.log('[API服务] getWorkOrderList 调用，参数:', params);
  console.log('[API服务] 请求URL: /api/WorkOrder');
  console.log('[API服务] 请求方法: GET');

  const result = await request('/api/WorkOrder', {
    method: 'GET',
    params,
  });

  console.log('[API服务] getWorkOrderList 原始响应结果:', result);

  // 处理后端返回的数据格式
  // 如果返回的是数组，转换为分页格式
  if (Array.isArray(result)) {
    console.log('[API服务] 检测到数组格式，转换为分页格式');
    const convertedResult = {
      data: result,
      total: result.length,
      success: true
    };
    console.log('[API服务] 转换后的结果:', convertedResult);
    return convertedResult;
  }

  // 如果已经是分页格式，直接返回
  console.log('[API服务] 检测到分页格式，直接返回');
  return result;
}

/**
 * 根据ID获取工单详情
 * @param id 工单ID
 * @returns 工单详情
 */
export async function getWorkOrderDetail(
  id: number
): Promise<WorkOrderDto> {
  return request(`/api/WorkOrder/${id}`, {
    method: 'GET',
  });
}

/**
 * 创建工单
 * @param data 创建工单DTO
 * @returns 创建成功的工单信息
 */
export async function createWorkOrder(
  data: CreateWorkOrderDto
): Promise<WorkOrderDto> {
  return request('/api/WorkOrder', {
    method: 'POST',
    data,
  });
}

/**
 * 更新工单
 * @param id 工单ID
 * @param data 更新工单DTO
 * @returns 更新后的工单信息
 */
export async function updateWorkOrder(
  id: number,
  data: UpdateWorkOrderDto
): Promise<WorkOrderDto> {
  return request(`/api/WorkOrder/${id}`, {
    method: 'PUT',
    data,
  });
}

/**
 * 删除工单
 * @param id 工单ID
 * @returns 删除结果
 */
export async function deleteWorkOrder(
  id: number
): Promise<any> {
  return request(`/api/WorkOrder/${id}`, {
    method: 'DELETE',
  });
}

/**
 * 批量删除工单
 * @param ids 工单ID数组
 * @returns 实际删除的工单数量
 */
export async function batchDeleteWorkOrder(
  ids: number[]
): Promise<any> {
  return request('/api/WorkOrder', {
    method: 'DELETE',
    data: ids,
  });
}
