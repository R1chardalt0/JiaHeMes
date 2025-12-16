import { PagedResult, BaseResponse } from './role';

/**
 * 操作日志项
 */
export interface OperationLogItem {
  /** 日志ID */
  logId?: number;
  /** 操作用户工号 */
  userCode?: string;
  /** 操作用户姓名 */
  userName?: string;
  /** 操作类型（INSERT、UPDATE、DELETE） */
  operationType?: string;
  /** 操作模块 */
  operationModule?: string;
  /** 操作对象ID */
  targetId?: string;
  /** 操作前数据（JSON格式） */
  beforeData?: string;
  /** 操作后数据（JSON格式） */
  afterData?: string;
  /** 操作时间 */
  operationTime?: string;
  /** 操作IP地址 */
  operationIp?: string;
  /** 操作备注 */
  operationRemark?: string;
  /** 操作状态（SUCCESS、FAIL） */
  operationStatus?: string;
}

/**
 * 操作日志查询DTO
 */
export interface OperationLogQueryDto {
  /** 当前页码 */
  current?: number;
  /** 每页条数 */
  pageSize?: number;
  /** 关键字搜索（工号、姓名、操作模块、操作对象ID） */
  keyword?: string;
  /** 操作用户工号 */
  userCode?: string;
  /** 操作用户姓名 */
  userName?: string;
  /** 操作类型（INSERT、UPDATE、DELETE） */
  operationType?: string;
  /** 操作模块 */
  operationModule?: string;
  /** 操作对象ID */
  targetId?: string;
  /** 操作状态（SUCCESS、FAIL） */
  operationStatus?: string;
  /** 操作时间开始 */
  operationTimeStart?: string;
  /** 操作时间结束 */
  operationTimeEnd?: string;
}

/**
 * 操作日志新增DTO
 */
export interface OperationLogAddDto {
  /** 操作用户工号 */
  userCode: string;
  /** 操作用户姓名 */
  userName: string;
  /** 操作类型 */
  operationType: string;
  /** 操作模块 */
  operationModule: string;
  /** 操作对象ID */
  targetId: string;
  /** 操作前数据 */
  beforeData?: string;
  /** 操作后数据 */
  afterData?: string;
  /** 操作IP地址 */
  operationIp: string;
  /** 操作备注 */
  operationRemark?: string;
  /** 操作状态 */
  operationStatus: string;
}

