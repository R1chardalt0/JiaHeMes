/**
* SN历史记录查询参数
*/
export interface MesSnListHistoryQueryDto {
  /** 状态ID */
  snListHistoryId?: string;
  /** 序列号/产品唯一码 */
  snNumber?: string;
  /** 产品ID */
  productListId?: string;
  /** 工单ID */
  orderListId?: string;
  /** 当前状态：1-合格，2-不合格，3-已包装，4-已入库，5-跳站 */
  stationStatus?: number;
  /** 当前站点ID */
  currentStationListId?: string;
  /** 线体ID */
  productionLineId?: string;
  /** 当前设备ID */
  resourceId?: string;
  /** 是否异常 */
  isAbnormal?: boolean;
  /** 异常代码 */
  abnormalCode?: string;
  /** 是否锁定（异常锁定） */
  isLocked?: boolean;
  /** 返工次数 */
  reworkCount?: number;
  /** 是否正在返工 */
  isReworking?: boolean;
  /** 返工原因 */
  reworkReason?: string;
  /** 返工时间 */
  reworkTime?: string;
  /** 创建人 */
  createBy?: string;
  /** 创建时间开始 */
  createTimeStart?: string;
  /** 创建时间结束 */
  createTimeEnd?: string;
  /** 更新人 */
  updateBy?: string;
  /** 更新时间开始 */
  updateTimeStart?: string;
  /** 更新时间结束 */
  updateTimeEnd?: string;
  /** 搜索值 */
  searchValue?: string;
  /** 页码 */
  pageIndex?: number;
  /** 每页条数 */
  pageSize?: number;
  /** 排序字段 */
  sortField?: string;
  /** 排序方向 */
  sortOrder?: string;
}

/**
 * SN历史记录数据传输对象
 */
export interface MesSnListHistoryDto {
  /** 状态ID */
  snListHistoryId: string;
  /** 序列号/产品唯一码 */
  snNumber: string;
  /** 产品ID */
  productListId: string;
  /** 产品编码 */
  productCode: string;
  /** 工单ID */
  orderListId: string;
  /** 工单编码 */
  orderCode: string;
  /** 当前状态：1-合格，2-不合格，3-已包装，4-已入库，5-跳站 */
  stationStatus: number;
  /** 当前站点ID */
  currentStationListId: string;
  /** 当前站点编码 */
  stationCode: string;
  /** 线体ID */
  productionLineId: string;
  /** 产线编码 */
  productionLineCode: string;
  /** 当前设备ID */
  resourceId: string;
  /** 设备编码 */
  deviceEnCode: string;
  /** 测试数据 */
  testData?: string;
  /** 批次数据 */
  batchResults?: string;
  /** 是否异常 */
  isAbnormal?: boolean;
  /** 异常代码 */
  abnormalCode?: string;
  /** 异常描述 */
  abnormalDescription?: string;
  /** 是否锁定（异常锁定） */
  isLocked?: boolean;
  /** 返工次数 */
  reworkCount?: number;
  /** 是否正在返工 */
  isReworking?: boolean;
  /** 返工原因 */
  reworkReason?: string;
  /** 返工时间 */
  reworkTime?: string;
  /** 创建人 */
  createBy?: string;
  /** 创建时间 */
  createTime?: string;
  /** 更新人 */
  updateBy?: string;
  /** 更新时间 */
  updateTime?: string;
  /** 备注 */
  remark?: string;
  /** 搜索值 */
  searchValue?: string;
}
