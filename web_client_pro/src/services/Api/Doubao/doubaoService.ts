/**
 * 豆包AI服务API封装
 * 支持普通请求和SSE流式返回
 */

import type {
  DoubaoChatRequest,
  DoubaoChatResponse,
  DoubaoStreamCallbacks,
} from '@/services/Model/Doubao/typings';

/**
 * 豆包AI API基础URL
 * 注意：实际请求会通过后端代理，避免前端直接暴露API Key
 */
const DOUBAO_API_BASE = '/api/doubao';

/**
 * 普通聊天请求（非流式）
 * @param request 请求参数
 * @returns Promise<DoubaoChatResponse>
 */
export async function chatWithDoubao(
  request: DoubaoChatRequest,
): Promise<DoubaoChatResponse> {
  try {
    // 使用 UmiJS 的 request 方法
    const response = await fetch(`${DOUBAO_API_BASE}/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        ...request,
        stream: false, // 确保非流式
      }),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        errorData.message || `请求失败: ${response.status} ${response.statusText}`,
      );
    }

    const data: DoubaoChatResponse = await response.json();
    return data;
  } catch (error) {
    // 处理网络错误
    if (error instanceof TypeError && error.message.includes('fetch')) {
      throw new Error('网络连接失败，请检查网络设置');
    }
    throw error;
  }
}

/**
 * 流式聊天请求（SSE，支持打字机效果）
 * @param request 请求参数
 * @param callbacks 回调函数
 * @returns Promise<void>
 */
export async function chatWithDoubaoStream(
  request: DoubaoChatRequest,
  callbacks: DoubaoStreamCallbacks = {},
): Promise<void> {
  const { onMessage, onError, onComplete } = callbacks;

  try {
    const response = await fetch(`${DOUBAO_API_BASE}/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        ...request,
        stream: true, // 启用流式返回
      }),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      const error = new Error(
        errorData.message || `请求失败: ${response.status} ${response.statusText}`,
      );
      onError?.(error);
      return;
    }

    // 检查响应类型是否为流式
    const contentType = response.headers.get('content-type');
    if (!contentType?.includes('text/event-stream')) {
      // 如果不是流式响应，尝试解析为JSON
      const data = await response.json();
      if (data.choices?.[0]?.message?.content) {
        onMessage?.(data.choices[0].message.content);
      }
      onComplete?.();
      return;
    }

    // 读取流式数据
    const reader = response.body?.getReader();
    if (!reader) {
      throw new Error('无法读取响应流');
    }

    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();

      if (done) {
        onComplete?.();
        break;
      }

      // 解码数据块
      buffer += decoder.decode(value, { stream: true });

      // 按行分割处理SSE格式
      const lines = buffer.split('\n');
      buffer = lines.pop() || ''; // 保留最后一个不完整的行

      for (const line of lines) {
        const trimmedLine = line.trim();
        if (!trimmedLine || trimmedLine === 'data: [DONE]') {
          continue;
        }

        // 解析SSE数据行
        if (trimmedLine.startsWith('data: ')) {
          try {
            const jsonStr = trimmedLine.slice(6); // 移除 "data: " 前缀
            const chunk = JSON.parse(jsonStr);

            // 提取内容
            if (chunk.choices?.[0]?.delta?.content) {
              onMessage?.(chunk.choices[0].delta.content);
            }

            // 检查是否完成
            if (chunk.choices?.[0]?.finish_reason) {
              onComplete?.();
              return;
            }
          } catch (parseError) {
            // 忽略JSON解析错误（可能是部分数据）
            console.warn('解析SSE数据失败:', parseError, trimmedLine);
          }
        }
      }
    }
  } catch (error) {
    // 处理网络错误
    if (error instanceof TypeError && error.message.includes('fetch')) {
      const networkError = new Error('网络连接失败，请检查网络设置');
      onError?.(networkError);
      return;
    }

    // 处理其他错误
    const err = error instanceof Error ? error : new Error(String(error));
    onError?.(err);
  }
}

/**
 * 获取错误提示信息（用户友好）
 * @param error 错误对象
 * @returns 错误提示文本
 */
export function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    const message = error.message;

    // 网络错误
    if (message.includes('网络') || message.includes('fetch')) {
      return '网络连接失败，请检查网络设置后重试';
    }

    // 频率限制
    if (message.includes('429') || message.includes('rate limit')) {
      return '请求过于频繁，请稍后再试';
    }

    // 认证错误
    if (message.includes('401') || message.includes('unauthorized')) {
      return 'API认证失败，请联系管理员';
    }

    // 参数错误
    if (message.includes('400') || message.includes('bad request')) {
      return '请求参数错误，请检查输入内容';
    }

    // 服务器错误
    if (message.includes('500') || message.includes('server error')) {
      return '服务器错误，请稍后重试';
    }

    return message || '未知错误，请重试';
  }

  return '发生未知错误，请重试';
}

