/**
 * 工艺路线主表相关类型定义
 */
import { ProcessRouteItem } from './ProcessRouteItem';

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
 * 工艺路线列表查询DTO
 */
export interface ProcessRouteQueryDto {
  /**
   * 当前页码
   */
  current: number;
  /**
   * 每页大小
   */
  pageSize: number;
  /**
   * 排序字段
   */
  sortField?: string;
  /**
   * 排序顺序
   */
  sortOrder?: string;
  /**
   * 工艺路线名称
   */
  routeName?: string;
  /**
   * 工艺路线编码
   */
  routeCode?: string;
  /**
   * 状态
   */
  status?: number;
}

/**
 * 工艺路线DTO
 */
export interface ProcessRouteDto {
  /**
   * 工艺路线ID
   */
  id: string;

  /**
   * 工艺路线名称
   */
  routeName: string;

  /**
   * 工艺路线编码
   */
  routeCode: string;

  /**
   * 状态
   */
  status: number;

  /**
   * 备注信息
   */
  remark?: string;

  /**
   * 工艺路线明细项集合（一对多关联）
   */
  processRouteItems?: ProcessRouteItem[];

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 创建者
   */
  createBy?: string;

  /**
   * 创建时间
   */
  createTime?: string;

  /**
   * 更新者
   */
  updateBy?: string;

  /**
   * 更新时间
   */
  updateTime?: string;

  /**
   * 请求参数
   */
  params?: Record<string, any>;
}

/**
 * 工艺路线创建DTO
 */
export interface ProcessRouteCreateDto {
  /**
   * 工艺路线名称
   */
  routeName: string;

  /**
   * 工艺路线编码
   */
  routeCode: string;

  /**
   * 状态
   */
  status: number;

  /**
   * 备注信息
   */
  remark?: string;
}

/**
 * 工艺路线更新DTO
 */
export interface ProcessRouteUpdateDto {
  /**
   * 工艺路线ID
   */
  id: string;

  /**
   * 工艺路线名称
   */
  routeName: string;

  /**
   * 工艺路线编码
   */
  routeCode: string;

  /**
   * 状态
   */
  status: number;

  /**
   * 备注信息
   */
  remark?: string;
}

/**
 * 分页结果
 */
export interface PaginatedList<T> {
  /**
   * 数据列表
   */
  data: T[];
  /**
   * 总记录数
   */
  total: number;
  /**
   * 当前页码
   */
  current: number;
  /**
   * 每页大小
   */
  pageSize: number;
  /**
   * 是否成功
   */
  success?: boolean;
}

/**
 * 工艺路线主表实体（兼容API服务使用）
 */
export type ProcessRoute = ProcessRouteDto;