// src/pages/infrastructure/Bom/columns.tsx
import type { ProColumns } from '@ant-design/pro-components';
import { Button, Popconfirm } from 'antd';
import { EyeOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { BomListDto } from '@/services/Model/Infrastructure/Bom/BomList';

interface ColumnsProps {
  onShowDetail: (record: BomListDto) => void;
  onEdit: (record: BomListDto) => void;
  onDelete: (bomId: string) => void;
}

/**
 * BOM列表表格列定义
 * @param props 回调函数
 * @returns 列定义数组
 */
export const getBomColumns = ({
  onShowDetail,
  onEdit,
  onDelete
}: ColumnsProps): ProColumns<BomListDto>[] => {
  return [
    {
      title: 'BOM名称',
      dataIndex: 'bomName',
      key: 'bomName',
      width: 150,
      search: true
    },
    {
      title: 'BOM编码',
      dataIndex: 'bomCode',
      key: 'bomCode',
      width: 150,
      search: true
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      search: true,
      valueEnum: {
        0: { text: '启用', status: 'Success' },
        1: { text: '禁用', status: 'Default' }
      }
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200,
      ellipsis: true,
      search: false
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      width: 180,
      valueType: 'dateTime',
      search: false
    },
    {
      title: '操作',
      key: 'operation',
      width: 180,
      fixed: 'right',
      search: false,
      render: (_, record) => (
        <>
          <Button
            type="link"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => onShowDetail(record)}
            style={{ marginRight: 8 }}
          >
            查看
          </Button>
          <Button
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={(e) => {
              e.stopPropagation();
              onEdit(record);
            }}
            style={{ marginRight: 8 }}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除"
            description="确定要删除该BOM吗？"
            onConfirm={(e) => {
              e?.stopPropagation();
              onDelete(record.bomId);
            }}
            okText="确定"
            cancelText="取消"
          >
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={(e) => e.stopPropagation()}
            >
              删除
            </Button>
          </Popconfirm>
        </>
      )
    }
  ];
};
