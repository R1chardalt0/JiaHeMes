/**
 * BOM子项相关类型定义
 */

/**
 * BOM子项查询DTO
 */
export interface BomItemQueryDto {
  /**
   * 当前页码
   */
  current: number;
  /**
   * 每页大小
   */
  pageSize: number;
  /**
   * 排序字段
   */
  sortField?: string;
  /**
   * 排序顺序
   */
  sortOrder?: string;
  /**
   * BOM ID
   */
  bomId?: string;
  /**
   * 站点编码
   */
  stationCode?: string;
  /**
   * 批次规则
   */
  batchRule?: string;
  /**
   * 批次SN数量
   */
  batchSNQty?: string;
  /**
   * 物料ID
   */
  productId?: string;

  /**
   * 产品名称
   */
  productName?: string;

  /**
   * 产品编码
   */
  productCode?: string;
/**
 * 日期序列
 */
dataIndex?: number;
/**
 * 数量序列
 */
numberIndex?: number;
/**
 * 物料保质期（分钟）
 */
shelfLife?: number;
/**
 * 物料号序列
 */
dateIndex?: number;
}

/**
 * BOM子项DTO
 */
export interface BomItemDto {
  /**
   * BOM明细ID
   */
  bomItemId: string;

  /**
   * 主表ID
   */
  bomId?: string;

  /**
   * 站点编码
   */
  stationCode: string;

  /**
   * 批次规则，正则表达
   */
  batchRule: string;

  /**
   * 批次数据固定
   */
  batchQty?: boolean;

  /**
   * 获取批次数量，固定位数
   */
  batchSNQty: string;

  /**
   * 物料ID
   */
  productId?: string;

  /**
   * 产品名称
   */
  productName?: string;

  /**
   * 产品编码
   */
  productCode?: string;
  /**
 * 日期序列
 */
dataIndex?: number;
/**
 * 数量序列
 */
numberIndex?: number;
/**
 * 物料保质期（分钟）
 */
shelfLife?: number;
/**
 * 物料号序列
 */
dateIndex?: number;

  /**
   * BOM主表（多对一关联）
   */
  bom?: import('./BomList').BomList;

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 创建者
   */
  createBy?: string;

  /**
   * 创建时间
   */
  createTime?: string;

  /**
   * 更新者
   */
  updateBy?: string;

  /**
   * 更新时间
   */
  updateTime?: string;

  /**
   * 请求参数
   */
  params?: Record<string, any>;
}

/**
 * BOM子项创建DTO
 */
export interface BomItemCreateDto {
  /**
   * 主表ID
   */
  bomId?: string;

  /**
   * 站点编码
   */
  stationCode: string;

  /**
   * 批次规则，正则表达
   */
  batchRule: string;

  /**
   * 批次数据固定
   */
  batchQty?: boolean;

  /**
   * 获取批次数量，固定位数
   */
  batchSNQty: string;

  /**
   * 物料ID
   */
  productId?: string;

  /**
   * 产品名称
   */
  productName?: string;

  /**
   * 产品编码
   */
  productCode?: string;
  /**
 * 产品序列
 */
productIndex?: number;
/**
 * 数量序列
 */
numberIndex?: number;
/**
 * 物料保质期（分钟）
 */
shelfLife?: number;
/**
 * 物料号序列
 */
dateIndex?: number;
}

/**
 * BOM子项更新DTO
 */
export interface BomItemUpdateDto {
  /**
   * BOM明细ID
   */
  bomItemId: string;

  /**
   * 主表ID
   */
  bomId?: string;

  /**
   * 站点编码
   */
  stationCode: string;

  /**
   * 批次规则，正则表达
   */
  batchRule: string;

  /**
   * 批次数据固定
   */
  batchQty?: boolean;

  /**
   * 获取批次数量，固定位数
   */
  batchSNQty: string;

  /**
   * 物料ID
   */
  productId?: string;

  /**
   * 产品名称
   */
  productName?: string;

  /**
   * 产品编码
   */
  productCode?: string;
  /**
 * 产品序列
 */
productIndex?: number;
/**
 * 数量序列
 */
numberIndex?: number;
/**
 * 物料保质期（分钟）
 */
shelfLife?: number;
/**
 * 物料号序列
 */
dateIndex?: number;
}

/**
 * BOM子项实体（兼容API服务使用）
 */
export type BomItem = BomItemDto;
