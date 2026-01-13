import React, { useState, useRef } from 'react';
import { Button, Card, message, Typography } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { feedMaterial } from '@/services/Api/Production/feedMaterial';
import { FeedMaterial, FeedMaterialFormData, RequestFeedMaterialParams } from '@/services/Model/Production/feedMaterial';
import FeedMaterialFormModal, { FeedMaterialFormModalRef } from './components/FeedMaterialFormModal';

const { Title } = Typography;

/**
 * 物料上传页面
 */
const FeedMaterialPage: React.FC = () => {
  // 控制表单模态框显示
  const [modalVisible, setModalVisible] = useState<boolean>(false);
  // 表单模态框引用
  const formModalRef = useRef<FeedMaterialFormModalRef>(null);

  // 打开上料模态框
  const handleOpenFeedModal = () => {
    setModalVisible(true);
  };

  // 关闭上料模态框
  const handleCloseFeedModal = () => {
    setModalVisible(false);
  };

  // 处理物料上传
  const handleFeedMaterial = async (values: FeedMaterialFormData) => {
    try {
      // 转换为后端接口需要的参数格式
      const requestParams: RequestFeedMaterialParams = {
        BatchCode: values.batchCode,
        Resource: values.resource,
        StationCode: values.stationCode,
        WorkOrderCode: values.workOrderCode,
        BatchQty: values.batchQty,
        ProductCode: values.productCode
      };

      // 调用物料上传接口
      const result = await feedMaterial(requestParams);

      if (result.code === 200) {
        message.success(result.message);
        setModalVisible(false);
      } else {
        message.error(result.message);
      }
    } catch (error) {
      message.error('上传失败，请稍后重试');
      console.error('物料上传失败:', error);
    }
  };

  return (
    <div style={{ padding: '20px' }}>
      <Card>
        <Title level={4} style={{ marginBottom: '20px' }}>物料上传</Title>

        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleOpenFeedModal}
          style={{ marginBottom: '20px' }}
        >
          上料
        </Button>

        {/* 物料上传表单模态框 */}
        <FeedMaterialFormModal
          ref={formModalRef}
          open={modalVisible}
          title="物料上传"
          onCancel={handleCloseFeedModal}
          onSave={handleFeedMaterial}
        />
      </Card>
    </div>
  );
};

export default FeedMaterialPage;
