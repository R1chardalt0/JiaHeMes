// 部门信息接口
interface DeptDto {
    createBy: string;
    createTime: string;
    updateBy: string;
    updateTime: string;
    remark: string;
    deptId: number;
    parentId: number;
    deptName: string;
    orderNum: number;
    leader: string;
    phone: string;
    email: string;
    status: string;
    delFlag: string;
    parentName: string;
    additionalProp1?: string;
    additionalProp2?: string;
    additionalProp3?: string;
}

// 角色信息接口
interface RoleDto {
    createBy: string;
    createTime: string;
    updateBy: string;
    updateTime: string;
    remark: string;
    roleId: number;
    roleName: string;
    roleKey: string;
    roleSort: string;
    dataScope: string;
    menuCheckStrictly: string;
    deptCheckStrictly: string;
    status: string;
    delFlag: string;
    flag: boolean;
    menuIds: number[];
    deptIds: number[];
    additionalProp1?: string;
    additionalProp2?: string;
    additionalProp3?: string;
}

// 用户信息接口
 interface UserDto {
    createBy: string;
    createTime: string;
    updateBy: string;
    updateTime: string;
    remark: string;
    userId: number;
    deptId: number;
    userName: string;
    nickName: string;
    email: string;
    password: string;
    phoneNumber: string;
    avatar: string;
    status: string;
    delFlag: string;
    loginIp: string;
    loginDate: string;
    pwdUpdateDate: string;
    dept: DeptDto;
    roles: RoleDto[];
    roleIds: number[];
    postIds: number[];
    roleId: number;
    additionalProp1?: string;
    additionalProp2?: string;
    additionalProp3?: string;
}


 type CurrentUser = {
    name?: string;
    avatar?: string;
    userid?: string;
    email?: string;
    signature?: string;
    title?: string;
    group?: string;
    tags?: { key?: string; label?: string }[];
    notifyCount?: number;
    unreadCount?: number;
    country?: string;
    access?: string;
    geographic?: {
      province?: { label?: string; key?: string };
      city?: { label?: string; key?: string };
    };
    address?: string;
    phone?: string;
  userName?: string;
  nickName?: string;
  };


// 登录结果接口
export interface LoginResponse {
    code: number;
    msg: string;
    data: string;
    token: string;
    user: UserDto;
    permissions: string[];
    roles: string[];
}

export interface UserLoginDto {
    userName: string;
    passWord: string;
};
