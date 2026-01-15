import React, { useState, useEffect } from 'react';
import { Modal, Input, Button, Table, Space } from 'antd';
import { getDeviceInfoList } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import type { DeviceInfo } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';

// 设备选择弹窗组件的Props接口
export interface DeviceSelectModalProps {
  open: boolean;
  onCancel: () => void;
  devices: DeviceInfo[];
  deviceTotal: number;
  deviceCurrent: number;
  devicePageSize: number;
  deviceSearchValues: { resource: string; resourceName: string };
  onDeviceSelect: (device: DeviceInfo) => void;
  onDeviceSearch: () => void;
  onDeviceSearchInputChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onDevicePaginationChange: (current: number, pageSize: number) => void;
}

const DeviceSelectModal: React.FC<DeviceSelectModalProps> = ({
  open,
  onCancel,
  devices,
  deviceTotal,
  deviceCurrent,
  devicePageSize,
  deviceSearchValues,
  onDeviceSelect,
  onDeviceSearch,
  onDeviceSearchInputChange,
  onDevicePaginationChange
}) => {
  const columns = [
    {
      title: '设备编码',
      dataIndex: 'resource',
      key: 'resource',
    },
    {
      title: '设备名称',
      dataIndex: 'resourceName',
      key: 'resourceName',
    },
    {
      title: '工单编码',
      dataIndex: 'workOrderCode',
      key: 'workOrderCode',
    },
    {
      title: '设备类型',
      dataIndex: 'resourceType',
      key: 'resourceType',
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
    },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: DeviceInfo) => (
        <Button type="link" onClick={() => onDeviceSelect(record)}>选择</Button>
      ),
    },
  ];

  return (
    <Modal
      title="选择设备"
      open={open}
      onCancel={onCancel}
      footer={null}
      width={1000}
      zIndex={1001}
    >
      <div style={{ marginBottom: 16, display: 'flex', gap: 8, flexWrap: 'wrap' }}>
        <Input
          name="resource"
          placeholder="设备编码"
          value={deviceSearchValues.resource}
          onChange={onDeviceSearchInputChange}
          style={{ width: 200 }}
        />
        <Input
          name="resourceName"
          placeholder="设备名称"
          value={deviceSearchValues.resourceName}
          onChange={onDeviceSearchInputChange}
          style={{ width: 200 }}
        />
        <Button type="primary" onClick={onDeviceSearch}>搜索</Button>
      </div>
      <Table
        dataSource={devices}
        columns={columns}
        rowKey="resourceId"
        pagination={{
          current: deviceCurrent,
          pageSize: devicePageSize,
          total: deviceTotal,
          onChange: onDevicePaginationChange,
        }}
      />
    </Modal>
  );
};

export default DeviceSelectModal;