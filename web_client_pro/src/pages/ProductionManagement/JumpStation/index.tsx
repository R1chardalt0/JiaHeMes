import React, { useRef } from 'react';
import { message } from 'antd';
import { jumpStation } from '@/services/Api/Production/jumpStation';
import { JumpStationFormData, JumpStationParams } from '@/services/Model/Production/jumpStation';
import JumpStationFormModal, { JumpStationFormModalRef } from './components/JumpStationFormModal';

/**
 * 跳站页面
 */
const JumpStationPage: React.FC = () => {
  // 表单引用
  const formModalRef = useRef<JumpStationFormModalRef>(null);

  // 处理跳站
  const handleJumpStation = async (values: JumpStationFormData, callback: (success: boolean) => void) => {
    try {
      // 转换为后端接口需要的参数格式
      const requestParams: JumpStationParams = {
        SN: values.sn,
        JumpStationCode: values.targetStationId
      };

      // 调用跳站接口
      const result = await jumpStation(requestParams);

      if (result.code === 200) {
        message.success(result.message);
        callback(true); // 成功时回调
      } else {
        message.error(result.message);
        callback(false); // 失败时回调
      }
    } catch (error) {
      message.error('跳站失败，请稍后重试');
      console.error('跳站失败:', error);
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
      {/* 直接显示跳站表单 */}
      <JumpStationFormModal
        ref={formModalRef}
        open={true}
        title="跳站操作"
        onCancel={handleCancel}
        onSave={handleJumpStation}
      />
    </div>
  );
};

export default JumpStationPage;