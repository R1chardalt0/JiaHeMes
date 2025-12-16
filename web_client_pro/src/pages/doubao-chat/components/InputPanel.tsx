/**
 * 输入面板组件
 * 包含输入框、发送按钮、快捷操作等
 */

import { SendOutlined, ClearOutlined } from '@ant-design/icons';
import { Button, Input, Space, message } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import styles from './InputPanel.less';

const { TextArea } = Input;

interface InputPanelProps {
  /** 是否正在加载 */
  loading?: boolean;
  /** 发送消息回调 */
  onSend: (content: string) => void;
  /** 清空消息回调 */
  onClear?: () => void;
  /** 占位符文本 */
  placeholder?: string;
  /** 是否禁用输入 */
  disabled?: boolean;
}

/**
 * 输入面板组件
 */
const InputPanel: React.FC<InputPanelProps> = ({
  loading = false,
  onSend,
  onClear,
  placeholder = '输入你的问题或需求...（支持Shift+Enter换行，Enter发送）',
  disabled = false,
}) => {
  const [inputValue, setInputValue] = useState('');
  const textAreaRef = useRef<any>(null);

  /**
   * 处理发送消息
   */
  const handleSend = () => {
    const trimmedValue = inputValue.trim();
    if (!trimmedValue) {
      message.warning('请输入内容');
      return;
    }

    if (loading) {
      message.warning('请等待上一条消息完成');
      return;
    }

    onSend(trimmedValue);
    setInputValue('');
    
    // 聚焦输入框
    setTimeout(() => {
      textAreaRef.current?.focus();
    }, 100);
  };

  /**
   * 处理键盘事件
   */
  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    // Enter发送，Shift+Enter换行
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  /**
   * 处理清空
   */
  const handleClear = () => {
    if (loading) {
      message.warning('请等待当前请求完成');
      return;
    }
    onClear?.();
  };

  // 自动调整输入框高度
  useEffect(() => {
    if (textAreaRef.current?.resizableTextArea?.textArea) {
      const textArea = textAreaRef.current.resizableTextArea.textArea;
      textArea.style.height = 'auto';
      textArea.style.height = `${Math.min(textArea.scrollHeight, 200)}px`;
    }
  }, [inputValue]);

  return (
    <div className={styles.inputPanel}>
      <div className={styles.inputContainer}>
        <TextArea
          ref={textAreaRef}
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          disabled={disabled || loading}
          autoSize={{ minRows: 1, maxRows: 6 }}
          className={styles.textArea}
          style={{
            backgroundColor: 'rgba(255, 255, 255, 0.05)',
            borderColor: 'rgba(255, 255, 255, 0.2)',
            color: 'rgba(240, 240, 240, 0.9)',
          }}
        />
        <Space className={styles.buttonGroup} size="small">
          {onClear && (
            <Button
              icon={<ClearOutlined />}
              onClick={handleClear}
              disabled={disabled || loading}
              className={styles.clearButton}
            >
              清空
            </Button>
          )}
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSend}
            loading={loading}
            disabled={disabled || !inputValue.trim()}
            className={styles.sendButton}
          >
            发送
          </Button>
        </Space>
      </div>
      <div className={styles.tips}>
        <span>💡 提示：按 Enter 发送，Shift + Enter 换行</span>
      </div>
    </div>
  );
};

export default InputPanel;

