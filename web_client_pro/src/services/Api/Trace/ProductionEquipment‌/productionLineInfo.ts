import { request } from '@umijs/max';
import type { productionLine, ProductionLineQueryParams } from '@/services/Model/Trace/ProductionEquipment‌/productionLineInfo';

/**
 * 获取生产线列表
 * @param params 查询参数
 * @returns 生产线列表
 */
export async function getProductionLineList(params: ProductionLineQueryParams): Promise<{ data: productionLine[]; success?: boolean; total?: number }> {
  return request('/api/ProductionLine/GetProductionLineList', {
    method: 'GET',
    params,
  });
}

/**
 * 根据ID获取生产线详情
 * @param id 生产线ID
 * @returns 生产线详情
 */
export async function getProductionLineById(id: string): Promise<{ data: productionLine }> {
  return request(`/api/ProductionLine/GetProductionLineById/${id}`, {
    method: 'GET',
  });
}

/**
 * 创建生产线
 * @param data 生产线数据
 * @returns 创建结果
 */
export async function createProductionLine(data: productionLine): Promise<{ success: boolean; message?: string }> {
  return request('/api/ProductionLine/CreateProductionLine', {
    method: 'POST',
    data, // 包装在productionLine字段中
  });
}

/**
 * 更新生产线
 * @param data 生产线数据
 * @returns 更新结果
 */
export async function updateProductionLine(data: productionLine): Promise<{ success: boolean; message?: string }> {
  return request('/api/ProductionLine/UpdateProductionLine', {
    method: 'POST',
    data,
  });
}

/**
 * 批量删除生产线
 * @param ids 生产线ID列表
 * @returns 删除结果
 */
export async function deleteProductionLineByIds(ids: string[]): Promise<{ success: boolean; message?: string }> {
  return request('/api/ProductionLine/DeleteProductionLineByIds', {
    method: 'POST',
    data: ids, // 直接传递ID数组，不包装在对象中
  });
}