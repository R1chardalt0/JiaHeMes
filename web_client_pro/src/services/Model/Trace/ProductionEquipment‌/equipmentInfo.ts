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
  resourceId?: string; // 对应后端 ResourceId
  resourceName: string; // 对应后端 ResourceName
  resource: string; // 对应后端 Resource (设备编码)
  productionLineId?: string;
  productionLineName?: string;
  status?: string;
  description?: string;
  createTime?: string;
  updateTime?: string;
  workOrderCode?: string;
  createBy?: string;
  updateBy?: string;
  remark?: string;
  avatar?: string | null; // 对应后端 Avatar
  resourcePicture?: string | null; // 对应后端 ResourcePicture
  resourceType?: string; // 对应后端 ResourceType
  resourceManufacturer?: string; // 对应后端 ResourceManufacturer
  expireTime?: number; // 对应后端 ExpireTime
  searchValue?: string; // 对应后端 SearchValue
  additionalProp1?: string;
  additionalProp2?: string;
  additionalProp3?: string;
}

// 设备查询参数
export interface DeviceInfoQueryParams {
  current?: number;
  pageSize?: number;
  resourceName?: string;
  resource?: string;
  resourceType?: string;
  productionLineId?: string;
  workOrderCode?: string;
  status?: string;
  startTime?: string;
  endTime?: string;
}

// 设备表单提交数据
export interface DeviceInfoFormData {
  resourceName: string;
  resource: string;
  productionLineId?: string;
  status?: string;
  description?: string;
  createTime?: string;
  updateTime?: string;
  createBy?: string;
  updateBy?: string;
  remark?: string;
  avatar?: string | null;
  resourcePicture?: string | null;
  resourceType: string;
  resourceManufacturer: string;
  expireTime?: number;
  workOrderCode?: string;
}