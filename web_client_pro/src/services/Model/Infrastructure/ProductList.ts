/**
 * 产品列表相关类型定义
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
 * 产品列表查询DTO
 */
export interface ProductListQueryDto {
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
   * 产品名称
   */
  productName?: string;
  /**
   * 产品编码
   */
  productCode?: string;
  /**
   * 产品类型
   */
  productType?: string;
}

/**
 * 产品列表DTO
 */
export interface ProductListDto {
  /**
   * 产品列表ID
   */
  productListId: string;
  /**
   * 产品名称
   */
  productName: string;
  /**
   * 产品编码
   */
  productCode: string;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * BOM编号
   */
  bomCode?: string;
  /**
   * BOM名称
   */
  bomName?: string;
  /**
   * 工艺路线 ID
   */
  processRouteId?: string;
  /**
   * 工艺路线编号
   */
  processRouteCode?: string;
  /**
   * 工艺路线名称
   */
  processRouteName?: string;
  /**
   * 产品类型
   */
  productType?: string;
  /**
   * 备注信息
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
 * 产品列表创建DTO
 */
export interface ProductListCreateDto {
  /**
   * 产品名称
   */
  productName: string;
  /**
   * 产品编码
   */
  productCode: string;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * 工艺路线 ID
   */
  processRouteId?: string;
  /**
   * 产品类型
   */
  productType?: string;
  /**
   * 备注信息
   */
  remark?: string;
}

/**
 * 产品列表更新DTO
 */
export interface ProductListUpdateDto {

  /**
   * 产品名称
   */
  productName: string;
  /**
   * 产品编码
   */
  productCode: string;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * 工艺路线 ID
   */
  processRouteId?: string;
  /**
   * 产品类型
   */
  productType?: string;
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
