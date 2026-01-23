/**
 * 工艺路线子项相关类型定义
 */

/**
 * 工艺路线子项查询DTO
 */
export interface ProcessRouteItemQueryDto {
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
   * 工艺路线ID
   */
  headId?: string;
  /**
   * 站点编码
   */
  stationCode?: string;
  /**
   * 工艺路线序号
   */
  routeSeq?: number;
  /**
   * 是否必经站点
   */
  mustPassStation?: boolean;
  /**
   * 检查站点列表
   */
  checkStationList?: string;
  /**
   * 是否首站点
   */
  firstStation?: boolean;
  /**
   * 是否检查所有
   */
  checkAll?: boolean;
  /**
   * 最大NG次数
   */
  maxNGCount?: number;
}

/**
 * 工艺路线子项DTO
 */
export interface ProcessRouteItemDto {
  /**
   * 工艺路线子项ID
   */
  id: string;

  /**
   * 主表ID
   */
  headId?: string;

  /**
   * 站点编码
   */
  stationCode: string;
  /**
   * 工艺路线序号
   */
  routeSeq: number;

  /**
   * 是否必经站点
   */
  mustPassStation: boolean;

  /**
   * 检查站点列表
   */
  checkStationList: string;

  /**
   * 是否首站点
   */
  firstStation: boolean;

  /**
   * 是否检查所有
   */
  checkAll: boolean;

  /**
   * 最大NG次数
   */
  maxNGCount?: number;

  /**
   * 工艺路线主表（多对一关联）
   */
  processRoute?: import('./ProcessRoute').ProcessRoute;

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
   * 备注信息
   */
  remark?: string;

  /**
   * 请求参数
   */
  params?: Record<string, any>;
}

/**
 * 工艺路线子项创建DTO
 */
export interface ProcessRouteItemCreateDto {
  /**
   * 主表ID
   */
  headId?: string;

  /**
   * 站点编码
   */
  stationCode: string;
  /**
   * 工艺路线序号
   */
  routeSeq: number;

  /**
   * 是否必经站点
   */
  mustPassStation: boolean;

  /**
   * 检查站点列表
   */
  checkStationList: string;

  /**
   * 是否首站点
   */
  firstStation: boolean;

  /**
   * 是否检查所有
   */
  checkAll: boolean;

  /**
   * 最大NG次数
   */
  maxNGCount?: number;

  /**
   * 备注信息
   */
  remark?: string;
}

/**
 * 工艺路线子项更新DTO
 */
export interface ProcessRouteItemUpdateDto {
  /**
   * 工艺路线子项ID
   */
  id: string;

  /**
   * 主表ID
   */
  headId?: string;

  /**
   * 站点编码
   */
  stationCode: string;
  /**
   * 工艺路线序号
   */
  routeSeq: number;

  /**
   * 是否必经站点
   */
  mustPassStation: boolean;

  /**
   * 检查站点列表
   */
  checkStationList: string;

  /**
   * 是否首站点
   */
  firstStation: boolean;

  /**
   * 是否检查所有
   */
  checkAll: boolean;

  /**
   * 最大NG次数
   */
  maxNGCount?: number;

  /**
   * 备注信息
   */
  remark?: string;
}

/**
 * 工艺路线子项实体（兼容API服务使用）
 */
export type ProcessRouteItem = ProcessRouteItemDto;