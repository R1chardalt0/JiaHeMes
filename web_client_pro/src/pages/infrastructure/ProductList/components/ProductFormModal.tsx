import React, { useImperativeHandle, forwardRef, useRef } from 'react';
import { Modal, Form, Input, Button } from 'antd';
import type { FormInstance } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { ProductListDto } from '@/services/Model/Infrastructure/ProductList';

// 产品表单模态框属性接口
export interface ProductFormModalProps {
  // 模态框是否可见
  open: boolean;
  // 模态框标题
  title: string;
  // 初始表单值
  initialValues?: ProductListDto;
  // 取消回调
  onCancel: () => void;
  // 保存回调
  onSave: (values: ProductListDto) => void;
  // 打开BOM选择弹窗回调
  onOpenBomModal: () => void;
  // 打开工艺路线选择弹窗回调
  onOpenProcessRouteModal: () => void;
}

// 产品表单模态框引用接口
export interface ProductFormModalRef {
  // 获取表单实例
  getForm: () => FormInstance<ProductListDto>;
}

/**
 * 产品表单模态框组件
 * 用于创建和编辑产品信息
 */
const ProductFormModal = forwardRef<ProductFormModalRef, ProductFormModalProps>(({
  open,
  title,
  initialValues,
  onCancel,
  onSave,
  onOpenBomModal,
  onOpenProcessRouteModal
}, ref) => {
  // 创建表单实例
  const [form] = Form.useForm<ProductListDto>();

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
          name="productListId"
          hidden
        >
          <Input />
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

        <Form.Item
          label="产品名称"
          name="productName"
          rules={[
            { required: true, message: '请输入产品名称' },
          ]}
        >
          <Input placeholder="请输入产品名称" />
        </Form.Item>

        <Form.Item label="BOM ID" rules={[{ required: true, message: '请选择BOM' }]}>
          <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
            <Form.Item name="bomId" noStyle>
              <Input placeholder="请选择BOM" readOnly style={{ flex: 1 }} />
            </Form.Item>
            <Button type="primary" icon={<SearchOutlined />} onClick={onOpenBomModal} style={{ marginTop: 4 }}>选择</Button>
          </div>
        </Form.Item>

        <Form.Item name="bomCode" label="BOM编号">
          <Input placeholder="BOM编号" readOnly disabled />
        </Form.Item>

        <Form.Item name="bomName" label="BOM名称">
          <Input placeholder="BOM名称" readOnly disabled />
        </Form.Item>

        <Form.Item label="工艺路线 ID" rules={[{ required: true, message: '请选择工艺路线' }]}>
          <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
            <Form.Item name="processRouteId" noStyle>
              <Input placeholder="请选择工艺路线" readOnly style={{ flex: 1 }} />
            </Form.Item>
            <Button type="primary" icon={<SearchOutlined />} onClick={onOpenProcessRouteModal} style={{ marginTop: 4 }}>选择</Button>
          </div>
        </Form.Item>

        <Form.Item name="processRouteCode" label="工艺路线编号">
          <Input placeholder="工艺路线编号" readOnly disabled />
        </Form.Item>

        <Form.Item name="processRouteName" label="工艺路线名称">
          <Input placeholder="工艺路线名称" readOnly disabled />
        </Form.Item>

        <Form.Item
          label="产品类型"
          name="productType"
          rules={[
            { required: true, message: '请输入产品类型' },
          ]}
        >
          <Input placeholder="请输入产品类型" />
        </Form.Item>

        <Form.Item
          label="备注"
          name="remark"
        >
          <Input.TextArea rows={4} placeholder="请输入备注" />
        </Form.Item>
      </Form>
    </Modal>
  );
});

ProductFormModal.displayName = 'ProductFormModal';

export default ProductFormModal;