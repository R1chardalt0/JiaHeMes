/**
 * 跳站操作参数接口
 */
export interface JumpStationParams {
  /**
   * SN码
   */
  SN: string;
  /**
   * 目标站点编码
   */
  JumpStationCode: string;
}

/**
 * 跳站表单数据接口
 */
export interface JumpStationFormData {
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
 * 跳站操作响应接口
 */
export interface JumpStationResponse {
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