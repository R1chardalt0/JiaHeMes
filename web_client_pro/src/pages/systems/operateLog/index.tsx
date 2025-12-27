import { ActionType, ProColumns, ProDescriptions } from '@ant-design/pro-components';
import { PageContainer, ProTable } from '@ant-design/pro-components';
import { useRequest } from '@umijs/max';
import { Button, Drawer, message, Tag, Space, Collapse } from 'antd';
import React, { useRef, useState } from 'react';
import { getOperationLogList } from '@/services/Api/Systems/operationLog';
import type { OperationLogItem, OperationLogQueryDto } from '@/services/Model/Systems/operationLog';
import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';

const { Panel: CollapsePanel } = Collapse;

const OperationLogList: React.FC = () => {
  const actionRef = useRef<ActionType>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<OperationLogItem>();

  // 格式化JSON数据
  const formatJson = (jsonStr?: string) => {
    if (!jsonStr) return '-';
    try {
      const obj = JSON.parse(jsonStr);
      return JSON.stringify(obj, null, 2);
    } catch {
      return jsonStr;
    }
  };

  // 获取操作类型显示文本
  const getOperationTypeText = (type?: string) => {
    switch (type) {
      case 'INSERT':
        return { text: '新增', color: 'success' };
      case 'UPDATE':
        return { text: '修改', color: 'processing' };
      case 'DELETE':
        return { text: '删除', color: 'error' };
      case 'LOGIN':
        return { text: '登录', color: 'default' };
      default:
        return { text: type || '未知', color: 'default' };
    }
  };

  // 获取操作状态显示文本
  const getOperationStatusText = (status?: string) => {
    switch (status) {
      case 'SUCCESS':
        return { text: '成功', icon: <CheckCircleOutlined />, color: 'success' };
      case 'FAIL':
        return { text: '失败', icon: <CloseCircleOutlined />, color: 'error' };
      default:
        return { text: status || '未知', icon: null, color: 'default' };
    }
  };

  const columns: ProColumns<OperationLogItem>[] = [
    {
      title: '日志ID',
      dataIndex: 'logId',
      hideInTable: true,
      hideInSearch: true,
    },
    {
      title: '操作用户工号',
      dataIndex: 'userCode',
      width: 120,
      render: (dom, entity) => (
        <a
          onClick={() => {
            setCurrentRow(entity);
            setShowDetail(true);
          }}
        >
          {dom}
        </a>
      ),
    },
    {
      title: '操作用户姓名',
      dataIndex: 'userName',
      width: 120,
    },
    {
      title: '操作类型',
      dataIndex: 'operationType',
      width: 100,
      valueType: 'select',
      valueEnum: {
        INSERT: { text: '新增' },
        UPDATE: { text: '修改' },
        DELETE: { text: '删除' },
        LOGIN: { text: '登录' },
      },
      render: (_, record) => {
        const { text, color } = getOperationTypeText(record.operationType);
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: '操作模块',
      dataIndex: 'operationModule',
      width: 150,
    },
    {
      title: '操作对象ID',
      dataIndex: 'targetId',
      width: 120,
    },
    {
      title: '操作时间',
      dataIndex: 'operationTime',
      valueType: 'dateTimeRange', // 搜索时使用范围选择器
      width: 180,
      sorter: true,
      // 格式化显示时间（表格中显示）
      render: (_, record) => {
        if (!record.operationTime) return '--';
        try {
          // 处理各种可能的时间格式
          let date: Date;
          const timeValue = record.operationTime as any;
          
          if (typeof timeValue === 'string') {
            // 尝试解析 ISO 格式或数据库格式
            date = new Date(timeValue);
            if (isNaN(date.getTime())) {
              // 如果标准解析失败，尝试其他格式
              return timeValue;
            }
          } else if (timeValue && typeof timeValue === 'object' && 'getTime' in timeValue) {
            // 是 Date 对象
            date = timeValue as Date;
          } else {
            return '--';
          }
          
          // 格式化为 YYYY-MM-DD HH:mm:ss
          const year = date.getFullYear();
          const month = String(date.getMonth() + 1).padStart(2, '0');
          const day = String(date.getDate()).padStart(2, '0');
          const hours = String(date.getHours()).padStart(2, '0');
          const minutes = String(date.getMinutes()).padStart(2, '0');
          const seconds = String(date.getSeconds()).padStart(2, '0');
          return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
        } catch (e) {
          console.error('格式化操作时间失败:', e, record.operationTime);
          return (record.operationTime as string) || '--';
        }
      },
      // 搜索配置
      search: {
        transform: (value: any) => {
          return {
            operationTime: value,
          };
        },
      },
    },
    {
      title: '操作IP',
      dataIndex: 'operationIp',
      width: 130,
      hideInSearch: true,
    },
    {
      title: '操作状态',
      dataIndex: 'operationStatus',
      width: 100,
      valueType: 'select',
      valueEnum: {
        SUCCESS: { text: '成功' },
        FAIL: { text: '失败' },
      },
      render: (_, record) => {
        const { text, icon, color } = getOperationStatusText(record.operationStatus);
        return (
          <Tag color={color} icon={icon}>
            {text}
          </Tag>
        );
      },
    },
    {
      title: '操作备注',
      dataIndex: 'operationRemark',
      hideInTable: true, // 在列表中隐藏，只在详情中显示
      hideInSearch: true,
    },
    {
      title: '操作前数据',
      dataIndex: 'beforeData',
      hideInTable: true,
      hideInSearch: true,
    },
    {
      title: '操作后数据',
      dataIndex: 'afterData',
      hideInTable: true,
      hideInSearch: true,
    },
  ];

  return (
    <PageContainer className="system-settings-page">
      <ProTable<OperationLogItem>
        rowKey="logId"
        actionRef={actionRef}
        columns={columns}
        request={async (params, sort, filter) => {
          try {
            // 处理操作时间范围
            let operationTimeStart: string | undefined;
            let operationTimeEnd: string | undefined;
            
            if (params.operationTime) {
              // dateTimeRange 返回的是数组格式 [start, end]
              if (Array.isArray(params.operationTime) && params.operationTime.length === 2) {
                const start = params.operationTime[0];
                const end = params.operationTime[1];
                
                // 格式化开始时间
                if (start) {
                  const startDate = start instanceof Date ? start : new Date(start);
                  // 使用 ISO 8601 格式，确保时区信息正确
                  operationTimeStart = startDate.toISOString();
                }
                
                // 格式化结束时间（设置为当天的最后一刻）
                if (end) {
                  const endDate = end instanceof Date ? end : new Date(end);
                  // 设置为当天的 23:59:59.999
                  endDate.setHours(23, 59, 59, 999);
                  operationTimeEnd = endDate.toISOString();
                }
              } else if (params.operationTime) {
                // 单个日期的情况（向后兼容）
                const time = params.operationTime;
                const date = time instanceof Date ? time : new Date(time);
                operationTimeStart = date.toISOString();
                const endTime = new Date(date);
                endTime.setHours(23, 59, 59, 999);
                operationTimeEnd = endTime.toISOString();
              }
            }
            
            // 调试日志（开发环境）
            if (process.env.NODE_ENV === 'development' && (operationTimeStart || operationTimeEnd)) {
              console.log('操作时间范围查询:', {
                operationTimeStart,
                operationTimeEnd,
                original: params.operationTime
              });
            }

            const queryParams: OperationLogQueryDto = {
              current: params.current || 1,
              pageSize: params.pageSize || 20,
              keyword: params.keyword,
              userCode: params.userCode,
              userName: params.userName,
              operationType: params.operationType,
              operationModule: params.operationModule,
              targetId: params.targetId,
              operationStatus: params.operationStatus,
              operationTimeStart: operationTimeStart,
              operationTimeEnd: operationTimeEnd,
            };

            const res = await getOperationLogList(queryParams);
            if (!res.success) {
              return {
                data: [],
                success: false,
                total: 0,
              };
            }

            const formattedData = (res.data || []).map((item: any) => {
              // 确保所有字段都正确映射，包括操作备注
              const formatted = {
                ...item,
                logId: item.logId ?? item.LogId ?? item.log_id,
                userCode: item.userCode ?? item.UserCode ?? item.user_code ?? '',
                userName: item.userName ?? item.UserName ?? item.user_name ?? '',
                operationType: item.operationType ?? item.OperationType ?? item.operation_type ?? '',
                operationModule: item.operationModule ?? item.OperationModule ?? item.operation_module ?? '',
                targetId: item.targetId ?? item.TargetId ?? item.target_id ?? '',
                beforeData: item.beforeData ?? item.BeforeData ?? item.before_data,
                afterData: item.afterData ?? item.AfterData ?? item.after_data,
                operationTime: item.operationTime ?? item.OperationTime ?? item.operation_time,
                operationIp: item.operationIp ?? item.OperationIp ?? item.operation_ip ?? '',
                operationRemark: item.operationRemark ?? item.OperationRemark ?? item.operation_remark ?? '',
                operationStatus: item.operationStatus ?? item.OperationStatus ?? item.operation_status ?? '',
              };
              
              // 调试日志（开发环境）
              if (process.env.NODE_ENV === 'development') {
                if (!formatted.operationTime) {
                  console.warn('操作时间为空的数据项:', {
                    original: item,
                    formatted: formatted
                  });
                } else {
                  console.log('操作时间数据:', {
                    original: item.operationTime ?? item.OperationTime ?? item.operation_time,
                    formatted: formatted.operationTime,
                    type: typeof formatted.operationTime
                  });
                }
              }
              
              return formatted;
            });

            return {
              data: formattedData,
              success: true,
              total: res.total || 0,
            };
          } catch (error) {
            message.error('获取操作日志列表失败');
            return {
              data: [],
              success: false,
              total: 0,
            };
          }
        }}
        pagination={{
          defaultPageSize: 20,
          pageSizeOptions: ['10', '20', '50', '100'],
          showSizeChanger: true,
          showTotal: (total) => `共 ${total} 条记录`,
        }}
        search={{
          labelWidth: 'auto',
          defaultCollapsed: true, // 默认收起查询区域
        }}
        expandable={{
          expandedRowRender: (record) => (
            <div style={{ padding: '16px', background: '#fafafa' }}>
              <Collapse defaultActiveKey={['before', 'after']} ghost>
                {record.beforeData && (
                  <CollapsePanel header="操作前数据" key="before">
                    <pre style={{ margin: 0, whiteSpace: 'pre-wrap', wordBreak: 'break-all' }}>
                      {formatJson(record.beforeData)}
                    </pre>
                  </CollapsePanel>
                )}
                {record.afterData && (
                  <CollapsePanel header="操作后数据" key="after">
                    <pre style={{ margin: 0, whiteSpace: 'pre-wrap', wordBreak: 'break-all' }}>
                      {formatJson(record.afterData)}
                    </pre>
                  </CollapsePanel>
                )}
                {!record.beforeData && !record.afterData && (
                  <div style={{ color: '#999' }}>暂无数据</div>
                )}
              </Collapse>
            </div>
          ),
        }}
      />

      {/* 详情抽屉 */}
      <Drawer
        width={800}
        open={showDetail}
        onClose={() => setShowDetail(false)}
        title="操作日志详情"
        closable={false}
        className="operation-log-drawer"
        rootClassName="operation-log-drawer"
        styles={{
          content: {
            background: '#ffffff',
            borderLeft: '1px solid #f0f0f0',
            boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
          },
          header: {
            background: '#ffffff',
            borderBottom: '1px solid #f0f0f0'
          },
          body: {
            background: '#ffffff'
          },
          mask: {
            background: 'rgba(0,0,0,0.1)'
          }
        }}
      >
        {currentRow && (
          <div>
            <ProDescriptions<OperationLogItem>
              column={2}
              title={`操作日志 #${currentRow.logId}`}
              dataSource={currentRow}
              columns={[
                { title: '日志ID', dataIndex: 'logId' },
                { title: '操作用户工号', dataIndex: 'userCode' },
                { title: '操作用户姓名', dataIndex: 'userName' },
                {
                  title: '操作类型',
                  dataIndex: 'operationType',
                  render: (_, record) => {
                    const { text, color } = getOperationTypeText(record.operationType);
                    return <Tag color={color}>{text}</Tag>;
                  }
                },
                { title: '操作模块', dataIndex: 'operationModule' },
                { title: '操作对象ID', dataIndex: 'targetId' },
                { title: '操作时间', dataIndex: 'operationTime', valueType: 'dateTime' },
                { title: '操作IP', dataIndex: 'operationIp' },
                {
                  title: '操作状态',
                  dataIndex: 'operationStatus',
                  render: (_, record) => {
                    const { text, icon, color } = getOperationStatusText(record.operationStatus);
                    return (
                      <Tag color={color} icon={icon}>
                        {text}
                      </Tag>
                    );
                  }
                },
                {
                  title: '操作备注',
                  dataIndex: 'operationRemark',
                  span: 2,
                  render: (text: any) => (text ? String(text) : '-')
                },
                {
                  title: '操作前数据',
                  dataIndex: 'beforeData',
                  span: 2,
                  render: (text: any) => {
                    if (!text) return '-';
                    const textStr = typeof text === 'string' ? text : String(text);
                    return (
                      <pre
                        style={{
                          margin: 0,
                          padding: 12,
                          background: '#fafafa',
                          borderRadius: 4,
                          whiteSpace: 'pre-wrap',
                          wordBreak: 'break-all',
                          maxHeight: 300,
                          overflow: 'auto',
                          border: '1px solid #f0f0f0',
                          color: '#000000',
                        }}
                      >
                        {formatJson(textStr)}
                      </pre>
                    );
                  }
                },
                {
                  title: '操作后数据',
                  dataIndex: 'afterData',
                  span: 2,
                  render: (text: any) => {
                    if (!text) return '-';
                    const textStr = typeof text === 'string' ? text : String(text);
                    return (
                      <pre
                        style={{
                          margin: 0,
                          padding: 12,
                          background: '#fafafa',
                          borderRadius: 4,
                          whiteSpace: 'pre-wrap',
                          wordBreak: 'break-all',
                          maxHeight: 300,
                          overflow: 'auto',
                          border: '1px solid #f0f0f0',
                          color: '#000000',
                        }}
                      >
                        {formatJson(textStr)}
                      </pre>
                    );
                  }
                },
              ]}
            />
          </div>
        )}
      </Drawer>
    </PageContainer>
  );
};

export default OperationLogList;

