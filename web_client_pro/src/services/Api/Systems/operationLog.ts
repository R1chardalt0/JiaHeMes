import { request } from '@umijs/max';
import { PagedResult } from '@/services/Model/Systems/role';
import { OperationLogItem, OperationLogQueryDto, OperationLogAddDto } from '@/services/Model/Systems/operationLog';

/** 获取操作日志列表 */
export async function getOperationLogList(
  params: OperationLogQueryDto = { current: 1, pageSize: 20 },
  options?: Record<string, any>
) {
  return request<PagedResult<OperationLogItem>>('/api/SysOperationLog/GetOperationLogList/list', {
    method: 'GET',
    params: {
      current: params.current,
      pageSize: params.pageSize,
      keyword: params.keyword,
      userCode: params.userCode,
      userName: params.userName,
      operationType: params.operationType,
      operationModule: params.operationModule,
      targetId: params.targetId,
      operationStatus: params.operationStatus,
      operationTimeStart: params.operationTimeStart,
      operationTimeEnd: params.operationTimeEnd,
    },
    ...(options || {}),
  });
}

/** 新增操作日志（供业务模块调用） */
export async function addOperationLog(data: OperationLogAddDto, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: number;
  }>('/api/SysOperationLog/AddOperationLog', {
    method: 'POST',
    data,
    headers: {
      'Content-Type': 'application/json',
    },
    ...(options || {}),
  });
}

