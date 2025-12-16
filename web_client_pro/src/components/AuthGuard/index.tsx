/**
 * 认证守卫组件
 * 用于检查用户是否已登录（通过 Cookie 标识值）
 * 如果未登录，自动重定向到登录页面
 */

import React, { useEffect } from 'react';
import { history } from '@umijs/max';
import { isAuthenticated } from '@/utils/cookieUtils';

interface AuthGuardProps {
  children: React.ReactNode;
}

const loginPath = '/user/login';

/**
 * 需要排除的路径（不需要认证检查的页面）
 */
const excludePaths = [
  loginPath,
  '/user/register',
  '/user/register-result',
  '/404',
  '/500',
];

/**
 * 检查路径是否需要认证
 */
const isPathRequiresAuth = (pathname: string): boolean => {
  return !excludePaths.some((path) => pathname === path || pathname.startsWith(path));
};

/**
 * 认证守卫组件
 * 在页面加载时检查 Cookie 中的认证标识值
 * 如果不存在且当前路径需要认证，则重定向到登录页面
 */
export const AuthGuard: React.FC<AuthGuardProps> = ({ children }) => {
  useEffect(() => {
    const checkAuth = () => {
      const currentPath = window.location.pathname;
      
      // 检查当前路径是否需要认证
      if (!isPathRequiresAuth(currentPath)) {
        return; // 不需要认证，直接返回
      }

      // 检查 Cookie 中是否存在认证标识值
      if (!isAuthenticated()) {
        console.warn(`🔐 未检测到认证标识，重定向到登录页面。当前路径: ${currentPath}`);
        
        // 保存当前路径，登录后可以重定向回来
        const redirectUrl = encodeURIComponent(currentPath);
        history.push(`${loginPath}?redirect=${redirectUrl}`);
      } else {
        console.log(`✅ 认证标识有效，允许访问。当前路径: ${currentPath}`);
      }
    };

    // 初始化时检查认证
    checkAuth();

    // 监听路由变化
    const unlisten = history.listen(({ location }) => {
      const newPath = location.pathname;
      
      if (!isPathRequiresAuth(newPath)) {
        return; // 不需要认证，直接返回
      }

      if (!isAuthenticated()) {
        console.warn(`🔐 未检测到认证标识，重定向到登录页面。当前路径: ${newPath}`);
        const redirectUrl = encodeURIComponent(newPath);
        history.push(`${loginPath}?redirect=${redirectUrl}`);
      } else {
        console.log(`✅ 认证标识有效，允许访问。当前路径: ${newPath}`);
      }
    });

    // 清理函数
    return () => {
      unlisten();
    };
  }, []);

  return <>{children}</>;
};

export default AuthGuard;

