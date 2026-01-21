// ProductionManagement/ManualPass/index.tsx
import { useLocation } from '@umijs/max';
import { useState, useEffect } from 'react';
import { Button, Input, Form, message, Card, Descriptions } from 'antd';
import { uploadData } from '@/services/Api/Production/UploadData';
import { getMenuTree } from '@/services/Api/Systems/menu';
import type { MenuItem } from '@/services/Model/Systems/menu';
import { getStationListById } from '@/services/Api/Infrastructure/StationList';
import { getDeviceInfoById } from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import { StationListDto } from '@/services/Model/Infrastructure/StationList';
import type { DeviceInfo, BaseResponse } from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';

// 接口响应类型
type ApiResponse<T = any> = {
  code: number;
  data: T;
  message?: string;
};

// 表单值接口
interface FormValues {
  items: Array<{
    batchNo: string;
    sn: string;
  }>;
}

// 输入项接口
interface InputItem {
  batchNo: string;
  sn: string;
}

// 组件属性接口
interface ManualPassProps {
  query?: string; // 自定义查询参数，可选
}

const ManualPass: React.FC<ManualPassProps> = ({ query }) => {
  // 从URL路径中提取routeName
  const location = useLocation();
  const pathname = location.pathname;
  const pathParts = pathname.split('/');
  const lastPart = pathParts[pathParts.length - 1];
  const routeName = lastPart.split('-').pop() || '';

  console.log('从URL提取的routeName:', routeName);
  // 通过location对象获取查询参数
  const searchParams = new URLSearchParams(location.search);
  const initialStationId = searchParams.get('stationId');
  const initialResourceId = searchParams.get('resourceId');

  // 加载状态
  const [submitting, setSubmitting] = useState(false);
  const [menuLoading, setMenuLoading] = useState(false);
  const [stationId, setStationId] = useState<string | null>(initialStationId);
  const [resourceId, setResourceId] = useState<string | null>(initialResourceId);
  const [hasValidParams, setHasValidParams] = useState(!!(initialStationId && initialResourceId));

  // 站点和设备信息
  const [stationInfo, setStationInfo] = useState<{ name: string; code: string } | null>(null);
  const [deviceInfo, setDeviceInfo] = useState<{ name: string; code: string } | null>(null);

  // 表单实例
  const [form] = Form.useForm();

  // 输入项状态
  const [inputItems, setInputItems] = useState<InputItem[]>([{
    batchNo: '',
    sn: ''
  }]);

  // 递归查找菜单中的指定路由名称节点
  const findMenuByRouteName = (menus: MenuItem[]): MenuItem | null => {
    for (const menu of menus) {
      if (menu.routeName === routeName && menu.query) {
        return menu;
      }
      if (menu.children && menu.children.length > 0) {
        const found = findMenuByRouteName(menu.children);
        if (found) {
          return found;
        }
      }
    }
    return null;
  };

  // 获取站点和设备信息
  const getStationAndDeviceInfo = async (stationId: string, resourceId: string) => {
    try {
      console.log('开始获取站点和设备信息:', { stationId, resourceId });

      // 并行获取站点和设备信息
      const [stationRes, deviceRes] = await Promise.all([
        getStationListById(stationId),
        getDeviceInfoById(resourceId)
      ]);

      // 处理站点信息
      if (stationRes) {
        const station = stationRes as any;
        setStationInfo({
          name: (station.stationName || station.name || '未知站点') as string,
          code: (station.stationCode || station.code || '未知编码') as string
        });
      }

      // 处理设备信息
      if (deviceRes) {
        const deviceResAny = deviceRes as any;
        // 检查deviceRes是否包含data字段（返回{ data: DeviceInfo }的情况）
        if (deviceResAny.data) {
          const device = deviceResAny.data as any;
          setDeviceInfo({
            name: (device.resourceName || device.name || '未知设备') as string,
            code: (device.resource || device.code || '未知编码') as string
          });
        }
        // 处理直接返回设备对象的情况（兼容旧格式）
        else if (deviceResAny.resourceName) {
          const device = deviceResAny as any;
          setDeviceInfo({
            name: (device.resourceName || device.name || '未知设备') as string,
            code: (device.resource || device.code || '未知编码') as string
          });
        }
      }
    } catch (error) {
      console.error('获取站点和设备信息失败:', error);
    }
  };

  // 从菜单数据中获取参数
  const getParamsFromMenu = async () => {
    setMenuLoading(true);
    try {
      console.log('开始从菜单获取参数');

      // 如果提供了自定义query参数，直接使用
      if (query) {
        console.log('使用自定义query参数:', query);
        const queryParams = new URLSearchParams(query);
        const parsedStationId = queryParams.get('stationId');
        const parsedResourceId = queryParams.get('resourceId');

        console.log('解析自定义参数结果:', {
          parsedStationId,
          parsedResourceId
        });

        // 更新状态
        if (parsedStationId && parsedResourceId) {
          setStationId(parsedStationId);
          setResourceId(parsedResourceId);
          setHasValidParams(true);

          // 获取站点和设备信息
          getStationAndDeviceInfo(parsedStationId, parsedResourceId);
        }
        return;
      }

      // 否则从菜单中获取
      const menuRes = await getMenuTree();
      console.log('菜单数据获取结果:', { hasMenuData: !!menuRes });

      if (menuRes) {
        // 正确处理MenuTreeResult类型，提取其中的MenuItem数组
        const menuItems = Array.isArray(menuRes) ? menuRes : (menuRes as any).data || [];
        const targetMenu = findMenuByRouteName(menuItems as MenuItem[]);
        console.log(`查找${routeName}菜单结果:`, {
          hasTargetMenu: !!targetMenu,
          targetMenuRouteName: targetMenu?.routeName,
          targetMenuQuery: targetMenu?.query
        });

        // 解析菜单中的query参数
        if (targetMenu?.query) {
          const queryParams = new URLSearchParams(targetMenu.query);
          const parsedStationId = queryParams.get('stationId');
          const parsedResourceId = queryParams.get('resourceId');

          console.log('解析参数结果:', {
            parsedStationId,
            parsedResourceId
          });

          // 更新状态
          if (parsedStationId && parsedResourceId) {
            setStationId(parsedStationId);
            setResourceId(parsedResourceId);
            setHasValidParams(true);

            // 获取站点和设备信息
            getStationAndDeviceInfo(parsedStationId, parsedResourceId);
          }
        }
      }
    } catch (error) {
      console.error('获取菜单参数失败:', error);
    } finally {
      setMenuLoading(false);
    }
  }

  // 组件挂载时获取菜单参数
  useEffect(() => {
    const fetchData = async () => {
      await getParamsFromMenu();
    };

    fetchData();
  }, []);

  // 提交表单
  const handleSubmit = async (values: any) => {
    setSubmitting(true);
    try {
      console.log('提交表单数据:', values);

      if (!stationId || !resourceId || !stationInfo?.code || !deviceInfo?.code) {
        message.error('参数信息不完整，无法提交');
        return;
      }

      // 处理表单数据，将扁平结构转换为数组结构
      let items: { batchNo: string; sn: string }[] = [];

      // 检查是否是扁平结构（如items[0].batchNo）
      const isFlatStructure = Object.keys(values).some(key => key.includes('items['));

      if (isFlatStructure) {
        // 提取所有索引
        const indices = new Set<number>();
        Object.keys(values).forEach(key => {
          const match = key.match(/items\[(\d+)\]/);
          if (match) {
            indices.add(Number(match[1]));
          }
        });

        // 转换为数组结构
        indices.forEach(index => {
          const batchNo = values[`items[${index}].batchNo`];
          const sn = values[`items[${index}].sn`];
          if (batchNo && sn) {
            items.push({ batchNo, sn });
          }
        });
      } else if (values.items && Array.isArray(values.items)) {
        // 处理嵌套数组结构
        items = values.items.filter((item: any) => item.batchNo && item.sn);
      }

      if (items.length === 0) {
        message.error('请输入至少一组SN和批次号');
        return;
      }

      // 按顺序循环调用后端接口
      let successCount = 0;
      let errorMessages: string[] = [];

      for (let i = 0; i < items.length; i++) {
        const item = items[i];

        try {
          // 构造上传参数
          const uploadParams = {
            SN: item.sn,
            Resource: deviceInfo.code, // 使用设备编码而不是设备ID
            StationCode: stationInfo.code,
            TestResult: 'PASS', // 测试结果设置为PASS
            WorkOrderCode: '', // 工单编号设置为空
            TestData: '', // 测试数据设置为空字符串
            BatchNo: item.batchNo // 批次号
          };

          console.log(`上传第${i + 1}组参数:`, uploadParams);

          // 调用上传接口
          const result = await uploadData(uploadParams as any);
          console.log(`上传第${i + 1}组结果:`, result);

          if (result.code === 200) {
            successCount++;
          } else {
            errorMessages.push(`第${i + 1}组：${result.message || '未知错误'}`);
          }
        } catch (error: any) {
          console.error(`提交第${i + 1}组失败:`, error);
          errorMessages.push(`第${i + 1}组：${error.message || '提交失败'}`);
        }
      }

      // 显示结果
      if (successCount === items.length) {
        message.success(`全部${successCount}组数据提交成功`);
        // 清空表单
        setInputItems([{
          batchNo: '',
          sn: ''
        }]);
        form.resetFields();
      } else if (successCount > 0) {
        message.warning(`${successCount}组提交成功，${errorMessages.length}组失败`);
        if (errorMessages.length > 0) {
          console.error('失败详情:', errorMessages);
        }
      } else {
        message.error(`全部${errorMessages.length}组提交失败`);
        if (errorMessages.length > 0) {
          console.error('失败详情:', errorMessages);
        }
      }
    } catch (error) {
      console.error('提交失败:', error);
      message.error('提交失败，请重试');
    } finally {
      setSubmitting(false);
    }
  };

  // 处理输入框回车事件
  const handleInputKeyPress = async (e: React.KeyboardEvent, index: number) => {
    if (e.key === 'Enter') {
      // 先验证当前表单字段
      await form.validateFields([`items[${index}].batchNo`, `items[${index}].sn`]);

      // 检查是否已经是最后一组
      if (index === inputItems.length - 1) {
        // 添加新的一组输入框
        setInputItems([...inputItems, { batchNo: '', sn: '' }]);
      }
    }
  };

  // 组件渲染
  return (
    <div className="manual-pass-container">
      <Card title={`手动过站-${stationInfo?.code || ''}`}>
        <Descriptions title="参数信息" layout="horizontal" column={2}>
          {/* <Descriptions.Item label="站点ID">{stationId || '无'}</Descriptions.Item> */}
          <Descriptions.Item label="站点名称">{stationInfo?.name || '无'}</Descriptions.Item>
          <Descriptions.Item label="站点编码">{stationInfo?.code || '无'}</Descriptions.Item>
          {/* <Descriptions.Item label="设备ID">{resourceId || '无'}</Descriptions.Item> */}
          <Descriptions.Item label="设备名称">{deviceInfo?.name || '无'}</Descriptions.Item>
          <Descriptions.Item label="设备编码">{deviceInfo?.code || '无'}</Descriptions.Item>
        </Descriptions>

        <Form
          form={form}
          layout="vertical"
          style={{ marginTop: 20 }}
          onFinish={handleSubmit}
        >
          {inputItems.map((item, index) => (
            <div key={index} style={{ marginBottom: 20, padding: 16, border: '1px solid #f0f0f0', borderRadius: 4 }}>
              <h4 style={{ marginBottom: 12 }}>第{index + 1}组</h4>
              <Form.Item
                name={`items[${index}].batchNo`}
                label="批次号（PDCASN)"
                rules={[{ required: false, message: '请输入批次号' }]}
              >
                <Input
                  placeholder="请输入批次号"
                  onKeyPress={(e) => handleInputKeyPress(e, index)}
                />
              </Form.Item>
              <Form.Item
                name={`items[${index}].sn`}
                label="SN号"
                rules={[{ required: false, message: '请输入SN号' }]}
              >
                <Input
                  placeholder="请输入SN号"
                  onKeyPress={(e) => handleInputKeyPress(e, index)}
                />
              </Form.Item>
              {index > 0 && (
                <Button
                  danger
                  onClick={() => {
                    const newItems = [...inputItems];
                    newItems.splice(index, 1);
                    setInputItems(newItems);
                  }}
                  style={{ marginTop: 8 }}
                >
                  删除
                </Button>
              )}
            </div>
          ))}

          <Form.Item>
            <Button type="primary" htmlType="submit" loading={submitting}>
              提交
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}

export default ManualPass;