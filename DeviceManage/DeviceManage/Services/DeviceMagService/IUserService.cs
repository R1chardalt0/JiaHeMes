using DeviceManage.Models;
using DeviceManage.Services.DeviceMagService.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService
{
    /// <summary>
    /// 用户管理业务接口
    /// </summary>
    public interface IUserService
    {
        Task<PaginatedList<User>> GetAllUsersAsync(UserSearchDto dto);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> AddUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
            /// <summary>
        /// 仅更新用户最后登录时间（不触发操作日志）
        /// </summary>
        Task UpdateLastLoginTimeAsync(int userId);
    }
}

