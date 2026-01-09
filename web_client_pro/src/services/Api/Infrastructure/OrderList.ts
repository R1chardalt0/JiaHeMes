import { request } from '@umijs/max';
import { OrderList, OrderListDto, OrderListQueryDto, OrderListCreateDto, OrderListUpdateDto } from '@/services/Model/Infrastructure/OrderList';

/**
 * 分页查询工单列表
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getOrderList(
  params: OrderListQueryDto
): Promise<any> {
  const result = await request('/api/OrderList/GetOrderListList', {
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
 * 根据ID获取工单详情
 * @param orderListId 工单ID
 * @returns 工单详情
 */
export async function getOrderById(
  orderListId: string
): Promise<OrderList> {
  const result = await request('/api/OrderList/GetOrderListById', {
    method: 'GET',
    params: { orderListId },
  });

  return result;
}

/**
 * 根据工单编码获取工单详情
 * @param orderCode 工单编码
 * @returns 工单详情
 */
export async function getOrderByCode(
  orderCode: string
): Promise<OrderList> {
  const result = await request('/api/OrderList/GetOrderListByCode', {
    method: 'GET',
    params: { orderCode },
  });

  return result;
}

/**
 * 创建工单
 * @param data 创建工单DTO
 * @returns 创建成功的工单信息
 */
export async function createOrder(
  data: OrderListCreateDto
): Promise<OrderList> {
  return request('/api/OrderList/CreateOrderList', {
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
export async function updateOrder(
  id: string,
  data: OrderListUpdateDto
): Promise<OrderList> {
  return request('/api/OrderList/UpdateOrderListById', {
    method: 'POST',
    params: { orderListId: id },
    data,
  });
}

/**
 * 删除工单
 * @param ids 工单ID或ID数组
 * @returns 删除结果
 */
export async function deleteOrder(
  ids: string | string[]
): Promise<any> {
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/OrderList/DeleteOrderListByIds', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: idArray,
  });
}

/**
 * 更新工单状态
 * @param orderListId 工单ID
 * @param orderStatus 工单状态
 * @returns 更新后的工单信息
 */
export async function updateOrderStatus(
  orderListId: string,
  orderStatus: number
): Promise<OrderList> {
  return request('/api/OrderList/UpdateOrderStatus', {
    method: 'POST',
    params: { orderListId, orderStatus },
  });
}

/**
 * 获取工单统计信息
 * @returns 工单状态统计
 */
export async function getOrderStatistics(): Promise<Record<string, number>> {
  const result = await request('/api/OrderList/GetOrderStatistics', {
    method: 'GET',
  });

  return result;
}
