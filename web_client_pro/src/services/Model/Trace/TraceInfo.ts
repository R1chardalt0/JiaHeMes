// 查询参数 DTO
export interface TraceInfoQueryDto {
    current: number;
    pageSize: number;
    id?: string;
    productCode?: string;
    pin?: string;
    vsn?: string;
}

// 定义可能包含value属性的对象类型
export interface ValueObject {
    value: string;
}

// 类型保护函数，检查对象是否包含value属性
export function isValueObject(value: any): value is ValueObject {
    return typeof value === 'object' && value !== null && 'value' in value;
}

// 列表项 DTO
export interface TraceInfoDto {
    id: string;
    productCode: string | ValueObject;
    pin: string | ValueObject;
    vsn: number;
    bomId: number;
    createdAt: string;
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

// 通用API响应处理函数，处理API返回数据的格式适配
export const processApiResponse = (response: any, isSingleItem = false): any => {
    // 如果是数组且需要单个项，返回第一个元素
    if (Array.isArray(response)) {
        return isSingleItem ? response[0] : response;
    }
    // 如果是对象且需要单个项，直接返回
    if (typeof response === 'object' && response !== null) {
        if (isSingleItem) {
            return response;
        }
        // 如果是对象但需要数组，尝试从data字段获取
        return response.data || [];
    }
    // 默认返回空数组或null
    return isSingleItem ? null : [];
};

// 通用value对象显示处理函数，处理可能包含value属性的对象的显示逻辑
export const getValueDisplay = (value: any): string => {
    if (isValueObject(value)) {
        return value.value || '';
    }
    return value?.toString() || '';
};
