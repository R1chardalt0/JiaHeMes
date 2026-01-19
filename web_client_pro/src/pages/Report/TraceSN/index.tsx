import React, { useState } from 'react';
import { PageContainer } from '@ant-design/pro-components';
import { Card, Table, Typography, Spin, message, Form, Input, Button } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { getTraceSN } from '@/services/Api/Trace/TraceSN';

const { Title, Text } = Typography;

const TraceSN: React.FC = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [traceData, setTraceData] = useState<any>(null);
  const [sn, setSn] = useState<string>('SN2026011500001');
  const [form] = Form.useForm();

  const fetchData = async (searchSn: string) => {
    if (!searchSn) {
      message.warning('请输入SN号');
      return;
    }

    try {
      setLoading(true);
      const data = await getTraceSN(searchSn);
      setTraceData(data);
      if (!data) {
        message.info('未找到该SN号的追溯数据');
      }
    } catch (error) {
      message.error('获取追溯数据失败');
      console.error('Error fetching trace data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    fetchData(sn);
  };

  const handleFormChange = (changedValues: any) => {
    if (changedValues.sn) {
      setSn(changedValues.sn);
    }
  };

  // 工位信息表格列定义
  const stationColumns = [
    {
      title: '工位代码',
      dataIndex: 'stationCode',
      key: 'stationCode',
      width: 120,
    },
    {
      title: '工位名称',
      dataIndex: 'stationName',
      key: 'stationName',
      width: 200,
    },
    {
      title: '工位状态',
      dataIndex: 'stationStatus',
      key: 'stationStatus',
      width: 100,
      render: (status: number) => status === 1 ? '正常' : '异常',
    },
    {
      title: '资源代码',
      dataIndex: 'resourceCode',
      key: 'resourceCode',
      width: 120,
    },
    {
      title: '测试结果',
      dataIndex: 'testResult',
      key: 'testResult',
      width: 100,
      render: (result: string) => (
        <Text style={{ color: result === '合格' ? 'green' : 'red' }}>
          {result}
        </Text>
      ),
    },{
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 160,
    },
    {
      title: '测试时间',
      dataIndex: 'testTime',
      key: 'testTime',
      width: 180,
      render: (time: string) => new Date(time).toLocaleString(),
    },
    {
      title: '测试数据',
      dataIndex: 'testData',
      key: 'testData',
      render: (testData: any[]) => {
        if (!testData || testData.length === 0) {
          return '无';
        }
        
        // 将testData并行转列显示，只显示parametricKey和testValue
      
        return (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '15px' }}>
            {testData.map((test, index) => (
              <div key={index} style={{ display: 'flex', alignItems: 'center' }}>
                <Text strong style={{ marginRight: '5px' ,color: test.testResult === 'PASS' ? 'green' : 'red' }}>{test.parametricKey}:</Text>
                <Text>{test.testValue}</Text>
              </div>
            ))}
          </div>
        );
      },
    },
  ];

  // 批次信息表格列定义
  const batchColumns = [
    {
      title: '批次代码',
      dataIndex: 'batchCode',
      key: 'batchCode',
    },
    {
      title: '工位代码',
      dataIndex: 'stationCode',
      key: 'stationCode',
    },
    {
      title: '产品代码',
      dataIndex: 'productCode',
      key: 'productCode',
    },
    {
      title: '批次数量',
      dataIndex: 'batchQty',
      key: 'batchQty',
      width: 100,
    },
    {
      title: '完成数量',
      dataIndex: 'completedQty',
      key: 'completedQty',
      width: 100,
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 80,
      render: (status: number) => status === 1 ? '正常' : '异常',
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      width: 180,
      render: (time: string) => new Date(time).toLocaleString(),
    },
  ];

  return (
    <PageContainer header={{ title: '单条码追溯' }}>
      {/* 查询条件表单 */}
      <Card size="small" style={{ marginBottom: 20 }}>
        <Form
          form={form}
          layout="inline"
          initialValues={{ sn }}
          onValuesChange={handleFormChange}
          style={{ width: '100%' }}
        >
          <Form.Item
            name="sn"
            label="SN号"
            rules={[{ required: true, message: '请输入SN号' }]}
            style={{ marginBottom: 0 }}
          >
            <Input
              placeholder="请输入SN号"
              style={{ width: 300 }}
              onPressEnter={handleSearch}
            />
          </Form.Item>
          <Form.Item style={{ marginBottom: 0 }}>
            <Button
              type="primary"
              icon={<SearchOutlined />}
              onClick={handleSearch}
              loading={loading}
            >
              查询
            </Button>
          </Form.Item>
        </Form>
      </Card>

      <Spin spinning={loading} tip="加载中...">
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {traceData ? (
            <>
              {/* 基本信息卡片 */}
              <Card title="基本信息" size="small">
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '15px' }}>
                  <div><Text strong>SN:</Text> {traceData.sn}</div>
                  <div><Text strong>订单号:</Text> {traceData.orderCode}</div>
                  <div><Text strong>产品代码:</Text> {traceData.productCode}</div>
                  <div><Text strong>当前工位:</Text> {traceData.currentStation}</div>
                  <div><Text strong>工位状态:</Text> {traceData.stationStatus === 1 ? '正常' : '异常'}</div>
                  <div><Text strong>是否异常:</Text> {traceData.isAbnormal ? '是' : '否'}</div>
                  <div><Text strong>创建时间:</Text> {new Date(traceData.createTime).toLocaleString()}</div>
                  <div><Text strong>更新时间:</Text> {new Date(traceData.updateTime).toLocaleString()}</div>
                </div>
              </Card>

              {/* 工位信息表格 */}
              <Card title="工位信息" size="small">
                <Table
                  columns={stationColumns}
                  dataSource={traceData.stations || []}
                  rowKey={(record, index) => (index || 0).toString()}
                  pagination={false}
                  scroll={{ x: 'max-content' }}
                />
              </Card>

              {/* 批次信息表格 */}
              <Card title="批次信息" size="small">
                <Table
                  columns={batchColumns}
                  dataSource={traceData.feedingBatches || []}
                  rowKey={(record, index) => (index || 0).toString()}
                  pagination={false}
                  scroll={{ x: 'max-content' }}
                />
              </Card>
            </>
          ) : (
            <Card size="small">
              <div style={{ textAlign: 'center', padding: '50px 0' }}>
                <Text type="secondary">请输入SN号查询追溯数据</Text>
              </div>
            </Card>
          )}
        </div>
      </Spin>
    </PageContainer>
  );
};

export default TraceSN;