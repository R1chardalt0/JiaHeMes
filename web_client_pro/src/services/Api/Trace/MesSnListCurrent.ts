import { request } from '@umijs/max';
import type { MesSnListCurrentDto, MesSnListCurrentQueryDto } from '@/services/Model/Trace/MesSnListCurrent';

/**
 * 获取SN实时状态列表
 * @param queryDto 查询参数
 * @returns SN实时状态列表
 */
export async function getMesSnListCurrentList(
  queryDto: MesSnListCurrentQueryDto
): Promise<any> {
  const result = await request('/api/MesSnListCurrent/GetMesSnListCurrentList', {
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
 * 根据ID获取SN实时状态详情
 * @param id SN实时状态ID
 * @returns SN实时状态详情
 */
export async function getMesSnListCurrentById(
  id: string
): Promise<MesSnListCurrentDto> {
  const result = await request('/api/MesSnListCurrent/GetMesSnListCurrentById', {
    method: 'GET',
    params: { id },
  });

  return result;
}

/**
 * 根据SN号获取SN实时状态详情
 * @param snNumber SN号
 * @returns SN实时状态详情
 */
export async function getMesSnListCurrentBySnNumber(
  snNumber: string
): Promise<MesSnListCurrentDto> {
  const result = await request('/api/MesSnListCurrent/GetMesSnListCurrentBySnNumber', {
    method: 'GET',
    params: { snNumber },
  });

  return result;
}
