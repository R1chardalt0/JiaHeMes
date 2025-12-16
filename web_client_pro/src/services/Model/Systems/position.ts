export interface PositionItem {
  postId: number;
  postCode: string;
  postName: string;
  postSort: string;
  status: string;
  remark?: string;
  createBy?: string;
  updateBy?: string;
  createTime?: string;
  updateTime?: string;
}

export interface PagedResult<T> {
  code: number;
  msg: string;
  success: boolean;
  data: T[];
  total: number;
}

export interface BaseResponse<T = any> {
  code: number;
  msg: string;
  data: T;
  success?: boolean;
}

export interface PositionQueryParams {
  current?: number;
  pageSize?: number;
  postName?: string;
  postCode?: string;
  status?: string;
}

