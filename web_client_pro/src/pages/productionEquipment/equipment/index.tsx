import { useRequest, useNavigate } from '@umijs/max';
import React, { useRef, useState, useEffect } from 'react';
import { Button, message, Modal, Space, Drawer, Tag, Image, Form, Select } from 'antd';
import { ProTable, ProDescriptions, ProColumns, RequestData, PageContainer } from '@ant-design/pro-components';
import type { ActionType } from '@ant-design/pro-components';
import { PlusOutlined, DeleteOutlined, EditOutlined, EyeOutlined } from '@ant-design/icons';
import { CreateEquipmentForm } from './CreateEqumentForm';
import {
  getDeviceInfoList,
  deleteDeviceInfoByIds,
  getDeviceInfoById,
  updateDeviceInfo
} from '@/services/Api/Trace/ProductionEquipment‌/equipmentInfo';
import { getOrderList } from '@/services/Api/Infrastructure/OrderList';
import type {
  DeviceInfo,
  DeviceInfoQueryParams,
} from '@/services/Model/Trace/ProductionEquipment‌/equipmentInfo';
import type { OrderList } from '@/services/Model/Infrastructure/OrderList';

// 设备状态映射
const statusMap = {
  '0': { text: '禁用', status: 'Default' },
  '1': { text: '启用', status: 'Success' },
};

// // 获取图片路径的辅助函数
// const getImagePath = (imageName?: string): string | undefined => {
//   if (!imageName) return undefined;

//   // 如果已经是完整的 HTTP/HTTPS URL，直接返回
//   if (imageName.startsWith('http://') || imageName.startsWith('https://')) {
//     return imageName;
//   }

//   // 如果已经是 /images/ 开头的路径，直接返回
//   if (imageName.startsWith('/images/')) {
//     return imageName;
//   }

//   // 检查是否是本地文件路径（Windows 路径格式，如 D:\ 或 D:/）
//   const isLocalPath = /^[A-Za-z]:[\\/]/.test(imageName) || // Windows 绝对路径 D:\ 或 D:/
//                       imageName.startsWith('\\') || // Windows 网络路径 \\server\share
//                       imageName.startsWith('file://'); // file:// 协议

//   if (isLocalPath) {
//     // 从本地路径提取文件名
//     try {
//       const normalizedPath = imageName.replace(/\\/g, '/');
//       const pathParts = normalizedPath.split('/');
//       const fileName = pathParts[pathParts.length - 1] || '';
//       if (fileName && fileName.includes('.')) {
//         // 直接使用文件名，浏览器会自动处理中文编码
//         return `/images/${fileName}`;
//       }
//     } catch (e) {
//       console.error('路径转换出错:', e);
//       return undefined;
//     }
//   }

//  // 检查是否包含路径分隔符（相对路径）
//   const hasPathSeparator = imageName.includes('/') || imageName.includes('\\');

//   if (hasPathSeparator && !isLocalPath) {
//     // 相对路径，提取文件名
//     try {
//       const normalizedPath = imageName.replace(/\\/g, '/');
//       const pathParts = normalizedPath.split('/');
//       const fileName = pathParts[pathParts.length - 1] || '';
//       if (fileName && fileName.includes('.')) {
//         return `/images/${fileName}`;
//       }
//     } catch (e) {
//       console.error('路径转换出错:', e);
//       return undefined;
//     }
//   }

//   // 纯文件名（如 "催化炉.png"），使用 /images/ 路径
//   if (imageName.includes('.')) {
//     // 直接使用文件名，浏览器会自动处理中文编码
//     return `/images/${imageName}`;
//   }

//   return undefined;
// };

const EquipmentPage: React.FC = () => {
  const navigate = useNavigate();
  const { Option } = Select;

  const [formModalVisible, setFormModalVisible] = useState(false);
  const [currentRow, setCurrentRow] = useState<DeviceInfo | null>(null);
  const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
  const [detailData, setDetailData] = useState<DeviceInfo | null>(null);
  const actionRef = useRef<ActionType>(null); // 使用正确的 ActionType ref 并传入初始值 null
  // 受控分页：确保“每页条数”选择器显示与实际一致
  const [pager, setPager] = useState({ current: 1, pageSize: 50 });

  // 批量修改工单编码相关状态
  const [batchModalVisible, setBatchModalVisible] = useState(false);
  const [selectedDevices, setSelectedDevices] = useState<DeviceInfo[]>([]);
  const [workOrders, setWorkOrders] = useState<OrderList[]>([]);
  const [workOrderLoading, setWorkOrderLoading] = useState(false);
  const [batchForm] = Form.useForm();
  const [selectedWorkOrderCode, setSelectedWorkOrderCode] = useState<string>('');
  const [canBatchEdit, setCanBatchEdit] = useState(false);

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
    // 路由切换时清理状态
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
  }, []);

  // 获取设备列表
  const fetchDeviceInfoList = async (params: DeviceInfoQueryParams) => {
    try {
      const requestParams: DeviceInfoQueryParams = {
        current: params.current,
        pageSize: params.pageSize,
        deviceName: params.deviceName,
        deviceEnCode: params.deviceEnCode,
        productionLineId: params.productionLineId,
        startTime: params.startTime,
        endTime: params.endTime,
      };

      const response = await getDeviceInfoList(requestParams);

      // 映射字段名：后端可能返回 resourceId/resourceName/resource 等，前端期望 deviceId/deviceName/deviceEnCode
      const mappedData = (response.data || []).map((item: any) => ({
        ...item,
        // 映射设备ID
        deviceId: item.deviceId || item.resourceId || '',
        // 映射设备名称
        deviceName: item.deviceName || item.resourceName || '',
        // 映射设备编码
        deviceEnCode: item.deviceEnCode || item.resource || '',
        // 映射设备类型
        deviceType: item.deviceType || item.resourceType || '',
        // 映射设备制造商
        deviceManufacturer: item.deviceManufacturer || item.resourceManufacturer || '',
        // 映射设备图片
        //devicePicture: item.devicePicture || item.resourcePicture || '',
      }));

      return {
        data: mappedData,
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
          background: '#ffffff',
          border: '1px solid #f0f0f0',
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
      // 获取设备ID，支持多种字段名（deviceId 或 resourceId）
      const deviceId = row.deviceId || (row as any).resourceId || '';

      if (!deviceId) {
        message.error('设备ID不存在，无法获取详情');
        return;
      }

      // 调试日志：检查请求参数
      console.log('📤 获取设备详情 - 设备ID:', deviceId);
      console.log('📤 获取设备详情 - 行数据:', row);

      const response = await getDeviceInfoById(deviceId);

      // 调试日志：检查响应数据
      console.log('📥 获取设备详情 - 响应数据:', response);

      if (response.data) {
        // 将后端返回的字段名映射到前端期望的字段名
        const detailData: DeviceInfo = {
          ...response.data,
          // 映射字段名：后端可能返回 resourceId，前端期望 deviceId
          deviceId: (response.data as any).deviceId || (response.data as any).resourceId || deviceId,
          // 映射设备名称
          deviceName: (response.data as any).deviceName || (response.data as any).resourceName || '',
          // 映射设备编码
          deviceEnCode: (response.data as any).deviceEnCode || (response.data as any).resource || '',
          // 映射设备类型
          deviceType: (response.data as any).deviceType || (response.data as any).resourceType || '',
          // 映射设备制造商
          deviceManufacturer: (response.data as any).deviceManufacturer || (response.data as any).resourceManufacturer || '',
          // 映射设备图片
          //devicePicture: (response.data as any).devicePicture || (response.data as any).resourcePicture || '',
          // 确保生产线名称被正确设置
          productionLineName: (response.data as any).productionLineName || row.productionLineName || (response.data as any).productionLine?.productionLineName || '-',
        };

        setDetailData(detailData);
        setDetailDrawerVisible(true);
      } else {
        message.error('设备详情数据为空');
      }
    } catch (error) {
      console.error('❌ 获取设备详情失败:', error);
      const errorMsg = (error as any)?.response?.data?.msg || (error as any)?.response?.data?.message || (error as any)?.message || '获取设备详情失败';
      message.error(errorMsg);
    }
  };

  // 打开编辑表单
  const handleEdit = (row: DeviceInfo) => {
    setCurrentRow(row);
    setFormModalVisible(true);
  };

  // 打开新增表单
  const handleAdd = () => {
    setCurrentRow(null);
    setFormModalVisible(true);
  };

  // 关闭表单
  const handleCancel = () => {
    setFormModalVisible(false);
    setCurrentRow(null);
  };

  // 检查选中设备的工单编码是否相同
  const checkSelectedDevices = (devices: DeviceInfo[]) => {
    if (devices.length === 0) {
      setCanBatchEdit(false);
      setSelectedWorkOrderCode('');
      return;
    }

    // 获取第一个设备的工单编码
    const firstWorkOrderCode = devices[0].workOrderCode;

    // 检查所有设备的工单编码是否与第一个相同
    const allSame = devices.every(device => device.workOrderCode === firstWorkOrderCode);

    setCanBatchEdit(allSame);
    setSelectedWorkOrderCode(firstWorkOrderCode || '');
  };

  // 批量修改工单编码入口函数
  const handleBatchUpdateWorkOrder = async (devices: DeviceInfo[]) => {
    if (devices.length === 0) {
      message.warning('请先选择要修改的设备');
      return;
    }

    if (!canBatchEdit) {
      message.error('选中的设备工单编码不一致，无法批量修改');
      return;
    }

    // 加载工单列表
    try {
      setWorkOrderLoading(true);
      const res = await getOrderList({ current: 1, pageSize: 1000 });
      if (res.data) {
        setWorkOrders(res.data);
      }
    } catch (error) {
      message.error('获取工单列表失败');
    } finally {
      setWorkOrderLoading(false);
    }

    // 设置选中设备和打开弹窗
    setSelectedDevices(devices);
    setBatchModalVisible(true);
    // 重置表单
    batchForm.resetFields();
  };

  // 表单提交成功
  const handleSuccess = () => {
    setFormModalVisible(false);
    setCurrentRow(null);
    actionRef.current?.reload(); // 使用 actionRef 重新加载
  };

  // 批量修改工单编码弹窗关闭
  const handleBatchModalCancel = () => {
    setBatchModalVisible(false);
    batchForm.resetFields();
    setSelectedDevices([]);
  };

  // 批量修改工单编码提交
  const handleBatchSubmit = async () => {
    try {
      // 验证表单
      await batchForm.validateFields();

      // 获取新的工单编码
      const newWorkOrderCode = batchForm.getFieldValue('workOrderCode');

      if (!newWorkOrderCode) {
        message.error('请选择新的工单编码');
        return;
      }

      // 批量修改每个设备的工单编码
      const updatePromises = selectedDevices.map(async (device) => {
        try {
          await updateDeviceInfo({
            ...device,
            workOrderCode: newWorkOrderCode,
          });
          return true;
        } catch (error) {
          console.error(`修改设备 ${device.deviceId} 失败:`, error);
          return false;
        }
      });

      // 等待所有修改完成
      const results = await Promise.all(updatePromises);

      // 统计成功和失败的数量
      const successCount = results.filter(result => result).length;
      const failCount = results.filter(result => !result).length;

      // 显示结果
      if (failCount === 0) {
        message.success(`成功修改 ${successCount} 台设备的工单编码`);
      } else {
        message.warning(`成功修改 ${successCount} 台设备，失败 ${failCount} 台`);
      }

      // 关闭弹窗并重新加载表格数据
      handleBatchModalCancel();
      actionRef.current?.reload();
    } catch (error) {
      console.error('批量修改工单编码失败:', error);
      message.error('批量修改工单编码失败');
    }
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
          background: '#ffffff',
          border: '1px solid #f0f0f0',
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
    // {
    //   title: '设备头像',
    //   dataIndex: 'avatar',
    //   key: 'avatar',
    //   width: 100,
    //   search: false,
    //   render: (dom: React.ReactNode, record: DeviceInfo) => {
    //     const imagePath = getImagePath(record.avatar);
    //     return imagePath ? (
    //       <Image
    //         src={imagePath}
    //         alt={record.deviceName || '设备头像'}
    //         width={50}
    //         height={50}
    //         style={{ objectFit: 'cover', borderRadius: 4, cursor: 'pointer' }}
    //         fallback="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='50' height='50'%3E%3Crect fill='%23f0f0f0' width='50' height='50'/%3E%3Ctext x='50%25' y='50%25' text-anchor='middle' dy='.3em' fill='%23999'%3E无图片%3C/text%3E%3C/svg%3E"
    //         onClick={() => handleNavigateToMonitor(record)}
    //         preview={false}
    //       />
    //     ) : (
    //       <span style={{ color: '#999', cursor: 'pointer' }} onClick={() => handleNavigateToMonitor(record)}>无头像</span>
    //     );
    //   },
    // },
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
    // {
    //   title: '设备照片',
    //   dataIndex: 'devicePicture',
    //   key: 'devicePicture',
    //   width: 100,
    //   search: false,
    //   render: (dom: React.ReactNode, record: DeviceInfo) => {
    //     const imagePath = getImagePath(record.devicePicture);
    //     return imagePath ? (
    //       <Image
    //         src={imagePath}
    //         alt={record.deviceName || '设备照片'}
    //         width={50}
    //         height={50}
    //         style={{ objectFit: 'cover', borderRadius: 4 }}
    //         fallback="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='50' height='50'%3E%3Crect fill='%23f0f0f0' width='50' height='50'/%3E%3Ctext x='50%25' y='50%25' text-anchor='middle' dy='.3em' fill='%23999'%3E无图片%3C/text%3E%3C/svg%3E"
    //       />
    //     ) : (
    //       <span style={{ color: '#999' }}>无照片</span>
    //     );
    //   },
    // },
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
      title: '工单编码',
      dataIndex: 'workOrderCode',
      key: 'workOrderCode',
      ellipsis: true,
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
    <PageContainer
      breadcrumb={{
        items: [
          {
            path: '/productionEquipment',
            title: '产线设备管理',
          },
          {
            path: '/productionEquipment/equipment',
            title: '设备管理',
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
      <div className="system-settings-page" style={{ padding: 24 }}>
        <ProTable<DeviceInfo>
          columns={columns}
          actionRef={actionRef} // 添加 actionRef
          key={'default'}
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
              };

              // 调试日志：检查查询参数
              console.log('📊 设备管理 - 查询参数:', {
                requestParams,
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
          rowSelection={{ // 添加 rowSelection 以支持批量操作
            onChange: (selectedRowKeys, selectedRows) => {
              setSelectedDevices(selectedRows as DeviceInfo[]);
              checkSelectedDevices(selectedRows as DeviceInfo[]);
            },
          }}
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
                  <Button
                    type="primary"
                    onClick={() => handleBatchUpdateWorkOrder(selectedRows as DeviceInfo[])}
                    disabled={!canBatchEdit}
                  >
                    批量修改工单编码
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
              {/* <ProDescriptions.Item label="设备头像" span={2}>
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
              </ProDescriptions.Item> */}
              <ProDescriptions.Item label="创建时间" dataIndex="createTime" />
              <ProDescriptions.Item label="更新时间" dataIndex="updateTime" />
              <ProDescriptions.Item label="设备描述" dataIndex="description" span={2} />
            </ProDescriptions>
          )}
        </Drawer>

        {/* 表单弹窗 */}
        <CreateEquipmentForm
          visible={formModalVisible}
          onCancel={handleCancel}
          onSuccess={handleSuccess}
          currentRow={currentRow}
        />

        {/* 批量修改工单编码弹窗 */}
        <Modal
          title="批量修改工单编码"
          visible={batchModalVisible}
          onCancel={handleBatchModalCancel}
          footer={[
            <Button key="cancel" onClick={handleBatchModalCancel}>
              取消
            </Button>,
            <Button key="submit" type="primary" onClick={handleBatchSubmit}>
              确认修改
            </Button>,
          ]}
          width={500}
        >
          <Form
            form={batchForm}
            layout="vertical"
            initialValues={{ workOrderCode: selectedWorkOrderCode }}
          >
            <Form.Item
              name="workOrderCode"
              label="新工单编码"
              rules={[{ required: true, message: '请选择工单编码' }]}
            >
              <Select
                placeholder="请选择工单编码"
                showSearch
                filterOption={(input, option) =>
                  (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
                }
                loading={workOrderLoading}
                style={{ width: '100%' }}
              >
                {workOrders.map((workOrder) => (
                  <Option key={workOrder.orderCode} value={workOrder.orderCode}>
                    {workOrder.orderCode}
                  </Option>
                ))}
              </Select>
            </Form.Item>
            <div style={{ marginBottom: 16 }}>
              <p>当前选中 {selectedDevices.length} 台设备</p>
              <p>当前工单编码：{selectedWorkOrderCode || '无'}</p>
            </div>
          </Form>
        </Modal>

        {/* 新增/编辑表单 */}
        <CreateEquipmentForm
          visible={formModalVisible}
          onCancel={handleCancel}
          onSuccess={handleSuccess}
          currentRow={currentRow}
        />
      </div>
    </PageContainer>
  );
};

export default EquipmentPage;