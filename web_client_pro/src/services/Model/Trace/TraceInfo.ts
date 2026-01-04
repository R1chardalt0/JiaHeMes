// 查询参数 DTO
export interface TraceInfoQueryDto {
    current: number;
    pageSize: number;
    id?: string;
    productCode?: string;
    pn?: string;
    vsn?: string;
}

// 定义可能包含value属性的对象类型
export interface ValueObject {
    value: string;
}

// 列表项 DTO
export interface TraceInfoDto {
    id: string;
    productCode: string | ValueObject;
    pn: string | ValueObject;
    pin?: string | ValueObject;
    vsn: number;
    bomId: number;
    createTime: string;
}

// 详情信息 DTO
export interface TraceInfoDetailDto {
    id: string;
    productCode: string | ValueObject;
    pn: string | ValueObject;
    pin?: string | ValueObject;
    vsn: number;
    bomRecipeId: number;
    productLine: string;
    createdAt: string;
}

// 物料信息 DTO
export interface MaterialInfoDto {
    id: string;
    vsn: number;
    traceInfoId: string;
    traceInfo: null;
    bomItemCode: string | ValueObject;
    bomId: number;
    bom: null;
    materialCode: string | ValueObject;
    measureUnit: string | ValueObject;
    quota: number;
    createdAt: string;
    deletedAt: string;
    isDeleted: boolean;
    sku: string | ValueObject;
    consumption: number;
}

// 过程信息 DTO
export interface ProcessInfoDto {
    id: string;
    vsn: number;
    traceInfoId: string;
    traceInfo: null;
    station: string;
    key: string;
    value: any[];
    isDeleted: boolean;
    createdAt: string;
    deletedAt: string;
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
