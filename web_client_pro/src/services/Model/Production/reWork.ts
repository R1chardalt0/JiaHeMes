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
  TargetStationCode: string;
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