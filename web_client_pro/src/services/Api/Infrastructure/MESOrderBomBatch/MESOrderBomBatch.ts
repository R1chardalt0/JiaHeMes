/**
 * MES工单BOM批次API调用文件
 */
import { request } from '@umijs/max';
import { MESOrderBomBatch, MESOrderBomBatchQueryDto } from '@/services/Model/Infrastructure/MESOrderBomBatch/MESOrderBomBatch';

/**
 * 根据ID查询工单BOM批次
 * @param id 工单BOM批次ID
 * @returns 工单BOM批次详情
 */
export async function getMESOrderBomBatchById(id: string): Promise<MESOrderBomBatch> {
  const result = await request('/api/trace/MESOrderBomBatch/GetMESOrderBomBatchById', {
    method: 'GET',
    params: { id },
  });
  return result;
}

/**
 * 查询工单BOM批次列表
 * @param params 查询参数
 * @returns 工单BOM批次列表
 */
export async function getMESOrderBomBatchList(params: Partial<MESOrderBomBatchQueryDto>): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatch/GetMESOrderBomBatchList', {
    method: 'GET',
    params,
  });

  // 处理后端返回的数据格式
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
 * 分页查询工单BOM批次
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getMESOrderBomBatchPagedList(params: MESOrderBomBatchQueryDto): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatch/GetMESOrderBomBatchPagedList', {
    method: 'GET',
    params,
  });

  // 处理后端返回的数据格式
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
 * 根据工单ID查询工单BOM批次列表
 * @param orderListId 工单ID
 * @returns 工单BOM批次列表
 */
export async function getMESOrderBomBatchByOrderListId(orderListId: string): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatch/GetMESOrderBomBatchByOrderListId', {
    method: 'GET',
    params: { orderListId },
  });

  // 处理后端返回的数据格式
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
 * 根据物料ID查询工单BOM批次列表
 * @param productListId 物料ID
 * @returns 工单BOM批次列表
 */
export async function getMESOrderBomBatchByProductListId(productListId: string): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatch/GetMESOrderBomBatchByProductListId', {
    method: 'GET',
    params: { productListId },
  });

  // 处理后端返回的数据格式
  if (Array.isArray(result)) {
    return {
      data: result,
      total: result.length,
      success: true
    };
  }

  return result;
}
