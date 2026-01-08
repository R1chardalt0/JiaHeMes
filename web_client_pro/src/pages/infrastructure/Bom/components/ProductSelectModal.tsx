import React, { useState, useCallback, useEffect } from 'react';
import { Modal, Input, Button, Table } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { getProductListList } from '@/services/Api/Infrastructure/ProductList';
import { ProductListDto, ProductListQueryDto } from '@/services/Model/Infrastructure/ProductList';

// 产品选择弹窗属性接口
interface ProductSelectModalProps {
  open: boolean;
  onCancel: () => void;
  onSelect: (product: ProductListDto) => void;
}

/**
 * 产品选择弹窗组件
 * 用于从产品列表中选择产品
 */
const ProductSelectModal: React.FC<ProductSelectModalProps> = ({
  open,
  onCancel,
  onSelect
}) => {
  // 产品列表状态
  const [products, setProducts] = useState<ProductListDto[]>([]);
  const [productTotal, setProductTotal] = useState<number>(0);
  const [productCurrent, setProductCurrent] = useState<number>(1);
  const [productPageSize, setProductPageSize] = useState<number>(10);
  const [productSearchParams, setProductSearchParams] = useState<ProductListQueryDto>({
    current: 1,
    pageSize: 10
  });
  const [productSearchValues, setProductSearchValues] = useState({
    productCode: '',
    productName: ''
  });

  /**
   * 获取产品列表数据
   */
  const fetchProducts = useCallback(async (params: ProductListQueryDto) => {
    try {
      const response = await getProductListList(params);
      setProducts(response.data || []);
      setProductTotal(response.total || 0);
      setProductCurrent(params.current);
      setProductPageSize(params.pageSize);
    } catch (error) {
      console.error('Fetch products error:', error);
    }
  }, []);

  /**
   * 处理搜索输入变化
   */
  const handleSearchInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setProductSearchValues(prev => ({
      ...prev,
      [name]: value
    }));
  }, []);

  /**
   * 处理产品搜索
   */
  const handleProductSearch = useCallback(() => {
    const params: ProductListQueryDto = {
      current: 1,
      pageSize: productPageSize,
      productCode: productSearchValues.productCode,
      productName: productSearchValues.productName
    };
    setProductSearchParams(params);
    fetchProducts(params);
  }, [productSearchValues, productPageSize, fetchProducts]);

  /**
   * 处理产品分页变化
   */
  const handleProductPaginationChange = useCallback((current: number, pageSize: number) => {
    const params: ProductListQueryDto = {
      ...productSearchParams,
      current,
      pageSize
    };
    setProductSearchParams(params);
    fetchProducts(params);
  }, [productSearchParams, fetchProducts]);

  /**
   * 处理产品选择
   */
  const handleProductSelect = useCallback((product: ProductListDto) => {
    onSelect(product);
  }, [onSelect]);

  /**
   * 重置搜索参数
   */
  const handleReset = useCallback(() => {
    setProductSearchValues({ productCode: '', productName: '' });
    const params: ProductListQueryDto = {
      current: 1,
      pageSize: productPageSize
    };
    setProductSearchParams(params);
    fetchProducts(params);
  }, [productPageSize, fetchProducts]);

  // 当弹窗打开时，加载产品列表
  useEffect(() => {
    if (open) {
      const params: ProductListQueryDto = {
        current: 1,
        pageSize: 10
      };
      setProductSearchParams(params);
      fetchProducts(params);
    }
  }, [open, fetchProducts]);

  return (
    <Modal
      title="选择产品"
      open={open}
      onCancel={onCancel}
      footer={null}
      width={800}
    >
      {/* 搜索框 */}
      <div style={{ marginBottom: 16, display: 'flex', gap: 16 }}>
        <Input
          name="productCode"
          placeholder="按产品编码搜索"
          value={productSearchValues.productCode}
          onChange={handleSearchInputChange}
          style={{ width: 200 }}
        />
        <Input
          name="productName"
          placeholder="按产品名称搜索"
          value={productSearchValues.productName}
          onChange={handleSearchInputChange}
          style={{ width: 200 }}
        />
        <Button type="primary" icon={<SearchOutlined />} onClick={handleProductSearch}>
          搜索
        </Button>
        <Button onClick={handleReset}>
          重置
        </Button>
      </div>
      <Table
        dataSource={products}
        columns={[
          {
            title: '产品编码',
            dataIndex: 'productCode',
            key: 'productCode'
          },
          {
            title: '产品名称',
            dataIndex: 'productName',
            key: 'productName'
          },
          {
            title: '产品类型',
            dataIndex: 'productType',
            key: 'productType'
          },
          {
            title: '产品ID',
            dataIndex: 'productListId',
            key: 'productListId'
          },
          {
            title: '操作',
            key: 'action',
            render: (_: any, record: ProductListDto) => (
              <Button type="link" onClick={() => handleProductSelect(record)}>
                选择
              </Button>
            )
          }
        ]}
        rowKey="productListId"
        pagination={{
          current: productCurrent,
          pageSize: productPageSize,
          total: productTotal,
          onChange: handleProductPaginationChange
        }}
      />
    </Modal>
  );
};

export default ProductSelectModal;