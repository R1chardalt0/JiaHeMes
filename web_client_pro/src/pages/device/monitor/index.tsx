import { Alert, Button, Card, List, Typography } from 'antd';
import type { DeviceMonitorDataType, DeviceDetailType, ApiResponse } from '../../../services/Model/Device/device_monitor_data';
import { getDeviceDetail } from '../../../services/Api/Device/Monitor/deviceMonitorService';
import useStyles from './style.style';
import { useNavigate, useRequest } from '@umijs/max';
import React, { useRef, useState, useEffect } from 'react';
import {
  FooterToolbar,
  ModalForm,
  PageContainer,
  ProDescriptions,
  ProFormText,
  ProFormTextArea,
  ProTable,
} from '@ant-design/pro-components';
import DeviceMonitorModal from './DeviceMonitorModal';
import { PlusOutlined } from '@ant-design/icons';

const { Paragraph } = Typography;

const CardList = () => {
  const { styles } = useStyles();

  // 🔧 关键修改：添加 formatResult 以保留完整响应
  const { data: apiResponse, loading } = useRequest(() => getDeviceDetail(), {
    formatResult: (res: ApiResponse<DeviceDetailType>) => res, // 👈 保留完整的 { success, data, ... } 对象
  });

  const [list, setList] = useState<DeviceDetailType[]>([]);

  // 🔧 调试：监控 apiResponse 变化
  useEffect(() => {
    console.log('【调试】apiResponse:', apiResponse); // 👈 查看实际获取的数据

    if (apiResponse && typeof apiResponse === 'object') {
      // 检查 success 字段（兼容 boolean 和 string）
      const isSuccess = apiResponse.success === true;
      if (isSuccess) {
        // 检查 data 字段是否存在且为数组
        if ('data' in apiResponse && Array.isArray(apiResponse.data)) {
          console.log('✅ 接口成功，设置设备列表，数量:', apiResponse.data.length); // 👈 确认进入这里
          setList(apiResponse.data);
        } else {
          console.warn('⚠️ 接口返回 success=true，但 data 字段缺失或不是数组:', apiResponse.data);
        }
      } else {
        console.warn('❌ 接口返回失败或 success 不为 true:', apiResponse);
      }
    } else {
      console.log('ℹ️ apiResponse 为空或不是对象:', apiResponse);
    }
  }, [apiResponse]);

  const content = (
    <div className={styles.pageHeaderContent}>
      {/* 内容可选 */}
    </div>
  );

  const extraContent = (
    <div className={styles.extraImg}>
      {/* 图片可选 */}
    </div>
  );

  // 🔧 如果你想保留“新增”按钮，请取消下面这行的注释，并注释掉 dataSource={[...list]}
  const nullData: Partial<DeviceDetailType> = {};

  const navigate = useNavigate();
  const [modalVisible, setModalVisible] = useState(false);
  const [selectedDevice, setSelectedDevice] = useState<DeviceDetailType | undefined>(undefined);

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);

  return (
    <PageContainer content={content} extraContent={extraContent}>
      <div className={styles.cardList}>
        <List<DeviceDetailType>
          rowKey="deviceId"
          loading={loading}
          grid={{
            gutter: 16,
            xs: 1,
            sm: 2,
            md: 3,
            lg: 3,
            xl: 4,
            xxl: 4,
          }}
          // ✅ dataSource 选择：根据是否需要“新增”按钮决定
          // 方案1：只显示设备列表
          dataSource={[...list]}
          // 方案2：在开头显示“新增”按钮 (取消注释下面一行，注释上面一行)
          // dataSource={[nullData, ...list]}
          renderItem={(item) => {
            // 🔧 调试：查看 renderItem 是否被调用
            // console.log('【渲染】renderItem:', item ? item.deviceName : 'nullData');

            // 🔧 检查 item 是否有 deviceId (用于正常设备) 或是 nullData
            if (item?.deviceId) {
              return (
                <List.Item key={item.deviceId}>
                  <Card
                    hoverable
                    className={styles.card}
                    actions={[
                      <a
                        key="option1"
                        onClick={() => {
                          setSelectedDevice(item);
                          setModalVisible(true);
                        }}
                      >
                        监控数据
                      </a>,
                    ]}
                  >
                    <Card.Meta
                      avatar={
                        <img
                          alt=""
                          className={styles.cardAvatar}
                          // 🔧 （可选）使用设备图片或占位图
                          src={item.avatar}
                        />
                      }
                      title={<a>{item.deviceName}</a>}
                      description={
                        <Paragraph
                          className={styles.item}
                          ellipsis={{
                            rows: 3,
                          }}
                        >
                          {item.description}
                        </Paragraph>
                      }
                    />
                  </Card>
                </List.Item>
              );
            }
            // 🔧 处理 nullData (用于“新增”按钮)
            // 注意：只有当你使用 dataSource={[nullData, ...list]} 时，这个分支才会执行
            return (
              <List.Item>
                <Button type="dashed" className={styles.newButton}>
                  <PlusOutlined /> 新增产品
                </Button>
              </List.Item>
            );
          }}
        />
      </div>

      {/* 监控数据弹窗 */}
      <DeviceMonitorModal
        visible={modalVisible}
        device={selectedDevice}
        onClose={() => setModalVisible(false)}
      />

      {/* 新建规则弹窗 */}
      <ModalForm
        title="新建规则"
        width="400px"
        open={createModalVisible}
        onVisibleChange={handleModalVisible}
        onFinish={async (value) => {
          // handleModalVisible(false);
        }}
      />
    </PageContainer>
  );
};

export default CardList;