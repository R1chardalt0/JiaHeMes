import React from 'react';
import { Modal, Input, Button, Table } from 'antd';

// ProcessRouteModal组件的Props接口
export interface ProcessRouteModalProps {
  open: boolean;
  onCancel: () => void;
  processRoutes: any[];
  processRouteTotal: number;
  processRouteCurrent: number;
  processRoutePageSize: number;
  processRouteSearchValues: { routeCode: string; routeName: string };
  onProcessRouteSelect: (processRoute: any) => void;
  onProcessRouteSearch: () => void;
  onProcessRouteSearchInputChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onProcessRoutePaginationChange: (current: number, pageSize: number) => void;
}

const ProcessRouteModal: React.FC<ProcessRouteModalProps> = ({
  open,
  onCancel,
  processRoutes,
  processRouteTotal,
  processRouteCurrent,
  processRoutePageSize,
  processRouteSearchValues,
  onProcessRouteSelect,
  onProcessRouteSearch,
  onProcessRouteSearchInputChange,
  onProcessRoutePaginationChange
}) => {
  return (
    <Modal
      title="选择工艺路线"
      open={open}
      onCancel={onCancel}
      footer={null}
      width={800}
      zIndex={1001}
    >
      <div style={{ marginBottom: 16, display: 'flex', gap: 8 }}>
        <Input
          name="routeCode"
          placeholder="工艺路线编码"
          value={processRouteSearchValues.routeCode}
          onChange={onProcessRouteSearchInputChange}
          style={{ width: 200 }}
        />
        <Input
          name="routeName"
          placeholder="工艺路线名称"
          value={processRouteSearchValues.routeName}
          onChange={onProcessRouteSearchInputChange}
          style={{ width: 200 }}
        />
        <Button type="primary" onClick={onProcessRouteSearch}>搜索</Button>
      </div>
      <Table
        dataSource={processRoutes}
        columns={[
          {
            title: '工艺路线编码',
            dataIndex: 'routeCode',
            key: 'routeCode',
          },
          {
            title: '工艺路线名称',
            dataIndex: 'routeName',
            key: 'routeName',
          },
          {
            title: '工艺路线 ID',
            dataIndex: 'id',
            key: 'id',
          },
          {
            title: '操作',
            key: 'action',
            render: (_, record) => (
              <Button type="link" onClick={() => onProcessRouteSelect(record)}>选择</Button>
            ),
          },
        ]}
        pagination={{
          current: processRouteCurrent,
          pageSize: processRoutePageSize,
          total: processRouteTotal,
          onChange: onProcessRoutePaginationChange,
        }}
      />
    </Modal>
  );
};

export default ProcessRouteModal;