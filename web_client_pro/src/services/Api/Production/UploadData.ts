import { request } from '@umijs/max';

// 测试数据项接口
export interface TestDataItem {
  code: string;
  name: string;
  ProductCode: string;
}

// 请求参数接口
export interface UploadDataParams {
  SN: string;
  Resource: string;
  StationCode: string;
  TestResult: string;
  WorkOrderCode?: string;
  TestData?: TestDataItem[];
  BatchNo?: string;
}

// 响应结果接口
interface UploadDataResponse {
  code: number;
  message: string;
  data?: any;
}

/**
 * 上传生产数据接口
 * @param params 上传数据参数
 * @returns 上传结果
 */
export async function uploadData(params: UploadDataParams): Promise<UploadDataResponse> {
  const result = await request<UploadDataResponse>('/api/CommonInterfase/UploadData', {
    method: 'POST',
    data: params,
  });

  return result;
}
