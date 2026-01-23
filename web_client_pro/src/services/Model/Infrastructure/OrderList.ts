/**
 * 工单相关类型定义
 */

/**
 * 工单查询DTO
 */
export interface OrderListQueryDto {
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
   * 工单编码
   */
  orderCode?: string;
  /**
   * 工单名称
   */
  orderName?: string;
  /**
   * 产品ID
   */
  productListId?: string;
  /**
   * 产品编码
   */
  productCode?: string;
  /**
   * 产品名称
   */
  productName?: string;
  /**
   * 最大返工次数
   */
  maxReworkCount?: number;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * BOM编码
   */
  bomCode?: string;
  /**
   * BOM名称
   */
  bomName?: string;
  /**
   * 工艺路线ID
   */
  processRouteId?: string;
  /**
   * 工艺路线编码
   */
  processRouteCode?: string;
  /**
   * 工艺路线名称
   */
  processRouteName?: string;
  /**
   * 工单类型
   */
  orderType?: number;
  /**
   * 工单状态
   */
  orderStatus?: number;
  /**
   * 优先级
   */
  priorityLevel?: number;
}

/**
 * 工单DTO
 */
export interface OrderListDto {
  /**
   * 工单ID
   */
  orderListId: string;
  /**
   * 工单编码
   */
  orderCode: string;
  /**
   * 工单名称
   */
  orderName: string;
  /**
   * 产品ID
   */
  productListId: string;
  /**
   * 产品编码
   */
  productCode: string;
  /**
   * 产品名称
   */
  productName: string;
  /**
   * 最大返工次数
   */
  maxReworkCount?: number;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * BOM编码
   */
  bomCode?: string;
  /**
   * BOM名称
   */
  bomName?: string;
  /**
   * 工艺路线ID
   */
  processRouteId?: string;
  /**
   * 工艺路线编码
   */
  processRouteCode: string;
  /**
   * 工艺路线名称
   */
  processRouteName: string;
  /**
   * 工单类型
   */
  orderType: number;
  /**
   * 计划数量
   */
  planQty: number;
  /**
   * 已完成数量
   */
  completedQty: number;
  /**
   * 优先级
   */
  priorityLevel: number;
  /**
   * 计划开始时间
   */
  planStartTime: string;
  /**
   * 计划结束时间
   */
  planEndTime: string;
  /**
   * 实际开始时间
   */
  actualStartTime?: string;
  /**
   * 实际结束时间
   */
  actualEndTime?: string;
  /**
   * 状态
   */
  orderStatus?: number;
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
 * 工单创建DTO
 */
export interface OrderListCreateDto {
  /**
   * 工单编码
   */
  orderCode: string;
  /**
   * 工单名称
   */
  orderName: string;
  /**
   * 产品ID
   */
  productListId: string;
  /**
   * 产品编码
   */
  productCode: string;
  /**
   * 产品名称
   */
  productName: string;
  /**
   * 最大返工次数
   */
  maxReworkCount?: number;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * BOM编码
   */
  bomCode?: string;
  /**
   * BOM名称
   */
  bomName?: string;
  /**
   * 工艺路线ID
   */
  processRouteId?: string;
  /**
   * 工艺路线编码
   */
  processRouteCode: string;
  /**
   * 工艺路线名称
   */
  processRouteName: string;
  /**
   * 工单类型
   */
  orderType: number;
  /**
   * 计划数量
   */
  planQty: number;
  /**
   * 已完成数量
   */
  completedQty: number;
  /**
   * 优先级
   */
  priorityLevel: number;
  /**
   * 计划开始时间
   */
  planStartTime: string;
  /**
   * 计划结束时间
   */
  planEndTime: string;
}

/**
 * 工单更新DTO
 */
export interface OrderListUpdateDto {
  /**
   * 工单ID
   */
  orderListId: string;
  /**
   * 工单编码
   */
  orderCode: string;
  /**
   * 工单名称
   */
  orderName: string;
  /**
   * 产品ID
   */
  productListId: string;
  /**
   * 产品编码
   */
  productCode: string;
  /**
   * 产品名称
   */
  productName: string;
  /**
   * 最大返工次数
   */
  maxReworkCount?: number;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * BOM编码
   */
  bomCode?: string;
  /**
   * BOM名称
   */
  bomName?: string;
  /**
   * 工艺路线ID
   */
  processRouteId?: string;
  /**
   * 工艺路线编码
   */
  processRouteCode: string;
  /**
   * 工艺路线名称
   */
  processRouteName: string;
  /**
   * 工单类型
   */
  orderType: number;
  /**
   * 计划数量
   */
  planQty: number;
  /**
   * 已完成数量
   */
  completedQty: number;
  /**
   * 优先级
   */
  priorityLevel: number;
  /**
   * 计划开始时间
   */
  planStartTime: string;
  /**
   * 计划结束时间
   */
  planEndTime: string;
  /**
   * 状态
   */
  status?: number;
}

/**
 * 工单实体（兼容API服务使用）
 */
export type OrderList = OrderListDto;
