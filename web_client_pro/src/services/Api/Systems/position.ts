import { request } from '@umijs/max';
import { PositionItem, PagedResult, BaseResponse, PositionQueryParams } from '@/services/Model/Systems/position';

/** 获取岗位列表 */
export async function getPositionList(
  params: PositionQueryParams = { current: 1, pageSize: 10 },
  options?: Record<string, any>
) {
  return request<PagedResult<PositionItem>>('/api/SysPost/GetPostList/list', {
    method: 'GET',
    params: {
      current: params.current,
      pageSize: params.pageSize,
      postName: params.postName,
      postCode: params.postCode,
      status: params.status,
    },
    ...(options || {})
  });
}

/** 获取岗位详情 */
export async function getPositionDetail(id: number, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: PositionItem;
  }>(`/api/SysPost/GetPostById/${id}`, {
    method: 'GET',
    ...(options || {})
  });
}

/** 创建岗位 */
export async function createPosition(data: Omit<PositionItem, 'postId'>) {
  // 确保状态值正确传递，不能使用 ?? 因为 '0' 会被认为是 falsy
  const statusValue = data.status !== undefined && data.status !== null 
    ? String(data.status) 
    : '0';
  
  const payload = {
    ...data,
    status: statusValue, // 确保状态值正确传递（'0'=启用，'1'=停用）
    postSort: data.postSort || '0'
  };
  
  // 调试日志
  if (process.env.NODE_ENV === 'development') {
    console.log('创建岗位API调用:', { 原始数据: data, 转换后payload: payload });
  }
  
  return request<BaseResponse<number>>('/api/SysPost/CreatePost', {
    method: 'POST',
    data: payload,
    headers: {
      'Content-Type': 'application/json'
    }
  });
}

/** 更新岗位 */
export async function updatePosition(data: PositionItem) {
  // 确保状态值正确传递
  const statusValue = data.status !== undefined && data.status !== null 
    ? String(data.status) 
    : '0';
  
  const payload = {
    postId: data.postId, // 确保postId存在
    postName: data.postName,
    postCode: data.postCode,
    postSort: String(data.postSort || '0'),
    status: statusValue, // 确保状态值正确传递（'0'=启用，'1'=停用）
    remark: data.remark,
  };
  
  // 调试日志
  if (process.env.NODE_ENV === 'development') {
    console.log('更新岗位API调用:', { 原始数据: data, 转换后payload: payload });
  }
  
  return request<BaseResponse<number>>('/api/SysPost/UpdatePost', {
    method: 'POST',
    data: payload,
    headers: {
      'Content-Type': 'application/json'
    }
  });
}

/** 删除岗位 */
export async function deletePosition(postIds: number[], options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: number;
  }>('/api/SysPost/DeletePostByIds', {
    method: 'POST',
    data: postIds,
    headers: {
      'Content-Type': 'application/json'
    },
    ...(options || {})
  });
}

