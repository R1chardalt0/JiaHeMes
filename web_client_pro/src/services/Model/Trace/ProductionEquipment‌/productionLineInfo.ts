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

export interface productionLine {
  productionLineId?: string;
  productionLineName: string;
  productionLineCode: string;
  description?: string;
  status: string;
  createdAt?: string;
  updatedAt?: string;
  companyName?: string;
}

// 产线查询参数接口
export interface ProductionLineQueryParams {
  current?: number;
  pageSize?: number;
  productionLineName?: string;
  productionLineCode?: string;
  status?: number;
  startTime?: string;
  endTime?: string;
}

// 产线表单提交数据接口
export interface ProductionLineFormData {
  productionLineName: string;
  productionLineCode: string;
  remark?: string;
  status: string; // 表单中使用字符串，提交时转换为数字
}

