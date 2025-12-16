import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions, FooterToolbar } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback } from 'react';
import { Drawer, Table, Button, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { getHistoryProductTraceInfoList } from '@/services/Api/Trace/HistoryProductTraceInfo';
import { ProductTraceInfoDto, ProductTraceInfoQueryDto } from '@/services/Model/Trace/ProductTraceInfo';
import type { RequestData } from '@ant-design/pro-components';

const TableList: React.FC = () => {
    const actionRef = useRef<ActionType | null>(null);
    const [showDetail, setShowDetail] = useState(false);
    const [currentRow, setCurrentRow] = useState<ProductTraceInfoDto>();
    const [selectedRowsState, setSelectedRowsState] = useState<ProductTraceInfoDto[]>([]);
    const [messageApi] = message.useMessage();
    const [currentSearchParams, setCurrentSearchParams] = useState<ProductTraceInfoQueryDto>({
        current: 1,
        pageSize: 50
    });

    // 表格列定义
    const columns: ProColumns<ProductTraceInfoDto>[] = [
        {
            title: '产品编码',
            dataIndex: 'sfc',
            key: 'sfc',
            width: 150,
            render: (dom, entity) => (
                <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
            )
        },
        {
            title: '产线',
            dataIndex: 'productionLine',
            key: 'productionLine',
            width: 120,
        },
        {
            title: '设备名称',
            dataIndex: 'deviceName',
            key: 'deviceName',
            width: 120,
        },
        {
            title: '设备编码',
            dataIndex: 'resource',
            key: 'resource',
            width: 120,
        },
        {
            title: '站点',
            dataIndex: 'site',
            key: 'site',
            width: 100,
            search: false,
        },
        {
            title: '活动ID',
            dataIndex: 'activityId',
            key: 'activityId',
            width: 100,
            hideInTable: true,
            search: false,
        },
        {
            title: '批次号',
            dataIndex: 'dcGroupRevision',
            key: 'dcGroupRevision',
            width: 120,
            hideInTable: true,
            search: false,
        },
        {
            title: '是否OK',
            dataIndex: 'isOK',
            key: 'isOK',
            width: 80,
            search: false,
            render: (status) => (
                <span style={{
                    color: status ? '#52c41a' : '#ff4d4f',
                    fontWeight: 500
                }}>
                    {status ? 'OK' : 'NG'}
                </span>
            )
        },
        {
            title: '发送时间',
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

        // 找出最大参数数量，用于动态生成表头
        const maxParamCount = Math.max(...selectedRowsState.map(row => 
            row.parametricDataArray && row.parametricDataArray.length ? row.parametricDataArray.length : 0
        ), 1); // 至少保留一组参数列

        // 动态生成表头
        const baseHeader = ['产品编码', '产线', '设备名称', '设备编码', '站点', '活动ID', '批次号', '状态', '发送时间', '创建时间'];
        const paramHeaders: string[] = [];
        
        // 根据最大参数数量生成多组参数列标题
        for (let i = 0; i < maxParamCount; i++) {
            paramHeaders.push(`参数名称${i > 0 ? i + 1 : ''}`, `参数类型${i > 0 ? i + 1 : ''}`, `参数值${i > 0 ? i + 1 : ''}`, `单位${i > 0 ? i + 1 : ''}`);
        }
        
        const header = [...baseHeader, ...paramHeaders];
        exportData.push(header.join(','));

        // 处理每一行数据
        selectedRowsState.forEach(row => {
            // 基础信息
            const baseInfo = [
                row.sfc || '',
                row.productionLine || '',
                row.deviceName || '',
                row.resource || '',
                row.site || '',
                row.activityId || '',
                row.dcGroupRevision || '',
                row.isOK ? '正常' : '异常',
                row.sendTime || '',
                row.createTime || ''
            ].map(val => `"${String(val).replace(/"/g, '""')}"`).join(',');

            // 处理参数信息 - 将多个参数合并到同一行的多列中
            let paramInfoArray: string[] = [];
            
            if (row.parametricDataArray && row.parametricDataArray.length > 0) {
                // 为每个参数创建一组列数据
                row.parametricDataArray.forEach(param => {
                    const paramInfo = [
                        param.name || '',
                        getParamTypeName(param.type),
                        param.value || '',
                        param.unit || ''
                    ].map(val => `"${String(val).replace(/"/g, '""')}"`);
                    
                    paramInfoArray = [...paramInfoArray, ...paramInfo];
                });
            }
            
            // 如果参数数量不足最大数量，补充空值
            const remainingSlots = maxParamCount * 4 - paramInfoArray.length;
            if (remainingSlots > 0) {
                paramInfoArray = [...paramInfoArray, ...Array(remainingSlots).fill('""')];
            }

            // 一条数据只导出一行，包含所有参数信息
            exportData.push(`${baseInfo},${paramInfoArray.join(',')}`);
        });

        // 实现CSV导出功能
        const csvContent = '\uFEFF' + exportData.join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);

        link.setAttribute('href', url);
        link.setAttribute('download', `历史产品追溯信息_选中数据_${new Date().toISOString().slice(0, 10)}.csv`);
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
            const totalResponse = await getHistoryProductTraceInfoList({
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
            const allDataResponse = await getHistoryProductTraceInfoList({
                ...currentSearchParams,
                pageSize: total,
                current: 1
            });

            const allData = allDataResponse.data || [];

            // 准备导出数据
            const exportData: string[] = [];

            // 找出最大参数数量，用于动态生成表头
            const maxParamCount = Math.max(...allData.map(row => 
                row.parametricDataArray && row.parametricDataArray.length ? row.parametricDataArray.length : 0
            ), 1); // 至少保留一组参数列

            // 动态生成表头
            const baseHeader = ['产品编码', '产线', '设备名称', '设备编码', '站点', '活动ID', '批次号', '状态', '发送时间', '创建时间'];
            const paramHeaders: string[] = [];
            
            // 根据最大参数数量生成多组参数列标题
            for (let i = 0; i < maxParamCount; i++) {
                paramHeaders.push(`参数名称${i > 0 ? i + 1 : ''}`, `参数类型${i > 0 ? i + 1 : ''}`, `参数值${i > 0 ? i + 1 : ''}`, `单位${i > 0 ? i + 1 : ''}`);
            }
            
            const header = [...baseHeader, ...paramHeaders];
            exportData.push(header.join(','));

            // 处理每一行数据
            allData.forEach(row => {
                // 基础信息
                const baseInfo = [
                    row.sfc || '',
                    row.productionLine || '',
                    row.deviceName || '',
                    row.resource || '',
                    row.site || '',
                    row.activityId || '',
                    row.dcGroupRevision || '',
                    row.isOK ? '正常' : '异常',
                    row.sendTime || '',
                    row.createTime || ''
                ].map(val => `"${String(val).replace(/"/g, '""')}"`).join(',');

                // 处理参数信息 - 将多个参数合并到同一行的多列中
                let paramInfoArray: string[] = [];
                
                if (row.parametricDataArray && row.parametricDataArray.length > 0) {
                    // 为每个参数创建一组列数据
                    row.parametricDataArray.forEach(param => {
                        const paramInfo = [
                            param.name || '',
                            getParamTypeName(param.type),
                            param.value || '',
                            param.unit || ''
                        ].map(val => `"${String(val).replace(/"/g, '""')}"`);
                        
                        paramInfoArray = [...paramInfoArray, ...paramInfo];
                    });
                }
                
                // 如果参数数量不足最大数量，补充空值
                const remainingSlots = maxParamCount * 4 - paramInfoArray.length;
                if (remainingSlots > 0) {
                    paramInfoArray = [...paramInfoArray, ...Array(remainingSlots).fill('""')];
                }

                // 一条数据只导出一行，包含所有参数信息
                exportData.push(`${baseInfo},${paramInfoArray.join(',')}`);
            });

            // 实现CSV导出功能
            const csvContent = '\uFEFF' + exportData.join('\n');

            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            const url = URL.createObjectURL(blob);

            link.setAttribute('href', url);
            link.setAttribute('download', `历史产品追溯信息_全部数据_${new Date().toISOString().slice(0, 10)}.csv`);
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
            <ProTable<ProductTraceInfoDto>
                headerTitle="历史产品追溯信息列表"
                actionRef={actionRef}
                rowKey={(record, index = 0) => index.toString()}
                className="product-trace-glass-table"
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
                ): Promise<RequestData<ProductTraceInfoDto>> => {
                    setCurrentSearchParams({
                        current: Math.max(1, params.current || 1),
                        pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
                        sfc: params.sfc,
                        productionLine: params.productionLine,
                        deviceName: params.deviceName,
                        resource: params.resource,
                        startTime: params.startTime,
                        endTime: params.endTime,
                    });

                    const queryParams: ProductTraceInfoQueryDto = {
                        current: Math.max(1, params.current || 1),
                        pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
                        sfc: params.sfc,
                        productionLine: params.productionLine,
                        deviceName: params.deviceName,
                        resource: params.resource,
                        startTime: params.startTime,
                        endTime: params.endTime,
                    };

                    try {
                        const response = await getHistoryProductTraceInfoList(queryParams);

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
                        setSelectedRowsState(selectedRows);
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
                title="产品详情"
                className="product-trace-info-drawer"
                rootClassName="product-trace-info-drawer"
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
                    <ProDescriptions<ProductTraceInfoDto>
                        column={2}
                        title={`产品：${currentRow.sfc}`}
                        dataSource={currentRow}
                        columns={[
                            {
                                title: '产品编码',
                                dataIndex: 'sfc',
                            },
                            {
                                title: '产线',
                                dataIndex: 'productionLine',
                            },
                            {
                                title: '设备名称',
                                dataIndex: 'deviceName',
                            },
                            {
                                title: '设备编码',
                                dataIndex: 'resource',
                            },
                            {
                                title: '站点',
                                dataIndex: 'site',
                            },
                            {
                                title: '活动ID',
                                dataIndex: 'activityId',
                            },
                            {
                                title: '批次号',
                                dataIndex: 'dcGroupRevision',
                            },
                            {
                                title: '是否ok',
                                dataIndex: 'isOK',
                                render: (status) => (
                                    <span style={{
                                        color: status ? '#52c41a' : '#ff4d4f',
                                        fontWeight: 500
                                    }}>
                                        {status ? 'OK' : 'NG'}
                                    </span>
                                )
                            },
                            {
                                title: '发送时间',
                                dataIndex: 'sendTime',
                                valueType: 'dateTime',
                            },
                            {
                                title: '创建时间',
                                dataIndex: 'createTime',
                                valueType: 'dateTime',
                            },
                            {
                                title: '参数信息',
                                dataIndex: 'parametricDataArray',
                                span: 2,
                                render: (parametricDataArray) => {
                                    if (!parametricDataArray) {
                                        return '无参数信息';
                                    }

                                    try {
                                        const paramsObj = typeof parametricDataArray === 'string'
                                            ? JSON.parse(parametricDataArray)
                                            : parametricDataArray;

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
                                                dataType: getParamTypeName(param.type)
                                            }));
                                        } else {
                                            paramsArray = Object.entries(paramsObj).map(([key, value]) => ({
                                                name: key,
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

