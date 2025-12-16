import { useRequest, useParams, useLocation } from '@umijs/max';
import React, { useRef, useState, useEffect, useMemo, useCallback } from 'react';
import { Button, message, Modal, Space, Drawer } from 'antd';
import { ProTable, ProDescriptions, RequestData } from '@ant-design/pro-components';
import { PlusOutlined } from '@ant-design/icons';
import CreateProductionLineForm from './CreateProductionLineForm';
import { getProductionLineList, createProductionLine, updateProductionLine, deleteProductionLineByIds, getProductionLineById } from '@/services/Api/Trace/ProductionEquipment‌/productionLineInfo';
import { getAllCompanies } from '@/services/Api/Systems/company';
import type { productionLine as ModelProductionLine, ProductionLineQueryParams } from '@/services/Model/Trace/ProductionEquipment‌/productionLineInfo';
import type { CompanyItem } from '@/services/Model/Systems/company';

// 从路径中提取 companyId 的辅助函数
const extractCompanyIdFromPath = (pathname: string): string | undefined => {
  // 匹配路径格式：/productionEquipment/company/{companyId}/productionLine
  // 或：/productionEquipment/company/{companyId}/equipment
  const match = pathname.match(/\/productionEquipment\/company\/([^/]+)\/(productionLine|equipment)/);
  return match ? match[1] : undefined;
};

// 定义产线类型接口，与后端保持一致
interface productionLine extends ModelProductionLine {
  productionLineId: string;
  productionLineName: string;
  productionLineCode: string;
  description?: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  companyName?: string; // 公司名称（用于显示）
}

// 使用从服务层导入的查询参数接口

const ProductionLineManagement: React.FC = () => {
  const location = useLocation();
  const { companyId: paramsCompanyId } = useParams<{ companyId: string }>();
  
  // 从路径中提取 companyId（作为备用方案）
  const pathCompanyId = extractCompanyIdFromPath(location.pathname);
  
  // 优先使用 useParams 获取的 companyId，如果没有则使用从路径提取的
  const companyId = paramsCompanyId || pathCompanyId;
  const normalizedCompanyId = companyId && !Number.isNaN(Number(companyId)) ? Number(companyId) : companyId;
  
  // 调试日志：检查路由参数
  useEffect(() => {
    console.log('🔍 产线管理 - 路由参数检查:', {
      paramsCompanyId,
      pathCompanyId,
      companyId,
      normalizedCompanyId,
      pathname: location.pathname,
    });
  }, [paramsCompanyId, pathCompanyId, companyId, normalizedCompanyId, location.pathname]);
  
  const actionRef = useRef<any>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<productionLine | undefined>();
  const [selectedRows, setSelectedRows] = useState<productionLine[]>([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [companyList, setCompanyList] = useState<CompanyItem[]>([]); // 公司列表
  const [currentSearchParams, setCurrentSearchParams] = useState<ProductionLineQueryParams>({
    current: 1,
    pageSize: 15
  });
  const companyIdValue = normalizedCompanyId ?? currentRow?.companyId ?? companyId;

  // 路由切换时清理状态，避免卡顿
  useEffect(() => {
    // 当 companyId 变化时，清理状态
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
  }, [normalizedCompanyId]); // 当 companyId 变化时清理状态

  // 加载公司列表并建立映射关系（只加载一次，避免重复请求）
  useEffect(() => {
    let isMounted = true; // 防止组件卸载后更新状态
    let abortController: AbortController | null = null;
    
    const fetchCompanies = async () => {
      try {
        // 创建 AbortController 用于取消请求
        abortController = new AbortController();
        
        const res = await getAllCompanies();
        if (isMounted && res.success && res.data) {
          setCompanyList(res.data); // 保存完整的公司列表
        }
      } catch (error: any) {
        // 如果是取消的请求，不显示错误
        if (error?.name === 'AbortError') {
          return;
        }
        // 静默失败，不影响主流程
        if (isMounted) {
          console.error('加载公司列表失败:', error);
        }
      }
    };

    // 只在组件挂载时加载一次
    fetchCompanies();
    
    return () => {
      isMounted = false;
      // 取消未完成的请求
      if (abortController) {
        abortController.abort();
      }
    };
  }, []);

  // 使用 useMemo 优化公司ID到名称的映射，避免每次渲染都重新计算
  const companyMap = useMemo(() => {
    const map = new Map<string | number, string>();
    companyList.forEach((company) => {
      if (company.companyId) {
        map.set(company.companyId, company.companyName || '');
      }
    });
    return map;
  }, [companyList]);

  // 获取产线列表
  const fetchProductionLineList = async (params: ProductionLineQueryParams) => {
    try {
      // 使用传入的 params.companyId（从 request 函数传入）
      // 如果 params.companyId 为 undefined，则使用 normalizedCompanyId（从路由参数获取）
      const finalCompanyId = params.companyId !== undefined ? params.companyId : normalizedCompanyId;
      
      // 调试日志：检查最终使用的 companyId
      console.log('🔧 产线管理 - fetchProductionLineList:', {
        paramsCompanyId: params.companyId,
        normalizedCompanyId,
        finalCompanyId,
      });
      
      // 更新搜索参数状态
      setCurrentSearchParams({
        current: Math.max(1, params.current || 1),
        pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
        productionLineName: params.productionLineName,
        productionLineCode: params.productionLineCode,
        startTime: params.startTime,
        endTime: params.endTime,
        companyId: finalCompanyId,
      });
      
      // 转换查询参数，与后端保持一致
      const requestParams = {
        current: Math.max(1, params.current || 1),
        pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
        productionLineName: params.productionLineName,
        productionLineCode: params.productionLineCode,
        startTime: params.startTime,
        endTime: params.endTime,
        companyId: finalCompanyId, // 使用最终确定的 companyId
      };

      // 调试日志：检查发送给后端的参数
      console.log('📤 产线管理 - 发送给后端的参数:', requestParams);

      const res = await getProductionLineList(requestParams);
      
      // 调试日志：检查后端返回的数据
      console.log('📥 产线管理 - 后端返回数据:', {
        dataCount: res.data?.length || 0,
        companyIds: res.data?.map((item: any) => item.companyId),
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
      const submitData = {
        ...payload,
        productionLineId: productionLineId || payload.productionLineId,
      };

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
        const errorMsg = error.message || (error as any)?.response?.data?.message || '操作失败';
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
    // 表单中已经包含了companyId，直接使用表单提交的值
    // 如果表单中没有companyId，则使用URL参数中的companyId作为后备
    const finalCompanyId = values.companyId || normalizedCompanyId;
    
    if (!finalCompanyId) {
      message.warning('请选择公司后再新增或编辑产线');
      return false;
    }
    
    // 判断是编辑还是新增
    const isEdit = !!currentRow?.productionLineId;
    const productionLineId = currentRow?.productionLineId;
    
    // 直接使用CreateProductionLineForm传递的原始数据格式
    // 明确传递编辑状态和ID，避免闭包问题
    await submitRun(
      { ...values, companyId: finalCompanyId },
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
      title: '公司名称',
      dataIndex: 'companyName',
      key: 'companyName',
      ellipsis: true,
      search: false,
      render: (_: any, record: productionLine) => {
        // 优先使用后端返回的 companyName
        if (record.companyName) {
          return record.companyName;
        }
        // 如果后端没有返回，则从映射中查找
        if (record.companyId && companyMap.has(record.companyId)) {
          return companyMap.get(record.companyId) || record.companyId;
        }
        // 如果都没有，则显示公司ID
        return record.companyId || '-';
      },
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
    <div className="system-settings-page">
      <ProTable<productionLine>
        rowKey="productionLineId"
        actionRef={actionRef}
        key={normalizedCompanyId || 'default'} // 添加 key，确保路由切换时重新渲染
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
              // 使用 normalizedCompanyId（从路由参数获取）
              // 如果为 undefined，则显示所有数据；如果有值，则只显示该公司的数据
              companyId: normalizedCompanyId,
            };
            
            // 调试日志：检查查询参数
            console.log('📊 产线管理 - 查询参数:', {
              queryParams,
              normalizedCompanyId,
              pathname: window.location.pathname,
            });
            
            const result = await fetchProductionLineList(queryParams);
            
            // 调试日志：检查返回结果
            console.log('📋 产线管理 - 返回结果:', {
              dataCount: result.data?.length || 0,
              total: result.total,
              firstItemCompanyId: result.data?.[0]?.companyId,
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
              if (!companyId) {
                message.warning('请先通过左侧公司菜单进入再新增产线');
                return;
              }
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
        companyId={companyIdValue !== undefined ? String(companyIdValue) : ''}
        companies={companyList} // 传递公司列表，避免表单组件重复加载
      />
    </div>
  );
};

export default ProductionLineManagement;