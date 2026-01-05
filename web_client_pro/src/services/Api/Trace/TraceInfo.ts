import { request } from '@umijs/max';
import { PagedResult, TraceInfoDto, TraceInfoQueryDto, TraceInfoDetailDto, MaterialInfoDto, ProcessInfoDto } from '@/services/Model/Trace/TraceInfo';

// 获取TraceInfo列表
export async function getTraceInfoList(
  params: TraceInfoQueryDto
): Promise<PagedResult<TraceInfoDto>> {
  // 转换参数名以匹配后端API
  const apiParams = {
    current: params.current,
    pageSize: params.pageSize,
    id: params.id,
    productCode: params.productCode,
    pin: params.pin,
    vsn: params.vsn
  };

  return request('/api/TraceInfo/GetTraceInfoList', {
    method: 'GET',
    params: apiParams,
  });
}

// 获取TraceInfo详情
export async function getTraceInfoDetail(
  id: string
): Promise<TraceInfoDetailDto> {
  return request('/api/TraceInfo/GetTraceInfoById', {
    method: 'GET',
    params: { id },
  });
}

// 获取物料信息列表
export async function getMaterialInfoList(
  traceInfoId: string
): Promise<MaterialInfoDto[]> {
  return request('/api/TraceBomItem/GetTraceBomItemByTraceInfoId', {
    method: 'GET',
    params: { traceInfoId },
  });
}

// 获取过程信息列表
export async function getProcessInfoList(
  traceInfoId: string
): Promise<ProcessInfoDto[]> {
  return request('/api/TraceProcItem/GetTraceProcItemByTraceInfoId', {
    method: 'GET',
    params: { traceInfoId },
  });
}

// 删除物料信息（支持单个或批量删除）
export async function deleteMaterialInfo(
  ids: string | string[]
): Promise<{ deletedCount: number; message: string }> {
  // 确保ids始终是数组格式
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/TraceBomItem/DeleteTraceBomItemsById', {
    method: 'POST',
    data: idArray,
  });
}

// 删除过程信息（支持单个或批量删除）
export async function deleteProcessInfo(
  ids: string | string[]
): Promise<{ deletedCount: number; message: string }> {
  // 确保ids始终是数组格式
  const idArray = Array.isArray(ids) ? ids : [ids];
  return request('/api/TraceProcItem/DeleteTraceProcItemsById', {
    method: 'POST',
    data: idArray,
  });
}
