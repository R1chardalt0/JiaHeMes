/**
 * 消息项组件
 * 用于显示单条聊天消息（用户/AI）
 */

import { CopyOutlined, CheckOutlined, UserOutlined, RobotOutlined } from '@ant-design/icons';
import { Button, Card, message, Typography } from 'antd';
import React, { useState } from 'react';
import type { ChatMessage } from '@/services/Model/Doubao/typings';
import styles from './MessageItem.less';

const { Paragraph } = Typography;

interface MessageItemProps {
  message: ChatMessage;
}

/**
 * 消息项组件
 */
const MessageItem: React.FC<MessageItemProps> = ({ message: msg }) => {
  const [copied, setCopied] = useState(false);

  /**
   * 复制消息内容到剪贴板
   */
  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(msg.content);
      setCopied(true);
      message.success('已复制到剪贴板');
      setTimeout(() => setCopied(false), 2000);
    } catch (error) {
      message.error('复制失败，请手动复制');
    }
  };

  const isUser = msg.role === 'user';
  const isAssistant = msg.role === 'assistant';

  return (
    <div className={`${styles.messageItem} ${isUser ? styles.userMessage : styles.assistantMessage}`}>
      <Card
        className={styles.messageCard}
        size="small"
        style={{
          backgroundColor: isUser ? 'rgba(24, 144, 255, 0.1)' : 'rgba(255, 255, 255, 0.05)',
          borderColor: isUser ? 'rgba(24, 144, 255, 0.3)' : 'rgba(255, 255, 255, 0.1)',
        }}
      >
        <div className={styles.messageHeader}>
          <div className={styles.messageRole}>
            {isUser ? (
              <UserOutlined className={styles.userIcon} />
            ) : (
              <RobotOutlined className={styles.assistantIcon} />
            )}
            <span className={styles.roleText}>
              {isUser ? '我' : 'AI助手'}
            </span>
          </div>
          {isAssistant && msg.content && (
            <Button
              type="text"
              size="small"
              icon={copied ? <CheckOutlined /> : <CopyOutlined />}
              onClick={handleCopy}
              className={styles.copyButton}
            >
              {copied ? '已复制' : '复制'}
            </Button>
          )}
        </div>

        <div className={styles.messageContent}>
          {msg.loading ? (
            <div className={styles.loadingContainer}>
              <span className={styles.loadingDot}>●</span>
              <span className={styles.loadingDot}>●</span>
              <span className={styles.loadingDot}>●</span>
            </div>
          ) : msg.error ? (
            <div className={styles.errorContainer}>
              <Typography.Text type="danger">{msg.error}</Typography.Text>
            </div>
          ) : (
            <Paragraph
              className={styles.messageText}
              style={{ margin: 0, color: 'rgba(240, 240, 240, 0.9)' }}
              copyable={false}
            >
              {msg.content || '（空消息）'}
            </Paragraph>
          )}
        </div>
      </Card>
    </div>
  );
};

export default MessageItem;

