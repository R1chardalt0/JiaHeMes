import type { ActionType, ProColumns } from '@ant-design/pro-components';
import { PageContainer, ProTable, ProDescriptions, ProForm } from '@ant-design/pro-components';
import React, { useRef, useState, useCallback, useEffect } from 'react';
import { Button, Tabs, message, Card, Row, Col, Modal, Form, Input, Select, Switch, Popconfirm, Table } from 'antd';
import { EyeOutlined, EditOutlined, DeleteOutlined, SearchOutlined, PlusOutlined } from '@ant-design/icons';
import { getBomList, getBomById, createBom, updateBom, deleteBom } from '@/services/Api/Infrastructure/Bom/BomList';
import { getBomItemsByBomId, createBomItem, updateBomItem, deleteBomItem, getBomItemById } from '@/services/Api/Infrastructure/Bom/BomItem';
import { getProductListList, getProductListById } from '@/services/Api/Infrastructure/ProductList';
import { getStationListList } from '@/services/Api/Infrastructure/StationList';
import { StationListDto } from '@/services/Model/Infrastructure/StationList';
import { BomListDto, BomListQueryDto, BomListCreateDto, BomListUpdateDto } from '@/services/Model/Infrastructure/Bom/BomList';
import { BomItem, BomItemCreateDto, BomItemUpdateDto } from '@/services/Model/Infrastructure/Bom/BomItem';
import { ProductListDto, ProductListQueryDto } from '@/services/Model/Infrastructure/ProductList';
import BomItemCard from './components/BomItemCard';
import BomItemFormModal from './components/BomItemFormModal';
import BomFormModal from './components/BomFormModal';
import ProductSelectModal from './components/ProductSelectModal';
import { getBomColumns } from './columns';

import type { RequestData } from '@ant-design/pro-components';

const { TabPane } = Tabs;
const { Option } = Select;

const BomPage: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [showDetail, setShowDetail] = useState(false);
  const [currentRow, setCurrentRow] = useState<BomListDto | null>(null);
  const [bomItems, setBomItems] = useState<BomItem[]>([]);
  const [activeTabKey, setActiveTabKey] = useState('items');
  const [messageApi] = message.useMessage();
  const [currentSearchParams, setCurrentSearchParams] = useState<BomListQueryDto>({
    current: 1,
    pageSize: 50
  });
  const [isAddModalVisible, setIsAddModalVisible] = useState(false);
  const [isEditModalVisible, setIsEditModalVisible] = useState(false);
  const [isBomEditModalVisible, setIsBomEditModalVisible] = useState(false);
  const [editingBomItem, setEditingBomItem] = useState<BomItem | null>(null);
  const [editingBom, setEditingBom] = useState<BomListDto | null>(null);
  const [form] = Form.useForm<BomItemCreateDto>();
  const [bomForm] = Form.useForm();
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([]);
  const [isAddBomModalVisible, setIsAddBomModalVisible] = useState(false);
  // 站点列表状态
  const [stations, setStations] = useState<StationListDto[]>([]);
  const [stationLoading, setStationLoading] = useState(false);
  // 产品选择弹窗相关状态
  const [isProductModalVisible, setIsProductModalVisible] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<ProductListDto | null>(null);
  const [selectedProductName, setSelectedProductName] = useState<string>('');

  // 获取站点列表数据
  const fetchStations = useCallback(async () => {
    try {
      setStationLoading(true);
      const response = await getStationListList({ current: 1, pageSize: 1000 });
      setStations(response.data || []);
    } catch (error) {
      message.error('获取站点列表失败');
      console.error('Fetch stations error:', error);
    } finally {
      setStationLoading(false);
    }
  }, []);

  // 组件初始化时获取站点列表
  useEffect(() => {
    fetchStations();
  }, [fetchStations]);

  // 显示BOM详情
  const handleShowDetail = useCallback(async (record: BomListDto) => {
    try {
      setCurrentRow(record);
      setShowDetail(true);

      // 获取BOM子项
      const itemsResponse = await getBomItemsByBomId(record.bomId);

      // 为每个BOM子项获取产品详情
      const updatedBomItems = await Promise.all(
        itemsResponse.map(async (item: BomItem) => {
          if (item.productId) {
            try {
              const product = await getProductListById(item.productId);
              return {
                ...item,
                productName: product.productName || '',
                productCode: product.productCode || ''
              };
            } catch (error) {
              console.error('获取产品详情失败:', error);
              return item;
            }
          }
          return item;
        })
      );

      setBomItems(updatedBomItems);
    } catch (error) {
      messageApi.error('获取详情失败');
      console.error('获取详情失败:', error);
    }
  }, [messageApi]);

  // 双击行显示详情
  const handleRowDoubleClick = useCallback((record: BomListDto) => {
    handleShowDetail(record);
  }, [handleShowDetail]);

  // 删除BOM
  const handleDeleteBom = useCallback((bomId: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '确认要删除这条BOM信息吗？',
      onOk: async () => {
        try {
          await deleteBom(bomId);
          message.success('BOM删除成功');
          // 刷新表格
          if (actionRef.current) {
            actionRef.current.reload();
          }
        } catch (error: any) {
          // 从错误响应中提取具体错误消息
          const errorMessage = error?.response?.data?.message || error?.message || '删除BOM失败';
          //message.error(errorMessage);
          console.error('Delete BOM error:', error);
        }
      }
    });
  }, []);

  // 打开编辑BOM模态框
  const handleEditBom = useCallback((bom: BomListDto) => {
    setEditingBom(bom);
    bomForm.setFieldsValue({
      bomName: bom.bomName,
      bomCode: bom.bomCode,
      status: bom.status,
      remark: bom.remark
    });
    setIsBomEditModalVisible(true);
  }, [bomForm]);

  // BOM列表表格列定义
  const columns = getBomColumns({
    onShowDetail: handleShowDetail,
    onEdit: handleEditBom,
    onDelete: handleDeleteBom
  });

  // 保存BOM（创建或更新）
  const handleSaveBom = useCallback(async () => {
    try {
      const values = await bomForm.validateFields();

      if (editingBom) {
        // 更新BOM
        const updatedBom: BomListUpdateDto = {
          bomId: editingBom.bomId,
          bomName: values.bomName,
          bomCode: values.bomCode,
          status: values.status,
          remark: values.remark
        };

        await updateBom(editingBom.bomId, updatedBom);
        message.success('BOM更新成功');
        setIsBomEditModalVisible(false);
        // 刷新表格
        if (actionRef.current) {
          actionRef.current.reload();
        }
      } else {
        // 创建新BOM
        const newBom: BomListCreateDto = {
          bomName: values.bomName,
          bomCode: values.bomCode,
          status: values.status,
          remark: values.remark
        };

        await createBom(newBom);
        message.success('BOM创建成功');
        setIsAddBomModalVisible(false);
        // 刷新表格
        if (actionRef.current) {
          actionRef.current.reload();
        }
      }
    } catch (error) {
      message.error('保存失败，请检查输入');
      console.error('Save BOM error:', error);
    }
  }, [bomForm, editingBom, actionRef]);

  // 打开新建BOM模态框
  const handleAddBom = useCallback(() => {
    bomForm.resetFields();
    setEditingBom(null);
    setIsAddBomModalVisible(true);
  }, [bomForm]);

  // 批量删除BOM
  const handleBatchDelete = useCallback(async () => {
    Modal.confirm({
      title: '确认批量删除',
      content: `确认要删除选中的${selectedRowKeys.length}条BOM信息吗？`,
      onOk: async () => {
        try {
          await deleteBom(selectedRowKeys);
          message.success('批量删除成功');
          actionRef.current?.reload();
          setSelectedRowKeys([]);
        } catch (error: any) {
          // 从错误响应中提取具体错误消息
          const errorMessage = error?.response?.data?.message || error?.message || '批量删除失败';
          message.error(errorMessage);
          console.error('Batch delete error:', error);
        }
      },
    });
  }, [selectedRowKeys, actionRef]);

  // 打开产品选择弹窗
  const handleOpenProductModal = useCallback(() => {
    setIsProductModalVisible(true);
  }, []);

  // 处理产品选择
  const handleProductSelect = useCallback(async (product: ProductListDto) => {
    setSelectedProduct(product);
    setSelectedProductName(product.productName);
    // 将选中的产品ID设置到表单中
    form.setFieldsValue({
      productId: product.productListId,
      productName: product.productName,
      productCode: product.productCode
    });

    // 延迟关闭弹窗，确保表单值更新
    setTimeout(() => {
      setIsProductModalVisible(false);
    }, 100);
  }, [form]);

  // 删除BOM子项
  const handleDeleteBomItem = useCallback((bomItemId: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '确认要删除这条BOM子项信息吗？',
      onOk: async () => {
        try {
          await deleteBomItem(bomItemId);
          message.success('BOM子项删除成功');
          // 重新获取子项列表
          if (currentRow) {
            const itemsResponse = await getBomItemsByBomId(currentRow.bomId);
            // 为每个BOM子项获取产品详情
            const updatedBomItems = await Promise.all(
              itemsResponse.map(async (item: BomItem) => {
                if (item.productId) {
                  try {
                    const product = await getProductListById(item.productId);
                    return {
                      ...item,
                      productName: product.productName || '',
                      productCode: product.productCode || ''
                    };
                  } catch (error) {
                    console.error('获取产品详情失败:', error);
                    return item;
                  }
                }
                return item;
              })
            );
            setBomItems(updatedBomItems);
          }
        } catch (error: any) {
          // 从错误响应中提取具体错误消息
          const errorMessage = error?.response?.data?.message || error?.message || 'BOM子项删除失败';
          message.error(errorMessage);
          console.error('Delete BOM item error:', error);
        }
      }
    });
  }, [currentRow]);

  // 打开添加BOM子项模态框
  const handleAddBomItem = useCallback(() => {
    form.resetFields();
    setEditingBomItem(null);
    setIsAddModalVisible(true);
  }, [form]);

  // 打开编辑BOM子项模态框
  const handleEditBomItem = useCallback(async (bomItem: BomItem) => {
    setEditingBomItem(bomItem);
    // 查找对应产品的名称和编码
    let productName = '';
    let productCode = '';
    if (bomItem.productId) {
      try {
        // 根据产品ID获取产品详情
        const product = await getProductListById(bomItem.productId);
        productName = product.productName || '';
        productCode = product.productCode || '';
      } catch (error) {
        console.error('获取产品详情失败:', error);
      }
    }
    form.setFieldsValue({
      stationCode: bomItem.stationCode,
      batchRule: bomItem.batchRule,
      batchQty: bomItem.batchQty,
      batchSNQty: bomItem.batchSNQty,
      productId: bomItem.productId,
      productName: productName,
      productCode: productCode
    });
    setIsEditModalVisible(true);
  }, [form]);

  // 保存BOM子项
  const handleSaveBomItem = useCallback(async () => {
    try {
      const values = await form.validateFields();

      if (editingBomItem) {
        // 更新BOM子项
        const updatedItem: BomItemUpdateDto = {
          bomItemId: editingBomItem.bomItemId,
          bomId: currentRow?.bomId,
          stationCode: values.stationCode,
          batchRule: values.batchRule,
          batchQty: values.batchQty,
          batchSNQty: values.batchSNQty,
          productId: values.productId,
          productName: values.productName,
          productCode: values.productCode
        };

        await updateBomItem(editingBomItem.bomItemId, updatedItem);
        message.success('BOM子项更新成功');
        setIsEditModalVisible(false);
      } else {
        // 创建BOM子项
        const newItem: BomItemCreateDto = {
          bomId: currentRow?.bomId,
          stationCode: values.stationCode,
          batchRule: values.batchRule,
          batchQty: values.batchQty,
          batchSNQty: values.batchSNQty,
          productId: values.productId,
          productName: values.productName,
          productCode: values.productCode
        };

        await createBomItem(newItem);
        message.success('BOM子项添加成功');
        setIsAddModalVisible(false);
      }

      // 重新获取子项列表
      if (currentRow) {
        const itemsResponse = await getBomItemsByBomId(currentRow.bomId);
        // 为每个BOM子项获取产品详情
        const updatedBomItems = await Promise.all(
          itemsResponse.map(async (item: BomItem) => {
            if (item.productId) {
              try {
                const product = await getProductListById(item.productId);
                return {
                  ...item,
                  productName: product.productName || '',
                  productCode: product.productCode || ''
                };
              } catch (error) {
                console.error('获取产品详情失败:', error);
                return item;
              }
            }
            return item;
          })
        );
        setBomItems(updatedBomItems);
      }
    } catch (error) {
      message.error('保存失败，请检查输入');
      console.error('Save BOM item error:', error);
    }
  }, [form, editingBomItem, currentRow]);

  return (
    <PageContainer title="BOM管理">
      <ProTable<BomListDto>
        actionRef={actionRef}
        rowKey="bomId"
        className="bom-table"
        columns={columns}
        search={{
          labelWidth: 120,
          layout: 'vertical',
        }}
        request={async (
          params
        ): Promise<RequestData<BomListDto>> => {
          setCurrentSearchParams({
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            bomName: params.bomName,
            bomCode: params.bomCode,
            status: params.status,
          });

          const queryParams: BomListQueryDto = {
            current: Math.max(1, params.current || 1),
            pageSize: Math.min(100, Math.max(1, params.pageSize || 10)),
            bomName: params.bomName,
            bomCode: params.bomCode,
            status: params.status,
          };

          try {
            const response = await getBomList(queryParams);

            return {
              data: response.data || [],
              total: response.total || 0,
              success: response.success !== false,
            };
          } catch (error) {
            console.error('BOM - API调用失败:', error);
            return {
              data: [],
              total: 0,
              success: false,
            };
          }
        }}
        rowSelection={{
          type: "checkbox",
          onChange: (selectedRowKeys) => setSelectedRowKeys(selectedRowKeys as string[]),
        }}
        toolBarRender={() => [
          <Button
            type="primary"
            key="add"
            icon={<PlusOutlined />}
            onClick={handleAddBom}>
            新建BOM
          </Button>,
          <Button
            danger
            key="batchDelete"
            icon={<DeleteOutlined />}
            onClick={handleBatchDelete}
            disabled={selectedRowKeys.length === 0}>
            批量删除
          </Button>,
        ]}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100'],
        }}
        onRow={(record) => ({
          onDoubleClick: () => handleRowDoubleClick(record),
        })}
        tableAlertRender={({ selectedRowKeys }) => (
          <div style={{ marginBottom: 8 }}>
            已选择 {selectedRowKeys?.length || 0} 项
          </div>
        )}
      />

      {showDetail && currentRow && (
        <div className="bom-detail" style={{ marginTop: 20 }}>
          <ProDescriptions<BomListDto>
            title="BOM基本信息"
            dataSource={currentRow}
            column={3}
          >
            <ProDescriptions.Item label="BOM ID">{currentRow.bomId}</ProDescriptions.Item>
            <ProDescriptions.Item label="BOM名称">{currentRow.bomName}</ProDescriptions.Item>
            <ProDescriptions.Item label="BOM编码">{currentRow.bomCode}</ProDescriptions.Item>
            <ProDescriptions.Item label="状态">
              {currentRow.status === 0 ? '启用' : '禁用'}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="备注">{currentRow.remark}</ProDescriptions.Item>
            <ProDescriptions.Item label="创建时间">{currentRow.createTime}</ProDescriptions.Item>
          </ProDescriptions>

          <Tabs activeKey={activeTabKey} onChange={setActiveTabKey} style={{ marginTop: 20 }}>
            <TabPane
              tab={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  BOM子项
                  <Button
                    type="primary"
                    style={{ marginLeft: 16 }}
                    icon={<PlusOutlined />}
                    onClick={handleAddBomItem}
                  >
                    添加子项
                  </Button>
                </div>
              }
              key="items"
            >
              <Row gutter={[16, 16]}>
                {bomItems.length > 0 ? (
                  bomItems.map((item) => (
                    <Col xs={24} sm={12} md={8} key={item.bomItemId}>
                      <BomItemCard
                        item={item}
                        onEdit={handleEditBomItem}
                        onDelete={handleDeleteBomItem}
                      />
                    </Col>
                  ))
                ) : (
                  <Col span={24}>
                    <div style={{ textAlign: 'center', padding: 40, color: '#999' }}>
                      暂无BOM子项数据
                    </div>
                  </Col>
                )}
              </Row>
            </TabPane>
          </Tabs>
        </div>
      )}

      {/* 添加/编辑BOM子项模态框 */}
      <BomItemFormModal
        open={isAddModalVisible || isEditModalVisible}
        onCancel={() => {
          setIsAddModalVisible(false);
          setIsEditModalVisible(false);
        }}
        onOk={handleSaveBomItem}
        editingItem={editingBomItem}
        stations={stations}
        stationLoading={stationLoading}
        onOpenProductModal={handleOpenProductModal}
        selectedProductName={selectedProductName}
        form={form}
      />

      {/* 产品选择弹窗 */}
      <ProductSelectModal
        open={isProductModalVisible}
        onCancel={() => setIsProductModalVisible(false)}
        onSelect={handleProductSelect}
      />

      {/* 编辑/新建BOM模态框 */}
      <BomFormModal
        open={isBomEditModalVisible || isAddBomModalVisible}
        onCancel={() => {
          setIsBomEditModalVisible(false);
          setIsAddBomModalVisible(false);
        }}
        onOk={handleSaveBom}
        editingBom={editingBom}
        form={bomForm}
      />
    </PageContainer>
  );
};

export default BomPage;