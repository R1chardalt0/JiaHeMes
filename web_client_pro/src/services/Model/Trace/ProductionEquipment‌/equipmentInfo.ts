export interface PagedResult<T> {
    code: number;
    msg: string;
    success: boolean; // 添加success属性
    data: T[];       // 直接返回数据数组而非嵌套对象
    total: number;   // 将total提升到根级别
}
export interface BaseResponse<T = any> {
  code: number;
  msg: string;
  data: T;
  success?: boolean; // 可选字段
}

// 设备信息实体
export interface DeviceInfo {
  deviceId?: string;
  deviceName: string;
  deviceEnCode: string;
  productionLineId?: string;
  productionLineName?: string;
  status?: string; // 从number改为string以匹配新接口
  description?: string;
  createTime?: string;
  updateTime?: string;
  createBy?: string; // 新增字段
  updateBy?: string; // 新增字段
  remark?: string; // 新增字段
  avatar?: string; // 新增字段
  devicePicture?: string; // 新增字段
  deviceType?: string; // 新增字段
  deviceManufacturer?: string; // 新增字段
  expireTime?: number; // 新增字段
  additionalProp1?: string; // 新增字段
  additionalProp2?: string; // 新增字段
  additionalProp3?: string; // 新增字段
}

// 设备查询参数
export interface DeviceInfoQueryParams {
  current?: number;
  pageSize?: number;
  deviceName?: string;
  deviceEnCode?: string;
  deviceType?: string;
  productionLineId?: string;
  status?: string;
  startTime?: string;
  endTime?: string;
}

// 设备表单提交数据
export interface DeviceInfoFormData {
  deviceName: string;
  deviceEnCode: string;
  productionLineId?: string;
  status?: string; // 保持为string
  description?: string;
  createTime?: string; // 新增字段
  updateTime?: string; // 新增字段
  createBy?: string; // 新增字段
  updateBy?: string; // 新增字段
  remark?: string; // 新增字段
  avatar: string; // 改为必需字段
  devicePicture: string; // 改为必需字段
  deviceType: string; // 改为必需字段
  deviceManufacturer: string; // 改为必需字段
  expireTime?: number; // 新增字段
}