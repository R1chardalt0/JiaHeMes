import { request } from '@umijs/max';

// 定义ProductionRecordsDto类型
export interface ProductionRecordsDto {
  productionLineName: string;
  resource: string;
  deviceName: string;
  totalCount: number;
  okCount: number;
  ngCount: number;
  yieldRate: number;
}

// 定义HourlyProductionRecordsDto类型
export interface HourlyProductionRecordsDto extends ProductionRecordsDto {
  hour: string; // 格式: yyyy-MM-dd HH:00
}

// 定义API响应类型
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  code?: string;
}

// 定义生产线类型
export interface ProductionLine {
  id: number;
  name: string;
}

/**
 * 获取生产记录数据
 * @param params 请求参数
 * @returns 生产记录列表
 */
export async function getProductionRecords(params: {
  productionLineName?: string;
  deviceName?: string;
  resource?: string;
  startTime?: Date;
  endTime?: Date;
}): Promise<ApiResponse<ProductionRecordsDto[]>> {
  const requestParams: Record<string, any> = {
    productionLineName: params.productionLineName,
    deviceName: params.deviceName,
    resource: params.resource,
  };

  if (params.startTime) {
    requestParams.startTime = params.startTime.toISOString();
  }
  if (params.endTime) {
    requestParams.endTime = params.endTime.toISOString();
  }

  const response = await request('/api/ProductTraceInfo/GetProductionRecords', {
    method: 'GET',
    params: requestParams,
  });
  
  // 对API返回的数据进行字段映射转换
  if (response.success && Array.isArray(response.data)) {
    response.data = response.data.map((item: any) => ({
      productionLineName: item.productionLineName,
      resource: item.resource,
      deviceName: item.deviceName,
      totalCount: item.totalProduction || 0,
      okCount: item.okNum || 0,
      ngCount: item.ngNum || 0,
      yieldRate: item.yield || 0
    }));
  }
  
  return response;
}

/**
 * 获取按小时统计的生产记录数据
 * @param params 请求参数
 * @returns 按小时统计的生产记录列表
 */
export async function getHourlyProductionRecords(params: {
  productionLineName?: string;
  deviceName?: string;
  resource?: string;
  startTime?: Date;
  endTime?: Date;
}): Promise<ApiResponse<HourlyProductionRecordsDto[]>> {
  const requestParams: Record<string, any> = {
    productionLineName: params.productionLineName,
    deviceName: params.deviceName,
    resource: params.resource,
  };

  if (params.startTime) {
    requestParams.startTime = params.startTime.toISOString();
  }
  if (params.endTime) {
    requestParams.endTime = params.endTime.toISOString();
  }

  const response = await request('/api/ProductTraceInfo/GetHourlyProductionRecords', {
    method: 'GET',
    params: requestParams,
  });
  
  // 对API返回的数据进行字段映射转换
  if (response.success && Array.isArray(response.data)) {
    response.data = response.data.map((item: any) => ({
      productionLineName: item.productionLineName,
      resource: item.resource,
      deviceName: item.deviceName,
      hour: item.hour,
      totalCount: item.totalProduction || 0,
      okCount: item.okNum || 0,
      ngCount: item.ngNum || 0,
      yieldRate: item.yield || 0
    }));
  }
  
  return response;
}

/**
 * 获取生产线列表
 * @returns 生产线列表
 */
export async function getProductionLines(): Promise<ApiResponse<{ list: ProductionLine[] }>> {
  return request('/api/ProductionLine/GetList', {
    method: 'GET',
  });
}
