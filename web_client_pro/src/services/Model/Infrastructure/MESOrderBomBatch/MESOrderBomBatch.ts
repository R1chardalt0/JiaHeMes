/**
 * MES工单BOM批次类型定义
 */
export interface MESOrderBomBatch {
  /**
   * 工单BOM批次ID
   */
  orderBomBatchId: string;

  /**
   * 物料ID
   */
  productListId?: string;

  /**
   * 产品编码
   */
  productCode?: string;

  /**
   * 批次编码
   */
  batchCode?: string;

  /**
   * 站点
   */
  stationListId?: string;

  /**
   * 站点编码
   */
  stationCode?: string;

  /**
   * 工单ID
   */
  orderListId?: string;

  /**
   * 工单编码
   */
  orderCode?: string;

  /**
   * 设备ID
   */
  resourceId?: string;
  /**
   * 设备编码
   */
  deviceEnCode?: string;

  /**
   * 状态：1-正常，2-已使用完，3-已下料
   */
  orderBomBatchStatus?: number;

  /**
   * 批次数量
   */
  batchQty?: number;

  /**
   * 已使用数量
   */
  completedQty?: number;

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 创建人
   */
  createBy?: string;

  /**
   * 创建时间
   */
  createTime?: string;

  /**
   * 更新人
   */
  updateBy?: string;

  /**
   * 更新时间
   */
  updateTime?: string;

  /**
   * 备注
   */
  remark?: string;
}

/**
 * MES工单BOM批次查询参数类型定义
 */
export interface MESOrderBomBatchQueryDto {
  /**
   * 工单BOM批次ID
   */
  orderBomBatchId?: string;

  /**
   * 物料ID
   */
  productListId?: string;

  /**
   * 批次编码
   */
  batchCode?: string;

  /**
   * 站点
   */
  stationListId?: string;

  /**
   * 状态：1-正常，2-已使用完，3-已下料
   */
  orderBomBatchStatus?: number;

  /**
   * 工单ID
   */
  orderListId?: string;

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 页码
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
   * 排序方向：asc/desc
   */
  sortOrder?: string;
}
