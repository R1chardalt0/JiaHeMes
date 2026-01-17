import React, { useRef } from 'react';
import { message } from 'antd';
import { reWork } from '@/services/Api/Production/reWork';
import { ReWorkFormData, ReWorkParams } from '@/services/Model/Production/reWork';
import ReWorkFormModal, { ReWorkFormModalRef } from './components/ReWorkFormModal';

/**
 * 返工页面
 */
const ReWorkPage: React.FC = () => {
  // 表单引用
  const formModalRef = useRef<ReWorkFormModalRef>(null);

  // 处理返工
  const handleReWork = async (values: ReWorkFormData, callback: (success: boolean) => void) => {
    try {
      // 转换为后端接口需要的参数格式
      const requestParams: ReWorkParams = {
        SN: values.sn,
        TargetStationCode: values.targetStationId
      };

      // 调用返工接口
      const result = await reWork(requestParams);

      if (result.code === 200) {
        message.success(result.message);
        callback(true); // 成功时回调
      } else {
        message.error(result.message);
        callback(false); // 失败时回调
      }
    } catch (error) {
      message.error('返工失败，请稍后重试');
      console.error('返工失败:', error);
      callback(false); // 异常时回调
    }
  };

  // 处理取消
  const handleCancel = () => {
    // 重置表单
    if (formModalRef.current) {
      const form = formModalRef.current.getForm();
      form.resetFields();
    }
  };

  return (
    <div style={{ padding: '20px' }}>
      {/* 直接显示返工表单 */}
      <ReWorkFormModal
        ref={formModalRef}
        open={true}
        title="返工操作"
        onCancel={handleCancel}
        onSave={handleReWork}
      />
    </div>
  );
};

export default ReWorkPage;