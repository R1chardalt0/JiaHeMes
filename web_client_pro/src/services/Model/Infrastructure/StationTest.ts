/**
 * 站点测试项相关类型定义
 */

/**
 * 站点测试项查询DTO
 */
export interface StationTestProjectQueryDto {
  /**
   * 站点ID
   */
  stationId?: string;

  /**
   * 测试项
   */
  parametricKey?: string;

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 创建时间起始（包含）
   */
  startTime?: string;

  /**
   * 创建时间结束（包含）
   */
  endTime?: string;

  /**
   * 当前页码（最小值为1）
   */
  current: number;

  /**
   * 每页记录数（最小值为1）
   */
  pageSize: number;
}

/**
 * 站点测试项DTO
 */
export interface StationTestProjectDto {
  /**
   * 测试项ID
   */
  stationTestProjectId: string;

  /**
   * 站点id
   */
  stationId?: string;

  /**
   * 上限
   */
  upperLimit?: number;

  /**
   * 下限
   */
  lowerLimit?: number;

  /**
   * 单位
   */
  units?: string;

  /**
   * 测试项
   */
  parametricKey?: string;

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
   * 是否检查
   */
  isCheck?: boolean;
}

/**
 * 站点测试项创建DTO
 */
export interface StationTestProjectCreateDto {
  /**
   * 站点id
   */
  stationId: string;

  /**
   * 上限
   */
  upperLimit?: number;

  /**
   * 下限
   */
  lowerLimit?: number;

  /**
   * 单位
   */
  units?: string;

  /**
   * 测试项
   */
  parametricKey: string;

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 备注信息
   */
  remark?: string;

  /**
   * 是否检查
   */
  isCheck?: boolean;
}

/**
 * 站点测试项更新DTO
 */
export interface StationTestProjectUpdateDto {
  /**
   * 测试项ID
   */
  stationTestProjectId: string;

  /**
   * 站点id
   */
  stationId: string;

  /**
   * 上限
   */
  upperLimit?: number;

  /**
   * 下限
   */
  lowerLimit?: number;

  /**
   * 单位
   */
  units?: string;

  /**
   * 测试项
   */
  parametricKey: string;

  /**
   * 搜索值
   */
  searchValue?: string;

  /**
   * 备注信息
   */
  remark?: string;

  /**
   * 是否检查
   */
  isCheck?: boolean;
}

/**
 * 分页结果
 */
export interface PaginatedList<T> {
  /**
   * 总记录数
   */
  total: number;

  /**
   * 数据列表
   */
  items: T[];
}
