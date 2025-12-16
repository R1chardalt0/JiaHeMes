import React, { useEffect } from 'react';
import { Form } from 'antd';
import {
  ModalForm,
  ProFormText,
  ProFormDigit,
  ProFormRadio,
  ProFormTextArea,
} from '@ant-design/pro-components';
import type { PositionItem } from '@/services/Model/Systems/position';

interface CreatePositionFormProps {
  open: boolean;
  onOpenChange: (visible: boolean) => void;
  currentRow?: PositionItem;
  onFinish: (values: any) => Promise<boolean | void>;
  loading?: boolean;
}

const CreatePositionForm: React.FC<CreatePositionFormProps> = ({
  open,
  onOpenChange,
  currentRow,
  onFinish,
  loading,
}) => {
  const [form] = Form.useForm();

  useEffect(() => {
    if (open) {
      if (currentRow) {
        // 编辑模式
        form.setFieldsValue({
          ...currentRow,
          postSort: Number(currentRow.postSort) || 1,
        });
      } else {
        // 新增模式
        form.resetFields();
        form.setFieldsValue({ postSort: 1, status: '0' });
      }
    } else {
      form.resetFields();
    }
  }, [open, currentRow, form]);

  return (
    <ModalForm
      title={currentRow ? '编辑岗位' : '新增岗位'}
      form={form}
      open={open}
      onOpenChange={onOpenChange}
      width={600}
      onFinish={onFinish}
      modalProps={{
        destroyOnClose: true,
        maskClosable: false,
        className: 'position-edit-modal',
        rootClassName: 'position-edit-modal',
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
      submitter={{
        submitButtonProps: {
          loading,
        }
      }}
    >
      <ProFormText
        name="postName"
        label="岗位名称"
        rules={[{ required: true, message: '请输入岗位名称' }]}
        placeholder="请输入岗位名称"
      />
      <ProFormText
        name="postCode"
        label="岗位编码"
        rules={[{ required: true, message: '请输入岗位编码' }]}
        placeholder="请输入岗位编码"
      />
      <ProFormDigit
        name="postSort"
        label="显示顺序"
        min={0}
        max={999}
        initialValue={1}
        placeholder="请输入显示顺序"
      />
      <ProFormRadio.Group
        name="status"
        label="状态"
        options={[
          { label: '启用', value: '0' },
          { label: '停用', value: '1' },
        ]}
        initialValue="0"
      />
      <ProFormTextArea 
        name="remark" 
        label="备注" 
        placeholder="请输入备注信息"
      />
    </ModalForm>
  );
};

export default CreatePositionForm;

