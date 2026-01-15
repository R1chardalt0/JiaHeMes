import { request } from '@umijs/max';

interface TestDataItem {
  parametricKey: string;
  testValue: string;
}

interface Station {
  stationCode: string;
  stationName: string;
  stationStatus: number;
  resourceCode: string;
  testResult: string;
  testTime: string;
  testData: TestDataItem[];
}

interface FeedingBatchItem {
  snNumber: string;
  createTime: string;
}

interface FeedingBatch {
  batchCode: string;
  stationCode: string;
  productCode: string;
  batchQty: number;
  completedQty: number;
  status: number;
  createTime: string;
  items: FeedingBatchItem[];
}

export interface TraceSNDto {
  sn: string;
  orderCode: string;
  productCode: string;
  currentStation: string;
  stationStatus: number;
  isAbnormal: boolean;
  createTime: string;
  updateTime: string;
  stations: Station[];
  feedingBatches: FeedingBatch[];
}

interface TraceSNResponse {
  case: string;
  fields: TraceSNDto[];
}

/**
 * 根据SN号获取单条码追溯信息
 * @param sn SN号
 * @returns 单条码追溯信息
 */
export async function getTraceSN(
  sn: string
): Promise<TraceSNDto | null> {
  const result = await request<TraceSNResponse>('/api/CommonInterfase/TraceSN', {
    method: 'GET',
    params: { sn },
  });

  if (result?.case === 'Ok' && result?.fields?.length > 0) {
    return result.fields[0];
  }

  return null;
}


