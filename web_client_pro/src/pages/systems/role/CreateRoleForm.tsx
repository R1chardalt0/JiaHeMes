import React, { useState, useEffect } from "react";
import { Form, Button, Tree, Card, Space, message, Spin } from "antd";
import {
  ModalForm,
  ProFormText,
  ProFormDigit,
  ProFormRadio,
  ProFormTextArea,
} from "@ant-design/pro-components";
import type { BasicDataNode, TreeProps } from "antd/es/tree";
import { getMenuTree } from "@/services/Api/Systems/menu"; // 按你项目路径调整
import { MenuItem as ApiMenuItem } from "@/services/Model/Systems/menu";

interface TreeMenuItem extends BasicDataNode {
  title: string;
  key: string;
  children?: TreeMenuItem[];
}

interface CreateRoleFormProps {
  open: boolean;
  onOpenChange: (visible: boolean) => void;
  currentRow?: any;
  checkedKeys: { checked: React.Key[]; halfChecked: React.Key[] };
  onCheckedChange: (keys: {
    checked: React.Key[];
    halfChecked: React.Key[];
  }) => void;
  onFinish: (values: any) => Promise<boolean | void>;
}

const CreateRoleForm: React.FC<CreateRoleFormProps> = ({
  open,
  onOpenChange,
  currentRow,
  checkedKeys,
  onCheckedChange,
  onFinish,
}) => {
  const [form] = Form.useForm();
  const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([]);
  const [autoExpandParent, setAutoExpandParent] = useState(true);
  const [menuData, setMenuData] = useState<TreeMenuItem[]>([]);
  const [loading, setLoading] = useState(false);

  // 递归把 API 菜单结构转成 Tree 需要的结构
  function transformMenuData(data: ApiMenuItem[]): TreeMenuItem[] {
    return data.map((item) => ({
      title: item.menuName,
      key: String(item.menuId),
      children: item.children ? transformMenuData(item.children) : [],
    }));
  }

  // 获取菜单树
  const fetchMenuTree = async () => {
    setLoading(true);
    try {
      const res = await getMenuTree();
      if (res.code === 200 && res.data) {
        const tree = transformMenuData(res.data);
        setMenuData(tree);
      } else {
        message.error(res.msg || "获取菜单失败");
      }
    } catch (err) {
      message.error("获取菜单失败");
    } finally {
      setLoading(false);
    }
  };

  // 处理菜单选中
  const onCheck: TreeProps<TreeMenuItem>["onCheck"] = (checked, info) => {
    onCheckedChange({
      checked: Array.isArray(checked) ? checked : checked.checked,
      halfChecked: info.halfCheckedKeys || [],
    });
  };

  // 展开/折叠所有
  const toggleExpand = () => {
    if (expandedKeys.length > 0) {
      setExpandedKeys([]);
    } else {
      const allKeys = getAllKeys(menuData);
      setExpandedKeys(allKeys);
    }
    setAutoExpandParent(true);
  };

  // 全选
  const selectAll = () => {
    onCheckedChange({ checked: getAllKeys(menuData), halfChecked: [] });
  };

  // 全不选
  const unselectAll = () => {
    onCheckedChange({ checked: [], halfChecked: [] });
  };

  // 递归获取所有 key
  function getAllKeys(data: TreeMenuItem[]): string[] {
    return data.flatMap((item) => [
      item.key,
      ...(item.children ? getAllKeys(item.children) : []),
    ]);
  }

  // 打开弹窗时加载菜单
  useEffect(() => {
    if (open) {
      fetchMenuTree();
      if (currentRow) {
        // 编辑模式
        form.setFieldsValue({
          ...currentRow,
          roleSort: Number(currentRow.roleSort) || 1,
        });
      } else {
        // 新增模式
        form.resetFields();
        form.setFieldsValue({ roleSort: 1, status: "1" });
      }
    } else {
      form.resetFields();
    }
  }, [open, currentRow]);

  return (
    <ModalForm
      title={currentRow ? "编辑角色" : "新增角色"}
      form={form}
      open={open}
      onOpenChange={onOpenChange}
      width={800}
      onFinish={onFinish}
      modalProps={{
        destroyOnClose: true,
        maskClosable: false,
        className: 'role-edit-modal',
        rootClassName: 'role-edit-modal',
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
        }
      }}
    >
      <ProFormText
        name="roleName"
        label="角色名称"
        rules={[{ required: true, message: "请输入角色名称" }]}
      />
      <ProFormText
        name="roleKey"
        label="角色标识"
        rules={[{ required: true, message: "请输入角色标识" }]}
      />
      <ProFormDigit
        name="roleSort"
        label="显示顺序"
        min={0}
        max={999}
        initialValue={1}
      />
      <ProFormRadio.Group
        name="status"
        label="状态"
        options={[
          { label: "启用", value: "1" },
          { label: "禁用", value: "0" },
        ]}
        initialValue="1"
      />
      <ProFormTextArea name="remark" label="备注" />

      <Card
        title="菜单权限"
        extra={
          <Space>
            <Button size="small" onClick={toggleExpand}>
              {expandedKeys.length > 0 ? "折叠" : "展开"}
            </Button>
            <Button size="small" onClick={selectAll}>
              全选
            </Button>
            <Button size="small" onClick={unselectAll}>
              全不选
            </Button>
          </Space>
        }
      >
        <Spin spinning={loading}>
          <Tree
            checkable
            selectable={false}
            blockNode
            onExpand={setExpandedKeys}
            expandedKeys={expandedKeys}
            autoExpandParent={autoExpandParent}
            onCheck={onCheck}
            checkedKeys={checkedKeys}
            treeData={menuData}
          />
        </Spin>
      </Card>
    </ModalForm>
  );
};

export default CreateRoleForm;
