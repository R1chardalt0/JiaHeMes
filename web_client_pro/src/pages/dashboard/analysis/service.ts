import { request } from '@umijs/max';

// 定义API响应类型
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  code?: string;
}

// 定义每小时产出统计类型
export interface HourlyOutputDto {
  hour: number;
  outputQuantity: number;
  passQuantity: number;
  failQuantity: number;
  hourStartTime: string;
  hourEndTime: string;
}

// 定义OEE计算请求参数类型
export interface OeeCalculationRequestDto {
  startTime: string;
  endTime: string;
  productionLineId: string;
  resourceId: string;
  plannedProductionTime: number;
  theoreticalCycleTime: number;
  actualRunTime?: number;
  actualOutput?: number;
  goodOutput?: number;
}

// 定义OEE计算结果类型
export interface OeeCalculationResultDto {
  availability: number;
  performanceEfficiency: number;
  qualityRate: number;
  oeeValue: number;
  theoreticalOutput: number;
  actualOutput: number;
  goodOutput: number;
  plannedProductionTime: number;
  actualRunTime: number;
  downtime: number;
}

// 定义各站NG件统计类型
export interface StationNGStatisticsDto {
  stationId: string;
  stationCode: string;
  stationName: string;
  ngCount: number;
  totalTestCount: number;
  ngRate: number;
  defectDetails: DefectDetailDto[];
}

// 定义缺陷明细类型
export interface DefectDetailDto {
  defectType: string;
  defectCount: number;
}

// 定义一次通过率类型
export interface FirstPassYieldDto {
  startTime: string;
  endTime: string;
  totalInput: number;
  firstPassCount: number;
  firstPassYield: number;
}

// 定义合格率/不良率类型
export interface QualityRateDto {
  startTime: string;
  endTime: string;
  totalTestCount: number;
  passCount: number;
  failCount: number;
  passRate: number;
  failRate: number;
}

// 定义过程能力指数计算请求参数类型
export interface ProcessCapabilityRequestDto {
  startTime: string;
  endTime: string;
  stationId: string;
  parametricKey: string;
  upperSpecLimit: number;
  lowerSpecLimit: number;
  targetValue?: number;
}

// 定义过程能力指数类型
export interface ProcessCapabilityDto {
  stationId: string;
  stationCode: string;
  parametricKey: string;
  sampleSize: number;
  mean: number;
  standardDeviation: number;
  upperSpecLimit: number;
  lowerSpecLimit: number;
  cp: number;
  cpk: number;
  ca: number;
  cpu: number;
  cpl: number;
}

// 定义TOP缺陷分析类型
export interface TopDefectDto {
  defectType: string;
  defectCount: number;
  defectPercentage: number;
  cumulativePercentage: number;
}

// 定义完工产品数量统计类型
export interface FinishedProductCountDto {
  startTime: string;
  endTime: string;
  productName: string;
  totalFinishedCount: number;
  passFinishedCount: number;
  failFinishedCount: number;
}

/**
 * 获取每小时产出统计
 * @param productionLineId 生产线ID
 * @param workOrderId 工单ID
 * @param resourceId 设备ID
 * @param startTime 开始时间
 * @param endTime 结束时间
 * @returns 每小时产出统计数据
 */
export async function getHourlyOutput(productionLineId?: string, workOrderId?: string, resourceId?: string, startTime?: string, endTime?: string): Promise<ApiResponse<HourlyOutputDto[]>> {
  const params: Record<string, any> = {};

  if (productionLineId) {
    params.productionLineId = productionLineId;
  }

  if (workOrderId) {
    params.workOrderId = workOrderId;
  }

  if (resourceId) {
    params.resourceId = resourceId;
  }

  if (startTime) {
    params.startTime = startTime;
  }

  if (endTime) {
    params.endTime = endTime;
  }

  const response = await request('/api/Report/HourlyOutput', {
    method: 'GET',
    params,
  });

  return response;
}

/**
 * 计算OEE（设备综合效率）
 * @param requestParam OEE计算请求参数
 * @returns OEE计算结果
 */
export async function calculateOEE(requestParam: OeeCalculationRequestDto): Promise<ApiResponse<OeeCalculationResultDto>> {
  const response = await request('/api/Report/CalculateOEE', {
    method: 'POST',
    data: requestParam,
  });

  return response;
}

/**
 * 获取各站NG件统计
 * @param startTime 统计开始时间
 * @param endTime 统计结束时间
 * @param productionLineId 生产线ID（可选）
 * @returns 各站NG件统计数据
 */
export async function getStationNGStatistics(startTime: string, endTime: string, productionLineId?: string): Promise<ApiResponse<StationNGStatisticsDto[]>> {
  const params: Record<string, any> = {
    startTime,
    endTime,
  };

  if (productionLineId) {
    params.productionLineId = productionLineId;
  }

  const response = await request('/api/Report/StationNGStatistics', {
    method: 'GET',
    params,
  });

  return response;
}

/**
 * 计算一次通过率
 * @param startTime 统计开始时间
 * @param endTime 统计结束时间
 * @param workOrderId 工单ID（可选）
 * @returns 一次通过率计算结果
 */
export async function calculateFirstPassYield(startTime: string, endTime: string, workOrderId?: string): Promise<ApiResponse<FirstPassYieldDto>> {
  const params: Record<string, any> = {
    startTime,
    endTime,
  };

  if (workOrderId) {
    params.workOrderId = workOrderId;
  }

  const response = await request('/api/Report/FirstPassYield', {
    method: 'GET',
    params,
  });

  return response;
}

/**
 * 计算合格率/不良率
 * @param startTime 统计开始时间
 * @param endTime 统计结束时间
 * @param productionLineId 生产线ID（可选）
 * @returns 合格率/不良率计算结果
 */
export async function calculateQualityRate(startTime: string, endTime: string, productionLineId?: string): Promise<ApiResponse<QualityRateDto>> {
  const params: Record<string, any> = {
    startTime,
    endTime,
  };

  if (productionLineId) {
    params.productionLineId = productionLineId;
  }

  const response = await request('/api/Report/QualityRate', {
    method: 'GET',
    params,
  });

  return response;
}

/**
 * 计算过程能力指数
 * @param requestParam 过程能力指数计算请求参数
 * @returns 过程能力指数计算结果
 */
export async function calculateProcessCapability(requestParam: ProcessCapabilityRequestDto): Promise<ApiResponse<ProcessCapabilityDto>> {
  const response = await request('/api/Report/ProcessCapability', {
    method: 'POST',
    data: requestParam,
  });

  return response;
}

/**
 * 获取TOP缺陷分析
 * @param startTime 统计开始时间
 * @param endTime 统计结束时间
 * @param topN 返回前N个缺陷类型（默认10）
 * @param productionLineId 生产线ID（可选）
 * @returns TOP缺陷分析结果
 */
export async function getTopDefects(startTime: string, endTime: string, topN: number = 10, productionLineId?: string): Promise<ApiResponse<TopDefectDto[]>> {
  const params: Record<string, any> = {
    startTime,
    endTime,
    topN,
  };

  if (productionLineId) {
    params.productionLineId = productionLineId;
  }

  const response = await request('/api/Report/TopDefects', {
    method: 'GET',
    params,
  });

  return response;
}

/**
 * 获取指定时间范围内的完工产品数量统计
 * @param startTime 统计开始时间
 * @param endTime 统计结束时间
 * @param productionLineId 生产线ID（可选）
 * @returns 完工产品数量统计数据
 */
export async function getFinishedProductCount(startTime: string, endTime: string, productionLineId?: string): Promise<ApiResponse<FinishedProductCountDto[]>> {
  const params: Record<string, any> = {
    startTime,
    endTime,
  };

  if (productionLineId) {
    params.productionLineId = productionLineId;
  }

  const response = await request('/api/Report/FinishedProductCount', {
    method: 'GET',
    params,
  });

  return response;
}
