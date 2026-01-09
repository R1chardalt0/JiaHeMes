// src/pages/infrastructure/Bom/components/BomFormModal.tsx
import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Button } from 'antd';
import { BomListDto } from '@/services/Model/Infrastructure/Bom/BomList';

interface BomFormModalProps {
  open: boolean;
  onCancel: () => void;
  onOk: () => void;
  editingBom: BomListDto | null;
  form: any;
}

const BomFormModal: React.FC<BomFormModalProps> = ({
  open,
  onCancel,
  onOk,
  editingBom,
  form
}) => {
  useEffect(() => {
    if (editingBom) {
      form.setFieldsValue({
        bomName: editingBom.bomName,
        bomCode: editingBom.bomCode,
        status: editingBom.status,
        remark: editingBom.remark
      });
    } else {
      form.resetFields();
    }
  }, [editingBom, form]);

  return (
    <Modal
      title={editingBom ? "编辑BOM" : "新建BOM"}
      open={open}
      onOk={onOk}
      onCancel={onCancel}
      width={600}
    >
      <Form form={form} layout="vertical">
        <Form.Item
          name="bomName"
          label="BOM名称"
          rules={[{ required: true, message: '请输入BOM名称' }]}
        >
          <Input placeholder="请输入BOM名称" />
        </Form.Item>
        <Form.Item
          name="bomCode"
          label="BOM编码"
          rules={[{ required: true, message: '请输入BOM编码' }]}
        >
          <Input placeholder="请输入BOM编码" />
        </Form.Item>
        <Form.Item
          name="status"
          label="状态"
          rules={[{ required: true, message: '请选择状态' }]}
        >
          <Select placeholder="请选择状态">
            <Select.Option value={1}>禁用</Select.Option>
            <Select.Option value={0}>启用</Select.Option>
          </Select>
        </Form.Item>
        <Form.Item
          name="remark"
          label="备注"
        >
          <Input.TextArea placeholder="请输入备注" rows={3} />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default BomFormModal;