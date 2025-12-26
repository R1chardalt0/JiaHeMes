import React, { useState, useRef } from 'react';
import {
    Modal, Form, Input, Select, InputNumber, Switch, Button, message,
    TreeSelect, Space, Radio, Drawer
} from 'antd';
import { PlusOutlined, CloseOutlined, DeleteOutlined, EditOutlined, DownOutlined, RightOutlined, ExclamationCircleFilled } from '@ant-design/icons';
import { ProTable, PageContainer, ProDescriptions } from '@ant-design/pro-components';
import type { ActionType, ProColumns, ProDescriptionsItemProps } from '@ant-design/pro-components';
import { useRequest } from '@umijs/max';
import { getDeptPagination, createDept, updateDept, deleteDept, getDeptTree } from '@/services/Api/Systems/dept';
import { DeptItem } from '@/services/Model/Systems/dept';

const { Option } = Select;

const DeptManagement: React.FC = () => {
    const [form] = Form.useForm();
    const actionRef = useRef<ActionType>(null);
    const [modalVisible, setModalVisible] = useState(false);
    const [currentDept, setCurrentDept] = useState<DeptItem | null>(null);
    const [showDetail, setShowDetail] = useState(false);
    const [currentSearchParams, setCurrentSearchParams] = useState<{
        current: number;
        pageSize: number;
    }>({ current: 1, pageSize: 10 });
    
    // 树形表格展开的行keys
    const [expandedRowKeys, setExpandedRowKeys] = useState<React.Key[]>([]);
    // 是否全部展开
    const [allExpanded, setAllExpanded] = useState(false);

    // 获取部门树数据（用于表格展示）
    const { data: deptTreeData, loading: treeLoading, run: refreshTree, refresh: refreshTreeData } = useRequest(() => getDeptTree(), {
        formatResult: (res) => {
            if (res.code === 200 && res.data) {
                // 转换数据格式，确保每个节点都有id字段
                const transformData = (items: any[]): DeptItem[] => {
                    return items.map(item => ({
                        ...item,
                        id: item.deptId || item.id,
                        children: item.children ? transformData(item.children) : undefined
                    }));
                };
                return transformData(res.data);
            }
            return [];
        },
        onSuccess: (data) => {
            // 页面初始化时默认收起所有部门
            setExpandedRowKeys([]);
            setAllExpanded(false);
            // 数据更新后自动刷新表格
            if (actionRef.current) {
                actionRef.current.reload();
            }
        },
        onError: () => {
            message.error('获取部门树失败');
        }
    });

    // 获取部门树数据（用于TreeSelect）
    const { data: deptTree } = useRequest(() => getDeptTree(), {
        formatResult: (res) => {
            if (res.code === 200 && res.data) {
                // 适配后端返回的部门树数据格式
                return [{ deptId: 0, deptName: '顶层部门', children: res.data.map(item => ({
                    ...item,
                    id: item.deptId || item.id
                })) || [] }];
            }
            return [{ deptId: 0, deptName: '顶层部门', children: [] }];
        }
    });

    /**
     * 展开/折叠所有部门
     */
    const handleExpandAll = () => {
        if (!deptTreeData || !Array.isArray(deptTreeData) || deptTreeData.length === 0) {
            return;
        }

        if (allExpanded) {
            // 折叠所有
            setExpandedRowKeys([]);
            setAllExpanded(false);
        } else {
            // 展开所有
            const getAllKeys = (depts: DeptItem[]): React.Key[] => {
                let keys: React.Key[] = [];
                depts.forEach((dept) => {
                    if (dept.children && dept.children.length > 0) {
                        keys.push(dept.deptId);
                        keys = keys.concat(getAllKeys(dept.children));
                    }
                });
                return keys;
            };
            const allKeys = getAllKeys(deptTreeData);
            setExpandedRowKeys(allKeys);
            setAllExpanded(true);
        }
    };

    const columns: ProColumns<DeptItem>[] = [
        {
            title: '部门名称',
            dataIndex: 'deptName',
            render: (_, record) => (
                <a onClick={() => {
                    setCurrentDept(record);
                    setShowDetail(true);
                }}>
                    {record.deptName}
                </a>
            )
        },
        {
            title: '负责人',
            dataIndex: 'leader'
        },
        {
            title: '联系电话',
            dataIndex: 'phone'
        },
        {
            title: '部门排序',
            dataIndex: 'orderNum'
        },
        {
            title: '状态',
            dataIndex: 'status',
            valueEnum: {
                '0': { text: '启用', status: 'Success' },
                '1': { text: '停用', status: 'Error' }
            }
        },
        {
            title: '操作',
            valueType: 'option',
            render: (_, record) => [
                <Button type="link" key="edit" onClick={() => showEditModal(record)}>
                    <EditOutlined />编辑
                </Button>,
                <Button danger type="link" key="delete" onClick={() => handleDelete(record.deptId)}>
                    <DeleteOutlined /> 删除
                </Button>
            ]
        }
    ];

    const showEditModal = (record?: DeptItem) => {
        if (record) {
            const formData = {
                ...record,
                id: record.deptId
            };
            form.setFieldsValue(formData);
            setCurrentDept(record);
        } else {
            form.resetFields();
            setCurrentDept(null);
        }
        setModalVisible(true);
    };

    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            let submitData;
            
            if (currentDept) {
                // 更新操作：保留现有deptId
                submitData = {
                    ...values,
                    deptId: values.id || currentDept.deptId
                };
                // 调用更新API
                await updateDept(submitData);
            } else {
                // 创建操作：不传递deptId或设为0，让后端自动生成
                submitData = { ...values };
                // 删除可能存在的id字段，避免作为deptId传递
                if (submitData.id) {
                    delete submitData.id;
                }
                // 调用创建API
                await createDept(submitData);
            }

            message.success('操作成功');
            setModalVisible(false);
            form.resetFields();
            setCurrentDept(null);
            
            // 刷新树形数据，onSuccess 回调会自动刷新表格
            await refreshTreeData();
        } catch (error) {
            console.error('表单验证失败', error);
        }
    };

    /**
     * 删除部门
     * - 显示确认弹框
     * - 删除成功刷新列表
     * - 如果存在关联员工，提示用户先移除或修改
     */
    const handleDelete = async (deptId?: number) => {
        if (!deptId) return;

        Modal.confirm({
            title: '确认删除',
            content: '确定要删除该部门吗？删除后可能影响相关数据。',
            icon: <ExclamationCircleFilled style={{ color: '#ffb84d' }} />,
            width: 420,
            style: {
                top: '16vh',
            },
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
                footer: {
                    background: '#ffffff',
                    borderTop: '1px solid #f0f0f0'
                },
                mask: {
                    background: 'rgba(0,0,0,0.1)'
                },
            },
            okText: '确认',
            okButtonProps: {
                type: 'primary',
                danger: false,
                style: {
                    background: 'linear-gradient(98deg, #2b7bff 0%, #3db2ff 100%)',
                    border: 'none',
                    boxShadow: '0 6px 14px rgba(45,123,255,0.35)',
                },
            },
            cancelText: '取消',
            cancelButtonProps: {
                style: {
                    color: 'rgba(255,255,255,0.85)',
                    borderColor: 'rgba(255,255,255,0.3)',
                    background: 'rgba(255,255,255,0.04)',
                },
            },
            onOk: async () => {
                try {
                    await deleteDept(deptId, { skipErrorHandler: true });
                    message.success('删除成功');
                    
                    // 刷新树形数据，onSuccess 回调会自动刷新表格
                    await refreshTreeData();
                } catch (error: any) {
                    const parseErrorMessage = (errorData: any): string => {
                        if (!errorData) {
                            return '删除失败，请稍后重试';
                        }

                        const friendlyMessage =
                            '删除失败：该部门下还有员工，请先移除或调整员工所属部门';

                        if (typeof errorData === 'string') {
                            // 尝试识别常见错误关键字
                            const firstLine = errorData.split('\n')[0];
                            if (
                                firstLine.includes('FK_') ||
                                firstLine.includes('外键约束') ||
                                firstLine.includes('SysUsers')
                            ) {
                                return friendlyMessage;
                            }
                            if (firstLine.length < 120) {
                                return firstLine;
                            }
                            return friendlyMessage;
                        }

                        if (typeof errorData === 'object') {
                            if (errorData.msg || errorData.message || errorData.errorMessage) {
                                const msg =
                                    errorData.msg || errorData.message || errorData.errorMessage;
                                if (msg.includes('FK_') || msg.includes('外键约束')) {
                                    return friendlyMessage;
                                }
                                return msg;
                            }
                            if (errorData.error) {
                                const errorStr =
                                    typeof errorData.error === 'string'
                                        ? errorData.error
                                        : JSON.stringify(errorData.error);
                                if (
                                    errorStr.includes('FK_') ||
                                    errorStr.includes('外键约束') ||
                                    errorStr.includes('SysUsers')
                                ) {
                                    return friendlyMessage;
                                }
                                const firstLine = errorStr.split('\n')[0];
                                if (firstLine.length < 120) {
                                    return firstLine;
                                }
                            }
                        }
                        return friendlyMessage;
                    };

                    let errorMessage = '删除失败，请稍后重试';
                    if (error?.response?.data) {
                        errorMessage = parseErrorMessage(error.response.data);
                    } else if (error?.message) {
                        errorMessage = parseErrorMessage(error.message);
                    }

                    message.error(errorMessage);
                    console.error('删除部门失败:', error);
                }
            },
        });
    };


    return (
        <PageContainer
            className="system-settings-page"
            header={{
                title: '部门管理',
            }}
        >
            <ProTable<DeptItem>
                actionRef={actionRef}
                columns={columns}
                // 使用树形数据，不走后端分页，在前端根据搜索条件进行过滤
                request={async (params) => {
                    if (deptTreeData && Array.isArray(deptTreeData)) {
                        const { deptName, leader, phone, status, orderNum } = params || {};

                        // 1. 扁平化树，方便过滤
                        const flatten = (nodes: DeptItem[]): DeptItem[] => {
                            const result: DeptItem[] = [];
                            nodes.forEach((node) => {
                                result.push(node);
                                if (node.children && node.children.length > 0) {
                                    result.push(...flatten(node.children));
                                }
                            });
                            return result;
                        };

                        let flatList = flatten(deptTreeData);

                        // 2. 按条件过滤
                        if (deptName) {
                            const keyword = String(deptName).trim();
                            if (keyword) {
                                flatList = flatList.filter((item) =>
                                    (item.deptName || '').toString().includes(keyword),
                                );
                            }
                        }

                        if (leader) {
                            const keyword = String(leader).trim();
                            if (keyword) {
                                flatList = flatList.filter((item) =>
                                    (item.leader || '').toString().includes(keyword),
                                );
                            }
                        }

                        if (phone) {
                            const keyword = String(phone).trim();
                            if (keyword) {
                                flatList = flatList.filter((item) =>
                                    (item.phone || '').toString().includes(keyword),
                                );
                            }
                        }

                        // 部门排序
                        if (orderNum !== undefined && orderNum !== null && orderNum !== '') {
                            const num = Number(orderNum);
                            if (!Number.isNaN(num)) {
                                flatList = flatList.filter((item) => {
                                    const value = (item as any).orderNum;
                                    return value === num || Number(value) === num;
                                });
                            }
                        }

                        if (status !== undefined && status !== null && status !== '') {
                            const s = String(status);
                            flatList = flatList.filter((item) => item.status === s);
                        }

                        // 3. 根据过滤结果重建树：保留匹配节点及其父节点
                        const filterTree = (nodes: DeptItem[]): DeptItem[] => {
                            const result: DeptItem[] = [];
                            nodes.forEach((node) => {
                                const children = node.children ? filterTree(node.children) : [];
                                const selfMatch = flatList.some((x) => x.deptId === node.deptId);
                                if (selfMatch || children.length > 0) {
                                    result.push({
                                        ...node,
                                        children: children.length > 0 ? children : undefined,
                                    });
                                }
                            });
                            return result;
                        };

                        const hasFilter =
                            (deptName && String(deptName).trim()) ||
                            (leader && String(leader).trim()) ||
                            (phone && String(phone).trim()) ||
                            (orderNum !== undefined && orderNum !== null && orderNum !== '') ||
                            (status !== undefined && status !== null && status !== '');

                        const filteredTree = hasFilter ? filterTree(deptTreeData) : deptTreeData;

                        // 4. 重新计算总数（包括所有子节点）
                        const countNodes = (nodes: DeptItem[]): number => {
                            let count = nodes.length;
                            nodes.forEach((node) => {
                                if (node.children && node.children.length > 0) {
                                    count += countNodes(node.children);
                                }
                            });
                            return count;
                        };
                        const total = countNodes(filteredTree);

                        return {
                            data: filteredTree,
                            total,
                            success: true,
                        };
                    }

                    return {
                        data: [],
                        total: 0,
                        success: false,
                    };
                }}
                loading={treeLoading}
                rowKey="deptId"
                search={{
                    labelWidth: 120,
                    defaultCollapsed: true, // 默认收起搜索/筛选区域
                }}
                scroll={{ x: 'max-content' }}
                pagination={false} // 树形结构不使用分页
                // 展开/折叠配置
                expandable={{
                    expandedRowKeys,
                    onExpandedRowsChange: (keys) => {
                        setExpandedRowKeys(keys as React.Key[]);
                        // 计算是否全部展开
                        if (!deptTreeData || !Array.isArray(deptTreeData) || deptTreeData.length === 0) {
                            return;
                        }
                        const getAllExpandableKeys = (depts: DeptItem[]): React.Key[] => {
                            let allKeys: React.Key[] = [];
                            depts.forEach((dept) => {
                                if (dept.children && dept.children.length > 0) {
                                    allKeys.push(dept.deptId);
                                    allKeys = allKeys.concat(getAllExpandableKeys(dept.children));
                                }
                            });
                            return allKeys;
                        };
                        const allExpandableKeys = getAllExpandableKeys(deptTreeData);
                        setAllExpanded(keys.length > 0 && keys.length >= allExpandableKeys.length);
                    },
                    indentSize: 24, // 子级缩进大小
                    // 自定义展开/收起图标
                    expandIcon: ({ expanded, onExpand, record }) => {
                        // 只有有子部门的项才显示展开图标
                        const hasChildren = record.children && record.children.length > 0;
                        if (!hasChildren) {
                            return null;
                        }
                        return (
                            <span
                                onClick={(e) => {
                                    e.stopPropagation();
                                    onExpand(record, e);
                                }}
                                style={{
                                    cursor: 'pointer',
                                    display: 'inline-flex',
                                    alignItems: 'center',
                                    justifyContent: 'center',
                                    width: 16,
                                    height: 16,
                                    marginRight: 8,
                                }}
                            >
                                {expanded ? <DownOutlined /> : <RightOutlined />}
                            </span>
                        );
                    },
                }}
                toolBarRender={() => [
                    <Button
                        type="primary"
                        key="add"
                        icon={<PlusOutlined />}
                        onClick={() => showEditModal()}
                    >
                        添加部门
                    </Button>,
                    <Button
                        type="primary"
                        key="expand"
                        icon={allExpanded ? <RightOutlined /> : <DownOutlined />}
                        onClick={handleExpandAll}
                    >
                        {allExpanded ? '收起部门' : '展开部门'}
                    </Button>,
                ]}
            />


            <Modal
                title={
                    <Space>
                        {currentDept ? '编辑部门' : '添加部门'}
                        <CloseOutlined
                            style={{ cursor: 'pointer' }}
                            onClick={() => setModalVisible(false)}
                        />
                    </Space>
                }
                open={modalVisible}
                onOk={handleSubmit}
                onCancel={() => setModalVisible(false)}
                width={600}
                destroyOnClose
                className="dept-edit-modal"
                rootClassName="dept-edit-modal"
                styles={{
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
                }}
            >
                <Form form={form} layout="vertical" autoComplete="off">
                    <Form.Item name="id" hidden>
                        <Input />
                    </Form.Item>

                    <Form.Item
                        name="parentId"
                        label="上级部门"
                        initialValue={0}
                    >
                        <TreeSelect
                            placeholder="请选择上级部门"
                            treeData={deptTree || []}
                            fieldNames={{ label: 'deptName', value: 'deptId', children: 'children' }}
                            treeDefaultExpandAll
                            popupClassName="dept-edit-modal-dropdown"
                            dropdownStyle={{
                              background: '#ffffff',
                              border: '1px solid #f0f0f0',
                              boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
                            }}
                        />
                    </Form.Item>

                    <Form.Item
                        name="deptName"
                        label="部门名称"
                        rules={[{ required: true, message: '请输入部门名称' }]}
                    >
                        <Input placeholder="请输入部门名称" autoComplete="off" />
                    </Form.Item>

                    <Form.Item
                        name="leader"
                        label="负责人"
                    >
                        <Input placeholder="请输入负责人姓名" autoComplete="off" name="leader_no_autofill" autoCorrect="off" autoCapitalize="off" spellCheck={false} />
                    </Form.Item>

                    <Form.Item
                        name="phone"
                        label="联系电话"
                    >
                        <Input placeholder="请输入联系电话" />
                    </Form.Item>

                    <Form.Item
                        name="email"
                        label="邮箱"
                    >
                        <Input placeholder="请输入邮箱地址" />
                    </Form.Item>

                    <Form.Item
                        name="orderNum"
                        label="显示排序"
                        rules={[{ required: true, message: '请输入排序号' }]}
                    >
                        <InputNumber min={0} style={{ width: '100%' }} placeholder="请输入排序号" />
                    </Form.Item>

                    <Form.Item
                        name="address"
                        label="地址"
                    >
                        <Input placeholder="请输入部门地址" />
                    </Form.Item>

                    <Form.Item
                        name="status"
                        label="部门状态"
                        initialValue="0"
                    >
                        <Radio.Group>
                            <Radio value="0">正常</Radio>
                            <Radio value="1">停用</Radio>
                        </Radio.Group>
                    </Form.Item>
                </Form>
            </Modal>

            <Drawer
                width={600}
                open={showDetail}
                onClose={() => {
                    setShowDetail(false);
                }}
                closable={false}
                className="dept-info-drawer"
                rootClassName="dept-info-drawer"
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
                {currentDept?.deptName && (
                    <ProDescriptions<DeptItem>
                        column={2}
                        title={currentDept?.deptName}
                        dataSource={currentDept}
                        columns={columns as ProDescriptionsItemProps<DeptItem>[]}
                    />
                )}
            </Drawer>
        </PageContainer>
    );
};

export default DeptManagement;