using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems
{
    public interface IUserService
    {
        /// <summary>
        /// 根据用户名获取用户信息（用于登录或校验）
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>用户实体</returns>
        Task<bool> CheckPassword(UserLoginDto dto);

        /// <summary>
        /// 校验用户名和密码是否正确
        /// </summary>
        /// <param name="dto">登录数据传输对象</param>
        /// <returns>是否验证通过</returns>
        Task<SysUser> GetCustomerAsync(string userName);

        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户实体</returns>
        Task<SysUser> GetUserByIdAsync(long id);

        /// <summary>
        /// 分页查询用户列表
        /// </summary>
        /// <param name="dto">查询条件</param>
        /// <returns>分页用户列表</returns>
        Task<PaginatedList<SysUser>> GetUserListAsync(UserQueryDto dto);

        /// <summary>
        /// 添加新用户
        /// </summary>
        /// <param name="dto">添加用户数据传输对象</param>
        /// <returns></returns>
        Task AddUserAsync(UserAddDto dto);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="dto">更新用户数据传输对象</param>
        /// <returns></returns>
        Task UpdateUserAsync(UserUpdateDto dto);

        /// <summary>
        /// 批量删除用户（含角色关联）
        /// </summary>
        /// <param name="userIds">用户ID数组</param>
        /// <returns></returns>
        Task DeleteUsersAsync(long[] userIds);

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">重置密码DTO</param>
        /// <returns></returns>
        Task ResetPasswordAsync(long userId, ResetPasswordDto dto);

        /// <summary>
        /// 更改用户状态（如启用/禁用）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="status">新状态值</param>
        /// <returns></returns>
        Task ChangeStatusAsync(long userId, string status);

        /// <summary>
        /// 获取用户拥有的角色ID列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色ID列表</returns>
        Task<List<long>> GetUserRolesAsync(long userId);

        /// <summary>
        /// 为用户授权角色（覆盖式授权）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleIds">角色ID数组</param>
        /// <returns></returns>
        Task AuthRolesAsync(long userId, long[] roleIds);

        /// <summary>
        /// 登录成功后更新最后登录时间与IP
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="ip">登录IP</param>
        Task UpdateLastLoginAsync(long userId, string? ip);
    }
}
