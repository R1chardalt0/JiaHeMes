/**
 * MES工单BOM批次明细类型定义
 */
import { MESOrderBomBatch } from './MESOrderBomBatch';

export interface MesOrderBomBatchItem {
  /**
   * 工单BOM批次明细ID
   */
  orderBomBatchItemId: string;

  /**
   * 工单BOM批次ID
   */
  orderBomBatchId: string;

  /**
   * SN编码
   */
  snNumber?: string;

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

  /**
   * 关联的工单BOM批次信息
   */
  mesOrderBomBatch?: MESOrderBomBatch;
}

/**
 * MES工单BOM批次明细查询参数类型定义
 */
export interface MESOrderBomBatchItemQueryDto {
  /**
   * 工单BOM批次明细ID
   */
  orderBomBatchItemId: string;

  /**
   * 工单BOM批次ID
   */
  orderBomBatchId: string;

  /**
   * SN编码
   */
  snNumber?: string;

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
