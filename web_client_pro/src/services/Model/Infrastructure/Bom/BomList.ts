/**
 * BOM主表相关类型定义
 */
import { BomItem } from './BomItem';

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
 * BOM列表查询DTO
 */
export interface BomListQueryDto {
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
   * BOM名称
   */
  bomName?: string;
  /**
   * BOM编码
   */
  bomCode?: string;
  /**
   * 状态
   */
  status?: number;
}

/**
 * BOM主表DTO
 */
export interface BomListDto {
  /**
   * BOM ID
   */
  bomId: string;

  /**
   * BOM名称
   */
  bomName: string;

  /**
   * BOM编码
   */
  bomCode: string;

  /**
   * 状态
   */
  status: number;

  /**
   * 备注信息
   */
  remark?: string;

  /**
   * BOM明细项集合（一对多关联）
   */
  bomItems?: BomItem[];

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
 * BOM创建DTO
 */
export interface BomListCreateDto {
  /**
   * BOM名称
   */
  bomName: string;

  /**
   * BOM编码
   */
  bomCode: string;

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
 * BOM更新DTO
 */
export interface BomListUpdateDto {
  /**
   * BOM ID
   */
  bomId: string;

  /**
   * BOM名称
   */
  bomName: string;

  /**
   * BOM编码
   */
  bomCode: string;

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
 * BOM主表实体（兼容API服务使用）
 */
export type BomList = BomListDto;
