
export interface ProductionRecordsDto {
  productionLineName: string;
  resource: string;
  deviceName: string;
  totalProduction: number;
  okNum: number;
  ngNum: number;
  yield: number; // 注意：后端字段是 "yield"，TS 一般用 camelCase
}

/**
 * 按小时统计的产量数据DTO
 */
export interface HourlyProductionRecordsDto {
  productionLineName: string;
  resource: string;
  deviceName: string;
  hour: string; // 格式：yyyy-MM-dd HH:00
  totalProduction: number;
  okNum: number;
  ngNum: number;
  yield: number;
}

// 请求参数类型
export interface GetProductionRecordsParams {
  productionLineName?: string;
  deviceName?: string;
  resource?: string;
  startTime: string; 
  endTime: string;
}

export interface ErrorField {
  name: string;
  errors: string[];
}

export interface ErrorInfo {
  code: string;
  message: string;
  errorFields?: ErrorField[];
}

export interface Resp<T> {
  success: boolean;
  data?: T;
  errorInfo?: ErrorInfo;
  errorKey: string;
  errorMessage: string;
}