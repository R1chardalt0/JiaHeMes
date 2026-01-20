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
  sn: string;
  batchNo: string;
}

const ManualPass: React.FC = () => {
  // 通过location对象获取查询参数
  const location = useLocation();
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

  // 递归查找菜单中的OP20节点
  const findOP20Menu = (menus: MenuItem[]): MenuItem | null => {
    for (const menu of menus) {
      if (menu.routeName === 'OP20' && menu.query) {
        return menu;
      }
      if (menu.children && menu.children.length > 0) {
        const found = findOP20Menu(menu.children);
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
      const menuRes = await getMenuTree();
      console.log('菜单数据获取结果:', { hasMenuData: !!menuRes });

      if (menuRes) {
        // 正确处理MenuTreeResult类型，提取其中的MenuItem数组
        const menuItems = Array.isArray(menuRes) ? menuRes : (menuRes as any).data || [];
        const op20Menu = findOP20Menu(menuItems as MenuItem[]);
        console.log('查找OP20菜单结果:', {
          hasOP20Menu: !!op20Menu,
          op20MenuRouteName: op20Menu?.routeName,
          op20MenuQuery: op20Menu?.query
        });

        // 解析OP20菜单中的query参数
        if (op20Menu?.query) {
          const queryParams = new URLSearchParams(op20Menu.query);
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
  const handleSubmit = async (values: FormValues) => {
    setSubmitting(true);
    try {
      console.log('提交表单数据:', values);

      if (!stationId || !resourceId || !stationInfo?.code || !deviceInfo?.code) {
        message.error('参数信息不完整，无法提交');
        return;
      }

      // 构造上传参数
      const uploadParams = {
        SN: values.sn,
        Resource: deviceInfo.code, // 使用设备编码而不是设备ID
        StationCode: stationInfo.code,
        TestResult: 'PASS', // 测试结果设置为PASS
        WorkOrderCode: '', // 工单编号设置为空
        TestData: '', // 测试数据设置为空字符串
        BatchNo: values.batchNo // 批次号
      };

      console.log('上传参数:', uploadParams);

      // 调用上传接口
      const result = await uploadData(uploadParams as any);
      console.log('上传结果:', result);

      if (result.code === 200) {
        message.success('提交成功');
      } else {
        message.error(`提交失败: ${result.message || '未知错误'}`);
      }
    } catch (error) {
      console.error('提交失败:', error);
      message.error('提交失败，请重试');
    } finally {
      setSubmitting(false);
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
          layout="vertical"
          style={{ marginTop: 20 }}
          onFinish={handleSubmit}
        >
          <Form.Item
            name="batchNo"
            label="批次号（PDCASN)"
            rules={[{ required: true, message: '请输入批次号' }]}
          >
            <Input placeholder="请输入批次号" />
          </Form.Item>
          <Form.Item
            name="sn"
            label="SN号"
            rules={[{ required: true, message: '请输入SN号' }]}
          >
            <Input placeholder="请输入SN号" />
          </Form.Item>

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