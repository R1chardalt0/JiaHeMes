export type Params = {
  name: string;
  id: string;
};

export type DeviceMonitorDataType = {
  id: string;
  encode: string;
  name: string;
  sendtime: string;
  createtime: string;
  avatar: string;
  status: 'normal' | 'exception' | 'active' | 'success';
  description: string;
};

/**
 * 设备参数类型
 */
export type Parameter = {
  name: string;
  type: number;
  value: string;
  unit: string;
};

/**
 * 设备追踪数据类型
 */
export type EquipmentTraceData = {
  equipmentTraceId: string;
  deviceEnCode: string;
  sendTime: string;
  alarmMessages: string;
  createTime: string;
  parameters: Parameter[];
};

/**
 * 错误字段类型
 */
export type ErrorField = {
  name: string;
  errors: string[];
};

/**
 * 错误信息类型
 */
export type ErrorInfo = {
  code: string;
  message: string;
  errorFields: ErrorField[];
};

/**
 * API响应类型
 */
export type ApiResponse<T = any> = {
  success: boolean;
  data: T;
  errorInfo: ErrorInfo;
  errorKey: string;
  errorMessage: string;
};

// 请求参数类型
export interface GetEquipmentTracinfosListParams {
  deviceEnCode: string;
  size: number;
}

export interface GetParamByDeviceEnCodeParams {
  deviceEnCode: string;
}

/**
 * 设备详细信息类型
 */
export type DeviceDetailType = {
  createBy: string;
  createTime: string;
  updateBy: string;
  updateTime: string;
  remark: string;
  deviceId: string;
  productionLineId: string;
  deviceName: string;
  deviceType: string;
  deviceEnCode: string;
  deviceManufacturer: string;
  description: string;
  status: string;
  expireTime: number;
  avatar: string;
  devicePicture: string;
  additionalProp1?: string;
  additionalProp2?: string;
  additionalProp3?: string;
};

/**
 * 获取设备详情的请求参数类型
 */
export interface GetDeviceDetailParams {
  deviceEnCode: string;
}

