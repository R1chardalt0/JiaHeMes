import type { ProColumns } from '@ant-design/pro-components';
import React from 'react';
import { Button, Popconfirm } from 'antd';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { ProductListDto } from '@/services/Model/Infrastructure/ProductList';

interface ProductColumnsProps {
  onEdit: (record: ProductListDto) => void;
  onDelete: (id: string) => void;
}

/**
 * 获取产品列表表格列定义
 * @param props 回调函数
 * @returns 表格列定义数组
 */
export const getProductColumns = (props: ProductColumnsProps): ProColumns<ProductListDto>[] => {
  const { onEdit, onDelete } = props;

  return [
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 180,
    },
    {
      title: '产品名称',
      dataIndex: 'productName',
      key: 'productName',
      width: 180,
    },
    {
      title: 'BOM编号',
      dataIndex: 'bomCode',
      key: 'bomCode',
      width: 180,
    },
    {
      title: 'BOM名称',
      dataIndex: 'bomName',
      key: 'bomName',
      width: 180,
    },
    {
      title: 'BOM ID',
      dataIndex: 'bomId',
      key: 'bomId',
      width: 180,
      hideInTable: true,
    },
    {
      title: '工艺路线编号',
      dataIndex: 'processRouteCode',
      key: 'processRouteCode',
      width: 180,
    },
    {
      title: '工艺路线名称',
      dataIndex: 'processRouteName',
      key: 'processRouteName',
      width: 180,
    },
    {
      title: '工艺路线 ID',
      dataIndex: 'processRouteId',
      key: 'processRouteId',
      width: 180,
      hideInTable: true,
    },
    {
      title: '产品类型',
      dataIndex: 'productType',
      key: 'productType',
      width: 180,
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      search: false,
    },
    {
      title: '操作',
      key: 'action',
      width: 150,
      search: false,
      render: (_, record) => (
        <div>
          <Button
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={(e) => {
              e.stopPropagation();
              onEdit(record);
            }}
          >
            编辑
          </Button>
          <Button
            type="link"
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={(e) => {
              e.stopPropagation();
              onDelete(record.productListId!);
            }}
          >
            删除
          </Button>
        </div>
      ),
    },
  ];
};
