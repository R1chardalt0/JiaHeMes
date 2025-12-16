// 可选：定义 ErrorInfo 结构
export interface ErrorInfo {
    code?: string;
    message?: string;
}

// 查询参数 DTO
export interface ProductTraceInfoQueryDto {
    current: number;
    pageSize: number;
    sfc?: string;
    productionLine?: string;
    deviceName?: string;
    resource?: string;
    startTime?: string;
    endTime?: string;
}

// 列表项 DTO
export interface ProductTraceInfoDto {
    sfc: string;
    productionLine: string;
    deviceName: string;
    site:string;
    activityId:string;
    resource: string;
    dcGroupRevision: string;
    isOK:boolean;
    sendTime: string;
    createTime: string;
    parametricDataArray: Array<{
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


