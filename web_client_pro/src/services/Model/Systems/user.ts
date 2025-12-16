// 用户信息接口定义
export interface UserItem {
  userId: number;
  userName: string;
  nickName: string;
  deptId: number;
  deptName?: string;
  postId?: number;
  postName?: string;
  roleIds?: number[];
  roleNames?: string;
  email?: string;
  phoneNumber?: string;
  status: string;
  createTime: string;
  createBy?: string;
  updateTime?: string;
  updateBy?: string;
}

// 用户查询参数
export interface UserQueryDto {
  current?: number;
  pageSize?: number;
  userName?: string;
  status?: string;
}

// 用户添加参数
export interface UserAddDto {
  userName: string;
  password: string;
  deptId: number;
  postId?: number;
  roleIds: number[];
  email?: string;
  phone?: string;
  status: string;
}

// 用户更新参数
export interface UserUpdateDto extends Omit<UserItem, 'userId'> {
  userId?: number;
  // 其他可能的字段...
}

// 重置密码参数
export interface ResetPasswordDto {
  NewPassword: string;
}