// src/services/Trace/ProductionRecordsService.ts

import { request } from '@umijs/max';
import { ProductionRecordsDto, Resp, GetProductionRecordsParams, HourlyProductionRecordsDto } from '@/services/Model/Trace/ProductionRecords';



/**
 * 获取产量报表
 * @param params 查询参数
 * @returns Promise<Resp<ProductionRecordsDto[]>>
 */
export async function getProductionRecords(
  params: GetProductionRecordsParams
): Promise<Resp<ProductionRecordsDto[]>> {
  return request<Resp<ProductionRecordsDto[]>>('/api/ProductTraceInfo/GetProductionRecords', {
    method: 'GET',
    params,
  });
}

/**
 * 按小时获取产量报表
 * @param params 查询参数
 * @returns Promise<Resp<HourlyProductionRecordsDto[]>>
 */
export async function getHourlyProductionRecords(
  params: GetProductionRecordsParams
): Promise<Resp<HourlyProductionRecordsDto[]>> {
  return request<Resp<HourlyProductionRecordsDto[]>>('/api/ProductTraceInfo/GetHourlyProductionRecords', {
    method: 'GET',
    params,
  });
}