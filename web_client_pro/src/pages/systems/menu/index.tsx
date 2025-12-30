import React, { useState, useRef } from 'react';
import {
    Modal, Form, Input, Select, InputNumber, Button, message,
    TreeSelect, Space, Radio, Drawer
} from 'antd';
import { 
    PlusOutlined, 
    CloseOutlined, 
    DeleteOutlined, 
    EditOutlined,
    DownOutlined,
    RightOutlined
} from '@ant-design/icons';
import { ProTable, PageContainer, FooterToolbar, ProDescriptions } from '@ant-design/pro-components';
import type { ActionType, ProColumns, ProDescriptionsItemProps } from '@ant-design/pro-components';
import { useRequest, useModel } from '@umijs/max';
import { getMenuTree, createMenu, deleteMenu, updateMenu } from '@/services/Api/Systems/menu';
import { MenuItem, MenuTreeResult } from '@/services/Model/Systems/menu';
import * as Icons from '@ant-design/icons';


const MenuManagement: React.FC = () => {
    const [form] = Form.useForm();
    const { initialState } = useModel('@@initialState');
    // 仅允许用户表中 UserId=1 的用户执行删除（含批量删除）
    // 注意：不同后端/登录接口字段可能是 userId/UserId/id 等，这里做一次兼容读取
    const currentUserId =
        (initialState as any)?.currentUser?.UserId ??
        (initialState as any)?.currentUser?.userId ??
        (initialState as any)?.currentUser?.id ??
        (initialState as any)?.currentUser?.Id;
    const isSuperAdmin = Number(currentUserId) === 1;
    const actionRef = useRef<ActionType>(null);
    const [modalVisible, setModalVisible] = useState(false);
    const [currentMenu, setCurrentMenu] = useState<MenuItem | null>(null);
    const [menuType, setMenuType] = useState<'M' | 'C' | 'F'>('C');
    const [showDetail, setShowDetail] = useState(false);
    const [selectedRows, setSelectedRows] = useState<MenuItem[]>([]);
    // 树形表格展开的行keys
    const [expandedRowKeys, setExpandedRowKeys] = useState<React.Key[]>([]);
    // 是否全部展开
    const [allExpanded, setAllExpanded] = useState(false);
    // 搜索条件
    const [searchParams, setSearchParams] = useState<{
        menuName?: string;
        status?: string;
        menuType?: string;
    }>({});

    /**
     * 获取菜单树数据
     * 使用 getMenuTree 接口获取完整的树形结构
     */
    const { data: menuTreeData, loading: treeLoading, run: refreshTree } = useRequest(
        getMenuTree,
        {
            formatResult: (res: MenuTreeResult) => res,
            onSuccess: (res: MenuTreeResult) => {
                if (res.code === 200 && res.data) {
                    // 页面初始化时默认收起所有菜单
                    setExpandedRowKeys([]);
                    setAllExpanded(false);
                }
            },
            onError: () => {
                message.error('获取菜单树失败');
            },
        }
    );

    /**
     * 获取菜单树数据（用于TreeSelect）
     */
    const { data: menuTree } = useRequest(getMenuTree, {
        formatResult: (res: MenuTreeResult) => res,
        onSuccess: (res: MenuTreeResult) => {
            if (res.code === 200 && res.data) {
                return [{
                    menuId: 0,
                    menuName: '主类目',
                    children: res.data
                }];
            }
            return [{ menuId: 0, menuName: '主类目', children: [] }];
        }
    });

    /**
     * 获取图标组件
     * @param iconName 图标名称（可能是小写如 "smile"，也可能是完整组件名如 "PieChartOutlined"）
     * @returns 图标组件或null
     */
    const getIconComponent = (iconName?: string) => {
        if (!iconName) return null;
        
        // 尝试直接匹配（如果已经是完整的组件名，如 "PieChartOutlined", "SmileOutlined"）
        let Icon = (Icons as any)[iconName];
        if (Icon) {
            return <Icon />;
        }
        
        // 如果名称已经包含 Outlined/Filled/TwoTone 后缀，说明格式正确但可能大小写不对
        // 尝试转换为 PascalCase
        if (iconName.includes('Outlined') || iconName.includes('Filled') || iconName.includes('TwoTone')) {
            // 已经是完整格式，尝试首字母大写
            const pascalCaseName = iconName.charAt(0).toUpperCase() + iconName.slice(1);
            Icon = (Icons as any)[pascalCaseName];
            if (Icon) {
                return <Icon />;
            }
        }
        
        // 处理小写或驼峰格式（如 "smile", "pieChart" -> "SmileOutlined", "PieChartOutlined"）
        // 将字符串转换为 PascalCase（首字母大写，其他保持原样）
        const toPascalCase = (str: string) => {
            // 如果已经是 PascalCase（首字母大写），直接使用
            if (str.charAt(0) === str.charAt(0).toUpperCase()) {
                return str;
            }
            // 否则首字母大写
            return str.charAt(0).toUpperCase() + str.slice(1);
        };
        
        const baseName = toPascalCase(iconName);
        
        // 尝试添加 Outlined 后缀（最常用）
        const outlinedName = `${baseName}Outlined`;
        Icon = (Icons as any)[outlinedName];
        if (Icon) {
            return <Icon />;
        }
        
        // 尝试添加 Filled 后缀
        const filledName = `${baseName}Filled`;
        Icon = (Icons as any)[filledName];
        if (Icon) {
            return <Icon />;
        }
        
        // 尝试添加 TwoTone 后缀
        const twoToneName = `${baseName}TwoTone`;
        Icon = (Icons as any)[twoToneName];
        if (Icon) {
            return <Icon />;
        }
        
        // 如果都不匹配，返回null
        return null;
    };

    /**
     * 展开/折叠所有菜单
     */
    const handleExpandAll = () => {
        if (!menuTreeData || !('code' in menuTreeData) || menuTreeData.code !== 200 || !('data' in menuTreeData) || !menuTreeData.data) {
            return;
        }

        if (allExpanded) {
            // 折叠所有
            setExpandedRowKeys([]);
            setAllExpanded(false);
        } else {
            // 展开所有
            const getAllKeys = (menus: MenuItem[]): React.Key[] => {
                let keys: React.Key[] = [];
                menus.forEach((menu) => {
                    if (menu.children && menu.children.length > 0) {
                        keys.push(menu.menuId);
                        keys = keys.concat(getAllKeys(menu.children));
                    }
                });
                return keys;
            };
            const allKeys = getAllKeys(menuTreeData.data);
            setExpandedRowKeys(allKeys);
            setAllExpanded(true);
        }
    };

    /**
     * 过滤树形数据
     * @param menus 菜单数组
     * @returns 过滤后的菜单数组
     */
    const filterTreeData = (menus: MenuItem[]): MenuItem[] => {
        return menus
            .filter((menu) => {
                // 根据搜索条件过滤
                if (searchParams.menuName && !menu.menuName.toLowerCase().includes(searchParams.menuName.toLowerCase())) {
                    // 如果子节点匹配，父节点也要显示
                    const hasMatchingChild = menu.children && filterTreeData(menu.children).length > 0;
                    return hasMatchingChild;
                }
                if (searchParams.status && menu.status !== searchParams.status) {
                    const hasMatchingChild = menu.children && filterTreeData(menu.children).length > 0;
                    return hasMatchingChild;
                }
                if (searchParams.menuType && menu.menuType !== searchParams.menuType) {
                    const hasMatchingChild = menu.children && filterTreeData(menu.children).length > 0;
                    return hasMatchingChild;
                }
                return true;
            })
            .map((menu) => ({
                ...menu,
                children: menu.children && menu.children.length > 0 ? filterTreeData(menu.children) : undefined,
            }));
    };

    /**
     * 表格列定义
     */
    const columns: ProColumns<MenuItem>[] = [
        {
            title: '菜单名称',
            dataIndex: 'menuName',
            width: 250,
            render: (text, record) => (
                <Space>
                    {getIconComponent(record.icon)}
                    <a 
                        onClick={() => {
                            setCurrentMenu(record);
                            setShowDetail(true);
                        }}
                    >
                        {text}
                    </a>
                </Space>
            )
        },
        {
            title: '图标',
            dataIndex: 'icon',
            width: 100,
            hideInSearch: true,
            hideInTable: true,
            render: (_, record) => getIconComponent(record.icon) || '-',
        },
        {
            title: '排序',
            dataIndex: 'orderNum',
            width: 80,
            hideInSearch: true,
            sorter: (a, b) => (a.orderNum || 0) - (b.orderNum || 0),
        },
        {
            title: '类型',
            dataIndex: 'menuType',
            width: 100,
            valueEnum: {
                'M': { text: '目录', status: 'Default' },
                'C': { text: '菜单', status: 'Processing' },
                'F': { text: '按钮', status: 'Warning' }
            }
        },
        {
            title: '权限标识',
            dataIndex: 'perms',
            width: 160,
            ellipsis: true,
        },
        {
            title: '组件路径',
            dataIndex: 'component',
            width: 160,
            hideInSearch: true,
            ellipsis: true,
        },
        {
            title: '状态',
            dataIndex: 'status',
            width: 120,
            align: 'center',
            valueEnum: {
                '0': { text: '正常', status: 'Success' },
                '1': { text: '停用', status: 'Error' }
            }
        },
        {
            title: '创建时间',
            dataIndex: 'createTime',
            width: 180,
            hideInSearch: true,
            hideInTable: true, // 隐藏创建时间列
            valueType: 'dateTime',
        },
        {
            title: '操作',
            valueType: 'option',
            width: 260,
            align: 'center',
            render: (_, record) => [
                <Button 
                    type="link" 
                    key="edit" 
                    size="small"
                    onClick={() => showEditModal(record)}
                >
                    <EditOutlined /> 修改
                </Button>,
                <Button 
                    type="link" 
                    key="add" 
                    size="small"
                    onClick={() => showEditModal(undefined, record.menuId)}
                >
                    <PlusOutlined /> 新增
                </Button>,
                <Button 
                    danger 
                    type="link" 
                    key="delete" 
                    size="small"
                    onClick={() => handleDelete(record.menuId)}
                >
                    <DeleteOutlined /> 删除
                </Button>
            ]
        }
    ];

    /**
     * 显示编辑/新增弹窗
     * @param record 要编辑的菜单项，如果为空则新增
     * @param parentId 父菜单ID，用于新增子菜单
     */
    const showEditModal = (record?: MenuItem, parentId?: number) => {
        if (record) {
            // 编辑模式：直接使用 record 数据填充表单
            setCurrentMenu(record);
            setIsEdit(true);
            setMenuType(record.menuType);
            form.setFieldsValue({
                menuId: record.menuId,
                parentId: record.parentId || 0,
                menuName: record.menuName,
                menuType: record.menuType,
                orderNum: record.orderNum,
                path: record.path,
                component: record.component,
                perms: record.perms,
                icon: record.icon,
                isFrame: record.isFrame || '1', // 默认选择"否"（不是外链）
                visible: record.visible || '0',
                status: record.status || '0',
                isCache: record.isCache || '0',
                query: record.query,
                routeName: record.routeName,
            });
        } else {
            // 新增模式
            setCurrentMenu(null);
            setIsEdit(false);
            setMenuType('C');
            form.resetFields();
            form.setFieldsValue({
                parentId: parentId || 0,
                menuType: 'C',
                orderNum: 0,
                isFrame: '1', // 默认选择"否"（不是外链）
                visible: '0',
                status: '0',
                isCache: '0',
            });
        }
        setModalVisible(true);
    };

    // 编辑状态
    const [isEdit, setIsEdit] = useState(false);

    /**
     * 表单提交处理
     */
    const handleSubmit = async () => {
        try {
            const values = await form.validateFields();
            
            // 根据菜单类型清理不需要的字段
            const submitData: MenuItem = {
                ...values,
                menuType,
                menuId: isEdit ? currentMenu?.menuId : undefined,
            };

            // 按钮类型：只保留必要的字段
            if (menuType === 'F') {
                submitData.path = undefined;
                submitData.component = undefined;
                submitData.routeName = undefined;
                submitData.query = undefined;
                submitData.isFrame = undefined;
                submitData.visible = undefined;
                submitData.isCache = undefined;
                submitData.icon = undefined;
            }
            // 目录类型：清空组件路径、路由名称、路由参数
            else if (menuType === 'M') {
                submitData.component = undefined;
                submitData.routeName = undefined;
                submitData.query = undefined;
            }
            // 菜单类型：保留所有字段

            let res;
            if (isEdit && currentMenu) {
                // 更新菜单
                res = await updateMenu({
                    ...submitData,
                    menuId: currentMenu.menuId,
                } as MenuItem);
            } else {
                // 创建菜单
                res = await createMenu(submitData);
            }

            if (res.code === 200) {
                message.success(isEdit ? '更新成功' : '新增成功');
                setModalVisible(false);
                refreshTree();
                actionRef.current?.reload();
            } else {
                message.error(res.msg || '操作失败');
            }
        } catch (error: any) {
            if (error.errorFields) {
                message.error('请检查表单填写是否正确');
            } else {
                message.error('操作失败');
            }
        }
    };

    /**
     * 删除菜单
     * @param id 菜单ID
     */
    const handleDelete = async (id: number) => {
        // 前端兜底：仅 UserId=1 允许删除
        if (!isSuperAdmin) {
            message.warning('您权限不足！');
            return;
        }

        Modal.confirm({
            title: '确认删除？',
            content: '删除后不可恢复，请确认是否继续。',
            okText: '删除',
            okButtonProps: { danger: true },
            cancelText: '取消',
            onOk: async () => {
                try {
                    const res = await deleteMenu([id]);
                    if ((res as any)?.code === 200) {
                        message.success('删除成功');
                        refreshTree();
                        actionRef.current?.reload();
                    } else {
                        message.error((res as any)?.msg || '删除失败');
                    }
                } catch (e: any) {
                    message.error(e?.message || '删除失败');
                }
            },
        });
    };

    const handleBatchDelete = async () => {
        if (!selectedRows.length) {
            message.warning('请选择要删除的项');
            return;
        }
        // 前端兜底：仅 UserId=1 允许批量删除
        if (!isSuperAdmin) {
            message.warning('您权限不足！');
            return;
        }

        const ids = selectedRows.map((row) => row.menuId);
        Modal.confirm({
            title: '确认批量删除？',
            content: `本次将删除 ${ids.length} 项，删除后不可恢复，请确认是否继续。`,
            okText: '删除',
            okButtonProps: { danger: true },
            cancelText: '取消',
            onOk: async () => {
                try {
                    const res = await deleteMenu(ids);
                    if ((res as any)?.code === 200) {
                        message.success('批量删除成功');
                        setSelectedRows([]);
                        refreshTree();
                        actionRef.current?.reload();
                    } else {
                        message.error((res as any)?.msg || '批量删除失败');
                    }
                } catch (e: any) {
                    message.error(e?.message || '批量删除失败');
                }
            },
        });
    };

    return (
        <PageContainer
            className="system-settings-page"
            header={{
                title: '菜单管理',
            }}
        >
            <ProTable<MenuItem>
                actionRef={actionRef}
                columns={columns}
                loading={treeLoading}
                cardProps={{
                    // 局部内联样式，避免被全局注入覆盖
                    style: (window as any).__panelStyles?.panelStyle,
                    headStyle: (window as any).__panelStyles?.headStyle,
                    bodyStyle: (window as any).__panelStyles?.bodyStyle,
                    bordered: false,
                    // 禁用全局自动样式注入，彻底隔离，防止抖动
                    ['data-panel-exempt']: 'true'
                } as any}
                request={async () => {
                    // 直接调用 getMenuTree 获取树形数据，确保每次都能获取最新数据
                    try {
                        const res = await getMenuTree();
                        if (res.code === 200 && res.data) {
                            // 过滤数据
                            const filteredData = filterTreeData(res.data);
                            
                            // 注意：展开/收起状态在 useRequest 的 onSuccess 中已经处理
                            // 这里不再重复设置，避免覆盖用户的手动操作
                            
                            return {
                                data: filteredData,
                                success: true,
                                total: filteredData.length,
                            };
                        }
                        return {
                            data: [],
                            success: false,
                            total: 0,
                        };
                    } catch (error) {
                        message.error('获取菜单数据失败');
                        return {
                            data: [],
                            success: false,
                            total: 0,
                        };
                    }
                }}
                rowKey="menuId"
                search={{
                    labelWidth: 120,
                    defaultCollapsed: true, // 默认收起搜索/筛选区域
                }}
                // 监听搜索表单变化
                form={{
                    onValuesChange: (changedValues: any) => {
                        setSearchParams({
                            ...searchParams,
                            ...changedValues,
                        });
                        // 延迟执行，等待状态更新
                        setTimeout(() => {
                            actionRef.current?.reload();
                        }, 0);
                    },
                }}
                // 取消强制横向滚动，自适应容器宽度
                // scroll={{ x: 'max-content' }}
                // 树形表格配置：移除分页，使用展开/折叠
                pagination={false}
                // 展开/折叠配置
                expandable={{
                    expandedRowKeys,
                    onExpandedRowsChange: (keys) => {
                        setExpandedRowKeys(keys as React.Key[]);
                        // 计算是否全部展开
                        if (!menuTreeData || !('code' in menuTreeData) || menuTreeData.code !== 200 || !('data' in menuTreeData) || !menuTreeData.data) {
                            return;
                        }
                        const getAllExpandableKeys = (menus: MenuItem[]): React.Key[] => {
                            let allKeys: React.Key[] = [];
                            menus.forEach((menu) => {
                                if (menu.children && menu.children.length > 0) {
                                    allKeys.push(menu.menuId);
                                    allKeys = allKeys.concat(getAllExpandableKeys(menu.children));
                                }
                            });
                            return allKeys;
                        };
                        const allExpandableKeys = getAllExpandableKeys(menuTreeData.data);
                        setAllExpanded(keys.length > 0 && keys.length >= allExpandableKeys.length);
                    },
                    indentSize: 24, // 子级缩进大小
                    // 自定义展开/收起图标
                    expandIcon: ({ expanded, onExpand, record }) => {
                        // 只有有子菜单的项才显示展开图标
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
                rowSelection={{
                    onChange: (_, selectedRows) => {
                        setSelectedRows(selectedRows);
                    },
                }}
                toolBarRender={() => [
                    <Button
                        type="primary"
                        key="add"
                        icon={<PlusOutlined />}
                        onClick={() => showEditModal()}
                    >
                        新增
                    </Button>,
                    <Button
                        type="primary"
                        key="expand"
                        icon={allExpanded ? <RightOutlined /> : <DownOutlined />}
                        onClick={handleExpandAll}
                    >
                        {allExpanded ? '折叠菜单' : '展开菜单'}
                    </Button>,
                ]}
            />

            {selectedRows.length > 0 && (
                <FooterToolbar
                    extra={
                        <div>
                            已选 <a style={{ fontWeight: 600 }}>{selectedRows.length}</a> 项
                        </div>
                    }
                >
                    <Button
                        danger
                        icon={<DeleteOutlined />}
                        onClick={handleBatchDelete}
                    >
                        批量删除
                    </Button>
                </FooterToolbar>
            )}

            <Modal
                title={
                    <Space>
                        {currentMenu ? '编辑菜单' : '添加菜单'}
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
                className="menu-edit-modal"
                rootClassName="menu-edit-modal"
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

                    {/* 上级菜单 - 所有类型都显示 */}
                    <Form.Item
                        name="parentId"
                        label="上级菜单"
                        rules={[{ required: true, message: '请选择上级菜单' }]}
                    >
                        <TreeSelect
                            placeholder="请选择上级菜单"
                            treeData={
                                menuTree && 'code' in menuTree && menuTree.code === 200 && 'data' in menuTree && menuTree.data
                                    ? [
                                          { title: '主类目', value: 0, key: 0 },
                                          ...(menuTree.data.map((menu) => ({
                                              title: menu.menuName,
                                              value: menu.menuId,
                                              key: menu.menuId,
                                              children: menu.children
                                                  ? menu.children.map((child) => ({
                                                        title: child.menuName,
                                                        value: child.menuId,
                                                        key: child.menuId,
                                                    }))
                                                  : undefined,
                                          })) as any),
                                      ]
                                    : [{ title: '主类目', value: 0, key: 0 }]
                            }
                            allowClear
                            showSearch
                            treeNodeFilterProp="title"
                            popupClassName="menu-edit-modal-dropdown"
                            dropdownStyle={{
                              background: '#ffffff',
                              border: '1px solid #f0f0f0',
                              boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
                            }}
                        />
                    </Form.Item>

                    {/* 菜单类型 - 所有类型都显示 */}
                    <Form.Item
                        name="menuType"
                        label="菜单类型"
                        rules={[{ required: true, message: '请选择菜单类型' }]}
                    >
                        <Radio.Group
                            value={menuType}
                            onChange={(e) => {
                                setMenuType(e.target.value);
                                form.setFieldsValue({ menuType: e.target.value });
                                // 切换类型时清空相关字段
                                if (e.target.value === 'F') {
                                    form.setFieldsValue({ 
                                        path: undefined, 
                                        component: undefined,
                                        routeName: undefined,
                                        query: undefined,
                                        isFrame: undefined,
                                        visible: undefined,
                                        isCache: undefined,
                                        icon: undefined,
                                    });
                                } else if (e.target.value === 'M') {
                                    // 目录类型：清空组件路径、路由名称、路由参数
                                    form.setFieldsValue({ 
                                        component: undefined,
                                        routeName: undefined,
                                        query: undefined,
                                    });
                                }
                            }}
                        >
                            <Radio value="M">目录</Radio>
                            <Radio value="C">菜单</Radio>
                            <Radio value="F">按钮</Radio>
                        </Radio.Group>
                    </Form.Item>

                    {/* 菜单图标 - 目录和菜单显示，按钮不显示 */}
                    {menuType !== 'F' && (
                        <Form.Item
                            name="icon"
                            label="菜单图标"
                        >
                            <Input
                                placeholder="点击选择图标"
                                addonAfter={<Button type="link">选择</Button>}
                            />
                        </Form.Item>
                    )}

                    {/* 显示排序 - 所有类型都显示 */}
                    <Form.Item
                        name="orderNum"
                        label="显示排序"
                        rules={[{ required: true, message: '请输入排序号' }]}
                    >
                        <InputNumber min={0} style={{ width: '100%' }} />
                    </Form.Item>

                    {/* 菜单名称 - 所有类型都显示 */}
                    <Form.Item
                        name="menuName"
                        label="菜单名称"
                        rules={[{ required: true, message: '请输入菜单名称' }]}
                    >
                        <Input placeholder="请输入菜单名称" autoComplete="off" name="menuName_no_autofill" autoCorrect="off" autoCapitalize="off" spellCheck={false} />
                    </Form.Item>

                    {/* 是否外链 - 目录和菜单显示，按钮不显示 */}
                    {menuType !== 'F' && (
                        <Form.Item
                            name="isFrame"
                            label="是否外链"
                            initialValue="1"
                        >
                            <Radio.Group>
                                <Radio value="0">是</Radio>
                                <Radio value="1">否</Radio>
                            </Radio.Group>
                        </Form.Item>
                    )}

                    {/* 路由地址 - 目录和菜单显示，按钮不显示 */}
                    {menuType !== 'F' && (
                        <Form.Item
                            name="path"
                            label="路由地址"
                            rules={[
                                { required: menuType === 'C', message: '请输入路由地址' },
                                { max: 200, message: '路由地址不能超过200个字符' },
                            ]}
                        >
                            <Input placeholder="请输入路由地址" />
                        </Form.Item>
                    )}

                    {/* 组件路径 - 仅菜单类型显示 */}
                    {menuType === 'C' && (
                        <Form.Item
                            name="component"
                            label="组件路径"
                            rules={[
                                { required: true, message: '请输入组件路径' },
                                { max: 200, message: '组件路径不能超过200个字符' },
                            ]}
                        >
                            <Input placeholder="请输入组件路径" />
                        </Form.Item>
                    )}

                    {/* 路由名称 - 仅菜单类型显示 */}
                    {menuType === 'C' && (
                        <Form.Item
                            name="routeName"
                            label="路由名称"
                            rules={[{ max: 100, message: '路由名称不能超过100个字符' }]}
                        >
                            <Input placeholder="请输入路由名称" />
                        </Form.Item>
                    )}

                    {/* 路由参数 - 仅菜单类型显示 */}
                    {menuType === 'C' && (
                        <Form.Item
                            name="query"
                            label="路由参数"
                            rules={[{ max: 200, message: '路由参数不能超过200个字符' }]}
                        >
                            <Input placeholder="请输入路由参数" />
                        </Form.Item>
                    )}

                    {/* 权限标识 - 菜单和按钮显示，目录不显示 */}
                    {(menuType === 'C' || menuType === 'F') && (
                        <Form.Item
                            name="perms"
                            label={menuType === 'F' ? '权限字符' : '权限标识'}
                            rules={[{ max: 100, message: '权限标识不能超过100个字符' }]}
                        >
                            <Input placeholder={menuType === 'F' ? '请输入权限标识' : '请输入权限标识，如：system:user:list'} />
                        </Form.Item>
                    )}

                    {/* 是否缓存 - 仅菜单类型显示 */}
                    {menuType === 'C' && (
                        <Form.Item
                            name="isCache"
                            label="是否缓存"
                            initialValue="0"
                        >
                            <Radio.Group>
                                <Radio value="0">缓存</Radio>
                                <Radio value="1">不缓存</Radio>
                            </Radio.Group>
                        </Form.Item>
                    )}

                    {/* 显示状态 - 目录和菜单显示，按钮不显示 */}
                    {menuType !== 'F' && (
                        <Form.Item
                            name="visible"
                            label="显示状态"
                            initialValue="0"
                        >
                            <Radio.Group>
                                <Radio value="0">显示</Radio>
                                <Radio value="1">隐藏</Radio>
                            </Radio.Group>
                        </Form.Item>
                    )}

                    {/* 菜单状态 - 所有类型都显示 */}
                    <Form.Item
                        name="status"
                        label="菜单状态"
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
                className="menu-info-drawer"
                rootClassName="menu-info-drawer"
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
                {currentMenu?.menuName && (
                    <ProDescriptions<MenuItem>
                        column={2}
                        title={currentMenu?.menuName}
                        dataSource={currentMenu}
                        columns={columns as ProDescriptionsItemProps<MenuItem>[]}
                    />
                )}
            </Drawer>
        </PageContainer>
    );
};

export default MenuManagement;