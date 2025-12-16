import React, { useEffect } from 'react';
import { Form, Input, Select, Modal, message, Button } from 'antd';
import type { DeviceInfo, DeviceInfoFormData } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';
import { createDeviceInfo, updateDeviceInfo } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import { getProductionLineList } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';

const { Option } = Select;

interface CreateEquipmentFormProps {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
  currentRow?: DeviceInfo | null;
  companyId?: string;
}

const layout = {
  labelCol: { span: 6 },
  wrapperCol: { span: 16 },
};

const tailLayout = {
  wrapperCol: { offset: 6, span: 16 },
};

export const CreateEquipmentForm: React.FC<CreateEquipmentFormProps> = ({ 
  visible, 
  onCancel, 
  onSuccess, 
  currentRow,
  companyId,
}) => {
  const [form] = Form.useForm<DeviceInfoFormData>();
  const [productionLines, setProductionLines] = React.useState<Array<{ productionLineId: string; productionLineName: string }>>([]);
  const [loading, setLoading] = React.useState(false);
  const resolvedCompanyId = companyId ?? currentRow?.companyId;
  const normalizedCompanyId = resolvedCompanyId !== undefined && resolvedCompanyId !== null && !Number.isNaN(Number(resolvedCompanyId))
    ? Number(resolvedCompanyId)
    : resolvedCompanyId;

  // 加载生产线列表
  useEffect(() => {
    const fetchProductionLines = async () => {
      try {
        const res = await getProductionLineList({ pageSize: 1000, companyId: normalizedCompanyId });
        if (res.data) {
          setProductionLines(
            res.data
              .filter((line: any) => line.productionLineId) // 过滤掉 productionLineId 为 undefined 的数据
              .map((line: any) => ({
                productionLineId: line.productionLineId as string,
                productionLineName: line.productionLineName as string,
              }))
          );
        }
      } catch (error) {
        message.error('获取生产线列表失败');
      }
    };

    if (visible) {
      fetchProductionLines();
    }
  }, [visible, normalizedCompanyId]);

  // 重置表单并根据currentRow设置初始值
  useEffect(() => {
    if (visible) {
      if (currentRow) {
        // 编辑模式：设置表单初始值
        form.setFieldsValue({
          deviceName: currentRow.deviceName,
          deviceEnCode: currentRow.deviceEnCode,
          productionLineId: currentRow.productionLineId,
          status: currentRow.status?.toString(),
          description: currentRow.description,
          avatar: currentRow.avatar,
          deviceType: currentRow.deviceType,
          devicePicture: currentRow.devicePicture,
          deviceManufacturer: currentRow.deviceManufacturer,
        });
      } else {
        // 新增模式：重置表单
        form.resetFields();
      }
    }
  }, [visible, currentRow, form]);

  // 处理表单提交
  const handleFinish = async (values: DeviceInfoFormData) => {
    setLoading(true);
    try {
      // 构造提交数据
      const submitData: DeviceInfo = {
        ...values,
        companyId: normalizedCompanyId,
        // 移除status字段的类型转换
        // status: values.status ? parseInt(values.status, 10) : undefined,
        // 确保日期类型正确
        createTime: currentRow?.createTime,
        updateTime: new Date().toISOString(),
        // 确保必填字段都有值
        avatar: values.avatar || '',
        deviceType: values.deviceType || '',
        devicePicture: values.devicePicture || '',
        deviceManufacturer: values.deviceManufacturer || '',
      };

      if (currentRow) {
        // 编辑模式
        await updateDeviceInfo({ ...submitData, deviceId: currentRow.deviceId });
        message.success('设备更新成功');
      } else {
        // 新增模式
        await createDeviceInfo(submitData);
        message.success('设备创建成功');
      }
      onSuccess();
    } catch (error) {
      message.error(currentRow ? '设备更新失败' : '设备创建失败');
    } finally {
      setLoading(false);
    }
  };

  // 处理表单取消
  const handleCancel = () => {
    form.resetFields();
    onCancel();
  };

  return (
    <Modal
      title={currentRow ? '编辑设备' : '新增设备'}
      open={visible}
      onCancel={handleCancel}
      footer={null}
      width={600}
      className="device-edit-modal"
      rootClassName="device-edit-modal"
      styles={{
        content: {
          background:
            'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
          backdropFilter: 'blur(14px) saturate(115%)',
          WebkitBackdropFilter: 'blur(14px) saturate(115%)',
          border: '1px solid rgba(72,115,255,0.28)',
          boxShadow:
            '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
        },
        header: {
          background: 'transparent',
          borderBottom: '1px solid rgba(72,115,255,0.22)'
        },
        body: {
          background: 'transparent'
        },
        mask: {
          background: 'rgba(4,10,22,0.35)',
          backdropFilter: 'blur(2px)'
        }
      }}
    >
      <Form
        form={form}
        layout="horizontal"
        {...layout}
        onFinish={handleFinish}
        autoComplete="off"
        initialValues={{
          status: '1', // 默认启用
        }}
      >
        <Form.Item
          name="deviceName"
          label="设备名称"
          rules={[{ required: true, message: '请输入设备名称' }]}
        >
          <Input placeholder="请输入设备名称" autoComplete="off" name="deviceName_no_autofill" autoCorrect="off" autoCapitalize="off" spellCheck={false} />
        </Form.Item>

        <Form.Item
          name="deviceEnCode"
          label="设备编码"
          rules={[{ required: true, message: '请输入设备编码' }]}
        >
          <Input placeholder="请输入设备编码" autoComplete="off" name="deviceEnCode_no_autofill" autoCorrect="off" autoCapitalize="off" spellCheck={false} />
        </Form.Item>

        <Form.Item
          name="productionLineId"
          label="所属生产线"
          rules={[{ required: true, message: '请选择所属生产线' }]}
        >
          <Select
            placeholder="请选择所属生产线"
            popupClassName="glass-dropdown"
            dropdownStyle={{
              background:
                'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
              backdropFilter: 'blur(12px) saturate(115%)',
              WebkitBackdropFilter: 'blur(12px) saturate(115%)',
              border: '1px solid rgba(72,115,255,0.28)',
              boxShadow:
                '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
            }}
          >
            {productionLines.map((line) => (
              <Option key={line.productionLineId} value={line.productionLineId}>
                {line.productionLineName}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="status"
          label="状态"
          rules={[{ required: true, message: '请选择设备状态' }]}
        >
          <Select
            placeholder="请选择设备状态"
            popupClassName="glass-dropdown"
            dropdownStyle={{
              background:
                'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
              backdropFilter: 'blur(12px) saturate(115%)',
              WebkitBackdropFilter: 'blur(12px) saturate(115%)',
              border: '1px solid rgba(72,115,255,0.28)',
              boxShadow:
                '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
            }}
          >
            <Option value="1">启用</Option>
            <Option value="0">禁用</Option>
          </Select>
        </Form.Item>

        <Form.Item
          name="deviceType"
          label="设备类型"
          rules={[{ required: true, message: '请输入设备类型' }]}
        >
          <Input placeholder="请输入设备类型" />
        </Form.Item>

        <Form.Item
          name="deviceManufacturer"
          label="设备制造商"
          rules={[{ required: true, message: '请输入设备制造商' }]}
        >
          <Input placeholder="请输入设备制造商" />
        </Form.Item>

        <Form.Item
          name="avatar"
          label="设备头像"
          rules={[{ required: true, message: '请输入设备头像URL' }]}
        >
          <Input placeholder="请输入设备头像URL" />
        </Form.Item>

        <Form.Item
          name="devicePicture"
          label="设备图片"
          rules={[{ required: true, message: '请输入设备图片URL' }]}
        >
          <Input placeholder="请输入设备图片URL" />
        </Form.Item>

        <Form.Item
          name="description"
          label="设备描述"
        >
          <Input.TextArea rows={4} placeholder="请输入设备描述（可选）" />
        </Form.Item>

        <Form.Item {...tailLayout}>
          <Button onClick={handleCancel} style={{ marginRight: 8 }}>
            取消
          </Button>
          <Button type="primary" htmlType="submit" loading={loading}>
            {currentRow ? '更新' : '创建'}
          </Button>
        </Form.Item>
      </Form>
    </Modal>
  );
};