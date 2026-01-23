import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { EyeOutlined } from '@ant-design/icons';
import { Table, Button, Tabs, message, Card, Row, Col, Modal } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { getTraceInfoList, getTraceInfoDetail, getMaterialInfoList, getProcessInfoList, deleteMaterialInfo, deleteProcessInfo } from '@/services/Api/Trace/TraceInfo';
import { TraceInfoDto, TraceInfoQueryDto, TraceInfoDetailDto, MaterialInfoDto, ProcessInfoDto, ValueObject, isValueObject, processApiResponse, getValueDisplay } from '@/services/Model/Trace/TraceInfo';

import type { RequestData } from '@ant-design/pro-components';

const { TabPane } = Tabs;

const TraceInfoPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<TraceInfoDto | null>(null);
  const [detailData, setDetailData] = useState<TraceInfoDetailDto | null>(null);
  const [materialList, setMaterialList] = useState<MaterialInfoDto[]>([]);
  const [processList, setProcessList] = useState<ProcessInfoDto[]>([]);
  const [activeTabKey, setActiveTabKey] = useState('material');
  const [messageApi] = message.useMessage();
  const [currentSearchParams, setCurrentSearchParams] = useState<TraceInfoQueryDto>({
    current: 1,
    pageSize: 50
  });

  // 表格列定义
  const columns: ProColumns<TraceInfoDto>[] = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 180,
      search: true
    },
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 120,
      search: false,
      render: getValueDisplay,
    },
    {
      title: 'PIN',
      dataIndex: 'pin',
      key: 'pin',
      width: 100,
      search: true,
      render: getValueDisplay,
    },
    {
      title: 'VSN',
      dataIndex: 'vsn',
      key: 'vsn',
      width: 80,
      search: true,
      render: getValueDisplay
    },
    {
      title: 'BOM Id',
      dataIndex: 'bomRecipeId',
      key: 'bomRecipeId',
      width: 80,
      search: false,
      render: getValueDisplay
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 160,
      search: false,
      valueType: 'dateTime'
    },
    {
      title: '操作',
      key: 'operation',
      width: 80,
      fixed: 'right',
      render: (_, record) => (
        <Button
          type="link"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => handleShowDetail(record)}
          style={{ marginRight: 8 }}
        >
          查看
        </Button>
      )
    }
  ];

  // 物料表格列定义
  const materialColumns: ColumnsType<MaterialInfoDto> = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 180
    },
    {
      title: 'VSN',
      dataIndex: 'vsn',
      key: 'vsn',
      width: 80,
      render: getValueDisplay
    },
    {
      title: 'BOM项编码',
      dataIndex: 'bomItemCode',
      key: 'bomItemCode',
      width: 150,
      render: getValueDisplay
    },
    {
      title: '物料编码',
      dataIndex: 'materialCode',
      key: 'materialCode',
      width: 150,
      render: getValueDisplay
    },
    {
      title: '计量单位',
      dataIndex: 'measureUnit',
      key: 'measureUnit',
      width: 100,
      render: getValueDisplay
    },
    {
      title: '定额',
      dataIndex: 'quota',
      key: 'quota',
      width: 80
    },
    {
      title: 'SKU',
      dataIndex: 'sku',
      key: 'sku',
      width: 120,
      render: getValueDisplay
    },
    {
      title: '消耗量',
      dataIndex: 'consumption',
      key: 'consumption',
      width: 100
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 180
    }
  ];

  // 过程表格列定义
  const processColumns: ColumnsType<ProcessInfoDto> = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 180
    },
    {
      title: 'VSN',
      dataIndex: 'vsn',
      key: 'vsn',
      width: 80,
      render: getValueDisplay
    },
    {
      title: '工位',
      dataIndex: 'station',
      key: 'station',
      width: 120
    },
    {
      title: '参数键',
      dataIndex: 'key',
      key: 'key',
      width: 150
    },
    {
      title: '参数值',
      dataIndex: 'value',
      key: 'value',
      width: 200,
      render: (text) => {
        if (Array.isArray(text) && text.length > 0) {
          return JSON.stringify(text);
        }
        return JSON.stringify(text || []);
      }
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 180
    },
    {
      title: '是否删除',
      dataIndex: 'isDeleted',
      key: 'isDeleted',
      width: 100,
      render: (text) => text ? '是' : '否'
    }
  ];

  // 显示详情
  const handleShowDetail = useCallback(async (record: TraceInfoDto) => {
    console.log('TraceInfo - 查看详情记录:', record);

    try {
      setCurrentRow(record);
      setShowDetail(true);

      // 获取详情信息
      console.log('TraceInfo - 获取详情信息, ID:', record.id);
      const detailResponse = await getTraceInfoDetail(record.id);
      console.log('TraceInfo - 详情信息返回:', detailResponse);
      setDetailData(processApiResponse(detailResponse, true) as TraceInfoDetailDto);

      // 获取物料信息
      console.log('TraceInfo - 获取物料信息, traceInfoId:', record.id);
      const materialResponse = await getMaterialInfoList(record.id);
      console.log('TraceInfo - 物料信息返回:', materialResponse);
      setMaterialList(processApiResponse(materialResponse) as MaterialInfoDto[]);

      // 获取过程信息
      console.log('TraceInfo - 获取过程信息, traceInfoId:', record.id);
      const processResponse = await getProcessInfoList(record.id);
      console.log('TraceInfo - 过程信息返回:', processResponse);
      setProcessList(processApiResponse(processResponse) as ProcessInfoDto[]);
    } catch (error) {
      messageApi.error('获取详情失败');
      console.error('获取详情失败:', error);
    }
  }, [messageApi]);

  // 双击行显示详情
  const handleRowDoubleClick = useCallback((record: TraceInfoDto) => {
    handleShowDetail(record);
  }, [handleShowDetail]);

  return (
    <PageContainer title="生产追溯">
      <ProTable<TraceInfoDto>
        actionRef={actionRef}
        rowKey="id"
        className="trace-info-table"
        columns={columns}
        search={{
          labelWidth: 120,
          layout: 'vertical',
        }}
        cardProps={{
          style: (window as any).__panelStyles?.panelStyle,
          headStyle: (window as any).__panelStyles?.headStyle,
          bodyStyle: (window as any).__panelStyles?.bodyStyle,
          bordered: false,
          ['data-panel-exempt']: 'true'
        } as any}
        request={async (
          params
        ): Promise<RequestData<TraceInfoDto>> => {
          console.log('TraceInfo - 请求参数:', params);

          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            id: params.id,
            productCode: params.productCode,
            pin: params.pin,
            vsn: params.vsn,
          });

          const queryParams: TraceInfoQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            id: params.id,
            productCode: params.productCode,
            pin: params.pin,
            vsn: params.vsn,
          };

          console.log('TraceInfo - API查询参数:', queryParams);

          try {
            const response = await getTraceInfoList(queryParams);
            console.log('TraceInfo - API返回数据:', response);
            //console.log('TraceInfo - 第一条数据详情:', JSON.stringify(response[0], null, 2));

            // 使用通用API响应处理函数获取数据
            const data = processApiResponse(response);
            const result = {
              data: data,
              total: response.total,
              success: response.success,
            };

            console.log('TraceInfo - 表格数据:', result);
            return result;
          } catch (error) {
            console.error('TraceInfo - API调用失败:', error);
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        rowSelection={false}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100'],
        }}
        onRow={(record) => ({
          onDoubleClick: () => handleRowDoubleClick(record),
        })}
      />

      {showDetail && currentRow && detailData && (
        <div className="trace-info-detail" style={{ marginTop: 20 }}>
          <ProDescriptions<TraceInfoDetailDto>
            title="产品信息"
            dataSource={detailData}
            column={3}
          >
            <ProDescriptions.Item label="ID">{detailData.id}</ProDescriptions.Item>
            <ProDescriptions.Item label="产品编码">
              {getValueDisplay(detailData.productCode)}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="PIN">
              {getValueDisplay(detailData.pin)}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="VSN">{detailData.vsn}</ProDescriptions.Item>
            <ProDescriptions.Item label="BOM ID">{detailData.bomRecipeId}</ProDescriptions.Item>
            <ProDescriptions.Item label="产线">{detailData.productLine}</ProDescriptions.Item>
            <ProDescriptions.Item label="创建时间">{detailData.createdAt}</ProDescriptions.Item>
          </ProDescriptions>

          <Tabs activeKey={activeTabKey} onChange={setActiveTabKey} style={{ marginTop: 20 }}>
            <TabPane tab="物料信息" key="material">
              <Row gutter={[16, 16]}>
                {materialList.map((material) => (
                  <Col xs={24} sm={12} md={8} key={material.id}>
                    <Card
                      title={`物料信息 - ${getValueDisplay(material.materialCode)}`}
                      extra={
                        !material.isDeleted && (
                          <Button
                            type="primary"
                            danger
                            onClick={() => {
                              Modal.confirm({
                                title: '确认删除',
                                content: '确认要删除这条物料信息吗？',
                                onOk: async () => {
                                  try {
                                    await deleteMaterialInfo(material.id);
                                    message.success('物料信息删除成功');
                                    // 重新获取物料信息
                                    if (currentRow) {
                                      const newMaterialList = await getMaterialInfoList(currentRow.id);
                                      setMaterialList(processApiResponse(newMaterialList) as MaterialInfoDto[]);
                                    }
                                  } catch (error) {
                                    message.error('物料信息删除失败');
                                    console.error('Delete material error:', error);
                                  }
                                },
                                onCancel: () => {
                                  // 取消删除操作
                                }
                              });
                            }}
                          >
                            删除
                          </Button>
                        )
                      }
                      style={{
                        backgroundColor: material.isDeleted ? '#f5f5f5' : '',
                        borderColor: material.isDeleted ? '#d9d9d9' : '',
                        opacity: material.isDeleted ? 0.6 : 1
                      }}
                    >
                      <div style={{ lineHeight: '1.8', fontSize: '14px', color: material.isDeleted ? '#8c8c8c' : '' }}>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>ID:</strong> {material.id}</p>
                        <p style={{ marginBottom: '8px' }}><strong>VSN:</strong> {material.vsn}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>追溯信息ID:</strong> {material.traceInfoId}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>BOM项编码:</strong> {getValueDisplay(material.bomItemCode)}</p>
                        <p style={{ marginBottom: '8px' }}><strong>BOM ID:</strong> {material.bomId}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>物料编码:</strong> {getValueDisplay(material.materialCode)}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>计量单位:</strong> {getValueDisplay(material.measureUnit)}</p>
                        <p style={{ marginBottom: '8px' }}><strong>定额:</strong> {material.quota}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>SKU:</strong> {getValueDisplay(material.sku)}</p>
                        <p style={{ marginBottom: '8px' }}><strong>消耗量:</strong> {material.consumption}</p>
                        <p style={{ marginBottom: '8px' }}><strong>是否删除:</strong> {material.isDeleted ? '是' : '否'}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>创建时间:</strong> {material.createdAt}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>删除时间:</strong> {material.deletedAt}</p>
                      </div>
                    </Card>
                  </Col>
                ))}
              </Row>
            </TabPane>
            <TabPane tab="过程信息" key="process">
              <Row gutter={[16, 16]}>
                {processList.map((process) => (
                  <Col xs={24} sm={12} md={8} key={process.id}>
                    <Card
                      title={`过程信息 - ${process.station}`}
                      extra={
                        !process.isDeleted && (
                          <Button
                            type="primary"
                            danger
                            onClick={() => {
                              Modal.confirm({
                                title: '确认删除',
                                content: '确认要删除这条过程信息吗？',
                                onOk: async () => {
                                  try {
                                    await deleteProcessInfo(process.id);
                                    message.success('过程信息删除成功');
                                    // 重新获取过程信息
                                    if (currentRow) {
                                      const newProcessList = await getProcessInfoList(currentRow.id);
                                      setProcessList(processApiResponse(newProcessList) as ProcessInfoDto[]);
                                    }
                                  } catch (error) {
                                    message.error('过程信息删除失败');
                                    console.error('Delete process error:', error);
                                  }
                                },
                                onCancel: () => {
                                  // 取消删除操作
                                }
                              });
                            }}
                          >
                            删除
                          </Button>
                        )
                      }
                      style={{
                        backgroundColor: process.isDeleted ? '#f5f5f5' : '',
                        borderColor: process.isDeleted ? '#d9d9d9' : '',
                        opacity: process.isDeleted ? 0.6 : 1
                      }}
                    >
                      <div style={{ lineHeight: '1.8', fontSize: '14px', color: process.isDeleted ? '#8c8c8c' : '' }}>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>ID:</strong> {process.id}</p>
                        <p style={{ marginBottom: '8px' }}><strong>VSN:</strong> {process.vsn}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>追溯信息ID:</strong> {process.traceInfoId}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>工位:</strong> {process.station}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>Key:</strong> {process.key}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>Value:</strong>
                          {Array.isArray(process.value) && process.value.length > 0 ?
                            <pre style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-all', backgroundColor: '#f0f0f0', padding: '8px', borderRadius: '4px', margin: '8px 0', fontSize: '12px', opacity: process.isDeleted ? 0.7 : 1 }}>
                              {JSON.stringify(process.value, null, 2)}
                            </pre> :
                            JSON.stringify(process.value || [])}
                        </p>
                        <p style={{ marginBottom: '8px' }}><strong>是否删除:</strong> {process.isDeleted ? '是' : '否'}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>创建时间:</strong> {process.createdAt}</p>
                        <p style={{ marginBottom: '8px', wordBreak: 'break-all' }}><strong>删除时间:</strong> {process.deletedAt}</p>
                      </div>
                    </Card>
                  </Col>
                ))}
              </Row>
            </TabPane>
          </Tabs>
        </div>
      )}
    </PageContainer>
  );
};

export default TraceInfoPage;
