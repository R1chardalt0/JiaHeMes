import React from 'react';
import { Form, Input, Select } from 'antd';

interface ProcessRouteFormProps {
  form: any;
}

/**
 * 工艺路线表单组件
 * 用于工艺路线的创建和编辑
 */
const ProcessRouteForm: React.FC<ProcessRouteFormProps> = ({ form }) => {
  return (
    <Form
      form={form}
      layout="vertical"
    >
      <Form.Item
        name="routeName"
        label="工艺路线名称"
        rules={[{ required: true, message: '请输入工艺路线名称' }]}
      >
        <Input placeholder="请输入工艺路线名称" />
      </Form.Item>
      <Form.Item
        name="routeCode"
        label="工艺路线编码"
        rules={[{ required: true, message: '请输入工艺路线编码' }]}
      >
        <Input placeholder="请输入工艺路线编码" />
      </Form.Item>
      <Form.Item
        name="status"
        label="状态"
        rules={[{ required: true, message: '请选择状态' }]}
      >
        <Select placeholder="请选择状态">
          <Select.Option value={0}>启用</Select.Option>
          <Select.Option value={1}>禁用</Select.Option>
        </Select>
      </Form.Item>
      <Form.Item
        name="remark"
        label="备注"
      >
        <Input.TextArea placeholder="请输入备注" rows={3} />
      </Form.Item>
    </Form>
  );
};

export default ProcessRouteForm;