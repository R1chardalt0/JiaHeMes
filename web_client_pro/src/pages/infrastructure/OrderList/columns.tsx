// src/pages/infrastructure/OrderList/columns.tsx
import type { ProColumns } from '@ant-design/pro-components';
import { Button, Popconfirm } from 'antd';
import { EyeOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { OrderListDto } from '@/services/Model/Infrastructure/OrderList';

interface ColumnsProps {
  onShowDetail: (record: OrderListDto) => void;
  onEdit: (record: OrderListDto) => void;
  onDelete: (orderListId: string) => void;
}

/**
 * 工单列表表格列定义
 * @param props 回调函数
 * @returns 列定义数组
 */
export const getOrderListColumns = ({
  onShowDetail,
  onEdit,
  onDelete
}: ColumnsProps): ProColumns<OrderListDto>[] => {
  return [
    {
      title: '工单编码',
      dataIndex: 'orderCode',
      key: 'orderCode',
      width: 150,
      search: true
    },
    {
      title: '工单名称',
      dataIndex: 'orderName',
      key: 'orderName',
      width: 150,
      search: true
    },
    {
      title: '产品编码',
      dataIndex: 'productCode',
      key: 'productCode',
      width: 150,
      search: true
    },
    {
      title: '产品名称',
      dataIndex: 'productName',
      key: 'productName',
      width: 150,
      search: true
    },
    {
      title: 'Bom编码',
      dataIndex: 'bomCode',
      key: 'bomCode',
      width: 150,
      search: true
    },
    {
      title: 'Bom名称',
      dataIndex: 'bomName',
      key: 'bomName',
      width: 150,
      search: true
    },
    {
      title: '工艺路线编码',
      dataIndex: 'processRouteCode',
      key: 'processRouteCode',
      width: 150,
      search: true
    },
    {
      title: '工艺路线名称',
      dataIndex: 'processRouteName',
      key: 'processRouteName',
      width: 150,
      search: true
    },
    {
      title: '工单类型',
      dataIndex: 'orderType',
      key: 'orderType',
      width: 100,
      search: true,
      render: (text) => {
        const value = typeof text === 'number' ? text : 0;
        switch (value) {
          case 1:
            return '生产工单';
          case 2:
            return '返工工单';
          default:
            return '-';
        }
      }
    },
    {
      title: '计划数量',
      dataIndex: 'planQty',
      key: 'planQy',
      width: 100,
      search: false
    },
    {
      title: '已完成数量',
      dataIndex: 'completedQty',
      key: 'completedQty',
      width: 120,
      search: false
    },
    {
      title: '优先级',
      dataIndex: 'priorityLevel',
      key: 'priorityLevel',
      width: 80,
      search: true,
      render: (text) => {
        const value = typeof text === 'number' ? text : 0;
        switch (value) {
          case 1:
            return '紧急';
          case 3:
            return '高';
          case 5:
            return '中';
          case 7:
            return '低';
          default:
            return '-';
        }
      }
    },
    {
      title: '计划开始时间',
      dataIndex: 'planStartTime',
      key: 'planStartTime',
      width: 180,
      valueType: 'dateTime',
      search: false
    },
    {
      title: '计划结束时间',
      dataIndex: 'planEndTime',
      key: 'planEndTime',
      width: 180,
      valueType: 'dateTime',
      search: false
    },
    {
      title: '实际开始时间',
      dataIndex: 'actualStartTime',
      key: 'actualStartTime',
      width: 180,
      valueType: 'dateTime',
      search: false
    },
    {
      title: '实际结束时间',
      dataIndex: 'actualEndTime',
      key: 'actualEndTime',
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
            description="确定要删除该工单吗？"
            onConfirm={(e) => {
              e?.stopPropagation();
              onDelete(record.orderListId);
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
