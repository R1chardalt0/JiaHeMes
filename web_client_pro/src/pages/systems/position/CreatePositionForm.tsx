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

