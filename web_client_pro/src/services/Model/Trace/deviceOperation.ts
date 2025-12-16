// 可选：定义 ErrorInfo 结构
export interface ErrorInfo {
  code?: string;
  message?: string;
}

// 查询参数 DTO
export interface EqumentTraceinfoQueryDto {
  current: number;
  pageSize: number;
  productionLine?: string;
  deviceName?: string;
  deviceEnCode?: string;
  startTime?: string;
  endTime?: string;
}

// 列表项 DTO
export interface EqumentTraceinfoDto {
  productionLine: string;
  deviceName: string;
  deviceEnCode: string;
  alarMessages: string;
  sendTime: string;
  createTime: string;
  parameters: Array<{
    name: string;
    type: number;
    value: string;
    unit: string;
  }>;
}

// 分页响应结果
export interface PagedResult<T> {
  total: number;
  page: number;
  success: boolean;
  data: T[];
  errorInfo: any | null;
  errorKey: string;
  errorMessage: string;
}


