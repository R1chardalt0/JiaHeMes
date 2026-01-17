import React, { useImperativeHandle, forwardRef, useState, useEffect } from 'react';
import { Modal, Form, Input, Button, Space, Select, message } from 'antd';
import type { FormInstance } from 'antd';
import { JumpStationFormData } from '@/services/Model/Production/jumpStation';
import { getMesSnListCurrentBySnNumber } from '@/services/Api/Trace/MesSnListCurrent';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';

// 跳站表单模态框属性接口
export interface JumpStationFormModalProps {
  // 模态框是否可见
  open: boolean;
  // 模态框标题
  title: string;
  // 初始表单值
  initialValues?: JumpStationFormData;
  // 取消回调
  onCancel: () => void;
  // 保存回调
  onSave: (values: JumpStationFormData, callback: (success: boolean) => void) => void;
}

// 跳站表单模态框引用接口
export interface JumpStationFormModalRef {
  // 获取表单实例
  getForm: () => FormInstance<JumpStationFormData>;
}

/**
 * 跳站表单模态框组件
 * 用于SN跳站操作
 */
const JumpStationFormModal = forwardRef<JumpStationFormModalRef, JumpStationFormModalProps>(({
  open,
  title,
  initialValues,
  onCancel,
  onSave
}, ref) => {
  // 创建表单实例
  const [form] = Form.useForm<JumpStationFormData>();
  // 站点列表状态
  const [stations, setStations] = useState<Array<{ value: string; label: string }>>([]);
  // 当前SN码
  const [currentSN, setCurrentSN] = useState<string>('');
  // 当前站点
  const [currentStation, setCurrentStation] = useState<string>('');
  // 加载状态
  const [loading, setLoading] = useState<boolean>(false);
  // 是否显示目标站点下拉框
  const [showTargetStation, setShowTargetStation] = useState<boolean>(false);
  // 是否显示当前站点信息
  const [showCurrentStation, setShowCurrentStation] = useState<boolean>(false);



  // 暴露form实例给父组件
  useImperativeHandle(ref, () => ({
    getForm: () => form
  }));

  // 生成站点列表（OP10到OP170）
  const generateStationList = () => {
    const stationList = [];
    for (let i = 10; i <= 170; i += 10) {
      const stationCode = `OP${i}`;
      stationList.push({ value: stationCode, label: stationCode });
    }
    return stationList;
  };

  // 监听SN码变化
  const handleSNChange = async (value: string) => {
    setCurrentSN(value);

    if (!value) {
      setShowTargetStation(false);
      setStations([]);
      setCurrentStation('');
      setShowCurrentStation(false);
      form.setFieldsValue({ targetStationId: '' });
      return;
    }

    try {
      setLoading(true);
      // 1. 调用真实API获取SN当前状态
      const snInfo = await getMesSnListCurrentBySnNumber(value);

      // 检查API返回结果
      if (!snInfo || !snInfo.currentStationListId) {
        message.error('未找到SN对应的当前站点信息');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 2. 从SN信息中获取当前站点ID
      const currentStationId = snInfo.currentStationListId;

      // 3. 调用API根据站点ID获取站点详情
      const stationInfo = await getStationListById(currentStationId);

      // 检查站点信息
      if (!stationInfo || !stationInfo.stationCode) {
        message.error('未找到当前站点详情');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 4. 获取当前站点编码
      const currentStationCode = stationInfo.stationCode;

      // 设置当前站点
      setCurrentStation(currentStationCode);
      setShowCurrentStation(true);

      // 生成所有站点
      const allStations = generateStationList();

      // 找到当前站点的索引
      const currentIndex = allStations.findIndex(s => s.value === currentStationCode);

      if (currentIndex === -1) {
        message.error('未找到当前站点信息');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 只显示当前站点之后的站点
      const availableStations = allStations.slice(currentIndex + 1);
      setStations(availableStations);
      setShowTargetStation(true);
      form.setFieldsValue({ targetStationId: '' });
    } catch (error) {
      message.error('获取SN当前状态失败，请稍后重试');
      console.error('获取SN当前状态失败:', error);
      setShowTargetStation(false);
      setStations([]);
      setCurrentStation('');
      setShowCurrentStation(false);
    } finally {
      setLoading(false);
    }
  };

  // 处理取消
  const handleCancel = () => {
    form.resetFields();
    setCurrentSN('');
    setCurrentStation('');
    setShowCurrentStation(false);
    setShowTargetStation(false);
    setStations([]);
    onCancel();
  };

  // 处理保存
  const handleSave = async () => {
    try {
      const values = await form.validateFields();
      // 调用保存回调，等待其完成
      const saveResult = await new Promise((resolve) => {
        // 传递一个回调函数给onSave，让它通知我们结果
        onSave(values, (success: boolean) => {
          resolve(success);
        });
      });

      // 只有在保存成功时才重置表单和状态
      if (saveResult === true) {
        form.resetFields();
        setCurrentSN('');
        setCurrentStation('');
        setShowCurrentStation(false);
        setShowTargetStation(false);
        setStations([]);
      }
    } catch (error) {
      // 表单验证失败或其他错误，不重置表单
      console.error('保存失败:', error);
    }
  };

  return (
    <>
      <div style={{ maxWidth: 600, margin: '0 auto' }}>
        <h2>{title}</h2>
        <Form
          form={form}
          layout="vertical"
          autoComplete="off"
          initialValues={initialValues}
        >
          <Form.Item
            label="SN码"
            name="sn"
            rules={[
              { required: true, message: '请输入SN码' },
            ]}
          >
            <Input
              placeholder="请输入SN码"
              onChange={(e) => {
                handleSNChange(e.target.value);
              }}
            />
          </Form.Item>

          <Form.Item
            label="当前站点编码"
          >
            <Input
              value={currentStation}
              readOnly
              style={{ backgroundColor: '#f5f5f5' }}
            />
          </Form.Item>

          <Form.Item
            label="目标站点编码"
            name="targetStationId"
            rules={[
              { required: true, message: '请选择目标站点编码' },
            ]}
          >
            <Select
              placeholder="请选择目标站点编码"
              options={stations}
              loading={loading}
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item>
            <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
              <Button onClick={handleCancel}>重置</Button>
              <Button type="primary" onClick={handleSave}>保存</Button>
            </div>
          </Form.Item>
        </Form>
      </div>
    </>
  );
});

JumpStationFormModal.displayName = 'JumpStationFormModal';

export default JumpStationFormModal;