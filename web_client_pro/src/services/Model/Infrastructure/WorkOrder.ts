/**
 * 工单管理相关类型定义
 */

/**
 * 分页响应结果
 */
export interface PagedResult<T> {
  total: number;
  page: number;
  success: boolean;
  data: T[];
  errorInfo: any | null;
  errorKey: string;
  errorMessage: string;
}

/**
 * 工单信息DTO
 */
export interface WorkOrderDto {
  /** 工单ID */
  id?: number;
  /** 工单编号 */
  code?: string;
  /** 产品编码 */
  productCode?: string;
  /** BOM配方ID */
  bomRecipeId?: string;
  /** 是否无限生产 */
  isInfinite?: boolean;
  /** 生产数量 */
  workOrderAmount?: number;
  /** 每个追踪信息的完成增量 */
  perTraceInfo?: number;
  /** 工单状态：0-草稿，1-已提交，2-已拒绝，3-已通过 */
  docStatus?: number;
  /** 创建人 */
  createdBy?: string;
  /** 创建时间 */
  createTime?: string;
  /** 更新人 */
  updatedBy?: string;
  /** 更新时间 */
  updateTime?: string;
}

/**
 * 工单查询DTO
 */
export interface WorkOrderQueryDto {
  /** 当前页码 */
  current?: number;
  /** 每页条数 */
  pageSize?: number;
  /** 排序字段 */
  sortField?: string;
  /** 排序方式 */
  sortOrder?: 'ascend' | 'descend';
  /** 工单编号 */
  code?: string;
  /** 产品编码 */
  productCode?: string;
  /** BOM配方ID */
  bomRecipeId?: string;
  /** 工单状态 */
  docStatus?: number;
}

/**
 * 创建工单DTO
 */
export interface CreateWorkOrderDto {
  /** 工单编号 */
  code: string;
  /** 产品编码 */
  productCode: string;
  /** BOM配方ID */
  bomRecipeId: string;
  /** 是否无限生产 */
  isInfinite: boolean;
  /** 生产数量 */
  workOrderAmount?: number;
  /** 每个追踪信息的完成增量 */
  perTraceInfo?: number;
  /** 工单状态：0-草稿，1-已提交，2-已拒绝，3-已通过 */
  docStatus?: number;
}

/**
 * 更新工单DTO
 */
export interface UpdateWorkOrderDto {
  /** 工单ID */
  id: number;
  /** 工单编号 */
  code?: string;
  /** 产品编码 */
  productCode?: string;
  /** BOM配方ID */
  bomRecipeId?: string;
  /** 是否无限生产 */
  isInfinite?: boolean;
  /** 生产数量 */
  workOrderAmount?: number;
  /** 每个追踪信息的完成增量 */
  perTraceInfo?: number;
  /** 工单状态 */
  docStatus?: number;
}

/**
 * 工单状态枚举
 */
export enum WorkOrderDocStatus {
  Drafting = 0,
  Commited = 1,
  Rejected = 2,
  Approved = 3,
}

/**
 * 工单状态文本映射
 */
export const WorkOrderStatusText: Record<number, string> = {
  [WorkOrderDocStatus.Drafting]: '草稿',
  [WorkOrderDocStatus.Commited]: '已提交',
  [WorkOrderDocStatus.Rejected]: '已拒绝',
  [WorkOrderDocStatus.Approved]: '已通过',
};

/**
 * 工单状态颜色映射
 */
export const WorkOrderStatusColor: Record<number, string> = {
  [WorkOrderDocStatus.Drafting]: '#faad14',
  [WorkOrderDocStatus.Commited]: '#1890ff',
  [WorkOrderDocStatus.Rejected]: '#ff4d4f',
  [WorkOrderDocStatus.Approved]: '#52c41a',
};
