import React, { useRef } from 'react';
import { message } from 'antd';
import { feedMaterial } from '@/services/Api/Production/feedMaterial';
import { FeedMaterialFormData, RequestFeedMaterialParams } from '@/services/Model/Production/feedMaterial';
import FeedMaterialFormModal, { FeedMaterialFormModalRef } from './components/FeedMaterialFormModal';

/**
 * 物料上传页面
 */
const FeedMaterialPage: React.FC = () => {
  // 表单引用
  const formModalRef = useRef<FeedMaterialFormModalRef>(null);

  // 处理物料上传
  const handleFeedMaterial = async (values: FeedMaterialFormData, callback: (success: boolean) => void) => {
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
        callback(true); // 成功时回调
      } else {
        message.error(result.message);
        callback(false); // 失败时回调
      }
    } catch (error) {
      message.error('上传失败，请稍后重试');
      console.error('物料上传失败:', error);
      callback(false); // 异常时回调
    }
  };

  // 处理取消
  const handleCancel = () => {
    // 由于表单现在直接显示在页面上，取消操作可以留空
    // 或者可以重置表单
    if (formModalRef.current) {
      const form = formModalRef.current.getForm();
      form.resetFields();
    }
  };

  return (
    <div style={{ padding: '20px' }}>
      {/* 直接显示物料上传表单 */}
      <FeedMaterialFormModal
        ref={formModalRef}
        open={true}
        title="物料上传"
        onCancel={handleCancel}
        onSave={handleFeedMaterial}
      />
    </div>
  );
};

export default FeedMaterialPage;
