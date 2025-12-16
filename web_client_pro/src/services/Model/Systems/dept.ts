// SysDept 部门实体接口定义
export interface SysDept {
  createBy?: string;
  createTime?: string;
  updateBy?: string;
  updateTime?: string;
  remark?: string;
  deptId: number;
  parentId: number;
  deptName: string;
  orderNum?: number;
  leader?: string;
  phone?: string;
  email?: string;
  status?: string;
  delFlag?: string;
  parentName?: string;
  children?: SysDept[];
  additionalProp1?: string;
  additionalProp2?: string;
  additionalProp3?: string;
}

// 部门项类型定义
export interface DeptItem {
  deptId: number;
  parentId: number;
  deptName: string;
  orderNum?: number;
  leader?: string;
  phone?: string;
  email?: string;
  status?: string;
  delFlag?: string;
  parentName?: string;
  children?: DeptItem[];
  id?: number; // 保留 id 作为可选属性以保持兼容性
}

// 部门查询参数类型
export interface DeptQueryParams {
  deptName?: string;
  leader?: string;
  status?: string;
  pageSize?: number;
  current?: number;
}

// 分页响应结构
export interface PagedResult<T> {
  code: number;
  msg: string;
  data: {
    records: T[];
    total: number;
  };
}

// 树形部门响应
export interface DeptTreeResult {
  code: number;
  msg: string;
  data: DeptItem[];
}

// 错误字段信息
export interface ErrorField {
  name: string;
  errors: string[];
}

// 错误信息结构
export interface ErrorInfo {
  code: string;
  message: string;
  errorFields?: ErrorField[];
}

// 通用响应结构
export interface CommonResponse<T = any> {
  success: boolean;
  data?: T;
  errorInfo?: ErrorInfo;
  errorKey?: string;
  errorMessage?: string;
  total?: number;
  page?: number;
}