import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions } from '@ant-design/pro-components';
import React, { useRef, useState } from 'react';
import { Drawer, Button, message } from 'antd';
import { getWorkOrderList } from '@/services/Api/Infrastructure/WorkOrder';
import { WorkOrderDto, WorkOrderQueryDto, WorkOrderStatusText, WorkOrderStatusColor } from '@/services/Model/Infrastructure/WorkOrder';
import type { RequestData } from '@ant-design/pro-components';

const WorkOrderManagement: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<WorkOrderDto>();
  const [messageApi] = message.useMessage();
  const [currentSearchParams, setCurrentSearchParams] = useState<WorkOrderQueryDto>({
    current: 1,
    pageSize: 50
  });

  const columns: ProColumns<WorkOrderDto>[] = [
    {
      title: '工单编号',
      dataIndex: 'code',
      key: 'code',
      width: 180,
      render: (dom, entity) => (
        <a onClick={() => { setCurrentRow(entity); setShowDetail(true); }}>{dom}</a>
      )
    },
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 150,
    },
    {
      title: 'BOM配方',
      dataIndex: 'bomRecipeName',
      key: 'bomRecipeName',
      width: 180,
      search: false,
    },
    {
      title: '是否无限生产',
      dataIndex: 'isInfinite',
      key: 'isInfinite',
      width: 120,
      search: false,
      render: (val) => val ? '是' : '否',
    },
    {
      title: '生产数量',
      dataIndex: 'workOrderAmount',
      key: 'workOrderAmount',
      width: 120,
      search: false,
      render: (val, record) => record.isInfinite ? '无限' : (val?.toLocaleString() || 0),
    },
    {
      title: '追踪增量',
      dataIndex: 'perTraceInfo',
      key: 'perTraceInfo',
      width: 120,
      search: false,
      render: (val) => val?.toLocaleString() || 0,
    },
    {
      title: '工单状态',
      dataIndex: 'docStatus',
      key: 'docStatus',
      width: 100,
      valueType: 'select',
      valueEnum: {
        0: { text: '草稿', status: 'Drafting' },
        1: { text: '已提交', status: 'Commited' },
        2: { text: '已拒绝', status: 'Rejected' },
        3: { text: '已通过', status: 'Approved' },
      },
      render: (status) => (
        <span style={{
          color: WorkOrderStatusColor[status as number] || '#000',
          fontWeight: 500
        }}>
          {WorkOrderStatusText[status as number] || '未知'}
        </span>
      )
    },
  ];

  return (
    <PageContainer>
      <ProTable<WorkOrderDto>
        headerTitle="工单管理列表"
        actionRef={actionRef}
        rowKey={(record, index = 0) => index.toString()}
        className="work-order-glass-table"
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
        ): Promise<RequestData<WorkOrderDto>> => {
          console.log('[工单管理] 开始请求数据，原始参数:', params);

          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            code: params.code,
            productCode: params.productCode,
            bomRecipeId: params.bomRecipeId,
            docStatus: params.docStatus,
          });

          const queryParams: WorkOrderQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            code: params.code,
            productCode: params.productCode,
            bomRecipeId: params.bomRecipeId,
            docStatus: params.docStatus,
          };

          console.log('[工单管理] 构建的查询参数:', queryParams);

          try {
            console.log('[工单管理] 调用API前...');
            const response = await getWorkOrderList(queryParams);
            console.log('[工单管理] API响应:', response);
            console.log('[工单管理] 响应数据:', response.data);
            console.log('[工单管理] 响应总数:', response.total);
            console.log('[工单管理] 响应成功状态:', response.success);

            const result = {
              data: response.data || [],
              total: response.total || 0,
              success: response.success ?? true,
            };

            console.log('[工单管理] 返回给表格的数据:', result);
            return result;
          } catch (error) {
            console.error('[工单管理] 请求失败，错误信息:', error);
            console.error('[工单管理] 错误详情:', JSON.stringify(error, null, 2));
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        columns={columns}
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
      />

      <Drawer
        width={600}
        placement="right"
        open={showDetail}
        onClose={() => setShowDetail(false)}
        closable={true}
        title="工单详情"
        className="work-order-info-drawer"
        rootClassName="work-order-info-drawer"
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
          <ProDescriptions<WorkOrderDto>
            column={2}
            title={`工单：${currentRow.code}`}
            dataSource={currentRow}
            columns={[
              {
                title: '工单编号',
                dataIndex: 'code',
              },
              {
                title: '产品编码',
                dataIndex: 'productCode',
              },
              {
                title: 'BOM配方ID',
                dataIndex: 'bomRecipeId',
              },
              {
                title: 'BOM配方名称',
                dataIndex: 'bomRecipeName',
              },
              {
                title: '是否无限生产',
                dataIndex: 'isInfinite',
                render: (val) => val ? '是' : '否',
              },
              {
                title: '生产数量',
                dataIndex: 'workOrderAmount',
                render: (val, record) => record.isInfinite ? '无限' : (val?.toLocaleString() || 0),
              },
              {
                title: '追踪增量',
                dataIndex: 'perTraceInfo',
                render: (val) => val?.toLocaleString() || 0,
              },
              {
                title: '工单状态',
                dataIndex: 'docStatus',
                render: (status) => (
                  <span style={{
                    color: WorkOrderStatusColor[status as number] || '#000',
                    fontWeight: 500
                  }}>
                    {WorkOrderStatusText[status as number] || '未知'}
                  </span>
                )
              },
            ]}
          />
        )}
      </Drawer>
    </PageContainer>
  );
};

export default WorkOrderManagement;
