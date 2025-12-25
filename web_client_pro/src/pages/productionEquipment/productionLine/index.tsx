import { useRequest } from '@umijs/max';
import React, { useRef, useState, useEffect } from 'react';
import { Button, message, Modal, Space, Drawer } from 'antd';
import { ProTable, ProDescriptions, RequestData, PageContainer } from '@ant-design/pro-components';
import { PlusOutlined } from '@ant-design/icons';
import CreateProductionLineForm from './CreateProductionLineForm';
import { getProductionLineList, createProductionLine, updateProductionLine, deleteProductionLineByIds, getProductionLineById } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';
import type { productionLine as ModelProductionLine, ProductionLineQueryParams } from '@/services/Model/Trace/ProductionEquipment‌/productionLineInfo';

// 定义产线类型接口，与后端保持一致
interface productionLine extends ModelProductionLine {
  productionLineId: string;
  productionLineName: string;
  productionLineCode: string;
  description?: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

// 使用从服务层导入的查询参数接口

const ProductionLineManagement: React.FC = () => {
  const actionRef = useRef<any>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<productionLine | undefined>();
  const [selectedRows, setSelectedRows] = useState<productionLine[]>([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [currentSearchParams, setCurrentSearchParams] = useState<ProductionLineQueryParams>({
    current: 1,
    pageSize: 15
  });

  // 路由切换时清理状态，避免卡顿
  useEffect(() => {
    // 路由切换时清理状态
    setShowDetail(false);
    setCurrentRow(undefined);
    setSelectedRows([]);
    setModalVisible(false);
    
    // 延迟重新加载表格数据，避免立即触发导致卡顿
    const timer = setTimeout(() => {
      if (actionRef.current) {
        actionRef.current.reload();
      }
    }, 100);
    
    return () => {
      clearTimeout(timer);
    };
  }, []); // 路由变化不再依赖 companyId


  // 获取产线列表
  const fetchProductionLineList = async (params: ProductionLineQueryParams) => {
    try {
      // 更新搜索参数状态
      setCurrentSearchParams({
        current: Math.max(1, params.current || 1),
        pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
        productionLineName: params.productionLineName,
        productionLineCode: params.productionLineCode,
        startTime: params.startTime,
        endTime: params.endTime,
      });
      
      // 转换查询参数，与后端保持一致
      const requestParams = {
        current: Math.max(1, params.current || 1),
        pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
        productionLineName: params.productionLineName,
        productionLineCode: params.productionLineCode,
        startTime: params.startTime,
        endTime: params.endTime,
      };

      // 调试日志：检查发送给后端的参数
      console.log('📤 产线管理 - 发送给后端的参数:', requestParams);

      const res = await getProductionLineList(requestParams);
      
      // 调试日志：检查后端返回的数据
      console.log('📥 产线管理 - 后端返回数据:', {
        dataCount: res.data?.length || 0,
      });
      
      // 根据后端返回结构调整数据格式
      return {
        data: res.data || [],
        success: res ? true : false,
        total: res?.data?.length || 0,
      };
    } catch (error) {
      console.error('❌ 产线管理 - 获取列表失败:', error);
      message.error('获取产线列表失败');
      return { data: [], success: false, total: 0 };
    }
  };

  // 删除产线请求
  const { run: delRun, loading: deleteLoading } = useRequest(deleteProductionLineByIds, {
    manual: true,
    onSuccess: () => {
      setSelectedRows([]);
      actionRef.current?.reload();
      message.success('删除成功');
    },
    onError: (error) => {
      message.error(error.message || '删除失败');
    },
  });

  // 创建/更新统一请求（优化：将 currentRow 作为参数传递，避免闭包问题）
  const { run: submitRun } = useRequest(
    async (payload: any, isEdit: boolean, productionLineId?: string) => {
      // 转换提交数据格式，与后端保持一致
      // 新增时不应该包含 productionLineId（后端会自动生成）
      const submitData: any = {
        productionLineName: payload.productionLineName,
        productionLineCode: payload.productionLineCode,
        status: payload.status,
        description: payload.description || '',
        createdAt: payload.createdAt,
        updatedAt: payload.updatedAt,
      };

      // 只有编辑模式才添加 productionLineId
      if (isEdit && (productionLineId || payload.productionLineId)) {
        submitData.productionLineId = productionLineId || payload.productionLineId;
      }

      // 调试日志：检查提交的数据
      console.log('📤 提交产线数据:', {
        isEdit,
        submitData,
        url: isEdit ? '/api/ProductionLine/UpdateProductionLine' : '/api/ProductionLine/CreateProductionLine'
      });

      return isEdit ? updateProductionLine(submitData) : createProductionLine(submitData);
    },
    {
      manual: true,
      onSuccess: (_, [payload, isEdit]) => {
        actionRef.current?.reload();
        message.success(isEdit ? '更新成功' : '新增成功');
        setModalVisible(false);
        setCurrentRow(undefined); // 清空当前行，避免状态残留
      },
      onError: (error) => {
        console.error('❌ 提交产线数据失败:', error);
        const errorMsg = error.message || (error as any)?.response?.data?.message || (error as any)?.response?.data?.msg || '操作失败';
        message.error(errorMsg);
      },
    }
  );

  // 获取产线详情
  const fetchDetail = async (id: string) => {
    try {
      const res = await getProductionLineById(id);
      if (res.data && 'productionLineCode' in res.data) {
        setCurrentRow(res.data as productionLine);
      }
      setShowDetail(true);
    } catch (error) {
      message.error('获取产线详情失败');
    }
  };

  // 处理删除
  const handleRemove = async (): Promise<void> => {
    if (!selectedRows.length) {
      message.warning('请选择要删除的产线');
      return;
    }
    await delRun(selectedRows.map((row) => row.productionLineId));
  };

  // 表单提交处理函数（优化：明确传递编辑状态和ID，避免闭包问题）
  const handleSubmit = async (values: any) => {
    // 不再从路由 companyId 维度做限制：是否需要 companyId 由表单本身/后端数据模型决定
    
    // 判断是编辑还是新增
    const isEdit = !!currentRow?.productionLineId;
    const productionLineId = currentRow?.productionLineId;
    
    // 直接使用CreateProductionLineForm传递的原始数据格式
    // 明确传递编辑状态和ID，避免闭包问题
    await submitRun(
      { ...values },
      isEdit,
      productionLineId
    );
    return true;
  };

  // 定义表格列配置
  const columns = [
    {
      title: '产线名称',
      dataIndex: 'productionLineName',
      render: (dom: string, entity: productionLine) => (
        <a onClick={() => fetchDetail(entity.productionLineId)}>{dom}</a>
      ),
    },
    {
      title: '产线编号',
      dataIndex: 'productionLineCode',
      search: true,
    },

    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        0: { text: '禁用', status: 'Error' },
        1: { text: '启用', status: 'Success' },
      },
      search: false,
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      valueType: 'dateTime',
      search: false,
    },
    {
      title: '操作',
      valueType: 'option',
      render: (_: any, record: productionLine) => [
        <Button
          key="edit"
          type="link"
          onClick={() => {
            setCurrentRow(record);
            setModalVisible(true);
          }}
        >
          编辑
        </Button>,
        <Button
          key="delete"
          type="link"
          danger
          onClick={() => {
            Modal.confirm({
              title: '确认删除',
              content: '确定删除该产线？',
              className: 'delete-confirm-modal',
              rootClassName: 'delete-confirm-modal',
              styles: {
                content: {
                  background:
                    'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
                  backdropFilter: 'blur(14px) saturate(115%)',
                  WebkitBackdropFilter: 'blur(14px) saturate(115%)',
                  border: '1px solid rgba(72,115,255,0.28)',
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
              },
              onOk: () => delRun([record.productionLineId]),
            });
          }}
        >
          删除
        </Button>,
      ],
    },
  ];

  return (
    <PageContainer
      breadcrumb={{
        items: [
          {
            path: '/productionEquipment',
            title: '产线设备管理',
          },
          {
            path: '/productionEquipment/productionLine',
            title: '产线管理',
          },
        ],
        itemRender: (route, params, routes, paths) => {
          const isLast = routes.indexOf(route) === routes.length - 1;
          return isLast ? (
            <span style={{ fontWeight: 600 }}>{route.title}</span>
          ) : (
            <span style={{ fontWeight: 600 }}>{route.title}</span>
          );
        },
      }}
    >
      <div className="system-settings-page">
        <ProTable<productionLine>
        rowKey="productionLineId"
        actionRef={actionRef}
        key={'default'}
        // 合并列配置，添加时间区间搜索字段
        columns={[
          ...columns,
          // 添加时间区间搜索字段
          {
            title: '时间区间',
            key: 'timeRange',
            dataIndex: 'createdAt',
            valueType: 'dateTimeRange',
            hideInTable: true,
          }
        ] as any}
        cardProps={{
          style: (window as any).__panelStyles?.panelStyle,
          headStyle: (window as any).__panelStyles?.headStyle,
          bodyStyle: (window as any).__panelStyles?.bodyStyle,
          bordered: false,
          ['data-panel-exempt']: 'true'
        } as any}
        request={async (params: ProductionLineQueryParams, sort: Record<string, any>, filter: Record<string, any>) => {
          try {
            // 处理时间范围参数
            const queryParams: ProductionLineQueryParams = {
              current: Math.max(1, params.current || 1),
              pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
              productionLineName: params.productionLineName,
              productionLineCode: params.productionLineCode, // 添加产线编号搜索参数
              startTime: params.startTime || (params as any).createdAt?.[0],
              endTime: params.endTime || (params as any).createdAt?.[1],
            };
            
            // 调试日志：检查查询参数
            console.log('📊 产线管理 - 查询参数:', {
              queryParams,
              pathname: window.location.pathname,
            });
            
            const result = await fetchProductionLineList(queryParams);
            
            // 调试日志：检查返回结果
            console.log('📋 产线管理 - 返回结果:', {
              dataCount: result.data?.length || 0,
              total: result.total,
            });
            
            // 确保返回类型符合RequestData格式
            return result as RequestData<productionLine>;
          } catch (error) {
            // 捕获错误，避免路由切换时卡顿
            console.error('获取产线列表失败:', error);
            return {
              data: [],
              success: false,
              total: 0,
            } as RequestData<productionLine>;
          }
        }}
        rowSelection={{ onChange: (_, rows) => setSelectedRows(rows) }}
        toolBarRender={() => [
          <Button
            key="add"
            type="primary"
            onClick={() => {
              setCurrentRow(undefined);
              setModalVisible(true);
            }}
          >
            <PlusOutlined /> 新增产线
          </Button>,
        ]}
        // 修复搜索配置：使用 search 属性而不是 options.search
        search={{
          labelWidth: 'auto',
          span: {
            xs: 24,
            sm: 24,
            md: 12,
            lg: 12,
            xl: 8,
            xxl: 6,
          },
        }}
        pagination={{
          pageSize: currentSearchParams.pageSize,
          pageSizeOptions: ['10', '20', '50', '100'],
          showSizeChanger: true,
          showTotal: (total) => `共 ${total} 条数据`,
          onChange: (current, pageSize) => {
            setCurrentSearchParams(prev => ({
              ...prev,
              current,
              pageSize
            }));
            // 确保立即重新加载数据
            setTimeout(() => {
              actionRef.current?.reload();
            }, 0);
          },
          onShowSizeChange: (current, pageSize) => {
            setCurrentSearchParams(prev => ({
              ...prev,
              current: 1,
              pageSize
            }));
            // 确保立即重新加载数据
            setTimeout(() => {
              actionRef.current?.reload();
            }, 0);
          },
        }}
        // 添加 options 配置
        options={{
          density: true,
          fullScreen: true,
          reload: () => actionRef.current?.reload(),
          setting: true,
        }}
        tableAlertOptionRender={false}
        tableAlertRender={false}
      />

      {/* 自定义单一批量操作工具栏（避免ProTable内置Alert双层包裹） */}
      {selectedRows.length > 0 && (
        <div
          style={{
            ...(window as any).__panelStyles?.panelStyle,
            padding: '8px 12px',
            marginTop: 8,
            borderRadius: 10,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 12,
          }}
        >
          <Space size={10} style={{ color: '#E6F7FF', fontWeight: 600 }}>
            <span>已选择 {selectedRows.length} 项</span>
          </Space>
          <Space size={10}>
            <Button
              type="link"
              style={{ color: '#91d5ff' }}
              onClick={() => {
                setSelectedRows([]);
                if (actionRef.current?.clearSelected) actionRef.current.clearSelected();
              }}
            >
              取消选择
            </Button>
            <Button
              danger
              type="primary"
              loading={deleteLoading}
              onClick={handleRemove}
            >
              批量删除
            </Button>
          </Space>
        </div>
      )}

      {/* 详情抽屉 - 从右侧滑出 */}
      <Drawer
        title="产线详情"
        placement="right"
        onClose={() => setShowDetail(false)}
        open={showDetail}
        width={600}
        className="production-line-info-drawer"
        rootClassName="production-line-info-drawer"
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
        footer={[
          <Button key="close" onClick={() => setShowDetail(false)}>
            关闭
          </Button>,
        ]}
      >
        {currentRow && (
          <ProDescriptions<productionLine>
            column={2}
            title=""
            dataSource={currentRow}
            columns={columns as any}
          />
        )}
      </Drawer>

      {/* 新增/编辑表单 */}
      <CreateProductionLineForm
        open={modalVisible}
        onOpenChange={setModalVisible}
        currentRow={currentRow}
        onFinish={handleSubmit}
      />
      </div>
    </PageContainer>
  );
};

export default ProductionLineManagement;