export interface SysMenu {
    createBy?: string;
    createTime?: string;
    updateBy?: string;
    updateTime?: string;
    remark?: string;
    menuId: number;
    menuName: string;
    parentName?: string;
    parentId?: number;
    orderNum?: number;
    path?: string;
    component?: string;
    query?: string;
    routeName?: string;
    isFrame?: string; // "0"是外链，"1"不是
    isCache?: string; // "0"缓存，"1"不缓存
    menuType: string; // "M"目录，"C"菜单，"F"按钮
    visible?: string; // "0"显示，"1"隐藏
    status?: string; // "0"正常，"1"停用
    perms?: string;
    icon?: string;
    children?: SysMenu[];
    additionalProp1?: string;
    additionalProp2?: string;
    additionalProp3?: string;
}

// 菜单项类型定义
export interface MenuItem {
  menuId: number; // Changed from id to menuId
  parentId?: number;
  menuType: 'M' | 'C' | 'F';
  menuName: string;
  icon?: string;
  orderNum: number;
  isFrame?: '0' | '1';
  path?: string;
  routeName?: string;
  component?: string;
  perms?: string;
  query?: string;
  isCache?: '0' | '1';
  visible?: '0' | '1';
  status?: '0' | '1';
  children?: MenuItem[];
  id?: number; // Keep id as optional for compatibility
}

// 菜单查询参数类型
export interface MenuQueryParams {
  menuName?: string;
  status?: string;
  pageSize?: number;
  current?: number;
}

/** 分页响应结构 */
export interface PagedResult<T> {
  code: number;
  msg: string;
  data: {
    records: T[];
    total: number;
  };
}

/** 树形菜单响应 */
export interface MenuTreeResult {
  code: number;
  msg: string;
  data: MenuItem[];
}