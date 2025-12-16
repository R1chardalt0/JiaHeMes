/**
 * 豆包AI智能问答/文案生成页面
 * 支持流式返回，实现打字机效果
 */

import { message } from 'antd';
import React, { useState, useCallback, useRef } from 'react';
import { PageContainer } from '@ant-design/pro-components';
import { chatWithDoubaoStream, getErrorMessage } from '@/services/Api/Doubao/doubaoService';
import type { ChatMessage, ChatMode } from '@/services/Model/Doubao/typings';
import InputPanel from './components/InputPanel';
import MessageList from './components/MessageList';
import styles from './index.less';

interface DoubaoChatProps {
  /** 页面标题 */
  title?: string;
  /** 聊天模式：chat-智能问答，writing-文案生成 */
  mode?: ChatMode;
  /** 模型端点ID（从字节跳动控制台获取） */
  model?: string;
}

/**
 * 生成唯一ID
 */
const generateId = (): string => {
  return `msg_${Date.now()}_${Math.random().toString(36).slice(2, 11)}`;
};

/**
 * 豆包AI聊天页面主组件
 */
const DoubaoChat: React.FC<DoubaoChatProps> = ({
  title = '智能问答',
  mode = 'chat',
  model, // 如果未传入，则从环境变量或默认值获取
}) => {
  // 从环境变量或默认值获取模型端点ID
  // 优先级：props > 环境变量 > 默认值
  // 注意：UmiJS 中环境变量需要通过 define 配置，运行时通过 process.env 访问
  // 默认模型端点ID：doubao-seed-1-6-251015（与后端配置保持一致）
  const defaultModel = 
    model ||
    (typeof process !== 'undefined' && process.env?.REACT_APP_DOUBAO_MODEL) ||
    'doubao-seed-1-6-251015'; // 默认模型，与后端 appsettings.json 中的 DefaultModel 保持一致
  
  const finalModel = defaultModel;
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  /**
   * 添加消息
   */
  const addMessage = useCallback((message: ChatMessage) => {
    setMessages((prev) => [...prev, message]);
  }, []);

  /**
   * 更新最后一条消息
   */
  const updateLastMessage = useCallback((updater: (msg: ChatMessage) => ChatMessage) => {
    setMessages((prev) => {
      if (prev.length === 0) return prev;
      const newMessages = [...prev];
      newMessages[newMessages.length - 1] = updater(newMessages[newMessages.length - 1]);
      return newMessages;
    });
  }, []);

  /**
   * 发送消息
   */
  const handleSend = useCallback(
    async (content: string) => {
      if (loading) {
        message.warning('请等待上一条消息完成');
        return;
      }

      // 添加用户消息
      const userMessage: ChatMessage = {
        id: generateId(),
        role: 'user',
        content,
        timestamp: Date.now(),
      };
      addMessage(userMessage);

      // 添加AI消息占位符（用于流式更新）
      const aiMessageId = generateId();
      const aiMessage: ChatMessage = {
        id: aiMessageId,
        role: 'assistant',
        content: '',
        timestamp: Date.now(),
        loading: true,
      };
      addMessage(aiMessage);

      setLoading(true);

      // 构建消息历史（用于上下文）
      const messageHistory = [...messages, userMessage].map((msg) => ({
        role: msg.role,
        content: msg.content,
      }));

      // 根据模式设置系统提示词
      const systemPrompt =
        mode === 'writing'
          ? '你是一个专业的文案创作助手，擅长撰写各种类型的文案，包括营销文案、产品介绍、社交媒体内容等。请根据用户需求，创作高质量、有吸引力的文案。'
          : '你是一个智能助手，能够回答用户的各种问题，提供准确、有用的信息。';

      // 构建请求消息列表
      const requestMessages = [
        { role: 'system' as const, content: systemPrompt },
        ...messageHistory,
      ];

      try {
        // 创建AbortController用于取消请求
        abortControllerRef.current = new AbortController();

        // 调用流式API
        await chatWithDoubaoStream(
          {
            model: finalModel,
            messages: requestMessages,
            stream: true,
            temperature: mode === 'writing' ? 0.8 : 0.7, // 文案生成时温度稍高，更有创意
            max_tokens: 2000,
          },
          {
            onMessage: (content: string) => {
              // 流式更新AI消息内容
              updateLastMessage((msg) => {
                if (msg.id === aiMessageId) {
                  return {
                    ...msg,
                    content: msg.content + content,
                    loading: false,
                  };
                }
                return msg;
              });
            },
            onError: (error: Error) => {
              // 更新错误信息
              updateLastMessage((msg) => {
                if (msg.id === aiMessageId) {
                  return {
                    ...msg,
                    loading: false,
                    error: getErrorMessage(error),
                  };
                }
                return msg;
              });
              message.error(getErrorMessage(error));
            },
            onComplete: () => {
              // 完成时移除loading状态
              updateLastMessage((msg) => {
                if (msg.id === aiMessageId) {
                  return {
                    ...msg,
                    loading: false,
                  };
                }
                return msg;
              });
              setLoading(false);
              abortControllerRef.current = null;
            },
          },
        );
      } catch (error) {
        const errorMsg = getErrorMessage(error);
        updateLastMessage((msg) => {
          if (msg.id === aiMessageId) {
            return {
              ...msg,
              loading: false,
              error: errorMsg,
            };
          }
          return msg;
        });
        message.error(errorMsg);
        setLoading(false);
        abortControllerRef.current = null;
      }
    },
    [loading, messages, addMessage, updateLastMessage, mode, model],
  );

  /**
   * 清空消息
   */
  const handleClear = useCallback(() => {
    if (loading) {
      message.warning('请等待当前请求完成');
      return;
    }
    setMessages([]);
    // 取消正在进行的请求
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
    }
    message.success('已清空对话记录');
  }, [loading]);

  return (
    <PageContainer
      title={title}
      className={styles.doubaoChatPage}
      data-panel-exempt="true"
    >
      <div className={styles.chatContainer}>
        <div className={styles.messagesArea}>
          <MessageList messages={messages} />
        </div>
        <div className={styles.inputArea}>
          <InputPanel
            loading={loading}
            onSend={handleSend}
            onClear={handleClear}
            placeholder={
              mode === 'writing'
                ? '输入你的文案需求，例如：写一篇关于新产品的营销文案...'
                : '输入你的问题，例如：什么是人工智能？...'
            }
            disabled={loading}
          />
        </div>
      </div>
    </PageContainer>
  );
};

export default DoubaoChat;

