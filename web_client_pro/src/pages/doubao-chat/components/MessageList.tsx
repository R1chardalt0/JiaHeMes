/**
 * 消息列表组件
 * 用于显示所有聊天消息
 */

import { Empty } from 'antd';
import React, { useEffect, useRef } from 'react';
import type { ChatMessage } from '@/services/Model/Doubao/typings';
import MessageItem from './MessageItem';
import styles from './MessageList.less';

interface MessageListProps {
  messages: ChatMessage[];
}

/**
 * 消息列表组件
 */
const MessageList: React.FC<MessageListProps> = ({ messages }) => {
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  /**
   * 自动滚动到底部
   */
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  // 当消息列表更新时，自动滚动到底部
  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  if (messages.length === 0) {
    return (
      <div className={styles.emptyContainer}>
        <Empty
          description="还没有消息，开始对话吧~"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      </div>
    );
  }

  return (
    <div ref={containerRef} className={styles.messageList}>
      {messages.map((message) => (
        <MessageItem key={message.id} message={message} />
      ))}
      <div ref={messagesEndRef} />
    </div>
  );
};

export default MessageList;

