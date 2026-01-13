/**
 * 物料上传接口参数类型定义
 */
export interface RequestFeedMaterialParams {
  /**
   * 批次号
   */
  BatchCode?: string;

  /**
   * 设备资源标识
   */
  Resource?: string;

  /**
   * 站点
   */
  StationCode?: string;

  /**
   * 工单编号
   */
  WorkOrderCode?: string;

  /**
   * 数量
   */
  BatchQty?: number;

  /**
   * 产品编码
   */
  ProductCode?: string;
}

/**
 * 物料上传接口响应类型定义
 */
export interface FeedMaterialResponse {
  /**
   * 响应码
   */
  code: number;

  /**
   * 响应消息
   */
  message: string;
}

/**
 * 物料上传实体类
 */
export interface FeedMaterial {
  /**
   * 批次号
   */
  batchCode: string;
  /**
   * 批次号ID（用于表单处理）
   */
  batchCodeId: string;

  /**
   * 设备资源标识
   */
  resource: string;
  /**
   * 设备资源标识ID（用于表单处理）
   */
  resourceId: string;

  /**
   * 站点编码
   */
  stationCode: string;
  /**
   * 站点编码ID（用于表单处理）
   */
  stationCodeId: string;

  /**
   * 工单编号
   */
  workOrderCode: string;
  /**
   * 工单编号ID（用于表单处理）
   */
  workOrderCodeId: string;

  /**
   * 批次数量
   */
  batchQty: number;
  /**
   * 批次数量ID（用于表单处理）
   */
  batchQtyId: string;

  /**
   * 产品编码
   */
  productCode: string;
  /**
   * 产品编码ID（用于表单处理）
   */
  productCodeId: string;

  /**
   * 操作结果
   */
  result?: {
    /**
     * 响应码
     */
    code: number;
    /**
     * 响应消息
     */
    message: string;
  };
}

/**
 * 物料上传表单数据
 */
export interface FeedMaterialFormData {
  /**
   * 批次号
   */
  batchCode: string;
  /**
   * 设备资源标识
   */
  resource: string;
  /**
   * 站点编码
   */
  stationCode: string;
  /**
   * 工单编号
   */
  workOrderCode: string;
  /**
   * 批次数量
   */
  batchQty: number;
  /**
   * 产品编码
   */
  productCode: string;
}
