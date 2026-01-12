import { request } from '@umijs/max';
import type { MesSnListHistoryDto, MesSnListHistoryQueryDto } from '@/services/Model/Trace/MesSnListHistory';

/**
 * 获取SN历史记录列表
 * @param queryDto 查询参数
 * @returns SN历史记录列表
 */
export async function getMesSnListHistoryList(
  queryDto: MesSnListHistoryQueryDto
): Promise<any> {
  const result = await request('/api/MesSnListHistory/GetMesSnListHistoryList', {
    method: 'GET',
    params: queryDto,
  });

  // 处理后端返回的数据格式
  // 如果返回的是数组，转换为分页格式
  if (Array.isArray(result)) {
    return {
      data: result,
      total: result.length,
      success: true
    };
  }

  return result;
}

/**
 * 根据ID获取SN历史记录详情
 * @param id SN历史记录ID
 * @returns SN历史记录详情
 */
export async function getMesSnListHistoryById(
  id: string
): Promise<MesSnListHistoryDto> {
  const result = await request('/api/MesSnListHistory/GetMesSnListHistoryById', {
    method: 'GET',
    params: { id },
  });

  return result;
}

/**
 * 根据SN号获取SN历史记录详情
 * @param snNumber SN号
 * @returns SN历史记录详情
 */
export async function getMesSnListHistoryBySnNumber(
  snNumber: string
): Promise<MesSnListHistoryDto> {
  const result = await request('/api/MesSnListHistory/GetMesSnListHistoryBySnNumber', {
    method: 'GET',
    params: { snNumber },
  });

  return result;
}
