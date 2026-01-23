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
  const result = await request('/api/WorkOrder/GetWorkOrderList', {
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
 * 根据ID获取工单详情
 * @param id 工单ID
 * @returns 工单详情
 */
export async function getWorkOrderDetail(
  id: number
): Promise<WorkOrderDto> {
  return request('/api/WorkOrder/GetWorkOrderById', {
    method: 'GET',
    params: { id },
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
  return request('/api/WorkOrder/CreateWorkOrder', {
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
  return request('/api/WorkOrder/UpdateWorkOrderById', {
    method: 'POST',
    params: { id },
    data,
  });
}

/**
 * 删除工单（支持单个删除和批量删除）
 * @param ids 工单ID数组
 * @returns 删除结果
 */
export async function deleteWorkOrder(
  ids: number[]
): Promise<any> {
  return request('/api/WorkOrder/DeleteWorkOrderByIds', {
    method: 'POST',
    data: ids,
  });
}


