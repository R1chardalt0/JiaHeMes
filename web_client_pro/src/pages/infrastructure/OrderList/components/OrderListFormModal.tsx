// src/pages/infrastructure/OrderList/components/OrderListFormModal.tsx
import React, { useEffect, useCallback } from 'react';
import { Modal, Form, Input, Select, Button, DatePicker, InputNumber, Row, Col } from 'antd';
import { OrderListDto, OrderListCreateDto, OrderListUpdateDto } from '@/services/Model/Infrastructure/OrderList';
import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';

// 表单专用类型，用于处理日期选择器的dayjs对象
interface OrderListFormValues {
  orderCode: string;
  orderName: string;
  productListId?: string;
  productCode?: string;
  productName?: string;
  bomId?: string;
  bomCode?: string;
  bomName?: string;
  processRouteId?: string;
  processRouteCode?: string;
  processRouteName?: string;
  orderType: number;
  planQty: number;
  completedQty: number;
  priorityLevel: number;
  planStartTime?: Dayjs | null;
  planEndTime?: Dayjs | null;
}

interface OrderListFormModalProps {
  open: boolean;
  onCancel: () => void;
  onOk: () => void;
  editingOrder: OrderListDto | null;
  form: any;
  onOpenProductModal: () => void;
}

const OrderListFormModal: React.FC<OrderListFormModalProps> = ({
  open,
  onCancel,
  onOk,
  editingOrder,
  form,
  onOpenProductModal
}) => {
  useEffect(() => {
    if (editingOrder) {
      form.setFieldsValue({
        orderCode: editingOrder.orderCode,
        orderName: editingOrder.orderName,
        productListId: editingOrder.productListId,
        productCode: editingOrder.productCode,
        productName: editingOrder.productName,
        bomId: editingOrder.bomId,
        bomCode: editingOrder.bomCode,
        bomName: editingOrder.bomName,
        processRouteId: editingOrder.processRouteId,
        processRouteCode: editingOrder.processRouteCode,
        processRouteName: editingOrder.processRouteName,
        orderType: editingOrder.orderType,
        planQty: editingOrder.planQty,
        completedQty: editingOrder.completedQty,
        priorityLevel: editingOrder.priorityLevel,
        planStartTime: editingOrder.planStartTime ? dayjs(editingOrder.planStartTime) : null,
        planEndTime: editingOrder.planEndTime ? dayjs(editingOrder.planEndTime) : null,
      });
    } else {
      form.resetFields();
    }
  }, [editingOrder, form]);

  return (
    <Modal
      title={editingOrder ? "编辑工单" : "新建工单"}
      open={open}
      onOk={onOk}
      onCancel={onCancel}
      width={800}
    >
      <Form form={form} layout="vertical">
        {/* 工单编码和工单名称 */}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="orderCode"
              label="工单编码"
              rules={[{ required: true, message: '请输入工单编码' }]}
            >
              <Input placeholder="请输入工单编码" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="orderName"
              label="工单名称"
              rules={[{ required: true, message: '请输入工单名称' }]}
            >
              <Input placeholder="请输入工单名称" />
            </Form.Item>
          </Col>
        </Row>

        {/* 工单类型、计划数量、已完成数量、优先级、计划开始时间、计划结束时间 */}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="orderType"
              label="工单类型"
            >
              <Select placeholder="请选择工单类型" style={{ width: '100%' }}>
                <Select.Option value={1}>生产工单</Select.Option>
                <Select.Option value={2}>返工工单</Select.Option>
              </Select>
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="planQty"
              label="计划数量"
            >
              <InputNumber placeholder="请输入计划数量" style={{ width: '100%' }} />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="completedQty"
              label="已完成数量"
            >
              <InputNumber placeholder="请输入已完成数量" style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="priorityLevel"
              label="优先级"
            >
              <Select placeholder="请选择优先级" style={{ width: '100%' }}>
                <Select.Option value={1}>紧急</Select.Option>
                <Select.Option value={3}>高</Select.Option>
                <Select.Option value={5}>中</Select.Option>
                <Select.Option value={7}>低</Select.Option>
              </Select>
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="planStartTime"
              label="计划开始时间"
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="planEndTime"
              label="计划结束时间"
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item label="产品ID">
          <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
            <Form.Item name="productListId" noStyle rules={[{ required: true, message: '请选择产品' }]}>
              <Input placeholder="请选择产品" readOnly style={{ flex: 1 }} />
            </Form.Item>
            <Button type="primary" onClick={onOpenProductModal} style={{ marginTop: 4 }}>
              选择产品
            </Button>
          </div>
        </Form.Item>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="productCode"
              label="产品编码"
            >
              <Input placeholder="产品编码" readOnly />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="productName"
              label="产品名称"
            >
              <Input placeholder="产品名称" readOnly />
            </Form.Item>
          </Col>
        </Row>
        {/* BOM信息 */}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="bomId"
              label="BOM ID"
            >
              <Input placeholder="BOM ID" readOnly />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="bomCode"
              label="BOM编码"
            >
              <Input placeholder="BOM编码" readOnly />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="bomName"
              label="BOM名称"
            >
              <Input placeholder="BOM名称" readOnly />
            </Form.Item>
          </Col>
        </Row>

        {/* 工艺路线信息 */}
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="processRouteId"
              label="工艺路线ID"
            >
              <Input placeholder="工艺路线ID" readOnly />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="processRouteCode"
              label="工艺路线编码"
            >
              <Input placeholder="工艺路线编码" readOnly />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              name="processRouteName"
              label="工艺路线名称"
            >
              <Input placeholder="工艺路线名称" readOnly />
            </Form.Item>
          </Col>
        </Row>
      </Form>


    </Modal>
  );
};

export default OrderListFormModal;