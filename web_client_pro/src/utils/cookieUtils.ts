/**
 * Cookie 管理工具类
 * 用于统一管理应用中的 Cookie 操作
 */

export const COOKIE_KEYS = {
  // 登录认证标识
  SITE_TOTAL_ID: 'SITE_TOTAL_ID',
  // Token（与 localStorage 的 authToken 一致）
  AUTH_TOKEN: 'AUTH_TOKEN',
  // 用户信息
  USER_ID: 'USER_ID',
  USER_NAME: 'USER_NAME',
  // 登录凭证（仅用于演示，极不安全）
  LOGIN_USERNAME: 'LOGIN_USERNAME',
  LOGIN_PASSWORD: 'LOGIN_PASSWORD',
} as const;

/**
 * 设置 Cookie
 * @param name - Cookie 名称
 * @param value - Cookie 值
 * @param days - 过期天数（默认7天）
 */
export const setCookie = (name: string, value: string, days: number = 7) => {
  try {
    const date = new Date();
    date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
    const expires = `expires=${date.toUTCString()}`;
    const path = 'path=/';
    const sameSite = 'SameSite=Lax';
    document.cookie = `${name}=${encodeURIComponent(value)}; ${expires}; ${path}; ${sameSite}`;
  } catch (error) {
    console.error(`设置 Cookie ${name} 失败:`, error);
  }
};

/**
 * 获取 Cookie
 * @param name - Cookie 名称
 * @returns Cookie 值或 null
 */
export const getCookie = (name: string): string | null => {
  try {
    const nameEQ = `${name}=`;
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
      cookie = cookie.trim();
      if (cookie.indexOf(nameEQ) === 0) {
        return decodeURIComponent(cookie.substring(nameEQ.length));
      }
    }
    return null;
  } catch (error) {
    console.error(`获取 Cookie ${name} 失败:`, error);
    return null;
  }
};

/**
 * 删除 Cookie
 * @param name - Cookie 名称
 */
export const deleteCookie = (name: string) => {
  try {
    setCookie(name, '', -1);
  } catch (error) {
    console.error(`删除 Cookie ${name} 失败:`, error);
  }
};

/**
 * 检查 Cookie 是否存在
 * @param name - Cookie 名称
 * @returns 是否存在
 */
export const hasCookie = (name: string): boolean => {
  return getCookie(name) !== null;
};

/**
 * 清除所有认证相关的 Cookie
 */
export const clearAuthCookies = () => {
  Object.values(COOKIE_KEYS).forEach((key) => {
    deleteCookie(key);
  });
};

/**
 * 设置 Token 到 Cookie
 */
export const setTokenCookie = (token: string, days: number = 7) => {
  setCookie(COOKIE_KEYS.AUTH_TOKEN, token, days);
};

/**
 * 从 Cookie 获取 Token
 */
export const getTokenCookie = (): string | null => {
  return getCookie(COOKIE_KEYS.AUTH_TOKEN);
};

/**
 * 设置登录认证标识
 * @param userId - 用户 ID
 */
export const setAuthCookie = (userId: string | number) => {
  const siteTotalId = `${userId}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  setCookie(COOKIE_KEYS.SITE_TOTAL_ID, siteTotalId, 7);
  return siteTotalId;
};

/**
 * 检查是否已登录（检查认证标识 Cookie）
 * @returns 是否已登录
 */
export const isAuthenticated = (): boolean => {
  return hasCookie(COOKIE_KEYS.SITE_TOTAL_ID);
};

/**
 * 获取认证标识
 * @returns 认证标识或 null
 */
export const getAuthCookie = (): string | null => {
  return getCookie(COOKIE_KEYS.SITE_TOTAL_ID);
};

