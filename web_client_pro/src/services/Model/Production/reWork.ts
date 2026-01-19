/**
 * 返工操作参数接口
 */
export interface ReWorkParams {
  /**
   * SN码
   */
  SN: string;
  /**
   * 目标站点编码
   */
  ReWorkStationCode: string;
  /**
   * 需要解绑的物料批次明细ID列表
   */
  UnbindMaterialIds?: string[];
}

/**
 * 返工表单数据接口
 */
export interface ReWorkFormData {
  /**
   * SN码
   */
  sn: string;
  /**
   * 目标站点ID
   */
  targetStationId: string;
}

/**
 * 返工操作响应接口
 */
export interface ReWorkResponse {
  /**
   * 响应状态码
   */
  code: number;
  /**
   * 响应消息
   */
  message: string;
  /**
   * 响应数据
   */
  data?: any;
}