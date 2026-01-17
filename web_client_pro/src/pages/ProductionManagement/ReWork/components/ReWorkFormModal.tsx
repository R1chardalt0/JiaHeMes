import { useImperativeHandle, forwardRef, useState, useEffect } from 'react';
import { Modal, Form, Input, Button, Space, message, Select } from 'antd';
import type { FormInstance } from 'antd';
import { ReWorkFormData } from '@/services/Model/Production/reWork';
import { getMesSnListCurrentBySnNumber } from '@/services/Api/Trace/MesSnListCurrent';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';

// 返工表单模态框属性接口
export interface ReWorkFormModalProps {
  // 模态框是否可见
  open: boolean;
  // 模态框标题
  title: string;
  // 初始表单值
  initialValues?: ReWorkFormData;
  // 取消回调
  onCancel: () => void;
  // 保存回调
  onSave: (values: ReWorkFormData, callback: (success: boolean) => void) => void;
}

// 返工表单模态框引用接口
export interface ReWorkFormModalRef {
  // 获取表单实例
  getForm: () => FormInstance<ReWorkFormData>;
}

/**
 * 返工表单模态框组件
 * 用于SN返工操作
 */
const ReWorkFormModal = forwardRef<ReWorkFormModalRef, ReWorkFormModalProps>(({
  title,
  initialValues,
  onCancel,
  onSave
}, ref) => {
  // 创建表单实例
  const [form] = Form.useForm<ReWorkFormData>();
  // 当前SN码
  const [currentSN, setCurrentSN] = useState<string>('');
  // 当前站点
  const [currentStation, setCurrentStation] = useState<string>('');
  // 加载状态
  const [loading, setLoading] = useState<boolean>(false);
  // 是否显示当前站点信息
  const [showCurrentStation, setShowCurrentStation] = useState<boolean>(false);
  // 站点列表
  const [stations, setStations] = useState<Array<{ value: string; label: string }>>([]);
  // 是否显示目标站点下拉框
  const [showTargetStation, setShowTargetStation] = useState<boolean>(false);

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
      console.log('SN信息API返回结果:', snInfo);

      // 检查API返回结果
      if (!snInfo) {
        message.error('未找到SN信息');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 兼容不同可能的返回结构
      const currentStationListId = snInfo.currentStationListId;
      if (!currentStationListId) {
        message.error('未找到SN对应的当前站点信息');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 2. 从SN信息中获取当前站点ID
      const currentStationId = currentStationListId;
      console.log('当前站点ID:', currentStationId);

      // 3. 调用API根据站点ID获取站点详情
      const stationInfo = await getStationListById(currentStationId);
      console.log('站点信息API返回结果:', stationInfo);

      // 检查站点信息
      if (!stationInfo) {
        message.error('未找到当前站点详情');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 兼容不同可能的返回结构
      const stationCode = stationInfo.stationCode;
      if (!stationCode) {
        message.error('未找到当前站点编码');
        setShowTargetStation(false);
        setStations([]);
        setCurrentStation('');
        setShowCurrentStation(false);
        return;
      }

      // 4. 获取当前站点编码
      const currentStationCode = stationCode;
      console.log('当前站点编码:', currentStationCode);

      // 设置当前站点
      setCurrentStation(currentStationCode);
      setShowCurrentStation(true);

      // 5. 生成所有站点
      const allStations = generateStationList();
      console.log('所有站点列表:', allStations);

      // 6. 找到当前站点的索引
      const currentIndex = allStations.findIndex(s => s.value === currentStationCode);
      console.log('当前站点索引:', currentIndex);

      if (currentIndex === -1) {
        message.error('当前站点不在预设站点列表中');
        setShowTargetStation(false);
        setStations([]);
        return;
      }

      // 7. 只显示当前站点之前的站点（返工操作只能选择之前的站点）
      const availableStations = allStations.slice(0, currentIndex);
      console.log('可用目标站点列表:', availableStations);
      setStations(availableStations);
      setShowTargetStation(true);
      form.setFieldsValue({ targetStationId: '' });

    } catch (error) {
      message.error('获取SN当前状态失败，请稍后重试');
      console.error('获取SN当前状态失败:', error);
      // 即使API调用失败，也显示目标站点选择框（使用所有站点）
      const allStations = generateStationList();
      setStations(allStations);
      setShowTargetStation(true);
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

          {showTargetStation && (
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
          )}

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

ReWorkFormModal.displayName = 'ReWorkFormModal';

export default ReWorkFormModal;