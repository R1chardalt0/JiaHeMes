import type { RequestOptions } from '@@/plugin-request/request';
import type { RequestConfig } from '@umijs/max';
import { message, notification } from 'antd';

// 错误处理方案： 错误类型
enum ErrorShowType {
  SILENT = 0,
  WARN_MESSAGE = 1,
  ERROR_MESSAGE = 2,
  NOTIFICATION = 3,
  REDIRECT = 9,
}
// 与后端约定的响应数据格式
interface ResponseStructure {
  success: boolean;
  data: any;
  errorCode?: number;
  errorMessage?: string;
  showType?: ErrorShowType;
}

/**
 * @name 错误处理
 * pro 自带的错误处理， 可以在这里做自己的改动
 * @doc https://umijs.org/docs/max/request#配置
 */
export const errorConfig: RequestConfig = {
  // 错误处理： umi@3 的错误处理方案。
  errorConfig: {
    // 错误抛出
    errorThrower: (res) => {
      const { success, data, errorCode, errorMessage, showType } =
        res as unknown as ResponseStructure;
      if (!success) {
        const error: any = new Error(errorMessage);
        error.name = 'BizError';
        error.info = { errorCode, errorMessage, showType, data };
        throw error; // 抛出自制的错误
      }
    },
    // 错误接收及处理
    errorHandler: (error: any, opts: any) => {
      if (opts?.skipErrorHandler) throw error;
      // 我们的 errorThrower 抛出的错误。
      if (error.name === 'BizError') {
        const errorInfo: ResponseStructure | undefined = error.info;
        if (errorInfo) {
          const { errorMessage, errorCode } = errorInfo;
          switch (errorInfo.showType) {
            case ErrorShowType.SILENT:
              // do nothing
              break;
            case ErrorShowType.WARN_MESSAGE:
              message.warning(errorMessage);
              break;
            case ErrorShowType.ERROR_MESSAGE:
              message.error(errorMessage);
              break;
            case ErrorShowType.NOTIFICATION:
              notification.open({
                description: errorMessage,
                message: errorCode,
              });
              break;
            case ErrorShowType.REDIRECT:
              // TODO: redirect
              break;
            default:
              message.error(errorMessage);
          }
        }
      } else if (error.response) {
        // Axios 的错误
        // 请求成功发出且服务器也响应了状态码，但状态代码超出了 2xx 的范围
        const status = error.response.status;
        let errorMessage = `操作失败 (${status})`;
        
        // 提取错误信息的辅助函数
        const extractErrorMessage = (errorData: any): string => {
          if (!errorData) return `操作失败 (${status})`;
          
          // 如果是字符串
          if (typeof errorData === 'string') {
            // 检查是否包含堆栈跟踪
            if (errorData.includes('\n') || errorData.includes('at ') || errorData.includes('Stack Trace')) {
              // 提取第一行关键错误信息
              const firstLine = errorData.split('\n')[0].trim();
              // 检查常见错误类型
              if (firstLine.includes('外键约束') || firstLine.includes('FK_')) {
                return '操作失败：存在关联数据，无法删除';
              } else if (firstLine.includes('transaction') || firstLine.includes('事务')) {
                return '操作失败：系统繁忙，请稍后重试';
              } else if (firstLine.length > 0 && firstLine.length < 150) {
                return firstLine;
              }
              return `操作失败 (${status})`;
            }
            // 尝试解析JSON
            try {
              const parsed = JSON.parse(errorData);
              if (parsed.msg || parsed.message || parsed.errorMessage) {
                return parsed.msg || parsed.message || parsed.errorMessage;
              }
            } catch {
              // 不是JSON，检查错误类型
              if (errorData.includes('外键约束') || errorData.includes('FK_')) {
                return '操作失败：存在关联数据，无法删除';
              }
              // 短字符串直接返回
              if (errorData.length < 150) {
                return errorData;
              }
            }
            return `操作失败 (${status})`;
          }
          
          // 如果是对象
          if (typeof errorData === 'object') {
            // 优先使用标准错误字段
            if (errorData.msg) return errorData.msg;
            if (errorData.message) return errorData.message;
            if (errorData.errorMessage) return errorData.errorMessage;
            
            // 检查error字段
            if (errorData.error) {
              const errorStr = typeof errorData.error === 'string' 
                ? errorData.error 
                : JSON.stringify(errorData.error);
              if (errorStr.includes('外键约束') || errorStr.includes('FK_')) {
                return '操作失败：存在关联数据，无法删除';
              }
              if (errorStr.includes('transaction') || errorStr.includes('事务')) {
                return '操作失败：系统繁忙，请稍后重试';
              }
              // 提取第一行
              const firstLine = errorStr.split('\n')[0].trim();
              if (firstLine.length > 0 && firstLine.length < 150) {
                return firstLine;
              }
            }
          }
          
          return `操作失败 (${status})`;
        };
        
        // 尝试解析错误响应数据
        if (error.response.data) {
          errorMessage = extractErrorMessage(error.response.data);
        }
        
        message.error(errorMessage);
      } else if (error.request) {
        // 请求已经成功发起，但没有收到响应
        // \`error.request\` 在浏览器中是 XMLHttpRequest 的实例，
        // 而在node.js中是 http.ClientRequest 的实例
        message.error('None response! Please retry.');
      } else {
        // 发送请求时出了点问题
        message.error('Request error, please retry.');
      }
    },
  },

  // 请求拦截器
  requestInterceptors: [
    (config: RequestOptions) => {
      // 拦截请求配置，进行个性化处理。
      const url = config?.url?.concat('?token=123');
      return { ...config, url };
    },
  ],

  // 响应拦截器
  responseInterceptors: [
    (response) => {
      // 拦截响应数据，进行个性化处理
      const { data } = response as unknown as ResponseStructure;

      if (data?.success === false) {
        message.error('请求失败！');
      }
      return response;
    },
  ],
};
