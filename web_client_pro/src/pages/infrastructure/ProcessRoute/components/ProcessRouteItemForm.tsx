import React from 'react';
import { Form, Select, Switch } from 'antd';
import { StationListDto } from '@/services/Model/Infrastructure/StationList';

interface ProcessRouteItemFormProps {
  form: any;
  stations: StationListDto[];
  stationLoading: boolean;
}

/**
 * 工艺路线子项表单组件
 * 用于添加工艺路线子项的表单
 */
const ProcessRouteItemForm: React.FC<ProcessRouteItemFormProps> = ({ form, stations, stationLoading }) => {
  return (
    <Form
      form={form}
      layout="vertical"
    >
      <Form.Item
        name="stationCode"
        label="站点编码"
        rules={[{ required: true, message: '请选择站点编码' }]}
      >
        <Select
          placeholder="请选择站点编码"
          showSearch
          filterOption={(input, option) =>
            (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
          }
          loading={stationLoading}
        >
          {stations.map((station) => (
            <Select.Option key={station.stationCode} value={station.stationCode}>
              {station.stationCode}
            </Select.Option>
          ))}
        </Select>
      </Form.Item>
      <Form.Item
        name="mustPassStation"
        label="是否必经站点"
        initialValue={false}
      >
        <Switch />
      </Form.Item>
      <Form.Item
        name="checkStationList"
        label="检查站点列表"
        rules={[
          { required: true, message: '请选择检查站点列表' },
          {
            validator: (_, value) => {
              if (Array.isArray(value)) {
                // 检查是否有重复项
                const uniqueValues = [...new Set(value)];
                if (uniqueValues.length < value.length) {
                  return Promise.reject(new Error('不允许重复选择相同站点'));
                }
              }
              return Promise.resolve();
            }
          }
        ]}
        valuePropName="value"
        getValueFromEvent={(values) => {
          // 从多选值转换为逗号分隔的字符串
          if (Array.isArray(values)) {
            // 去重并过滤空值
            const uniqueValues = [...new Set(values)].filter(item => item.trim());
            return uniqueValues.join(',');
          }
          return values;
        }}
        normalize={(value) => {
          // 从逗号分隔的字符串转换为数组，用于编辑时的显示
          if (typeof value === 'string') {
            // 分割字符串，过滤空值，然后去重
            return value.split(',').map(item => item.trim()).filter(item => item);
          }
          return value;
        }}
      >
        <Select
          mode="multiple"
          placeholder="请选择检查站点列表"
          showSearch
          filterOption={(input, option) =>
            (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
          }
          loading={stationLoading}
        >
          {stations.map((station) => (
            <Select.Option key={station.stationCode} value={station.stationCode}>
              {station.stationCode}
            </Select.Option>
          ))}
        </Select>
      </Form.Item>
      <Form.Item
        name="firstStation"
        label="是否首站点"
        initialValue={false}
      >
        <Switch />
      </Form.Item>
      <Form.Item
        name="checkAll"
        label="是否检查所有"
        initialValue={false}
      >
        <Switch />
      </Form.Item>
    </Form>
  );
};

export default ProcessRouteItemForm;