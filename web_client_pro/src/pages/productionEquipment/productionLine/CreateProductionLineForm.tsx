import React, { useEffect, useMemo } from 'react';
import { Form, message } from 'antd';
import { ModalForm, ProFormText, ProFormSelect, ProFormTextArea } from '@ant-design/pro-components';

/**
 * 创建/编辑产线表单组件
 */
interface CreateProductionLineFormProps {
  open: boolean;
  onOpenChange: (visible: boolean) => void;
  currentRow?: any;
  onFinish: (values: any) => Promise<boolean | void>;
}

const CreateProductionLineForm: React.FC<CreateProductionLineFormProps> = ({
  open,
  onOpenChange,
  currentRow,
  onFinish,
}) => {
  const [form] = Form.useForm();
  
  // 使用 useMemo 缓存模态框宽度计算
  const modalWidth = useMemo(() => {
    const windowWidth = window.innerWidth;
    if (windowWidth < 576) return '90%'; // 移动端占90%宽度
    if (windowWidth < 768) return 500;
    return 600;
  }, []);
  
  // 表单提交处理函数（不使用 useCallback，避免依赖问题导致卡顿）
  const handleFinish = async (values: any) => {
    // 根据最新的API要求，直接传递表单数据，不再包装在productionLine字段中
    // 添加当前时间作为创建时间，解决1901-01-01 00:00:00的问题
    const currentTime = new Date().toISOString();
    
    // 构建提交数据，使用 camelCase（ASP.NET Core 会自动转换为 PascalCase）
    const submitValues = {
      productionLineName: values.productionLineName,
      productionLineCode: values.productionLineCode,
      status: values.status, // Status 在后端是 string 类型，直接使用
      description: values.description || '', // 描述字段
      createdAt: currentTime, // 新增模式时传递当前时间
      updatedAt: currentTime  // 同时更新修改时间
    };
    
    try {
      return await onFinish(submitValues);
    } catch (error) {
      console.error('表单提交错误:', error);
      message.error('提交失败，请重试');
      return false;
    }
  };
  
  // 打开弹窗时设置初始值（优化依赖项，避免不必要的重新执行）
  useEffect(() => {
    if (!open) {
      form.resetFields();
      return;
    }

    // 使用 setTimeout 确保在弹窗完全打开后再设置值，避免卡顿
    const timer = setTimeout(() => {
      if (currentRow) {
        // 编辑模式：设置表单初始值
        form.setFieldsValue({
          ...currentRow,
          status: currentRow.status?.toString() || '1',
        });
      } else {
        // 新增模式：设置默认值
        form.resetFields();
        form.setFieldsValue({ 
          status: '1',
        });
      }
    }, 0);

    return () => {
      clearTimeout(timer);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, currentRow?.productionLineId]); // 只依赖关键字段，避免频繁更新

  return (
    <ModalForm
      title={currentRow ? '编辑产线' : '新增产线'}
      form={form}
      open={open}
      onOpenChange={onOpenChange}
      width={modalWidth}
      onFinish={handleFinish}
      submitter={{
        resetButtonProps: { hidden: true },
      }}
      modalProps={{
        destroyOnClose: true,
        maskClosable: false,
        className: 'production-line-edit-modal',
        rootClassName: 'production-line-edit-modal',
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
        }
      }}
    >
      <ProFormText
        name="productionLineName"
        label="产线名称"
        placeholder="请输入产线名称"
        rules={[
          {
            required: true,
            message: '请输入产线名称',
          },
          {
            max: 50,
            message: '产线名称不能超过50个字符',
          },
        ]}
      />
      <ProFormText
        name="productionLineCode"
        label="产线编号"
        placeholder="请输入产线编号"
        rules={[
          {
            required: true,
            message: '请输入产线编号',
          },
          {
            max: 10,
            message: '产线编号不能超过10个字符',
          },
          {
            pattern: /^[A-Za-z0-9_-]+$/,
            message: '产线编号只能包含字母、数字、下划线和横线',
          },
        ]}
      />

      <ProFormSelect
        name="status"
        label="状态"
        placeholder="请选择状态"
        rules={[
          {
            required: true,
            message: '请选择产线状态',
          },
        ]}
        options={[
          {
            value: '1',
            label: '启用',
          },
          {
            value: '0',
            label: '禁用',
          },
        ]}
        initialValue="1"
        fieldProps={{
          popupClassName: 'production-line-edit-modal-dropdown',
          dropdownStyle: {
            background: '#ffffff',
            border: '1px solid #f0f0f0',
            boxShadow: '0 4px 16px rgba(0,0,0,0.1)'
          }
        }}
      />
      
      <ProFormTextArea 
        name="description"
        label="备注信息"
        placeholder="请输入备注信息（选填），最多200个字符"
        rows={3}
        maxLength={200}
        showCount
      />
    </ModalForm>
  );
};

export default CreateProductionLineForm;