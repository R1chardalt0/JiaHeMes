import React from 'react';
import { Card, Button, Col } from 'antd';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { ProcessRouteItem } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItem';

interface ProcessRouteItemCardProps {
  item: ProcessRouteItem;
  onEdit: (item: ProcessRouteItem) => void;
  onDelete: (id: string) => void;
}

/**
 * 工艺路线子项卡片组件
 * 用于显示单个工艺路线子项的详细信息
 */
const ProcessRouteItemCard: React.FC<ProcessRouteItemCardProps> = ({ item, onEdit, onDelete }) => {
  return (
    <Col xs={24} sm={12} md={8} style={{ marginBottom: 16 }}>
      <Card
        title={`站点: ${item.stationCode}`}
        bordered={true}
        extra={
          <div>
            <Button
              type="link"
              size="small"
              icon={<EditOutlined />}
              onClick={() => onEdit(item)}
              style={{ marginRight: 8 }}
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
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>工艺路线子项ID:</strong> {item.id}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>主表ID:</strong> {item.headId}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>站点编码:</strong> {item.stationCode}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>工艺路线序号:</strong> {item.routeSeq}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>是否必经站点:</strong> {item.mustPassStation ? '是' : '否'}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>检查站点列表:</strong> {item.checkStationList}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>是否首站点:</strong> {item.firstStation ? '是' : '否'}</p>
          <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>是否检查所有:</strong> {item.checkAll ? '是' : '否'}</p>
        </div>
      </Card>
    </Col>
  );
};

export default ProcessRouteItemCard;