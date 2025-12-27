import { LinkOutlined } from '@ant-design/icons';
import type { Settings as LayoutSettings } from '@ant-design/pro-components';
import { SettingDrawer } from '@ant-design/pro-components';
import type { RequestConfig, RunTimeLayoutConfig } from '@umijs/max';
import { history, Link, useLocation } from '@umijs/max';
import React, { useEffect } from 'react';
import {
  AvatarDropdown,
  AvatarName,
  Question,
  SelectLang,
} from '@/components';
import { currentUser as queryCurrentUser } from '@/services/Api/Systems/login';
import { getMenuTree } from '@/services/Api/Systems/menu';
import type { MenuItem, MenuTreeResult } from '@/services/Model/Systems/menu';
import * as Icons from '@ant-design/icons';

import defaultSettings from '../config/defaultSettings';
import { errorConfig } from './requestErrorConfig';
import '@ant-design/v5-patch-for-react-19';

const isDev = process.env.NODE_ENV === 'development';
const isDevOrTest = isDev || process.env.CI;
const loginPath = '/user/login';

// 全局背景组件：为除登录页面外的所有页面添加背景
const GlobalBackground: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const location = useLocation();
  
  useEffect(() => {
    // 注入统一面板样式到 window，作为全局单一来源
    const ensurePanelStyles = () => {
      const anyWin: any = window as any;
      if (!anyWin.__panelStyles) {
        anyWin.__panelStyles = {
          panelStyle: {
            background: '#ffffff',
            border: '1px solid #f0f0f0',
            borderRadius: 8,
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            overflow: 'hidden',
          },
          headStyle: {
            background: '#fafafa',
            color: 'rgba(0, 0, 0, 0.85)',
            borderBottom: '1px solid #f0f0f0',
            fontWeight: 600,
          },
          bodyStyle: {
            background: '#ffffff',
            padding: 16,
            color: 'rgba(0, 0, 0, 0.85)',
          },
        };
      }
    };

    // 将统一样式应用到页面现有的 Card（可通过 data-panel-exempt 跳过）
    const applyPanelStyles = () => {
      try {
        const anyWin: any = window as any;
        const styles = anyWin.__panelStyles;
        if (!styles) return;
        // 仅选择尚未应用过样式的卡片，避免重复写样式引发布局抖动
        const cards = Array.from(
          document.querySelectorAll<HTMLElement>('.ant-card:not([data-__panel-applied="true"])')
        );
        if (cards.length === 0) return; // 无新增卡片则跳过
        cards.forEach((card) => {
          // 跳过带有 data-panel-exempt 的卡片
          if (card.getAttribute('data-panel-exempt') === 'true') return;
          // 应用容器样式
          Object.assign(card.style, styles.panelStyle || {});
          // 头部
          const head = card.querySelector<HTMLElement>('.ant-card-head');
          if (head && styles.headStyle) Object.assign(head.style, styles.headStyle);
          // 标题文字颜色
          const headTitle = card.querySelector<HTMLElement>('.ant-card-head-title');
          if (headTitle && styles.headStyle?.color) headTitle.style.color = styles.headStyle.color;
          // 内容区
          const body = card.querySelector<HTMLElement>('.ant-card-body');
          if (body && styles.bodyStyle) Object.assign(body.style, styles.bodyStyle);
          // 描边由 panelStyle 控制，去掉 antd 默认边框
          card.classList.add('ant-card-bordered');
          card.dataset.__panelApplied = 'true';
        });
      } catch (e) {
        // 忽略运行时样式应用错误，避免影响功能
      }
    };

    // 更新背景的函数（仅应用面板样式，不再设置背景图）
    const updateBackground = () => {
      const currentPath = location.pathname;
      const isLoginPage = currentPath === loginPath || currentPath.startsWith('/user/login');
      
      if (!isLoginPage) {
        // 确保已注入统一面板样式
        ensurePanelStyles();
        // 延迟应用卡片样式，等待页面节点渲染
        setTimeout(applyPanelStyles, 50);
      }
    };
    
    // 延迟执行，确保 DOM 已加载
    const timer = setTimeout(() => {
      updateBackground();
    }, 100);
    
    // 监听路由变化
    const unlisten = history.listen(({ location: newLocation }) => {
      const currentPath = newLocation.pathname;
      const isLoginPage = currentPath === loginPath || currentPath.startsWith('/user/login');
      
      setTimeout(() => {
        if (!isLoginPage) {
          ensurePanelStyles();
          setTimeout(applyPanelStyles, 50);
        }
      }, 100);
    });
    
    // 观察 DOM 变化，动态给后续渲染的卡片应用样式（节流以避免频繁重绘）
    let rafId: number | null = null;
    let applyPending = false;
    const scheduleApply = () => {
      if (applyPending) return;
      applyPending = true;
      rafId = window.requestAnimationFrame(() => {
        applyPending = false;
        const currentPath = location.pathname;
        const isLoginPage = currentPath === loginPath || currentPath.startsWith('/user/login');
        if (!isLoginPage) applyPanelStyles();
      });
    };
    const observer = new MutationObserver(() => {
      scheduleApply();
    });
    const observeTarget = document.querySelector('.ant-layout') || document.getElementById('root') || document.body;
    observer.observe(observeTarget, { childList: true, subtree: true });

    // 清理函数：组件卸载时移除监听器
    return () => {
      clearTimeout(timer);
      unlisten();
      observer.disconnect();
      if (rafId) {
        cancelAnimationFrame(rafId);
      }
    };
  }, [location.pathname]);
  
  return <>{children}</>;
};

type InitialState = {
  settings?: Partial<LayoutSettings>;
  currentUser?: API.CurrentUser;
  loading?: boolean;
  fetchUserInfo?: () => Promise<API.CurrentUser | undefined>;
  companyMenuVersion?: number;
};

/**
 * @see https://umijs.org/docs/api/runtime-config#getinitialstate
 * */
export async function getInitialState(): Promise<InitialState> {
  const fetchUserInfo = async () => {
    // 1. 检查本地Token是否存在
    const token = localStorage.getItem('authToken');
    if (!token) {
      // 不在这里重定向，让路由配置处理重定向逻辑
      return undefined;
    }

    try {
      const msg = await queryCurrentUser({
        skipErrorHandler: true,
      });
      return msg.data;
    } catch (error: any) {
      // 2. 区分错误状态码处理
      if (error.response?.status === 401) {
        // Token无效/过期：清除Token并重定向
        localStorage.removeItem('authToken');
        // 只有在非登录页面时才重定向
        const { location } = history;
        if (![loginPath, '/user/register', '/user/register-result'].includes(location.pathname)) {
          history.push(`${loginPath}?redirect=${encodeURIComponent(location.pathname)}`);
        }
      } else {
        console.error('获取用户信息失败，请刷新页面重试', error);
      }
    }
    return undefined;
  };
  // 如果不是登录页面，执行
  const { location } = history;
  if (
    ![loginPath, '/user/register', '/user/register-result'].includes(
      location.pathname,
    )
  ) {
    const currentUser = await fetchUserInfo();
    // 如果没有用户信息且没有token，重定向到登录页
    if (!currentUser && !localStorage.getItem('authToken')) {
      history.push(loginPath);
    }
    return {
      fetchUserInfo,
      currentUser,
      settings: defaultSettings as Partial<LayoutSettings>,
      companyMenuVersion: 0,
    };
  }
  return {
    fetchUserInfo,
    settings: defaultSettings as Partial<LayoutSettings>,
    companyMenuVersion: 0,
  };
}

// ProLayout 支持的api https://procomponents.ant.design/components/layout
export const layout: RunTimeLayoutConfig = ({
  initialState,
  setInitialState,
}) => {
  /**
   * 将菜单中的 icon 字段（字符串）转换为 Ant Design 图标组件
   * 支持：
   *  - 直接写组件名：PieChartOutlined、UserOutlined
   *  - 写基础名：pieChart、user（自动补 Outlined 后缀并转为 PascalCase）
   *  - 兼容大小写差异
   */
  const getIconComponent = (iconName?: string) => {
    if (!iconName) return undefined;

    // 1. 优先直接按原样匹配
    let Icon: any = (Icons as any)[iconName];
    if (Icon) return <Icon />;

    // 2. 如果已经带 Outlined/Filled/TwoTone 后缀，但大小写可能不一致
    if (
      iconName.includes('Outlined') ||
      iconName.includes('Filled') ||
      iconName.includes('TwoTone')
    ) {
      const pascalCaseName =
        iconName.charAt(0).toUpperCase() + iconName.slice(1);
      Icon = (Icons as any)[pascalCaseName];
      if (Icon) return <Icon />;
    }

    // 3. 处理简单名称：smile、pieChart → SmileOutlined / PieChartOutlined
    const toPascalCase = (str: string) => {
      if (!str) return str;
      return str.charAt(0).toUpperCase() + str.slice(1);
    };

    const baseName = toPascalCase(iconName);
    const candidates = [
      `${baseName}Outlined`,
      `${baseName}Filled`,
      `${baseName}TwoTone`,
    ];

    for (const name of candidates) {
      Icon = (Icons as any)[name];
      if (Icon) return <Icon />;
    }

    return undefined;
  };

  return {
    menu: {
      params: {
        companyMenuVersion: initialState?.companyMenuVersion,
      },
      request: async (
        { companyMenuVersion }: { companyMenuVersion?: number } = {},
        defaultMenuData,
      ) => {
        void companyMenuVersion;

        // 规范化后端返回的菜单字段，兼容大小写差异，并过滤掉按钮类型(F)
        const normalizeMenuNode = (node: any) => ({
          menuId: node.menuId ?? node.MenuId,
          parentId: node.parentId ?? node.ParentId,
          menuType: node.menuType ?? node.MenuType,
          menuName: node.menuName ?? node.MenuName,
          icon: node.icon ?? node.Icon,
          orderNum: node.orderNum ?? node.OrderNum ?? 0,
          isFrame: node.isFrame ?? node.IsFrame,
          path: node.path ?? node.Path,
          routeName: node.routeName ?? node.RouteName,
          component: node.component ?? node.Component,
          perms: node.perms ?? node.Perms,
          query: node.query ?? node.Query,
          isCache: node.isCache ?? node.IsCache,
          visible: node.visible ?? node.Visible,
          status: node.status ?? node.Status,
          children: node.children ?? node.Children,
        });

        const resolvePath = (parentPath: string, childPath?: string) => {
          if (!childPath) return parentPath || '/';
          if (childPath.startsWith('/')) return childPath;
          const base = parentPath && parentPath !== '/' ? parentPath : '';
          return `${base}/${childPath}`.replace(/\/+/, '/');
        };

        const buildMenuFromSysMenus = (menus: any[], parentPath: string = ''): any[] =>
          (menus || [])
            .map((n) => normalizeMenuNode(n))
            .filter((m) => (m.status ?? '0') !== '1' && (m.menuType ?? 'C') !== 'F') // 过滤停用与按钮
            .map((m) => {
              const fullPath = resolvePath(parentPath, m.path);
              return {
                name: m.menuName,
                path: fullPath,
                icon: getIconComponent(m.icon),
                hideInMenu: m.visible === '1',
                children:
                  m.children && m.children.length > 0
                    ? buildMenuFromSysMenus(m.children, fullPath)
                    : undefined,
              };
            });

        try {
          // 1. 先从后端获取完整菜单树，作为主菜单数据源
          const menuRes: any = await getMenuTree();
          const resCode = menuRes?.code ?? menuRes?.Code;
          const resData = Array.isArray(menuRes?.data)
            ? menuRes.data
            : Array.isArray(menuRes?.Data)
            ? menuRes.Data
            : [];

          let menuData: any[] =
            resCode === 200 && Array.isArray(resData)
              ? buildMenuFromSysMenus(resData)
              : (defaultMenuData || []);

          return menuData as any;
        } catch (error) {
          console.error('加载系统菜单失败', error);
          return defaultMenuData as any;
        }
      },
    },
    actionsRender: () => [
      <Question key="doc" />,
      <SelectLang key="SelectLang" />,
    ],
    avatarProps: {
      src: initialState?.currentUser?.avatar,
      title: <AvatarName />,
      render: (_, avatarChildren) => (
        <AvatarDropdown>{avatarChildren}</AvatarDropdown>
      ),
    },
    // 关闭全局水印，避免在系统设置等二级页面出现背景文字
    waterMarkProps: undefined,
    footerRender: false,
    onPageChange: () => {
      const { location } = history;
      // 如果没有登录，重定向到 login
      if (!initialState?.currentUser && location.pathname !== loginPath) {
        history.push(loginPath);
      }
    },
    bgLayoutImgList: [
      {
        src: 'https://mdn.alipayobjects.com/yuyan_qk0oxh/afts/img/D2LWSqNny4sAAAAAAAAAAAAAFl94AQBr',
        left: 85,
        bottom: 100,
        height: '303px',
      },
      {
        src: 'https://mdn.alipayobjects.com/yuyan_qk0oxh/afts/img/C2TWRpJpiC0AAAAAAAAAAAAAFl94AQBr',
        bottom: -68,
        right: -45,
        height: '303px',
      },
      {
        src: 'https://mdn.alipayobjects.com/yuyan_qk0oxh/afts/img/F6vSTbj8KpYAAAAAAAAAAAAAFl94AQBr',
        bottom: 0,
        left: 0,
        width: '331px',
      },
    ],
    links: isDevOrTest
      ? [
          <Link key="openapi" to="/umi/plugin/openapi" target="_blank">
            <LinkOutlined />
            <span>OpenAPI 文档</span>
          </Link>,
        ]
      : [],
    menuHeaderRender: false,
    // 自定义 403 页面
    // unAccessible: <div>unAccessible</div>,
    // 增加一个 loading 的状态
    childrenRender: (children) => {
      // if (initialState?.loading) return <PageLoading />;
      return (
        <GlobalBackground>
          {children}
          {isDevOrTest && (
            <SettingDrawer
              disableUrlParams
              enableDarkTheme
              settings={initialState?.settings}
              onSettingChange={(settings) => {
                setInitialState((preInitialState) => ({
                  ...preInitialState,
                  settings,
                }));
              }}
            />
          )}
        </GlobalBackground>
      );
    },
    ...initialState?.settings,
  };
};

/**
 * @name 动态路由配置
 * @description 从后端获取菜单数据，动态注册路由
 * @doc https://umijs.org/docs/api/runtime-config#patchclientroutes
 * 
 * 注意：此函数是同步执行的，不能进行异步操作。
 * 动态路由的实际加载由 DynamicRouteLoader 组件处理。
 * 这里主要用于在路由表中预先注册路由路径，避免 404。
 */
export function patchClientRoutes({ routes }: { routes: any[] }) {
  // 由于 patchClientRoutes 是同步的，我们不能在这里异步获取菜单数据
  // 实际的动态路由加载由 DynamicRouteLoader 组件处理
  // 这里可以做一些路由的预处理，比如调整路由顺序等
  
  // 确保通配符路由在最后，让 DynamicRouteLoader 处理所有未匹配的路径
  const wildcardIndex = routes.findIndex((r: any) => r.path === '*');
  if (wildcardIndex > -1 && wildcardIndex !== routes.length - 1) {
    const wildcardRoute = routes.splice(wildcardIndex, 1)[0];
    routes.push(wildcardRoute);
  }
}

/**
 * @name request 配置，可以配置错误处理
 * 它基于 axios 和 ahooks 的 useRequest 提供了一套统一的网络请求和错误处理方案。
 * @doc https://umijs.org/docs/max/request#配置
 */
export const request: RequestConfig = {
  baseURL: isDev ? '' : '/',// 使用相对路径，便于nginx代理转发
  ...errorConfig,
  // 添加请求拦截器
  requestInterceptors: [
    // 为config参数添加显式类型注解
    (config: RequestConfig) => {
      const token = localStorage.getItem('authToken');
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
  ],
};
