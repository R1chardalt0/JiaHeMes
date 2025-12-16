import React, { useEffect, useState, Suspense } from 'react';
import { useLocation, history } from '@umijs/max';
import { Spin, Result } from 'antd';
import { getMenuTree } from '@/services/Api/Systems/menu';
import type { MenuItem, MenuTreeResult } from '@/services/Model/Systems/menu';
import NotFoundPage from './404';
import WelcomePage from './Welcome';

type LoadStatus = 'loading' | 'ready' | 'notFound' | 'error';

/**
 * 动态页面加载器
 * - 根据当前 URL，从后端菜单数据中找到匹配的菜单项
 * - 使用组件路径（Component 字段）进行懒加载并渲染对应页面
 * - 未找到或加载失败时，回退到 404 或错误提示
 *
 * 依赖：
 * - 后端菜单表中的 Path 与前端路由 path 一致（如 /dashboard/analysis）
 * - Component 字段相对于 src/pages 目录（如 dashboard/analysis、systems/user、Welcome）
 */
const DynamicRouteLoader: React.FC = () => {
  const location = useLocation();
  const [status, setStatus] = useState<LoadStatus>('loading');
  const [LoadedComponent, setLoadedComponent] = useState<React.ComponentType | null>(null);

  const resolvePath = (parentPath: string, childPath?: string) => {
    if (!childPath) return parentPath || '/';
    if (childPath.startsWith('/')) return childPath;
    const base = parentPath && parentPath !== '/' ? parentPath : '';
    return `${base}/${childPath}`.replace(/\/+/, '/');
  };

  const flattenMenus = (menus: any[]): any[] => {
    const list: any[] = [];
    const walk = (items: any[], parentPath: string = '') => {
      items.forEach((m) => {
        const kids = m.children && m.children.length > 0 ? m.children : (m.Children && m.Children.length > 0 ? m.Children : []);
        const fullPath = resolvePath(parentPath, m.path ?? m.Path);
        const node = { ...m, fullPath };
        list.push(node);
        if (kids && kids.length > 0) {
          walk(kids, fullPath);
        }
      });
    };
    walk(menus || [], '');
    return list;
  };

  // 统一菜单字段大小写，兼容后端可能返回的 PascalCase/CamelCase
  const normalizeMenu = (m: any) => ({
    ...m,
    menuId: m.menuId ?? m.MenuId,
    parentId: m.parentId ?? m.ParentId ?? 0,
    menuType: m.menuType ?? m.MenuType,
    menuName: m.menuName ?? m.MenuName,
    path: m.path ?? m.Path,
    component: m.component ?? m.Component,
    status: m.status ?? m.Status,
    visible: m.visible ?? m.Visible,
  });

  const normalizeComponentPath = (comp?: string): string | null => {
    if (!comp) return null;
    // 去掉开头 './' 或前导 '/'
    let p = String(comp).replace(/^\.?\//, '');
    // 去掉结尾的 '/index' 或 '.tsx' 之类
    p = p.replace(/\.(t|j)sx?$/i, '');
    p = p.replace(/\/?index$/i, '');
    return p;
  };

  const toPascalCasePath = (p: string) =>
    p
      .split('/')
      .map((seg) => (seg ? seg.charAt(0).toUpperCase() + seg.slice(1) : seg))
      .join('/');

  // 当后端未返回任何菜单时的本地兜底映射：路径 -> 组件路径（相对 src/pages）
  const STATIC_FALLBACK_ROUTES: Record<string, string> = {
    '/welcome': 'Welcome',
    '/dashboard/analysis': 'dashboard/analysis',
    '/trace/productTraceInfo': 'Trace/ProductTraceInfo',
  };

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setStatus('loading');
      setLoadedComponent(null);
      try {
        const res: any = await getMenuTree();
        const code = res?.code ?? res?.Code;
        const rawData = (Array.isArray(res?.data) ? res.data : Array.isArray(res?.Data) ? res.Data : []) as any[];
        if (!res || code !== 200 || !rawData) {
          if (!cancelled) setStatus('error');
          return;
        }

        // 兼容后端字段大小写差异，先规范化树
        const normalizeTree = (nodes: any[]): any[] => {
          if (!nodes) return [];
          return nodes.map((n) => {
            const m = normalizeMenu(n);
            const kids = n.children ?? n.Children ?? [];
            return { ...m, children: normalizeTree(kids) };
          });
        };

        const normalizedTree = normalizeTree(rawData as any[]);
        const allMenus = flattenMenus(normalizedTree);
        const currentPath = location.pathname;

        // 如果用户点击了一个带动态参数占位符的菜单（例如：/devicechart/monitor/:deviceType），
        // 直接跳转到该功能的入口页，避免 404。
        const dynamicRouteFallbacks: Record<string, string> = {
          '/devicechart/monitor/:deviceType': '/devicemonitor/index2',
        };
        if (dynamicRouteFallbacks[currentPath]) {
          history.replace(dynamicRouteFallbacks[currentPath]);
          return;
        }

        // 处理根路径：重定向到 /welcome
        if (currentPath === '/') {
          if (!cancelled) {
            history.replace('/welcome');
          }
          return;
        }

        // 路径匹配函数：支持动态参数匹配
        const matchPath = (menuPath: string | undefined, currentPath: string): boolean => {
          if (!menuPath) return false;
          
          // 精确匹配
          if (menuPath === currentPath) return true;
          
          // 动态参数匹配：将 :param 转换为正则表达式
          // 例如：/productionEquipment/company/:companyId/equipment
          // 匹配：/productionEquipment/company/2/equipment
          const pattern = menuPath
            .replace(/:[^/]+/g, '[^/]+') // 将 :param 替换为 [^/]+
            .replace(/\//g, '\\/'); // 转义斜杠
          const regex = new RegExp(`^${pattern}$`);
          
          return regex.test(currentPath);
        };

        // 优先精确匹配，然后尝试动态参数匹配
        let target = allMenus.find(
          (m) => (m.fullPath || m.path) === currentPath && m.menuType !== 'F' && m.status !== '1',
        );
        
        // 如果精确匹配失败，尝试动态参数匹配（使用 fullPath 优先）
        if (!target) {
          target = allMenus.find(
            (m) => 
              (m.fullPath || m.path) && 
              String(m.fullPath || m.path).includes(':') && // 包含动态参数
              matchPath(String(m.fullPath || m.path), currentPath) && 
              m.menuType !== 'F' && 
              m.status !== '1',
          );
        }

        if (!target || !target.component) {
          // 兜底 1：静态映射（在后端返回空菜单时，仍可直达常用页面）
          const staticComp = STATIC_FALLBACK_ROUTES[currentPath];
          if (staticComp) {
            const p = normalizeComponentPath(staticComp)!;
            const candidates: string[] = Array.from(new Set([
              p,
              `${p}/index`,
              toPascalCasePath(p),
              `${toPascalCasePath(p)}/index`,
              p.toLowerCase(),
              `${p.toLowerCase()}/index`,
            ]));
            let loadErr: any = null;
            for (const cp of candidates) {
              try {
                // @ts-ignore
                const mod = await import(`@/pages/${cp}`);
                const anyMod: any = mod as any;
                const PageComp = anyMod.default || anyMod[Object.keys(anyMod)[0]];
                if (PageComp) {
                  if (!cancelled) {
                    setLoadedComponent(() => PageComp);
                    setStatus('ready');
                  }
                  return;
                }
              } catch (err) {
                loadErr = err;
                continue;
              }
            }
            console.error('静态兜底加载失败：', candidates, loadErr);
          }

          // 兜底 2：重定向到第一个可访问的叶子菜单
          const firstAllowed = allMenus.find((m) =>
            (m.component || m.Component) &&
            (m.menuType ?? m.MenuType) !== 'F' &&
            String(m.status ?? m.Status ?? '0') !== '1'
          );
          const fallbackPath = firstAllowed?.fullPath || firstAllowed?.path || firstAllowed?.Path;

          // 特殊处理：如果访问的是 /welcome 且菜单未配置欢迎页，则直接使用本地 Welcome 组件
          if (currentPath === '/welcome' && !fallbackPath) {
            if (!cancelled) {
              setLoadedComponent(() => WelcomePage);
              setStatus('ready');
              return;
            }
          }

          if (fallbackPath && fallbackPath !== currentPath) {
            history.replace(fallbackPath);
            return;
          }

          if (!cancelled) setStatus('notFound');
          return;
        }

        const compPath = normalizeComponentPath(target.component);
        if (!compPath) {
          if (!cancelled) setStatus('notFound');
          return;
        }

        // 使用动态 import 加载组件，尝试多种大小写/索引形式
        const candidates: string[] = Array.from(
          new Set([
            compPath,
            `${compPath}/index`,
            toPascalCasePath(compPath),
            `${toPascalCasePath(compPath)}/index`,
            compPath.toLowerCase(),
            `${compPath.toLowerCase()}/index`,
          ]),
        );

        let loadErr: any = null;
        for (const p of candidates) {
          try {
            // @ts-ignore
            const mod = await import(`@/pages/${p}`);
            const anyMod: any = mod as any;
            const PageComp = anyMod.default || anyMod[Object.keys(anyMod)[0]];
            if (PageComp) {
              if (!cancelled) {
                setLoadedComponent(() => PageComp);
                setStatus('ready');
              }
              return;
            }
          } catch (err) {
            loadErr = err;
            continue;
          }
        }

        console.error('动态加载页面失败，已尝试：', candidates, loadErr);
        if (!cancelled) setStatus('error');
      } catch (e) {
        console.error('获取菜单树失败：', e);
        if (!cancelled) setStatus('error');
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, [location.pathname]);

  if (status === 'loading') {
    return (
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          height: '60vh',
        }}
      >
        <Spin size="large" tip="页面加载中，请稍候..." />
      </div>
    );
  }

  if (status === 'notFound') {
    return <NotFoundPage />;
  }

  if (status === 'error') {
    return (
      <Result
        status="500"
        title="页面加载失败"
        subTitle="未能根据当前菜单配置加载对应页面，请联系管理员检查菜单组件路径或权限配置。"
      />
    );
  }

  if (status === 'ready' && LoadedComponent) {
    return (
      <Suspense
        fallback={
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              height: '60vh',
            }}
          >
            <Spin size="large" tip="页面加载中，请稍候..." />
          </div>
        }
      >
        <LoadedComponent />
      </Suspense>
    );
  }

  return null;
};

export default DynamicRouteLoader;


