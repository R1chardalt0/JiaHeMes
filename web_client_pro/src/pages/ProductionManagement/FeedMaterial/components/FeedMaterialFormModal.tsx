import React, { useImperativeHandle, forwardRef, useState, useEffect } from 'react';
import { Modal, Form, Input, Button, InputNumber, Space, Select, message } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import type { FormInstance } from 'antd';
import { FeedMaterialFormData } from '@/services/Model/Production/feedMaterial';
import DeviceSelectModal from './DeviceSelectModal';
import { getDeviceInfoList } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import type { DeviceInfo } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';
import { getBomById } from '@/services/Api/Infrastructure/Bom/BomList';
import { getBomItemsByBomId } from '@/services/Api/Infrastructure/Bom/BomItem';
import { getOrderByCode } from '@/services/Api/Infrastructure/OrderList';
import { getProductListById } from '@/services/Api/Infrastructure/ProductList';
import type { BomList } from '@/services/Model/Infrastructure/Bom/BomList';
import type { BomItem } from '@/services/Model/Infrastructure/Bom/BomItem';
import type { OrderList } from '@/services/Model/Infrastructure/OrderList';
import type { ProductListDto } from '@/services/Model/Infrastructure/ProductList';

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
  onSave: (values: FeedMaterialFormData, callback: (success: boolean) => void) => void;
}

// 设备查询参数
interface DeviceQueryParams {
  current: number;
  pageSize: number;
  resource?: string;
  resourceName?: string;
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

  // 设备选择弹窗相关状态
  const [deviceModalVisible, setDeviceModalVisible] = useState(false);
  const [devices, setDevices] = useState<DeviceInfo[]>([]);
  const [deviceTotal, setDeviceTotal] = useState(0);
  const [deviceCurrent, setDeviceCurrent] = useState(1);
  const [devicePageSize, setDevicePageSize] = useState(10);
  const [deviceSearchValues, setDeviceSearchValues] = useState({
    resource: '',
    resourceName: ''
  });

  // 设备编码和工单编码状态
  const [resourceValue, setResourceValue] = useState<string>('');
  const [workOrderCodeValue, setWorkOrderCodeValue] = useState<string>('');

  // BOM和站点相关状态
  const [bomId, setBomId] = useState<string>('');
  const [bomInfo, setBomInfo] = useState<BomList | null>(null);
  const [stations, setStations] = useState<Array<{ value: string; label: string }>>([]);
  const [bomItems, setBomItems] = useState<BomItem[]>([]);
  const [products, setProducts] = useState<Array<{ value: string; label: string }>>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // 暴露form实例给父组件
  useImperativeHandle(ref, () => ({
    getForm: () => form
  }));

  // 获取设备列表
  const fetchDevices = async (params: DeviceQueryParams) => {
    try {
      // 由于API定义的返回类型可能与实际不符，我们先获取原始响应
      const response: any = await getDeviceInfoList({
        current: params.current,
        pageSize: params.pageSize,
        resource: params.resource,
        resourceName: params.resourceName,
      });

      // 处理设备列表数据
      if (response && response.data) {
        setDevices(response.data);
      } else {
        setDevices([]);
      }

      // 尝试获取总数，优先使用total属性，否则使用数组长度
      const total = response?.total ?? (response?.data ? response.data.length : 0);
      setDeviceTotal(total);
    } catch (error) {
      console.error('获取设备列表失败:', error);
    }
  };

  // 初始化设备列表
  useEffect(() => {
    if (deviceModalVisible) {
      fetchDevices({
        current: deviceCurrent,
        pageSize: devicePageSize,
        resource: deviceSearchValues.resource,
        resourceName: deviceSearchValues.resourceName,
      });
    }
  }, [deviceModalVisible, deviceCurrent, devicePageSize]);

  // 处理设备搜索
  const handleDeviceSearch = () => {
    setDeviceCurrent(1);
    fetchDevices({
      current: 1,
      pageSize: devicePageSize,
      resource: deviceSearchValues.resource,
      resourceName: deviceSearchValues.resourceName,
    });
  };

  // 处理设备搜索输入变化
  const handleDeviceSearchInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setDeviceSearchValues(prev => ({
      ...prev,
      [name]: value
    }));
  };

  // 处理设备分页变化
  const handleDevicePaginationChange = (current: number, pageSize: number) => {
    setDeviceCurrent(current);
    setDevicePageSize(pageSize);
  };

  // 根据工单编码获取BOM信息和站点列表
  const fetchBomAndStations = async (workOrderCode: string) => {
    if (!workOrderCode) return;

    setLoading(true);
    try {
      // 根据工单编码查询工单信息
      const workOrder: OrderList = await getOrderByCode(workOrderCode);

      // 检查响应是否成功
      if (!workOrder) {
        console.error('未找到工单信息:', workOrderCode);
        message.error('未找到工单信息，请检查工单编码是否正确');
        setStations([]);
        return;
      }

      // 获取BOM编码
      const bomId = workOrder.bomId;

      // 检查是否有BOM编码
      if (!bomId) {
        console.error('工单没有关联的BOM编码:', workOrderCode);
        message.error('该工单没有关联的BOM编码，请检查工单信息');
        setStations([]);
        return;
      }

      setBomId(bomId);

      // 获取BOM详情
      const bomDetails = await getBomById(bomId);
      setBomInfo(bomDetails);

      // 获取BOM子项
      const bomItemsData = await getBomItemsByBomId(bomId);
      console.log('获取到的BOM子项:', bomItemsData);
      setBomItems(bomItemsData);

      // 提取站点列表
      const stationList = Array.from(new Set(bomItemsData.map((item: BomItem) => item.stationCode)))
        .filter(stationCode => stationCode) // 过滤空值
        .map(stationCode => ({ value: stationCode, label: stationCode }));

      setStations(stationList);

      console.log('获取到的站点列表:', stationList);
    } catch (error) {
      console.error('获取BOM和站点信息失败:', error);
      message.error('获取BOM和站点信息失败，请稍后重试');
      setStations([]);
    } finally {
      setLoading(false);
    }
  };

  // 处理设备选择
  const handleDeviceSelect = (device: DeviceInfo) => {
    // 确保设备对象和resource属性存在
    if (device && device.resource) {
      // 更新状态
      setResourceValue(device.resource);
      setWorkOrderCodeValue(device.workOrderCode || '');

      // 同时更新表单值
      form.setFieldsValue({
        resource: device.resource,
        workOrderCode: device.workOrderCode || ''
      });

      // 如果有工单编码，获取BOM和站点列表
      if (device.workOrderCode) {
        fetchBomAndStations(device.workOrderCode);
      }

      // 验证表单值是否正确设置
      setTimeout(() => {
        const values = form.getFieldsValue();

        // 延迟关闭弹窗，确保表单值更新
        setDeviceModalVisible(false);
      }, 100);
    } else {
      console.error('Device or device.resource is undefined:', device);
      setDeviceModalVisible(false);
    }
  };

  // 处理站点编码选择变化
  const handleStationChange = async (stationCode: string) => {
    console.log('站点编码选择变化:', stationCode);
    console.log('当前BOM子项数量:', bomItems.length);
    console.log('当前BOM子项:', bomItems);

    if (!stationCode || bomItems.length === 0) {
      console.log('站点编码为空或BOM子项为空');
      setProducts([]);
      form.setFieldsValue({ productCode: '' });
      return;
    }

    // 根据站点编码筛选BOM子项
    const filteredItems = bomItems.filter(item => item.stationCode === stationCode);
    console.log('筛选后的BOM子项:', filteredItems);

    // 提取产品ID列表
    const productIds = Array.from(new Set(filteredItems.map(item => item.productId || '')))
      .filter(productId => productId); // 过滤空值
    console.log('提取的产品ID列表:', productIds);

    // 根据产品ID获取产品编码
    const productList: Array<{ value: string; label: string }> = [];
    for (const productId of productIds) {
      try {
        const product: ProductListDto = await getProductListById(productId);
        if (product && product.productCode) {
          productList.push({ value: product.productCode, label: product.productCode });
        }
      } catch (error) {
        console.error('获取产品信息失败:', error);
      }
    }

    console.log('提取的产品编码列表:', productList);

    setProducts(productList);

    // 如果只有一个产品编码，自动填入
    if (productList.length === 1) {
      console.log('只有一个产品编码，自动填入:', productList[0].value);
      form.setFieldsValue({ productCode: productList[0].value });
    } else {
      console.log('多个产品编码或无产品编码，清空产品编码');
      form.setFieldsValue({ productCode: '' });
    }
  };

  // 处理取消
  const handleCancel = () => {
    form.resetFields();
    // 重置状态
    setResourceValue('');
    setWorkOrderCodeValue('');
    setBomId('');
    setBomInfo(null);
    setStations([]);
    setBomItems([]);
    setProducts([]);
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

      // 只有在保存成功时才重置表单
      if (saveResult === true) {
        form.resetFields();
        // 重置状态
        setResourceValue('');
        setWorkOrderCodeValue('');
        setBomId('');
        setBomInfo(null);
        setStations([]);
        setBomItems([]);
        setProducts([]);
      }
    } catch (error) {
      // 表单验证失败或其他错误，不重置表单
      console.error('保存失败:', error);
    }
  };

  return (
    <>
      <div style={{ maxWidth: 800, margin: '0 auto' }}>
        <h2>{title}</h2>
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
            label="设备编码"
            name="resource"
            rules={[
              { required: true, message: '请选择设备' },
            ]}
            initialValue={resourceValue}
          >
            <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
              <Input
                placeholder="请选择设备"
                readOnly
                style={{ flex: 1 }}
                value={resourceValue}
              />
              <Button
                type="primary"
                icon={<SearchOutlined />}
                onClick={() => setDeviceModalVisible(true)}
              >
                选择
              </Button>
            </div>
          </Form.Item>

          <Form.Item
            label="工单编号"
            name="workOrderCode"
            rules={[
              { required: false, message: '工单编号由设备自动带出' },
            ]}
            initialValue={workOrderCodeValue}
          >
            <Input
              placeholder="工单编号由设备自动带出"
              disabled
              value={workOrderCodeValue}
            />
          </Form.Item>

          <Form.Item
            label="站点编码"
            name="stationCode"
            rules={[
              { required: true, message: '请选择站点编码' },
            ]}
          >
            <Select
              placeholder="请选择站点编码"
              options={stations}
              loading={loading}
              style={{ width: '100%' }}
              onChange={handleStationChange}
            />
          </Form.Item>

          <Form.Item
            label="产品编码"
            name="productCode"
            rules={[
              { required: true, message: '请选择产品编码' },
            ]}
          >
            <Select
              placeholder="请选择产品编码"
              options={products}
              loading={loading}
              style={{ width: '100%' }}
            />
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

          <Form.Item>
            <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
              <Button onClick={handleCancel}>重置</Button>
              <Button type="primary" onClick={handleSave}>保存</Button>
            </div>
          </Form.Item>
        </Form>
      </div>

      {/* 设备选择弹窗 */}
      <DeviceSelectModal
        open={deviceModalVisible}
        onCancel={() => setDeviceModalVisible(false)}
        devices={devices}
        deviceTotal={deviceTotal}
        deviceCurrent={deviceCurrent}
        devicePageSize={devicePageSize}
        deviceSearchValues={deviceSearchValues}
        onDeviceSelect={handleDeviceSelect}
        onDeviceSearch={handleDeviceSearch}
        onDeviceSearchInputChange={handleDeviceSearchInputChange}
        onDevicePaginationChange={handleDevicePaginationChange}
      />
    </>
  );
});

FeedMaterialFormModal.displayName = 'FeedMaterialFormModal';

export default FeedMaterialFormModal;