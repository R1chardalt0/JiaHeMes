/**
 * @name umi 的路由配置
 * @description 只支持 path,component,routes,redirect,wrappers,name,icon 的配置
 * @param path  path 只支持两种占位符配置，第一种是动态参数 :id 的形式，第二种是 * 通配符，通配符只能出现路由字符串的最后。
 * @param component 配置 location 和 path 匹配后用于渲染的 React 组件路径。可以是绝对路径，也可以是相对路径，如果是相对路径，会从 src/pages 开始找起。
 * @param routes 配置子路由，通常在需要为多个路径增加 layout 组件时使用。
 * @param redirect 配置路由跳转
 * @param wrappers 配置路由组件的包装组件，通过包装组件可以为当前的路由组件组合进更多的功能。 比如，可以用于路由级别的权限校验
 * @param name 配置路由的标题，默认读取国际化文件 menu.ts 中 menu.xxxx 的值，如配置 name 为 login，则读取 menu.ts 中 menu.login 的取值作为标题
 * @param icon 配置路由的图标，取值参考 https://ant.design/components/icon-cn， 注意去除风格后缀和大小写，如想要配置图标为 <StepBackwardOutlined /> 则取值应为 stepBackward 或 StepBackward，如想要配置图标为 <UserOutlined /> 则取值应为 user 或者 User
 * @doc https://umijs.org/docs/guides/routes
 */
export default [
  {
    path: '/user',
    layout: false,
    routes: [
      {
        name: 'login',
        path: '/user/login',
        component: './user/login',
      },
    ],
  },
  // 以下路由已改为从后端动态加载，由 DynamicRouteLoader 处理
  // {
  //   path: '/welcome',
  //   name: 'welcome',
  //   icon: 'smile',
  //   component: './Welcome',
  // },
  // {
  //   path: '/dashboard',
  //   name: 'dashboard',
  //   icon: 'dashboard',
  //   routes: [
  //     {
  //       path: '/dashboard',
  //       redirect: '/dashboard/analysis',
  //     },
  //     {
  //       name: 'analysis',
  //       icon: 'smile',
  //       path: '/dashboard/analysis',
  //       component: './dashboard/analysis',
  //     },
  //     {
  //       name: 'monitor',
  //       icon: 'smile',
  //       path: '/dashboard/monitor',
  //       component: './dashboard/monitor',
  //     },
  //     {
  //       name: 'workplace',
  //       icon: 'smile',
  //       path: '/dashboard/workplace',
  //       component: './dashboard/workplace',
  //     },
  //   ],
  // },
  // {
  //   path: '/admin',
  //   name: 'admin',
  //   icon: 'crown',
  //   //access: 'canAdmin',
  //   routes: [
  //     {
  //       path: '/admin',
  //       redirect: '/admin/sub-page',
  //     },
  //     {
  //       path: '/admin/sub-page',
  //       name: 'sub-page',
  //       component: './Admin',
  //     },
  //   ],
  // },
  // 设备监控相关路由已改为从后端动态加载
  // 系统设置相关路由已改为从后端动态加载
  // {
  //   path: '/systems',
  //   name: '系统设置',
  //   icon: 'setting',
  //   routes: [
  //     {
  //       path: '/systems/user',
  //       name: '用户管理',
  //       icon: 'user',
  //       component: '@/pages/systems/user',
  //     },
  //     {
  //       path: '/systems/role',
  //       name: '角色管理',
  //       icon: 'setting',
  //       component: '@/pages/systems/role',
  //     },
  //     {
  //       path: '/systems/menu',
  //       name: '菜单管理',
  //       icon: 'menu',
  //       component: '@/pages/systems/menu',
  //     },
  //     {
  //       path: '/systems/dept',
  //       name: '部门管理',
  //       icon: 'setting',
  //       component: '@/pages/systems/dept',
  //     },
  //     {
  //       path: '/systems/company',
  //       name: '公司管理',
  //       icon: 'bank',
  //       component: '@/pages/systems/company',
  //     },
  //   ],
  // },
  // 产线设备管理相关路由已改为从后端动态加载
  // 注意：带动态参数的路由（如 /productionEquipment/company/:companyId）如果后端菜单数据中有配置，也可以动态加载
  // {
  //   path: '/productionEquipment',
  //   name: '产线设备管理',
  //   icon: 'tool',
  //   routes: [
  //     {
  //       path: '/productionEquipment/productionLine',
  //       name: '产线管理',
  //       component: '@/pages/productionEquipment/productionLine',      
  //     },
  //     {
  //       path: '/productionEquipment/equipment',
  //       name: '设备管理',
  //       component: '@/pages/productionEquipment/equipment',      
  //     },
  //     {
  //       path: '/productionEquipment/company/:companyId',
  //       name: '公司详情',
  //       hideInMenu: true,
  //       routes: [
  //         {
  //           path: '/productionEquipment/company/:companyId/productionLine',
  //           name: '产线管理',
  //           component: '@/pages/productionEquipment/productionLine',
  //         },
  //         {
  //           path: '/productionEquipment/company/:companyId/equipment',
  //           name: '设备管理',
  //           component: '@/pages/productionEquipment/equipment',
  //         },
  //       ],
  //     },
  //   ],
  // },
  // 信息追溯相关路由已改为从后端动态加载
  // {
  //   path: '/trace',
  //   name: '信息追溯',
  //   icon: 'table',
  //   routes: [
  //     {
  //       path: '/trace/deviceTraceInfo',
  //       name: '设备信息追溯', 
  //       component: '@/pages/Trace/DeviceTraceInfo',
  //     },
  //     {
  //       path: '/trace/productTraceInfo',
  //       name: '产品信息追溯',
  //       component: '@/pages/Trace/ProductTraceInfo',
  //     },
  //     {
  //       path: '/trace/productionRecords',
  //       name: '产量报表',
  //       component: '@/pages/Trace/ProductionRecords',
  //     }
  //   ],
  // },
  // {
  //   name: '设备信息追溯',
  //   icon: 'table',
  //   path: '/trace',
  //   component: './Trace',
  // },
  // 根路径重定向已改为由后端动态路由处理
  // 如果需要根路径重定向，可以在 DynamicRouteLoader 中处理，或者在后端菜单中配置
  // {
  //   path: '/',
  //   redirect: '/welcome',
  // },
  // 动态路由：所有未匹配的路径都由 DynamicRouteLoader 处理
  // DynamicRouteLoader 会根据后端菜单数据动态加载对应的组件
  // 根路径强制重定向到欢迎页，确保已登录用户或有效 Cookie 进入即显示欢迎页
  {
    path: '/',
    redirect: '/welcome',
  },
  // 豆包AI智能问答/文案生成
  {
    path: '/doubao-chat',
    name: '智能问答',
    icon: 'message',
    component: './doubao-chat',
  },
  {
    component: './DynamicRouteLoader',
    path: '*',
  },
];
