// src/pages/Bom/components/BomItemFormModal.tsx
import React, { useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, Button } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { StationListDto } from '@/services/Model/Infrastructure/StationList';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';

interface BomItemFormModalProps {
  open: boolean;
  onCancel: () => void;
  onOk: () => void;
  editingItem: any | null;
  stations: StationListDto[];
  stationLoading: boolean;
  onOpenProductModal: () => void;
  selectedProductName: string;
  form: any;
}

const BomItemFormModal: React.FC<BomItemFormModalProps> = ({
  open,
  onCancel,
  onOk,
  editingItem,
  stations,
  stationLoading,
  onOpenProductModal,
  selectedProductName,
  form
}) => {
  useEffect(() => {
    if (editingItem) {
      form.setFieldsValue({
        stationCode: editingItem.stationCode,
        batchRule: editingItem.batchRule,
        batchQty: editingItem.batchQty,
        batchSNQty: editingItem.batchSNQty,
        productId: editingItem.productId,
        productName: editingItem.productName,
        productCode: editingItem.productCode
      });
    } else {
      form.resetFields();
    }
  }, [editingItem, form]);

  return (
    <Modal
      title={editingItem ? "编辑BOM子项" : "添加BOM子项"}
      open={open}
      onOk={onOk}
      onCancel={onCancel}
      width={600}
    >
      <Form form={form} layout="vertical">
        <Form.Item name="stationCode" label="站点编码" rules={[{ required: true, message: '请选择站点编码' }]}>
          <Select
            placeholder="请选择站点编码"
            loading={stationLoading}
            showSearch
            filterOption={(input, option) =>
              (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
            }
          >
            {stations.map((s) => (
              <Select.Option key={s.stationCode} value={s.stationCode}>
                {s.stationCode}
              </Select.Option>
            ))}
          </Select>
        </Form.Item>
        <Form.Item name="batchRule" label="批次规则" rules={[{ required: true, message: '请输入批次规则' }]}>
          <Input placeholder="请输入批次规则" />
        </Form.Item>
        <Form.Item name="batchQty" label="批次数据固定" initialValue={true}>
          <Switch />
        </Form.Item>
        <Form.Item noStyle shouldUpdate={(prevValues, currentValues) => {
          if (prevValues.batchQty !== currentValues.batchQty) {
            if (!currentValues.batchQty) {
              form.setFieldsValue({ batchSNQty: undefined });
            }
            return true;
          }
          return false;
        }}>
          <Form.Item name="batchSNQty" label="批次SN数量" rules={[
            {
              validator: (_, value, callback) => {
                const batchQty = form.getFieldValue('batchQty');
                if (batchQty && !value) {
                  callback('请输入批次SN数量');
                } else {
                  callback();
                }
              }
            }
          ]}>
            <Input placeholder="请输入批次SN数量" />
          </Form.Item>
        </Form.Item>
        <Form.Item label="产品ID">
          <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
            <Form.Item name="productId" noStyle rules={[{ required: true, message: '请选择产品' }]}>
              <Input placeholder="请选择产品" readOnly style={{ flex: 1 }} />
            </Form.Item>
            <Button type="primary" icon={<SearchOutlined />} onClick={onOpenProductModal} style={{ marginTop: 4 }}>
              选择
            </Button>
          </div>
        </Form.Item>
        <Form.Item name="productName" label="产品名称">
          <Input placeholder="产品名称" readOnly disabled />
        </Form.Item>
        <Form.Item name="productCode" label="产品编码">
          <Input placeholder="产品编码" readOnly disabled />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default BomItemFormModal;