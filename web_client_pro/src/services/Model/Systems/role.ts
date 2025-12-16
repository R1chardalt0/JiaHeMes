export  interface RoleItem {
  roleId: number;
  roleName: string;
  roleKey: string;
  status: string; // 关键修改点
  roleSort: string;
  dataScope: string;
  menuCheckStrictly: string;
  deptCheckStrictly: string;
  delFlag: string;
  flag: boolean;
  menuIds: number[];
  deptIds: number[];
  remark: string;
  createBy?: string;
  updateBy?: string;
  createTime?: string;
  updateTime?: string;
}

export interface AllocateRoleMenusDto {
    RoleId: number;
    MenuIds: number[];
}
export interface PagedResult<T> {
    code: number;
    msg: string;
    success: boolean; // 添加success属性
    data: T[];       // 直接返回数据数组而非嵌套对象
    total: number;   // 将total提升到根级别
}
export interface CreateRoleFormValues {
    roleId: number;
    roleName: string;
    roleKey: string;
    status: string; // 从number改为string
    roleSort: string;
    dataScope: string;
    menuCheckStrictly: string;
    deptCheckStrictly: string;
    delFlag: string;
    flag: boolean;
    menuIds: number[];
    deptIds: number[];
    remark: string;
}
export interface BaseResponse<T = any> {
  code: number;
  msg: string;
  data: T;
  success?: boolean; // 可选字段
}