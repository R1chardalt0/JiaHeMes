/**
 * MES工单BOM批次明细API调用文件
 */
import { request } from '@umijs/max';
import { MesOrderBomBatchItem, MESOrderBomBatchItemQueryDto } from '@/services/Model/Infrastructure/MESOrderBomBatch/MESOrderBomBatchItem';

/**
 * 根据ID查询工单BOM批次明细
 * @param id 工单BOM批次明细ID
 * @returns 工单BOM批次明细详情
 */
export async function getMESOrderBomBatchItemById(id: string): Promise<MesOrderBomBatchItem> {
  const result = await request('/api/trace/MESOrderBomBatchItem/GetMESOrderBomBatchItemById', {
    method: 'GET',
    params: { id },
  });
  return result;
}

/**
 * 查询工单BOM批次明细列表
 * @param params 查询参数
 * @returns 工单BOM批次明细列表
 */
export async function getMESOrderBomBatchItemList(params: Partial<MESOrderBomBatchItemQueryDto>): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatchItem/GetMESOrderBomBatchItemList', {
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
 * 分页查询工单BOM批次明细
 * @param params 查询参数
 * @returns 分页结果
 */
export async function getMESOrderBomBatchItemPagedList(params: MESOrderBomBatchItemQueryDto): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatchItem/GetMESOrderBomBatchItemPagedList', {
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
 * 根据工单BOM批次ID查询明细列表
 * @param orderBomBatchId 工单BOM批次ID
 * @returns 工单BOM批次明细列表
 */
export async function getMESOrderBomBatchItemByOrderBomBatchId(orderBomBatchId: string): Promise<any> {
  const result = await request('/api/trace/MESOrderBomBatchItem/GetMESOrderBomBatchItemByOrderBomBatchId', {
    method: 'GET',
    params: { orderBomBatchId },
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
 * 根据SN编码查询工单BOM批次明细
 * @param snNumber SN编码
 * @returns 工单BOM批次明细详情
 */
export async function getMESOrderBomBatchItemBySnNumber(snNumber: string): Promise<MesOrderBomBatchItem[]> {
  const result = await request('/api/trace/MESOrderBomBatchItem/GetMESOrderBomBatchItemBySnNumber', {
    method: 'GET',
    params: { snNumber },
  });
  return result;
}

