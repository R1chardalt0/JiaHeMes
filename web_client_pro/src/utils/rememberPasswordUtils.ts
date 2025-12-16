/**
 * 记住密码工具函数
 * 用于管理 localStorage 中的账号密码（带过期时间）
 */

const STORAGE_KEYS = {
  USER_ACCOUNT: 'userAccount',
  USER_PASSWORD: 'userPassword',
} as const;

const EXPIRE_DAYS = 7; // 7天过期

/**
 * 存储账号到 localStorage（不过期）
 * @param account - 账号（手机号/邮箱/用户名）
 */
export const saveUserAccount = (account: string) => {
  try {
    localStorage.setItem(STORAGE_KEYS.USER_ACCOUNT, account);
  } catch (error) {
    console.error('保存账号失败:', error);
  }
};

/**
 * 存储密码到 localStorage（带7天过期时间）
 * @param password - 密码
 */
export const saveUserPassword = (password: string) => {
  try {
    const expireTime = Date.now() + EXPIRE_DAYS * 24 * 60 * 60 * 1000;
    const data = {
      value: password,
      expireTime: expireTime,
    };
    localStorage.setItem(STORAGE_KEYS.USER_PASSWORD, JSON.stringify(data));
  } catch (error) {
    console.error('保存密码失败:', error);
  }
};

/**
 * 清除密码（保留账号）
 */
export const clearUserPassword = () => {
  try {
    localStorage.removeItem(STORAGE_KEYS.USER_PASSWORD);
  } catch (error) {
    console.error('清除密码失败:', error);
  }
};

/**
 * 获取账号
 * @returns 账号或 null
 */
export const getUserAccount = (): string | null => {
  try {
    return localStorage.getItem(STORAGE_KEYS.USER_ACCOUNT);
  } catch (error) {
    console.error('获取账号失败:', error);
    return null;
  }
};

/**
 * 获取密码（检查是否过期）
 * @returns 密码或 null（如果过期或不存在）
 */
export const getUserPassword = (): string | null => {
  try {
    const dataStr = localStorage.getItem(STORAGE_KEYS.USER_PASSWORD);
    if (!dataStr) {
      return null;
    }

    const data = JSON.parse(dataStr);
    const now = Date.now();

    // 检查是否过期
    if (data.expireTime && now > data.expireTime) {
      // 已过期，清除数据
      localStorage.removeItem(STORAGE_KEYS.USER_PASSWORD);
      return null;
    }

    return data.value || null;
  } catch (error) {
    console.error('获取密码失败:', error);
    // 如果解析失败，清除无效数据
    localStorage.removeItem(STORAGE_KEYS.USER_PASSWORD);
    return null;
  }
};

/**
 * 检查是否有保存的密码（且未过期）
 * @returns 是否有有效的密码
 */
export const hasValidPassword = (): boolean => {
  return getUserPassword() !== null;
};

