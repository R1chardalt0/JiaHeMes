import { request } from '@umijs/max';
import type { DeviceInfo, DeviceInfoQueryParams } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';

/**
 * 获取设备列表
 * @param params 查询参数
 * @returns 设备列表
 */
export async function getDeviceInfoList(params: DeviceInfoQueryParams): Promise<{ data: DeviceInfo[] }> {
  return request('/api/Deviceinfo/GetDeviceInfoList', {
    method: 'GET',
    params,
  });
}

/**
 * 根据设备ID获取设备详情
 * @param deviceId 设备ID
 * @returns 设备详情
 */
export async function getDeviceInfoById(deviceId: string): Promise<{ data: DeviceInfo }> {
  return request(`/api/Deviceinfo/${deviceId}`, {
    method: 'GET',
  });
}

/**
 * 根据设备编码获取设备信息
 * @param deviceEnCode 设备编码
 * @returns 设备信息
 */
export async function getDeviceInfoByEnCode(deviceEnCode: string): Promise<{ data: DeviceInfo }> {
  return request(`/api/Deviceinfo/ByEnCode/${deviceEnCode}`, {
    method: 'GET',
  });
}

/**
 * 创建设备信息
 * @param data 设备数据
 * @returns 创建结果
 */
export async function createDeviceInfo(data: DeviceInfo): Promise<{ success: boolean; message?: string }> {
  return request('/api/Deviceinfo/CreateDeviceInfo', {
    method: 'POST',
    data,
  });
}

/**
 * 更新设备信息
 * @param data 设备数据
 * @returns 更新结果
 */
export async function updateDeviceInfo(data: DeviceInfo): Promise<{ success: boolean; message?: string }> {
  return request('/api/Deviceinfo/UpdateDeviceInfo', {
    method: 'POST',
    data,
  });
}

/**
 * 批量删除设备信息
 * @param deviceIds 设备ID列表
 * @returns 删除结果
 */
export async function deleteDeviceInfoByIds(deviceIds: string[]): Promise<{ success: boolean; message?: string }> {
  return request('/api/Deviceinfo/DeleteDeviceInfoByIds', {
    method: 'POST',
    data: deviceIds,
  });
}

/**
 * 获取所有设备列表
 * @returns 所有设备列表
 */
export async function getAllDeviceInfos(): Promise<{ data: DeviceInfo[] }> {
  return request('/api/Deviceinfo/All', {
    method: 'GET',
  });
}

/**
 * 根据生产线ID获取设备列表
 * @param productionLineId 生产线ID
 * @returns 设备列表
 */
export async function getDeviceInfosByProductionLineId(productionLineId: string): Promise<{ data: DeviceInfo[] }> {
  return request(`/api/Deviceinfo/ByProductionLineId/${productionLineId}`, {
    method: 'GET',
  });
}

