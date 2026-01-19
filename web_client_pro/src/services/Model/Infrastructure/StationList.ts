/**
 * 站点管理相关类型定义
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
 * 站点列表查询DTO
 */
export interface StationListQueryDto {
  /**
   * 当前页码
   */
  pageIndex: number;
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
   * 站点名称
   */
  stationName?: string;
  /**
   * 站点编码
   */
  stationCode?: string;
}

/**
 * 站点列表DTO
 */
export interface StationListDto {
  /**
   * 站点ID
   */
  stationId: string;
  /**
   * 站点名称
   */
  stationName: string;
  /**
   * 站点编码
   */
  stationCode: string;
  /**
   * 备注
   */
  remark?: string;
  /**
   * 创建时间
   */
  createTime?: string;
  /**
   * 创建人
   */
  createBy?: string;
  /**
   * 更新时间
   */
  updateTime?: string;
  /**
   * 更新人
   */
  updateBy?: string;
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
