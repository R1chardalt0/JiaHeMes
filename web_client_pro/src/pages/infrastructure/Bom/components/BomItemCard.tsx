// src/pages/Bom/components/BomItemCard.tsx
import React from 'react';
import { Card, Button, Popconfirm, Row, Col } from 'antd';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';

interface BomItemCardProps {
  item: any;
  onEdit: (item: any) => void;
  onDelete: (id: string) => void;
}

const BomItemCard: React.FC<BomItemCardProps> = ({ item, onEdit, onDelete }) => {
  return (
    <Card
      title={`BOM子项`}
      extra={
        <div>
          <Button
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={(e) => {
              e.stopPropagation();
              onEdit(item);
            }}
            style={{ marginRight: 8 }}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除"
            description="确定要删除该BOM子项吗？"
            onConfirm={(e) => {
              e?.stopPropagation();
              onDelete(item.bomItemId);
            }}
            okText="确定"
            cancelText="取消"
          >
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={(e) => e.stopPropagation()}
            >
              删除
            </Button>
          </Popconfirm>
        </div>
      }
    >
      <div style={{ lineHeight: '1.8', fontSize: '14px' }}>
  <div style={{ display: 'flex', flexWrap: 'wrap' }}>
    <div style={{ width: '50%', marginBottom: '8px', paddingRight: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>站点编码:</strong> {item.stationCode}</p>
    </div>
    <div style={{ width: '50%', marginBottom: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>批次规则:</strong> {item.batchRule}</p>
    </div>
    
    <div style={{ width: '50%', marginBottom: '8px', paddingRight: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>批次数据固定:</strong> {item.batchQty ? '是' : '否'}</p>
    </div>
    <div style={{ width: '50%', marginBottom: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>批次SN数量:</strong> {item.batchSNQty}</p>
    </div>
    
    <div style={{ width: '50%', marginBottom: '8px', paddingRight: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>产品名称:</strong> {item.productName || '-'}</p>
    </div>
    <div style={{ width: '50%', marginBottom: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>产品编码:</strong> {item.productCode || '-'}</p>
    </div>
    
    <div style={{ width: '50%', marginBottom: '8px', paddingRight: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>日期序列:</strong> {item.dateIndex || '-'}</p>
    </div>
    <div style={{ width: '50%', marginBottom: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>数量序列:</strong> {item.numberIndex || '-'}</p>
    </div>
    
    <div style={{ width: '50%', marginBottom: '8px', paddingRight: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>物料保质期（分钟）:</strong> {item.shelfLife || '-'}</p>
    </div>
    <div style={{ width: '50%', marginBottom: '8px' }}>
      <p style={{ margin: 0, wordBreak: 'break-all' }}><strong>物料号序列:</strong> {item.productIndex || '-'}</p>
    </div>
  </div>
</div>
    </Card>
  );
};

export default BomItemCard;