/**
 * 豆包AI相关类型定义
 */

/**
 * 消息角色类型
 */
export type MessageRole = 'user' | 'assistant' | 'system';

/**
 * 消息内容类型
 */
export interface MessageContent {
  role: MessageRole;
  content: string;
}

/**
 * 豆包AI聊天请求参数
 */
export interface DoubaoChatRequest {
  /** 模型端点ID（从字节跳动控制台获取） */
  model: string;
  /** 消息列表 */
  messages: MessageContent[];
  /** 是否启用流式返回 */
  stream?: boolean;
  /** 温度参数（0-2，控制随机性） */
  temperature?: number;
  /** 最大生成token数 */
  max_tokens?: number;
  /** Top-p采样参数 */
  top_p?: number;
  /** 频率惩罚 */
  frequency_penalty?: number;
  /** 存在惩罚 */
  presence_penalty?: number;
}

/**
 * 豆包AI聊天响应（非流式）
 */
export interface DoubaoChatResponse {
  id: string;
  object: string;
  created: number;
  model: string;
  choices: Array<{
    index: number;
    message: MessageContent;
    finish_reason: string | null;
  }>;
  usage: {
    prompt_tokens: number;
    completion_tokens: number;
    total_tokens: number;
  };
}

/**
 * 流式响应数据块
 */
export interface DoubaoStreamChunk {
  id: string;
  object: string;
  created: number;
  model: string;
  choices: Array<{
    index: number;
    delta: {
      content?: string;
      role?: MessageRole;
    };
    finish_reason: string | null;
  }>;
}

/**
 * 流式请求回调函数类型
 */
export interface DoubaoStreamCallbacks {
  /** 收到新内容时的回调 */
  onMessage?: (content: string) => void;
  /** 发生错误时的回调 */
  onError?: (error: Error) => void;
  /** 流式返回完成时的回调 */
  onComplete?: () => void;
}

/**
 * 聊天消息（用于UI显示）
 */
export interface ChatMessage {
  /** 消息ID */
  id: string;
  /** 角色 */
  role: MessageRole;
  /** 内容 */
  content: string;
  /** 创建时间 */
  timestamp: number;
  /** 是否正在加载 */
  loading?: boolean;
  /** 错误信息 */
  error?: string;
}

/**
 * 聊天模式
 */
export type ChatMode = 'chat' | 'writing';

