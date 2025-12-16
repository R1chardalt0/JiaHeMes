import type { ActionType, ProColumns, ProFormInstance } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions, FooterToolbar } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { Drawer, Table, Button, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { getEqumentTraceinfoList } from '@/services/Api/Trace/deviceOperation';
import { EqumentTraceinfoDto, EqumentTraceinfoQueryDto } from '@/services/Model/Trace/deviceOperation';
import type { RequestData } from '@ant-design/pro-components';

const stripSeparators = (value: string): string =>
  value.replace(/[\s_\-:：|]/g, '').toLowerCase();

const stripDevicePrefix = (paramName?: string, deviceCode?: string): string => {
  if (!paramName) return '';
  if (!deviceCode) return paramName;

  const normalizedCode = stripSeparators(deviceCode);
  if (!normalizedCode) return paramName;

  const separatorPattern = /[\s_\-:：|]/;
  let normalizedCaptured = '';
  let cursor = 0;

  while (cursor < paramName.length && normalizedCaptured.length < normalizedCode.length) {
    const char = paramName[cursor];

    if (separatorPattern.test(char)) {
      cursor += 1;
      continue;
    }

    normalizedCaptured += char.toLowerCase();

    if (!normalizedCode.startsWith(normalizedCaptured)) {
      return paramName;
    }

    cursor += 1;
  }

  if (normalizedCaptured !== normalizedCode) {
    return paramName;
  }

  while (cursor < paramName.length && separatorPattern.test(paramName[cursor])) {
    cursor += 1;
  }

  return paramName.slice(cursor).trimStart();
};

const getTodayRange = () => {
  const start = dayjs().startOf('day');
  const end = dayjs().endOf('day');
  return { start, end };
};

const formatDateTimeParam = (value?: dayjs.ConfigType): string | undefined => {
  if (!value) return undefined;
  return dayjs(value).format('YYYY-MM-DD HH:mm:ss');
};

const TableList: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const formRef = useRef<ProFormInstance | undefined>(undefined);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<EqumentTraceinfoDto>();
  const [selectedRowsState, setSelectedRows] = useState<EqumentTraceinfoDto[]>([]);
  const [messageApi] = message.useMessage();
  const [currentSearchParams, setCurrentSearchParams] = useState<EqumentTraceinfoQueryDto>({
    current: 1,
    pageSize: 50
  });
  const hasAppliedDefaultTimeRef = useRef(false);

  // 表格列定义
  const columns: ProColumns<EqumentTraceinfoDto>[] = [
    {
      title: '设备编码',
      dataIndex: 'deviceEnCode',
      key: 'deviceEnCode',
      width: 120,
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      )
    },
    {
      title: '设备名称',
      dataIndex: 'deviceName',
      key: 'deviceName',
      width: 120,
    },
    {
      title: '产线',
      dataIndex: 'productionLine',
      key: 'productionLine',
      width: 100,
    },
    {
      title: '报警信息',
      dataIndex: 'alarMessages',
      key: 'alarMessages',
      width: 180,
      search: false,
    },
    {
      title: '设备发送时间',
      dataIndex: 'sendTime',
      key: 'sendTime',
      valueType: 'dateTime',
      width: 180,
      search: false,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      valueType: 'dateTime',
      width: 180,
      search: false,
    },
    {
      title: '开始时间',
      dataIndex: 'startTime',
      key: 'startTime',
      valueType: 'dateTime',
      hideInTable: true,
    },
    {
      title: '结束时间',
      dataIndex: 'endTime',
      key: 'endTime',
      valueType: 'dateTime',
      hideInTable: true,
    },
  ];

  // 将参数类型数字转换为可读名称
  const getParamTypeName = (type: number): string => {
    switch (type) {
      case 0: return '数值型';
      case 1: return '文本型';
      case 2: return '公式型';
      case 3: return '布尔型';
      default: return '未知类型';
    }
  };

  // 处理导出选中数据功能（包含参数）
  const handleExport = useCallback(() => {
    if (!selectedRowsState.length) {
      messageApi.warning('请先选择要导出的数据');
      return;
    }

    // 准备导出数据
    const exportData: string[] = [];
    
    // 分析数据中最大的参数数量
    const maxParamCount = selectedRowsState.reduce((max, row) => {
      const paramCount = row.parameters ? (Array.isArray(row.parameters) ? row.parameters.length : 1) : 0;
      return Math.max(max, paramCount);
    }, 0);

    // 动态生成表头，根据最大参数数量创建多组参数列
    const header = ['设备编码', '设备名称', '产线', '报警信息', '设备发送时间', '创建时间'];
    for (let i = 0; i < maxParamCount; i++) {
      if (i === 0) {
        // 第一组参数列不添加序号
        header.push('参数名称', '参数类型', '参数值', '单位');
      } else {
        // 后续组参数列添加序号区分
        header.push(`参数名称${i+1}`, `参数类型${i+1}`, `参数值${i+1}`, `单位${i+1}`);
      }
    }
    exportData.push(header.join(','));

    // 处理每一行数据
    selectedRowsState.forEach(row => {
      // 基础信息
      const baseInfo = [
        row.deviceEnCode || '',
        row.deviceName || '',
        row.productionLine || '',
        row.alarMessages || '',
        row.sendTime || '',
        row.createTime || ''
      ].map(val => `"${String(val).replace(/"/g, '""')}"`).join(',');

      // 参数信息数组
      const params = Array.isArray(row.parameters) ? row.parameters : (row.parameters ? [row.parameters] : []);
      
      // 处理参数信息，将多个参数横向展开为多组参数列
          const paramColumns: string[] = [];
      for (let i = 0; i < maxParamCount; i++) {
        if (params[i]) {
          const param = params[i];
              const displayName = stripDevicePrefix(param.name, row.deviceEnCode);
              paramColumns.push(`"${(displayName || '').replace(/"/g, '""')}"`);
          paramColumns.push(`"${getParamTypeName(param.type).replace(/"/g, '""')}"`);
          paramColumns.push(`"${(param.value || '').replace(/"/g, '""')}"`);
          paramColumns.push(`"${(param.unit || '').replace(/"/g, '""')}"`);
        } else {
          // 参数数量不足时，用空值填充
          paramColumns.push('""', '""', '""', '""');
        }
      }

      // 一条数据导出为一行
      exportData.push(`${baseInfo},${paramColumns.join(',')}`);
    });

    // 实现CSV导出功能
    const csvContent = '\uFEFF' + exportData.join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', `设备追溯信息_选中数据_${new Date().toISOString().slice(0, 10)}.csv`);
    link.style.visibility = 'hidden';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    messageApi.success(`成功导出 ${selectedRowsState.length} 条数据`);
  }, [selectedRowsState, messageApi]);

  // 处理导出全部搜索结果（包含参数）
  const handleExportAll = useCallback(async () => {
    messageApi.loading('正在获取全部数据，请稍候...', 0);

    try {
      // 获取总条数
      const totalResponse = await getEqumentTraceinfoList({
        ...currentSearchParams,
        pageSize: 1
      });

      const total = totalResponse.total || 0;

      if (total === 0) {
        messageApi.destroy();
        messageApi.warning('当前搜索条件下没有数据');
        return;
      }

      messageApi.destroy();
      messageApi.loading(`正在获取全部数据(${total}条)，请稍候...`, 0);

      // 一次性获取所有数据
      const allDataResponse = await getEqumentTraceinfoList({
        ...currentSearchParams,
        pageSize: total,
        current: 1
      });

      const allData = allDataResponse.data || [];

      // 准备导出数据
      const exportData: string[] = [];
      
      // 分析数据中最大的参数数量
      const maxParamCount = allData.reduce((max, row) => {
        const paramCount = row.parameters ? (Array.isArray(row.parameters) ? row.parameters.length : 1) : 0;
        return Math.max(max, paramCount);
      }, 0);

      // 动态生成表头，根据最大参数数量创建多组参数列
      const header = ['设备编码', '设备名称', '产线', '报警信息', '设备发送时间', '创建时间'];
      for (let i = 0; i < maxParamCount; i++) {
        if (i === 0) {
          // 第一组参数列不添加序号
          header.push('参数名称', '参数类型', '参数值', '单位');
        } else {
          // 后续组参数列添加序号区分
          header.push(`参数名称${i+1}`, `参数类型${i+1}`, `参数值${i+1}`, `单位${i+1}`);
        }
      }
      exportData.push(header.join(','));

      // 处理每一行数据
      allData.forEach(row => {
        // 基础信息
        const baseInfo = [
          row.deviceEnCode || '',
          row.deviceName || '',
          row.productionLine || '',
          row.alarMessages || '',
          row.sendTime || '',
          row.createTime || ''
        ].map(val => `"${String(val).replace(/"/g, '""')}"`).join(',');

        // 参数信息数组
        const params = Array.isArray(row.parameters) ? row.parameters : (row.parameters ? [row.parameters] : []);
        
        // 处理参数信息，将多个参数横向展开为多组参数列
        const paramColumns: string[] = [];
        for (let i = 0; i < maxParamCount; i++) {
          if (params[i]) {
            const param = params[i];
            const displayName = stripDevicePrefix(param.name, row.deviceEnCode);
            paramColumns.push(`"${(displayName || '').replace(/"/g, '""')}"`);
            paramColumns.push(`"${getParamTypeName(param.type).replace(/"/g, '""')}"`);
            paramColumns.push(`"${(param.value || '').replace(/"/g, '""')}"`);
            paramColumns.push(`"${(param.unit || '').replace(/"/g, '""')}"`);
          } else {
            // 参数数量不足时，用空值填充
            paramColumns.push('""', '""', '""', '""');
          }
        }

        // 一条数据导出为一行
        exportData.push(`${baseInfo},${paramColumns.join(',')}`);
      });

      // 实现CSV导出功能
      const csvContent = '\uFEFF' + exportData.join('\n');

      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);

      link.setAttribute('href', url);
      link.setAttribute('download', `设备追溯信息_全部数据_${new Date().toISOString().slice(0, 10)}.csv`);
      link.style.visibility = 'hidden';

      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      messageApi.destroy();
      messageApi.success(`成功导出 ${allData.length} 条数据`);
    } catch (error) {
      messageApi.destroy();
      messageApi.error('导出数据失败，请重试');
      console.error('导出数据失败:', error);
    }
  }, [currentSearchParams, messageApi]);

  return (
    <PageContainer>
      <ProTable<EqumentTraceinfoDto>
        headerTitle="设备追溯信息列表"
        actionRef={actionRef}
        formRef={formRef}
        rowKey={(record, index = 0) => index.toString()}
        className="device-trace-glass-table"
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
        ): Promise<RequestData<EqumentTraceinfoDto>> => {
          let startTime = params.startTime;
          let endTime = params.endTime;

          if (!hasAppliedDefaultTimeRef.current && !startTime && !endTime) {
            const todayRange = getTodayRange();
            startTime = todayRange.start;
            endTime = todayRange.end;
            formRef.current?.setFieldsValue({
              startTime: todayRange.start,
              endTime: todayRange.end
            });
          }
          hasAppliedDefaultTimeRef.current = true;

          const normalizedStartTime = formatDateTimeParam(startTime);
          const normalizedEndTime = formatDateTimeParam(endTime);

          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            productionLine: params.productionLine,
            deviceName: params.deviceName,
            deviceEnCode: params.deviceEnCode,
            startTime: normalizedStartTime,
            endTime: normalizedEndTime,
          });

          const queryParams: EqumentTraceinfoQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            productionLine: params.productionLine,
            deviceName: params.deviceName,
            deviceEnCode: params.deviceEnCode,
            startTime: normalizedStartTime,
            endTime: normalizedEndTime,
          };

          try {
            const response = await getEqumentTraceinfoList(queryParams);

            return {
              data: response.data || [],
              total: response.total || 0,
              success: response.success ?? true,
            };
          } catch (error) {
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        columns={columns}
        // 替换现有的 pagination 配置
        pagination={{
        pageSize: currentSearchParams.pageSize,
        pageSizeOptions: ['10', '20', '50', '100'],
        showSizeChanger: true,
        showTotal: (total) => `共 ${total} 条数据`,
        }}
        toolBarRender={false}
        onRow={(record) => ({
          onClick: () => {
            setCurrentRow(record);
            setShowDetail(true);
          },
        })}
        rowSelection={{
          onChange: (_, selectedRows) => {
            setSelectedRows(selectedRows);
          },
        }}
      />

      {selectedRowsState?.length > 0 && (
        <FooterToolbar
          extra={
            <div>
              已选择 <a style={{ fontWeight: 600 }}>{selectedRowsState.length}</a> 项数据
            </div>
          }
        >
          <Button type="primary" onClick={handleExport}>
            导出选中数据
          </Button>
        </FooterToolbar>
      )}

      <FooterToolbar>
        <Button onClick={handleExportAll}>
          导出全部搜索结果
        </Button>
      </FooterToolbar>

      <Drawer
        width={600}
        placement="right"
        open={showDetail}
        onClose={() => setShowDetail(false)}
        closable={true}
        title="设备详情"
        className="device-trace-info-drawer"
        rootClassName="device-trace-info-drawer"
        styles={{
          content: {
            background:
              'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
            backdropFilter: 'blur(14px) saturate(115%)',
            WebkitBackdropFilter: 'blur(14px) saturate(115%)',
            borderLeft: '1px solid rgba(72,115,255,0.32)',
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
        {currentRow && (
          <ProDescriptions<EqumentTraceinfoDto>
            column={2}
            title={`设备：${currentRow.deviceName} (${currentRow.deviceEnCode})`}
            dataSource={currentRow}
            columns={[
              {
                title: '设备编码',
                dataIndex: 'deviceEnCode',
              },
              {
                title: '设备名称',
                dataIndex: 'deviceName',
              },
              {
                title: '产线',
                dataIndex: 'productionLine',
              },
              {
                title: '设备发送时间',
                dataIndex: 'sendTime',
                valueType: 'dateTime',
              },
              {
                title: '报警信息',
                dataIndex: 'alarMessages',
              },
              {
                title: '创建时间',
                dataIndex: 'createTime',
                valueType: 'dateTime',
              },
              {
                title: '参数信息',
                dataIndex: 'parameters',
                span: 2,
                render: (parameters) => {
                  if (!parameters) {
                    return '无参数信息';
                  }

                  try {
                    const paramsObj = typeof parameters === 'string'
                      ? JSON.parse(parameters)
                      : parameters;

                    if (!paramsObj || typeof paramsObj !== 'object') {
                      return (
                        <div className="text-gray-500">
                          无法解析参数信息
                        </div>
                      );
                    }

                    let paramsArray: any[] = [];

                    if (Array.isArray(paramsObj)) {
                      paramsArray = paramsObj.map(param => ({
                        ...param,
                        name: stripDevicePrefix(param.name, currentRow.deviceEnCode),
                        dataType: getParamTypeName(param.type)
                      }));
                    } else {
                      paramsArray = Object.entries(paramsObj).map(([key, value]) => ({
                        name: stripDevicePrefix(key, currentRow.deviceEnCode),
                        value: value,
                        dataType: getParamTypeName(1),
                        unit: ''
                      }));
                    }

                    const paramColumns: ColumnsType<any> = [
                      {
                        title: '参数名称',
                        dataIndex: 'name',
                        key: 'name',
                        ellipsis: true,
                        width: 150
                      },
                      {
                        title: '数据类型',
                        dataIndex: 'dataType',
                        key: 'dataType',
                        render: (type: string) => {
                          switch (type?.toUpperCase()) {
                            case 'NUMBER': return '数值型';
                            case 'TEXT': return '文本型';
                            case 'FORMULA': return '公式型';
                            case 'BOOLEAN': return '布尔型';
                            default: return type || '未知类型';
                          }
                        },
                        width: 100
                      },
                      {
                        title: '参数值',
                        dataIndex: 'value',
                        key: 'value',
                        ellipsis: true,
                        render: (value: any, record: any) => {
                          if (record.dataType?.toUpperCase() === 'BOOLEAN') {
                            return value ? 'true' : 'false';
                          } else if (typeof value === 'object') {
                            return JSON.stringify(value);
                          }
                          return String(value);
                        }
                      },
                      {
                        title: '单位',
                        dataIndex: 'unit',
                        key: 'unit',
                        width: 80
                      }
                    ];

                    return (
                      <div className="mt-2">
                        <Table
                          columns={paramColumns}
                          dataSource={paramsArray}
                          pagination={false}
                          rowKey={(record, index = 0) => index.toString()}
                          size="small"
                          scroll={{ x: 'max-content' }}
                        />
                      </div>
                    );
                  } catch (e) {
                    return (
                      <div className="text-red-500 bg-red-50 p-4 rounded-md">
                        解析参数信息失败: {String(e)}
                      </div>
                    );
                  }
                },
              }
            ]}
          />
        )}
      </Drawer>
    </PageContainer>
  );
};

export default TableList;

