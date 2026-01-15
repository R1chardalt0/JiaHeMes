/**
 * 工艺路线工位测试相关类型定义
 */

/**
 * 工艺路线工位测试查询DTO
 */
export interface ProcessRouteItemTestQueryDto {
  /**
   * 当前页码
   */
  pageIndex: number;
  /**
   * 每页大小
   */
  pageSize: number;
  /**
   * 工艺路线工位测试ID
   */
  proRouteItemStationTestId?: string;
  /**
   * 工艺路线明细ID
   */
  processRouteItemId?: string;
  /**
   * 测试项
   */
  parametricKey?: string;
  /**
   * 是否检查
   */
  isCheck?: boolean;
}

/**
 * 工艺路线工位测试DTO
 */
export interface ProcessRouteItemTestDto {
  /**
   * 工艺路线工位测试ID
   */
  proRouteItemStationTestId: string;

  /**
   * 工艺路线明细ID
   */
  processRouteItemId?: string;

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

  /**
   * 请求参数
   */
  params?: Record<string, any>;
}

/**
 * 工艺路线工位测试创建DTO
 */
export interface ProcessRouteItemTestCreateDto {
  /**
   * 工艺路线明细ID
   */
  processRouteItemId?: string;

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
   * 备注信息
   */
  remark?: string;

  /**
   * 是否检查
   */
  isCheck?: boolean;
}

/**
 * 工艺路线工位测试更新DTO
 */
export interface ProcessRouteItemTestUpdateDto {
  /**
   * 工艺路线工位测试ID
   */
  proRouteItemStationTestId: string;

  /**
   * 工艺路线明细ID
   */
  processRouteItemId?: string;

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
   * 备注信息
   */
  remark?: string;

  /**
   * 是否检查
   */
  isCheck?: boolean;
}

/**
 * 工艺路线工位测试实体（兼容API服务使用）
 */
export type ProcessRouteItemTest = ProcessRouteItemTestDto;