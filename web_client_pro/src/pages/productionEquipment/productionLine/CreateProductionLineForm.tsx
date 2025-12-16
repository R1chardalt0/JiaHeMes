import React, { useEffect, useMemo, useCallback } from 'react';
import { Form, message } from 'antd';
import { ModalForm, ProForm, ProFormText, ProFormSelect, ProFormTextArea } from '@ant-design/pro-components';
import type { CompanyItem } from '@/services/Model/Systems/company';

/**
 * 创建/编辑产线表单组件
 */
interface CreateProductionLineFormProps {
  open: boolean;
  onOpenChange: (visible: boolean) => void;
  currentRow?: any;
  onFinish: (values: any) => Promise<boolean | void>;
  companyId?: string;
  companies?: CompanyItem[]; // 从父组件传递公司列表，避免重复加载
}

const CreateProductionLineForm: React.FC<CreateProductionLineFormProps> = ({
  open,
  onOpenChange,
  currentRow,
  onFinish,
  companyId,
  companies = [], // 从父组件传递，默认为空数组
}) => {
  const [form] = Form.useForm();
  
  // 使用 useMemo 缓存模态框宽度计算
  const modalWidth = useMemo(() => {
    const windowWidth = window.innerWidth;
    if (windowWidth < 576) return '90%'; // 移动端占90%宽度
    if (windowWidth < 768) return 500;
    return 600;
  }, []);
  
  // 使用 useMemo 优化公司选项的生成，避免每次渲染都重新计算
  const companyOptions = useMemo(() => {
    return companies.map((company) => ({
      value: company.companyId,
      label: company.companyName,
    }));
  }, [companies]);
  
  // 表单提交处理函数（不使用 useCallback，避免依赖问题导致卡顿）
  const handleFinish = async (values: any) => {
    // 根据最新的API要求，直接传递表单数据，不再包装在productionLine字段中
    // 添加当前时间作为创建时间，解决1901-01-01 00:00:00的问题
    const currentTime = new Date().toISOString();
    
    // 优先使用表单中选择的companyId，如果没有则使用props传入的companyId
    const finalCompanyId = values.companyId || companyId;
    
    if (!finalCompanyId) {
      message.error('请选择公司');
      return false;
    }
    
    const submitValues = {
      ...values,
      companyId: finalCompanyId,
      // 注意：ProductionLine接口中status被定义为number类型，需要转换
      status: parseInt(values.status, 10).toString(), // 将字符串状态转换为数字
      createdAt: currentTime, // 新增模式时传递当前时间
      updatedAt: currentTime  // 同时更新修改时间
    };
    
    try {
      return await onFinish(submitValues);
    } catch (error) {
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
        // 编辑模式：设置表单初始值，包括companyId
        form.setFieldsValue({
          ...currentRow,
          status: currentRow.status?.toString() || '1',
          companyId: currentRow.companyId || (companyId ? Number(companyId) : undefined),
        });
      } else {
        // 新增模式：如果有传入的companyId，则设置为默认值
        form.resetFields();
        form.setFieldsValue({ 
          status: '1',
          companyId: companyId ? Number(companyId) : undefined,
        });
      }
    }, 0);

    return () => {
      clearTimeout(timer);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, currentRow?.productionLineId, companyId]); // 只依赖关键字段，避免频繁更新

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
        name="companyId"
        label="所属公司"
        placeholder="请选择公司"
        rules={[
          {
            required: true,
            message: '请选择所属公司',
          },
        ]}
        options={companyOptions}
        fieldProps={{
          showSearch: true,
          filterOption: (input, option) =>
            (option?.label ?? '').toString().toLowerCase().includes(input.toLowerCase()),
          popupClassName: 'glass-dropdown',
          dropdownStyle: {
            background:
              'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
            backdropFilter: 'blur(12px) saturate(115%)',
            WebkitBackdropFilter: 'blur(12px) saturate(115%)',
            border: '1px solid rgba(72,115,255,0.28)',
            boxShadow:
              '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
          }
        }}
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
          popupClassName: 'glass-dropdown',
          dropdownStyle: {
            background:
              'radial-gradient(120% 120% at 0% 0%, rgba(54,78,148,0.16) 0%, rgba(10,18,35,0) 60%), linear-gradient(180deg, rgba(7,16,35,0.52) 0%, rgba(7,16,35,0.34) 100%)',
            backdropFilter: 'blur(12px) saturate(115%)',
            WebkitBackdropFilter: 'blur(12px) saturate(115%)',
            border: '1px solid rgba(72,115,255,0.28)',
            boxShadow:
              '0 0 0 1px rgba(72,115,255,0.12) inset, 0 12px 40px rgba(10,16,32,0.55), 0 0 20px rgba(64,196,255,0.16)'
          }
        }}
      />
      
      <ProFormTextArea 
        name="description"
        label="备注信息"
        placeholder="请输入备注信息（选填），最多200个字符"
        rows={3}
           rules={[
          {
            required: true,
            message: '请输入备注信息',
          },
        ]}
        maxLength={200}
        showCount={{}}
      />
    </ModalForm>
  );
};

export default CreateProductionLineForm;