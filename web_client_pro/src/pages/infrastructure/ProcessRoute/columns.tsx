import type { ProColumns } from '@ant-design/pro-components';
import { Button, Popconfirm } from 'antd';
import { EyeOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { ProcessRouteDto } from '@/services/Model/Infrastructure/ProcessRoute/ProcessRoute';

/**
 * 工艺路线列表表格列定义
 * @param handleShowDetail 查看详情回调函数
 * @param handleEditProcessRoute 编辑工艺路线回调函数
 * @param handleDeleteProcessRoute 删除工艺路线回调函数
 * @returns 表格列定义数组
 */
export const getProcessRouteColumns = (
  handleShowDetail: (record: ProcessRouteDto) => void,
  handleEditProcessRoute: (record: ProcessRouteDto) => void,
  handleDeleteProcessRoute: (id: string) => void
): ProColumns<ProcessRouteDto>[] => {
  return [
    {
      title: '工艺路线名称',
      dataIndex: 'routeName',
      key: 'routeName',
      width: 150,
      search: true
    },
    {
      title: '工艺路线编码',
      dataIndex: 'routeCode',
      key: 'routeCode',
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
            onClick={() => handleShowDetail(record)}
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
              handleEditProcessRoute(record);
            }}
            style={{ marginRight: 8 }}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除"
            description="确定要删除该工艺路线吗？"
            onConfirm={(e) => {
              e?.stopPropagation();
              handleDeleteProcessRoute(record.id);
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