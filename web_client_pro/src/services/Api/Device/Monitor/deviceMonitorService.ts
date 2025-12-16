import { request } from '@umijs/max';
import type { ApiResponse, DeviceMonitorDataType, EquipmentTraceData, GetEquipmentTracinfosListParams, DeviceDetailType, GetDeviceDetailParams } from '../../../../services/Model/Device/device_monitor_data';

export async function queryFakeList(params: {
  count: number;
}): Promise<{ data: { list: DeviceMonitorDataType[] } }> {
  return request('/api/card_fake_list', {
    params,
  });
}

/**
 * 根据设备编码获取设备追踪信息列表
 * @param params 请求参数，包含设备编码和获取数量
 * @param options 可选的请求配置
 * @returns 包含设备追踪信息列表的响应体
 */
export async function getEquipmentTracinfosListByDeviceEnCode(
  params: GetEquipmentTracinfosListParams,
  options?: Record<string, any>
): Promise<ApiResponse<EquipmentTraceData[]>> {
  try {
    return await request<ApiResponse<EquipmentTraceData[]>>('/api/EqumentTraceinfo/GetEquipmentTracinfosListByDeviceEnCode', {
      method: 'GET',
      params: { // GET请求参数放在params中
        deviceEnCode: params.deviceEnCode,
        size: params.size,
      },
      ...(options || {}),
    });
  } catch (error) {
    // 请求失败或网络错误处理
    console.error('获取设备追踪信息列表失败:', error);
    return {
      success: false,
      data: [], 
      errorInfo: {
        code: '500',
        message: (error as Error).message || '网络请求异常',
        errorFields: [],
      },
      errorKey: '',        
      errorMessage: '',   
    };
  }
}

/**
 * 获取所有设备详细信息
 * @param options 可选的请求配置
 * @returns 包含设备详细信息的响应体
 */
export async function getDeviceDetail(
  options?: Record<string, any>
): Promise<ApiResponse<DeviceDetailType[]>> {
  try {
    return await request<ApiResponse<DeviceDetailType[]>>('/api/Deviceinfo/GetAllDeviceInfos/All', {
      method: 'GET',
      ...(options || {}),
    });
  } catch (error) {
    // 请求失败或网络错误处理
    console.error('获取设备详细信息失败:', error);
    return {
      success: false,
      data: [],
      errorInfo: {
        code: '500',
        message: (error as Error).message || '网络请求异常',
        errorFields: [],
      },
      errorKey: '',
      errorMessage: '',
    };
  }
}

