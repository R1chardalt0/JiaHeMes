import React from 'react';
import { Modal, Input, Button, Table } from 'antd';

// BomSelectModal组件的Props接口
export interface BomSelectModalProps {
  open: boolean;
  onCancel: () => void;
  boms: any[];
  bomTotal: number;
  bomCurrent: number;
  bomPageSize: number;
  bomSearchValues: { bomCode: string; bomName: string };
  onBomSelect: (bom: any) => void;
  onBomSearch: () => void;
  onBomSearchInputChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onBomPaginationChange: (current: number, pageSize: number) => void;
}

const BomSelectModal: React.FC<BomSelectModalProps> = ({
  open,
  onCancel,
  boms,
  bomTotal,
  bomCurrent,
  bomPageSize,
  bomSearchValues,
  onBomSelect,
  onBomSearch,
  onBomSearchInputChange,
  onBomPaginationChange
}) => {
  return (
    <Modal
      title="选择BOM"
      open={open}
      onCancel={onCancel}
      footer={null}
      width={800}
      zIndex={1001}
    >
      <div style={{ marginBottom: 16, display: 'flex', gap: 8 }}>
        <Input
          name="bomCode"
          placeholder="BOM编码"
          value={bomSearchValues.bomCode}
          onChange={onBomSearchInputChange}
          style={{ width: 200 }}
        />
        <Input
          name="bomName"
          placeholder="BOM名称"
          value={bomSearchValues.bomName}
          onChange={onBomSearchInputChange}
          style={{ width: 200 }}
        />
        <Button type="primary" onClick={onBomSearch}>搜索</Button>
      </div>
      <Table
        dataSource={boms}
        columns={[
          {
            title: 'BOM编码',
            dataIndex: 'bomCode',
            key: 'bomCode',
          },
          {
            title: 'BOM名称',
            dataIndex: 'bomName',
            key: 'bomName',
          },
          {
            title: 'BOM ID',
            dataIndex: 'bomId',
            key: 'bomId',
          },
          {
            title: '操作',
            key: 'action',
            render: (_, record) => (
              <Button type="link" onClick={() => onBomSelect(record)}>选择</Button>
            ),
          },
        ]}
        pagination={{
          current: bomCurrent,
          pageSize: bomPageSize,
          total: bomTotal,
          onChange: onBomPaginationChange,
        }}
      />
    </Modal>
  );
};

export default BomSelectModal;