import { request } from '@umijs/max';
import { PagedResult, BaseResponse } from '@/services/Model/Systems/role';
import { UserItem, UserQueryDto, UserAddDto, UserUpdateDto, ResetPasswordDto } from '@/services/Model/Systems/user';

/** 获取用户列表 */
export async function getUserList(
    params: UserQueryDto = { current: 1, pageSize: 10 },
    options?: Record<string, any>
) {
    return request<PagedResult<UserItem>>('/api/SysUser/GetUserList/list', {
        method: 'GET',
        params: {
            current: params.current,
            pageSize: params.pageSize,
            userName: params.userName,
            status: params.status,
            _v: Date.now(), // 防止缓存，确保看到最新的最后登录时间
        },
        ...(options || {}),
    });
}

/** 获取用户详情 */
export async function getUserDetail(userId: number, options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: UserItem;
    }>(`/api/SysUser/GetUserById/${userId}`, {
        method: 'GET',
        ...(options || {}),
    });
}

/** 新增用户 */
export async function createUser(data: UserAddDto, options?: Record<string, any>) {
    return request<BaseResponse<number>>('/api/SysUser/AddUser', {
        method: 'POST',
        data,
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}

/** 更新用户 */
export async function updateUser(userId: number, data: UserUpdateDto, options?: Record<string, any>) {
    return request<BaseResponse<number>>(`/api/SysUser/UpdateUser/${userId}`, {

        method: 'POST',
        data,
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}

/** 删除用户 */
export async function deleteUsers(userIds: number[], options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: number;
    }>('/api/SysUser/DeleteUsers', {
        method: 'POST',
        data: userIds,
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}

/** 重置密码 */
export async function resetPassword(userId: number, data: ResetPasswordDto, options?: Record<string, any>) {
    // 确保发送正确的字段名 NewPassword (PascalCase)，兼容旧代码可能发送的其他字段名
    const dataAny = data as any;
    let passwordValue: string = '';
    
    // 优先使用 NewPassword，如果没有则尝试其他可能的字段名
    if (data.NewPassword !== undefined && data.NewPassword !== null && data.NewPassword !== '') {
        passwordValue = String(data.NewPassword);
    } else if (dataAny.newPassword !== undefined && dataAny.newPassword !== null && dataAny.newPassword !== '') {
        passwordValue = String(dataAny.newPassword);
    } else if (dataAny.password !== undefined && dataAny.password !== null && dataAny.password !== '') {
        passwordValue = String(dataAny.password);
    } else if (dataAny.confirmPassword !== undefined && dataAny.confirmPassword !== null && dataAny.confirmPassword !== '') {
        passwordValue = String(dataAny.confirmPassword);
    }
    
    // 构建正确的请求体，确保字段名是 NewPassword (PascalCase)
    const payload = {
        NewPassword: passwordValue
    };
    
    // 调试日志
    if (process.env.NODE_ENV === 'development') {
        console.log('重置密码API调用:', {
            userId,
            url: `/api/SysUser/ResetPassword/${userId}/resetPwd`,
            originalData: data,
            payload: payload
        });
    }
    
    return request<{
        code: number;
        msg: string;
        data: boolean;
    }>(`/api/SysUser/ResetPassword/${userId}/resetPwd`, {
        method: 'POST',
        data: payload,
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}

/** 修改用户状态 */
export async function changeStatus(userId: number, status: string, options?: Record<string, any>) {
  return request<{
    code: number;
    msg: string;
    data: boolean;
  }>('/api/SysUser/ChangeStatus', {
    method: 'POST',
    params: {
      userId: userId,
      status: String(status)
    },
    ...(options || {}),
  });
}

/** 获取用户角色（做多端点兜底，避免405/415） */
export async function getUserRoles(userId: number, options?: Record<string, any>) {
  // 1. GET + query
  try {
    return await request<{
      code: number;
      msg: string;
      data: number[];
    }>(`/api/SysUser/GetUserRoles`, {
      method: 'GET',
      params: { userId },
      ...(options || {}),
    });
  } catch (err) {
    if (process.env.NODE_ENV === 'development') {
      console.warn('[GetUserRoles] GET ?userId= 失败，尝试 /{userId}', err);
    }
  }

  // 2. GET + path param
  try {
    return await request<{
      code: number;
      msg: string;
      data: number[];
    }>(`/api/SysUser/GetUserRoles/${userId}`, {
      method: 'GET',
      ...(options || {}),
    });
  } catch (err) {
    if (process.env.NODE_ENV === 'development') {
      console.warn('[GetUserRoles] GET /{userId} 失败，尝试 /{userId}/roles', err);
    }
  }

  // 3. GET + /roles
  try {
    return await request<{
      code: number;
      msg: string;
      data: number[];
    }>(`/api/SysUser/GetUserRoles/${userId}/roles`, {
      method: 'GET',
      ...(options || {}),
    });
  } catch (err) {
    if (process.env.NODE_ENV === 'development') {
      console.warn('[GetUserRoles] GET /{userId}/roles 失败，尝试 POST /GetUserRoles', err);
    }
  }

  // 4. POST + query（部分后端这样实现）
  try {
    return await request<{
      code: number;
      msg: string;
      data: number[];
    }>(`/api/SysUser/GetUserRoles`, {
      method: 'POST',
      params: { userId },
      ...(options || {}),
    });
  } catch (err) {
    if (process.env.NODE_ENV === 'development') {
      console.warn('[GetUserRoles] POST ?userId= 失败，尝试 POST /{userId}/roles', err);
    }
  }

  // 5. POST + /{userId}/roles（不带 body，避免415）
  return request<{
    code: number;
    msg: string;
    data: number[];
  }>(`/api/SysUser/GetUserRoles/${userId}/roles`, {
    method: 'POST',
    ...(options || {}),
  });
}

/** 用户授权角色（按后端Swagger：仅使用一个接口） */
export async function authUserRoles(userId: number, roleIds: number[], options?: Record<string, any>) {
  // 按照文档：POST /api/SysUser/AuthRoles/{userId}/roles
  // Body: application/json，纯数字数组
  const payload = (roleIds || []).map((id) => Number(id)).filter((id) => !Number.isNaN(id));
  return request<{
    code: number;
    msg: string;
    data: boolean;
  }>(`/api/SysUser/AuthRoles/${userId}/roles`, {
    method: 'POST',
    data: payload,
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    },
    ...(options || {}),
  });
}

/** 获取部门树 */
export async function getDeptTree(options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: any[]; // 这里可以根据实际部门树结构定义更具体的类型
    }>('/api/SysUser/GetDeptTree/deptTree', {
        method: 'GET',
        ...(options || {}),
    });
}

/** 检查用户名是否已存在 */
export async function checkUserNameExists(userName: string, excludeUserId?: number, options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: boolean; // true表示存在，false表示不存在
    }>('/api/SysUser/CheckUserNameExists', {
        method: 'POST',
        data: { userName, excludeUserId },
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}

/** 检查手机号是否已存在 */
export async function checkPhoneNumberExists(phoneNumber: string, excludeUserId?: number, options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: boolean; // true表示存在，false表示不存在
    }>('/api/SysUser/CheckPhoneNumberExists', {
        method: 'POST',
        data: { phoneNumber, excludeUserId },
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}

/** 检查邮箱是否已存在 */
export async function checkEmailExists(email: string, excludeUserId?: number, options?: Record<string, any>) {
    return request<{
        code: number;
        msg: string;
        data: boolean; // true表示存在，false表示不存在
    }>('/api/SysUser/CheckEmailExists', {
        method: 'POST',
        data: { email, excludeUserId },
        headers: {
            'Content-Type': 'application/json',
        },
        ...(options || {}),
    });
}