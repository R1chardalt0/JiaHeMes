import { useRequest, useParams, useNavigate, useLocation } from '@umijs/max';
import React, { useRef, useState, useEffect } from 'react';
import { Button, message, Modal, Space, Drawer, Tag, Image } from 'antd';
import { ProTable, ProDescriptions, ProColumns, RequestData } from '@ant-design/pro-components';
import type { ActionType } from '@ant-design/pro-components';
import { PlusOutlined, DeleteOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import { CreateEquipmentForm } from './CreateEqumentForm';
import {
  getDeviceInfoList,
  deleteDeviceInfoByIds,
  getDeviceInfoById,
} from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import type {
  DeviceInfo,
  DeviceInfoQueryParams,
} from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';

// 从路径中提取 companyId 的辅助函数
const extractCompanyIdFromPath = (pathname: string): string | undefined => {
  // 匹配路径格式：/productionEquipment/company/{companyId}/productionLine
  // 或：/productionEquipment/company/{companyId}/equipment
  const match = pathname.match(/\/productionEquipment\/company\/([^/]+)\/(productionLine|equipment)/);
  return match ? match[1] : undefined;
};

// 设备状态映射
const statusMap = {
  '0': { text: '禁用', status: 'Default' },
  '1': { text: '启用', status: 'Success' },
};

// 获取图片路径的辅助函数
const getImagePath = (imageName?: string): string | undefined => {
  if (!imageName) return undefined;
  
  // 如果已经是完整的 HTTP/HTTPS URL，直接返回
  if (imageName.startsWith('http://') || imageName.startsWith('https://')) {
    return imageName;
  }
  
  // 如果已经是 /images/ 开头的路径，直接返回
  if (imageName.startsWith('/images/')) {
    return imageName;
  }
  
  // 检查是否是本地文件路径（Windows 路径格式，如 D:\ 或 D:/）
  const isLocalPath = /^[A-Za-z]:[\\/]/.test(imageName) || // Windows 绝对路径 D:\ 或 D:/
                      imageName.startsWith('\\') || // Windows 网络路径 \\server\share
                      imageName.startsWith('file://'); // file:// 协议
  
  if (isLocalPath) {
    // 从本地路径提取文件名
    try {
      const normalizedPath = imageName.replace(/\\/g, '/');
      const pathParts = normalizedPath.split('/');
      const fileName = pathParts[pathParts.length - 1] || '';
      if (fileName && fileName.includes('.')) {
        // 直接使用文件名，浏览器会自动处理中文编码
        return `/images/${fileName}`;
      }
    } catch (e) {
      console.error('路径转换出错:', e);
      return undefined;
    }
  }
  
  // 检查是否包含路径分隔符（相对路径）
  const hasPathSeparator = imageName.includes('/') || imageName.includes('\\');
  
  if (hasPathSeparator && !isLocalPath) {
    // 相对路径，提取文件名
    try {
      const normalizedPath = imageName.replace(/\\/g, '/');
      const pathParts = normalizedPath.split('/');
      const fileName = pathParts[pathParts.length - 1] || '';
      if (fileName && fileName.includes('.')) {
        return `/images/${fileName}`;
      }
    } catch (e) {
      console.error('路径转换出错:', e);
      return undefined;
    }
  }
  
  // 纯文件名（如 "催化炉.png"），使用 /images/ 路径
  if (imageName.includes('.')) {
    // 直接使用文件名，浏览器会自动处理中文编码
    return `/images/${imageName}`;
  }
  
  return undefined;
};

const EquipmentPage: React.FC = () => {
  const location = useLocation();
  const { companyId: paramsCompanyId } = useParams<{ companyId: string }>();
  const navigate = useNavigate();
  
  // 从路径中提取 companyId（作为备用方案）
  const pathCompanyId = extractCompanyIdFromPath(location.pathname);
  
  // 优先使用 useParams 获取的 companyId，如果没有则使用从路径提取的
  const companyId = paramsCompanyId || pathCompanyId;
  const normalizedCompanyId = companyId && !Number.isNaN(Number(companyId)) ? Number(companyId) : companyId;
  
  // 调试日志：检查路由参数
  useEffect(() => {
    console.log('🔍 设备管理 - 路由参数检查:', {
      paramsCompanyId,
      pathCompanyId,
      companyId,
      normalizedCompanyId,
      pathname: location.pathname,
    });
  }, [paramsCompanyId, pathCompanyId, companyId, normalizedCompanyId, location.pathname]);
  
  const [formModalVisible, setFormModalVisible] = useState(false);
  const [currentRow, setCurrentRow] = useState<DeviceInfo | null>(null);
  const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
  const [detailData, setDetailData] = useState<DeviceInfo | null>(null);
  const actionRef = useRef<ActionType>(null); // 使用正确的 ActionType ref 并传入初始值 null
  // 受控分页：确保“每页条数”选择器显示与实际一致
  const [pager, setPager] = useState({ current: 1, pageSize: 50 });

  // 跳转到设备监控页面
  const handleNavigateToMonitor = (device: DeviceInfo) => {
    if (!device.deviceType) {
      message.warning('该设备没有设备类型，无法跳转到监控页面');
      return;
    }
    // 跳转到监控页面，传递设备类型参数
    navigate(`/devicechart/monitor/${encodeURIComponent(device.deviceType)}`);
  };

  // 路由切换时清理状态，避免卡顿
  useEffect(() => {
    // 当 companyId 变化时，清理状态
    setDetailDrawerVisible(false);
    setCurrentRow(null);
    setFormModalVisible(false);
    
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

  // 获取设备列表
  const fetchDeviceInfoList = async (params: DeviceInfoQueryParams) => {
    try {
      // 使用传入的 params.companyId（从 request 函数传入）
      // 如果 params.companyId 为 undefined，则使用 normalizedCompanyId（从路由参数获取）
      const finalCompanyId = params.companyId !== undefined ? params.companyId : normalizedCompanyId;
      
      // 调试日志：检查最终使用的 companyId
      console.log('🔧 设备管理 - fetchDeviceInfoList:', {
        paramsCompanyId: params.companyId,
        normalizedCompanyId,
        finalCompanyId,
      });
      
      const requestParams: DeviceInfoQueryParams = {
        current: params.current,
        pageSize: params.pageSize,
        deviceName: params.deviceName,
        deviceEnCode: params.deviceEnCode,
        productionLineId: params.productionLineId,
        startTime: params.startTime,
        endTime: params.endTime,
        companyId: finalCompanyId, // 使用最终确定的 companyId
      };

      // 调试日志：检查发送给后端的参数
      console.log('📤 设备管理 - 发送给后端的参数:', requestParams);

      const response = await getDeviceInfoList(requestParams);
      
      // 调试日志：检查后端返回的数据
      console.log('📥 设备管理 - 后端返回数据:', {
        dataCount: response.data?.length || 0,
        companyIds: response.data?.map((item: any) => item.companyId),
      });
      
      return {
        data: response.data || [],
        success: true,
        total: (response as any).total || 0,
      };
    } catch (error) {
      console.error('❌ 设备管理 - 获取列表失败:', error);
      message.error('获取设备列表失败');
      return {
        data: [],
        success: false,
        total: 0,
      };
    }
  };

  // 删除设备请求
  const delRun = async (id: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '确定要删除该设备吗？',
      okText: '确定',
      cancelText: '取消',
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
      onOk: async () => {
        try {
          const response = await deleteDeviceInfoByIds([id]);
          if (response.success) {
            message.success('删除成功');
            actionRef.current?.reload(); // 使用 actionRef 重新加载
          } else {
            message.error(response.message || '删除失败');
          }
        } catch (error) {
          message.error('删除失败');
        }
      },
    });
  };

  // 打开详情抽屉
  const showDetailDrawer = async (row: DeviceInfo) => {
    try {
      const response = await getDeviceInfoById(row.deviceId || '');
      if (response.data) {
        // 确保生产线名称被正确设置：优先使用后端返回的值，如果没有则使用表格行中的值
        const detailDataWithProductionLine = {
          ...response.data,
          productionLineName: response.data.productionLineName || row.productionLineName || (response.data as any).productionLine?.productionLineName || '-',
        };
        setDetailData(detailDataWithProductionLine);
        setDetailDrawerVisible(true);
      }
    } catch (error) {
      message.error('获取设备详情失败');
    }
  };

  // 打开编辑表单
  const handleEdit = (row: DeviceInfo) => {
    setCurrentRow(row);
    setFormModalVisible(true);
  };

  // 打开新增表单
  const handleAdd = () => {
    if (!companyId) {
      message.warning('请先通过左侧公司菜单进入再新增设备');
      return;
    }
    setCurrentRow(null);
    setFormModalVisible(true);
  };

  // 关闭表单
  const handleCancel = () => {
    setFormModalVisible(false);
    setCurrentRow(null);
  };

  // 表单提交成功
  const handleSuccess = () => {
    setFormModalVisible(false);
    setCurrentRow(null);
    actionRef.current?.reload(); // 使用 actionRef 重新加载
  };

  // 批量删除
  const handleBatchDelete = async (selectedRows: DeviceInfo[]) => {
    Modal.confirm({
      title: '确认删除',
      content: `确定要删除选中的 ${selectedRows.length} 条设备信息吗？`,
      okText: '确定',
      cancelText: '取消',
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
      onOk: async () => {
        try {
          const ids = selectedRows.map((row) => row.deviceId || '');
          const response = await deleteDeviceInfoByIds(ids);
          if (response.success) {
            message.success('批量删除成功');
            actionRef.current?.reload(); // 使用 actionRef 重新加载
          } else {
            message.error(response.message || '批量删除失败');
          }
        } catch (error) {
          message.error('批量删除失败');
        }
      },
    });
  };

  // 表格列配置
  const columns: ProColumns<DeviceInfo>[] = [
    {
      title: '设备头像',
      dataIndex: 'avatar',
      key: 'avatar',
      width: 100,
      search: false,
      render: (dom: React.ReactNode, record: DeviceInfo) => {
        const imagePath = getImagePath(record.avatar);
        return imagePath ? (
          <Image
            src={imagePath}
            alt={record.deviceName || '设备头像'}
            width={50}
            height={50}
            style={{ objectFit: 'cover', borderRadius: 4, cursor: 'pointer' }}
            fallback="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='50' height='50'%3E%3Crect fill='%23f0f0f0' width='50' height='50'/%3E%3Ctext x='50%25' y='50%25' text-anchor='middle' dy='.3em' fill='%23999'%3E无图片%3C/text%3E%3C/svg%3E"
            onClick={() => handleNavigateToMonitor(record)}
            preview={false}
          />
        ) : (
          <span style={{ color: '#999', cursor: 'pointer' }} onClick={() => handleNavigateToMonitor(record)}>无头像</span>
        );
      },
    },
    {
      title: '设备ID',
      dataIndex: 'deviceId',
      key: 'deviceId',
      ellipsis: true,
      hideInTable: true, // 默认隐藏，可以通过列设置显示
      hideInSearch: true, // 隐藏搜索表单中的设备ID输入框
    },
    {
      title: '设备名称',
      dataIndex: 'deviceName',
      key: 'deviceName',
      ellipsis: true,
      render: (dom: React.ReactNode, record: DeviceInfo) => (
        <a 
          onClick={() => handleNavigateToMonitor(record)}
          style={{ cursor: 'pointer' }}
        >
          {record.deviceName}
        </a>
      ),
    },
    {
      title: '设备编码',
      dataIndex: 'deviceEnCode',
      key: 'deviceEnCode',
      ellipsis: true
    },
    {
      title: '生产线名称',
      dataIndex: 'productionLineName',
      key: 'productionLineName',
      ellipsis: true,
      search: false,
      render: (dom: React.ReactNode, entity: DeviceInfo) => {
        return entity.productionLineName || '-';
      },
    },
    {
      title: '设备照片',
      dataIndex: 'devicePicture',
      key: 'devicePicture',
      width: 100,
      search: false,
      render: (dom: React.ReactNode, record: DeviceInfo) => {
        const imagePath = getImagePath(record.devicePicture);
        return imagePath ? (
          <Image
            src={imagePath}
            alt={record.deviceName || '设备照片'}
            width={50}
            height={50}
            style={{ objectFit: 'cover', borderRadius: 4 }}
            fallback="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='50' height='50'%3E%3Crect fill='%23f0f0f0' width='50' height='50'/%3E%3Ctext x='50%25' y='50%25' text-anchor='middle' dy='.3em' fill='%23999'%3E无图片%3C/text%3E%3C/svg%3E"
          />
        ) : (
          <span style={{ color: '#999' }}>无照片</span>
        );
      },
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      valueType: 'select', 
      search: false,
      valueEnum: statusMap,
      render: (dom, entity) => ( // 修正 render 函数签名
        <Tag color={statusMap[entity.status as keyof typeof statusMap]?.status === 'Success' ? 'green' : 'default'}>
          {statusMap[entity.status as keyof typeof statusMap]?.text}
        </Tag>
      )
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      valueType: 'dateTime',
      search: false,
    },
    {
      title: '开始时间',
      key: 'startTime',
      dataIndex: 'startTime',
      hideInTable: true,
      valueType: 'dateTimeRange',
      search: {
        transform: (value) => {
          if (value && value.length === 2) {
            return {
              startTime: value[0],
              endTime: value[1],
            };
          }
          return {};
        },
      },
    },
    {
      title: '操作',
      key: 'action',
      valueType: 'option',
      render: (dom, entity) => [ // 修正 render 函数签名
        <Button
          key="detail"
          type="link"
          icon={<EyeOutlined />}
          onClick={() => showDetailDrawer(entity)}
        >
          详情
        </Button>,
        <Button
          key="edit"
          type="link"
          icon={<EditOutlined />}
          onClick={() => handleEdit(entity)}
        >
          编辑
        </Button>,
        <Button
          key="delete"
          type="link"
          danger
          icon={<DeleteOutlined />}
          onClick={() => delRun(entity.deviceId || '')}
        >
          删除
        </Button>,
      ],
    },
  ];

  return (
    <div className="system-settings-page" style={{ padding: 24 }}>
      <ProTable<DeviceInfo>
        columns={columns}
        actionRef={actionRef} // 添加 actionRef
        key={normalizedCompanyId || 'default'} // 添加 key，确保路由切换时重新渲染
        scroll={{ x: 'max-content' }} // 添加横向滚动
        cardProps={{
          style: (window as any).__panelStyles?.panelStyle,
          headStyle: (window as any).__panelStyles?.headStyle,
          bodyStyle: (window as any).__panelStyles?.bodyStyle,
          bordered: false,
          ['data-panel-exempt']: 'true'
        } as any}
        request={async (params: DeviceInfoQueryParams) => {
          try {
            // 处理时间范围参数
            const requestParams: DeviceInfoQueryParams = {
              ...params,
              // 处理标准的时间范围参数
              startTime: params.startTime,
              endTime: params.endTime,
              // 使用 normalizedCompanyId（从路由参数获取）
              // 如果为 undefined，则显示所有数据；如果有值，则只显示该公司的数据
              companyId: normalizedCompanyId,
            };
            
            // 调试日志：检查查询参数
            console.log('📊 设备管理 - 查询参数:', {
              requestParams,
              normalizedCompanyId,
              pathname: window.location.pathname,
            });
            
            // 同步受控分页到状态，确保显示正确
            if (params.current && params.current !== pager.current || params.pageSize && params.pageSize !== pager.pageSize) {
              setPager({ current: params.current || 1, pageSize: params.pageSize || 50 });
            }
            const result = await fetchDeviceInfoList(requestParams);
            
            // 调试日志：检查返回结果
            console.log('📋 设备管理 - 返回结果:', {
              dataCount: result.data?.length || 0,
              total: result.total,
              firstItemCompanyId: result.data?.[0]?.companyId,
            });
            
            return result;
          } catch (error) {
            // 捕获错误，避免路由切换时卡顿
            console.error('获取设备列表失败:', error);
            return {
              data: [],
              success: false,
              total: 0,
            } as RequestData<DeviceInfo>;
          }
        }}
        rowKey="deviceId"
        search={{
          labelWidth: 120,
          span: 8,
        }}
        pagination={{
          current: pager.current,
          pageSize: pager.pageSize,
          pageSizeOptions: ['10', '20', '50', '100'],
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total) => `共 ${total} 条数据`,
          onChange: (current, pageSize) => {
            setPager({ current, pageSize });
            // 触发表格刷新以应用新分页
            actionRef.current?.reload();
          },
          onShowSizeChange: (current, pageSize) => {
            setPager({ current, pageSize });
            actionRef.current?.reload();
          },
        }}
        headerTitle="设备管理"
        rowSelection={{}} // 添加 rowSelection 以支持批量操作
        tableAlertRender={({ selectedRowKeys, selectedRows }) => (
          selectedRowKeys.length > 0 && (
            <div style={{ marginBottom: 16 }}>
              <Space>
                <span>已选择 {selectedRowKeys.length} 项</span>
                <Button
                  danger
                  icon={<DeleteOutlined />}
                  onClick={() => handleBatchDelete(selectedRows)}
                >
                  批量删除
                </Button>
              </Space>
            </div>
          )
        )}
        toolBarRender={() => [
          <Button
            key="add"
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleAdd}
          >
            新增设备
          </Button>,
        ]}
      />

      {/* 详情抽屉 */}
      <Drawer
        title="设备详情"
        width={600}
        placement="right"
        onClose={() => setDetailDrawerVisible(false)}
        open={detailDrawerVisible}
        className="device-info-drawer"
        rootClassName="device-info-drawer"
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
        {detailData && (
          <ProDescriptions
            column={2}
            title="设备信息详情"
            dataSource={detailData}
          >
            <ProDescriptions.Item label="设备ID" dataIndex="deviceId" />
            <ProDescriptions.Item label="设备名称" dataIndex="deviceName" />
            <ProDescriptions.Item label="设备编码" dataIndex="deviceEnCode" />
            <ProDescriptions.Item label="所属生产线" dataIndex="productionLineName">
              {detailData.productionLineName || '-'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="状态">
              {detailData.status ? (
                <Tag color={statusMap[detailData.status as keyof typeof statusMap]?.status === 'Success' ? 'green' : 'default'}>
                  {statusMap[detailData.status as keyof typeof statusMap]?.text || '未知'}
                </Tag>
              ) : '-'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="设备头像" span={2}>
              {detailData.avatar ? (
                <Image
                  src={getImagePath(detailData.avatar)}
                  alt={detailData.deviceName || '设备头像'}
                  width={100}
                  height={100}
                  style={{ objectFit: 'cover', borderRadius: 4 }}
                  fallback="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100'%3E%3Crect fill='%23f0f0f0' width='100' height='100'/%3E%3Ctext x='50%25' y='50%25' text-anchor='middle' dy='.3em' fill='%23999'%3E无图片%3C/text%3E%3C/svg%3E"
                />
              ) : (
                <span style={{ color: '#999' }}>无头像</span>
              )}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="设备照片" span={2}>
              {detailData.devicePicture ? (
                <Image
                  src={getImagePath(detailData.devicePicture)}
                  alt={detailData.deviceName || '设备照片'}
                  width={200}
                  height={150}
                  style={{ objectFit: 'cover', borderRadius: 4 }}
                  fallback="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='150'%3E%3Crect fill='%23f0f0f0' width='200' height='150'/%3E%3Ctext x='50%25' y='50%25' text-anchor='middle' dy='.3em' fill='%23999'%3E无图片%3C/text%3E%3C/svg%3E"
                />
              ) : (
                <span style={{ color: '#999' }}>无照片</span>
              )}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="创建时间" dataIndex="createTime" />
            <ProDescriptions.Item label="更新时间" dataIndex="updateTime" />
            <ProDescriptions.Item label="设备描述" dataIndex="description" span={2} />
          </ProDescriptions>
        )}
      </Drawer>

      {/* 新增/编辑表单 */}
      <CreateEquipmentForm
        visible={formModalVisible}
        onCancel={handleCancel}
        onSuccess={handleSuccess}
        currentRow={currentRow}
        companyId={normalizedCompanyId !== undefined ? String(normalizedCompanyId) : ''}
      />
    </div>
  );
};

export default EquipmentPage;