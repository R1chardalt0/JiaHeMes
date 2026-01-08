import React from 'react';
import { Row, Col, Button } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { ProcessRouteItem } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRouteItem';
import ProcessRouteItemCard from './ProcessRouteItemCard';

interface ProcessRouteItemListProps {
  processRouteItems: ProcessRouteItem[];
  onEdit: (item: ProcessRouteItem) => void;
  onDelete: (id: string) => void;
}

/**
 * 工艺路线子项列表组件
 * 用于显示工艺路线子项的列表
 */
const ProcessRouteItemList: React.FC<ProcessRouteItemListProps> = ({ 
  processRouteItems, 
  onEdit, 
  onDelete 
}) => {
  return (
    <>
      <Row gutter={[16, 16]}>
        {processRouteItems.length > 0 ? (
          processRouteItems.map((item) => (
            <ProcessRouteItemCard 
              key={item.id} 
              item={item} 
              onEdit={onEdit} 
              onDelete={onDelete} 
            />
          ))
        ) : (
          <Col span={24}>
            <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>
              暂无工艺路线子项数据
            </div>
          </Col>
        )}
      </Row>
    </>
  );
};

export default ProcessRouteItemList;