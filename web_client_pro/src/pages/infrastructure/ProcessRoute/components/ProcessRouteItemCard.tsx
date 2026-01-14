import React from 'react';
import { Card, Button, Col } from 'antd';
import { EditOutlined, DeleteOutlined, EyeOutlined } from '@ant-design/icons';
import { ProcessRouteItem } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItem';

interface ProcessRouteItemCardProps {
  item: ProcessRouteItem;
  onEdit: (item: ProcessRouteItem) => void;
  onDelete: (id: string) => void;
  onViewTestItems: (processRouteItemId: string) => void;
}

/**
 * 工艺路线子项卡片组件
 * 用于显示单个工艺路线子项的详细信息
 */
const ProcessRouteItemCard: React.FC<ProcessRouteItemCardProps> = ({ item, onEdit, onDelete, onViewTestItems }) => {
  return (
    <Col xs={24} sm={12} md={8} style={{ marginBottom: 16 }}>
      <Card
        title={`${item.stationCode}`}
        bordered={true}
        extra={
          <div style={{ display: 'flex', gap: 8 }}>
            <Button
              type="link"
              size="small"
              icon={<EyeOutlined />}
              onClick={() => onViewTestItems(item.id)}
            >
              查看测试项
            </Button>
            <Button
              type="link"
              size="small"
              icon={<EditOutlined />}
              onClick={() => onEdit(item)}
            >
              编辑
            </Button>
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={() => onDelete(item.id)}
            >
              删除
            </Button>
          </div>
        }
      >
        <div style={{ lineHeight: '1.8', fontSize: '14px' }}>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px', marginBottom: '8px' }}>
            <div style={{ flex: '1 1 40%', minWidth: '150px' }}>
              <strong>站点编码:</strong> {item.stationCode}
            </div>
            <div style={{ flex: '1 1 40%', minWidth: '150px' }}>
              <strong>工艺路线序号:</strong> {item.routeSeq}
            </div>
          </div>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px', marginBottom: '8px' }}>
            <div style={{ flex: '1 1 40%', minWidth: '150px' }}>
              <strong>是否必经站点:</strong> {item.mustPassStation ? '是' : '否'}
            </div>
            <div style={{ flex: '1 1 40%', minWidth: '150px' }}>
              <strong>是否首站点:</strong> {item.firstStation ? '是' : '否'}
            </div>
          </div>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px', marginBottom: '8px' }}>
            <div style={{ flex: '1 1 40%', minWidth: '150px' }}>
              <strong>是否检查所有:</strong> {item.checkAll ? '是' : '否'}
            </div>
            <div style={{ flex: '1 1 40%', minWidth: '150px' }}>
              <strong>最大NG次数:</strong> {item.maxNGCount}
            </div>
          </div>
          <div style={{ marginBottom: '8px', wordBreak: 'break-all' }}>
            <strong>检查站点列表:</strong> {item.checkStationList}
          </div>
        </div>
      </Card>
    </Col>
  );
};

export default ProcessRouteItemCard;