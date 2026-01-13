import React, { useImperativeHandle, forwardRef } from 'react';
import { Modal, Form, Input, Button, InputNumber } from 'antd';
import type { FormInstance } from 'antd';
import { FeedMaterialFormData } from '@/services/Model/Production/feedMaterial';

// 物料上传表单模态框属性接口
export interface FeedMaterialFormModalProps {
  // 模态框是否可见
  open: boolean;
  // 模态框标题
  title: string;
  // 初始表单值
  initialValues?: FeedMaterialFormData;
  // 取消回调
  onCancel: () => void;
  // 保存回调
  onSave: (values: FeedMaterialFormData) => void;
}

// 物料上传表单模态框引用接口
export interface FeedMaterialFormModalRef {
  // 获取表单实例
  getForm: () => FormInstance<FeedMaterialFormData>;
}

/**
 * 物料上传表单模态框组件
 * 用于物料批次上传
 */
const FeedMaterialFormModal = forwardRef<FeedMaterialFormModalRef, FeedMaterialFormModalProps>(({
  open,
  title,
  initialValues,
  onCancel,
  onSave
}, ref) => {
  // 创建表单实例
  const [form] = Form.useForm<FeedMaterialFormData>();

  // 暴露form实例给父组件
  useImperativeHandle(ref, () => ({
    getForm: () => form
  }));

  // 处理取消
  const handleCancel = () => {
    form.resetFields();
    onCancel();
  };

  // 处理保存
  const handleSave = () => {
    form.validateFields().then(values => {
      onSave(values);
      form.resetFields();
    });
  };

  return (
    <Modal
      title={title}
      open={open}
      onCancel={handleCancel}
      onOk={handleSave}
      width={600}
      destroyOnClose
    >
      <Form
        form={form}
        layout="vertical"
        autoComplete="off"
        initialValues={initialValues}
      >
        <Form.Item
          label="批次号"
          name="batchCode"
          rules={[
            { required: true, message: '请输入批次号' },
          ]}
        >
          <Input placeholder="请输入批次号" />
        </Form.Item>

        <Form.Item
          label="设备资源标识"
          name="resource"
          rules={[
            { required: true, message: '请输入设备资源标识' },
          ]}
        >
          <Input placeholder="请输入设备资源标识" />
        </Form.Item>

        <Form.Item
          label="站点编码"
          name="stationCode"
          rules={[
            { required: true, message: '请输入站点编码' },
          ]}
        >
          <Input placeholder="请输入站点编码" />
        </Form.Item>

        <Form.Item
          label="工单编号"
          name="workOrderCode"
          rules={[
            { required: false, message: '请输入工单编号' },
          ]}
        >
          <Input placeholder="请输入工单编号" />
        </Form.Item>

        <Form.Item
          label="批次数量"
          name="batchQty"
          rules={[
            { required: false, message: '请输入批次数量' },
            { type: 'number', min: 0, message: '批次数量必须大于等于0' },
          ]}
        >
          <InputNumber placeholder="请输入批次数量" style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item
          label="产品编码"
          name="productCode"
          rules={[
            { required: true, message: '请输入产品编码' },
          ]}
        >
          <Input placeholder="请输入产品编码" />
        </Form.Item>
      </Form>
    </Modal>
  );
});

FeedMaterialFormModal.displayName = 'FeedMaterialFormModal';

export default FeedMaterialFormModal;