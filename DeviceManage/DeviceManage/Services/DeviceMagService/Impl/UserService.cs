using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Helpers;
using DeviceManage.Models;
using DeviceManage.Services;
using DeviceManage.Services.DeviceMagService;
using DeviceManage.Services.DeviceMagService.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManage.Services.DeviceMagService.Impl
{
    /// <summary>
    /// 用户管理业务实现类
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repo;
        private readonly AppDbContext _db;
        private readonly ILogger<UserService> _logger;

        public UserService(IRepository<User> repo, AppDbContext db, ILogger<UserService> logger)
        {
            _repo = repo;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有用户（分页，排除已删除的用户）
        /// </summary>
        public async Task<PaginatedList<User>> GetAllUsersAsync(UserSearchDto dto)
        {
            var query = _db.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .AsQueryable();

            // 用户名搜索
            if (!string.IsNullOrEmpty(dto.Username))
            {
                query = query.Where(u => u.Username != null && u.Username.Contains(dto.Username));
            }

            // 真实姓名搜索
            if (!string.IsNullOrEmpty(dto.RealName))
            {
                query = query.Where(u => u.RealName != null && u.RealName.Contains(dto.RealName));
            }

            // 角色搜索（使用RoleString字段，因为Role是枚举属性）
            if (!string.IsNullOrEmpty(dto.Role))
            {
                query = query.Where(u => u.RoleString != null && u.RoleString.Contains(dto.Role));
            }

            // 是否启用筛选
            if (dto.IsEnabled.HasValue)
            {
                query = query.Where(u => u.IsEnabled == dto.IsEnabled.Value);
            }

            // 分页查询
            var list = await query.RetrievePagedListAsync(dto.current, dto.pageSize);
            return list;
        }

        /// <summary>
        /// 根据ID获取用户（排除已删除的用户）
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _db.Users
                .Where(u => u.Id == id && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据用户名获取用户（排除已删除的用户）
        /// </summary>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _db.Users
                .Where(u => u.Username == username && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        public async Task<User> AddUserAsync(User user)
        {
            // 验证用户名唯一性
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                var existingUser = await GetUserByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"用户名 '{user.Username}' 已存在，请使用其他用户名。");
                }
            }

            // 对密码进行MD5加密
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                user.Password = MD5Helper.Encrypt(user.Password);
            }

            // 确保软删除字段为false
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = null;

            await _repo.InsertAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task<User> UpdateUserAsync(User user)
        {
            var exist = await _repo.GetAsync(u => u.Id == user.Id && !u.IsDeleted);
            if (exist == null) return user;

            // 验证用户名唯一性（如果用户名发生变化）
            if (!string.IsNullOrWhiteSpace(user.Username) && exist.Username != user.Username)
            {
                var existingUser = await GetUserByUsernameAsync(user.Username);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    throw new InvalidOperationException($"用户名 '{user.Username}' 已存在，请使用其他用户名。");
                }
            }

            exist.Username = user.Username;
            
            // 如果提供了新密码，则进行MD5加密；否则保持原密码不变
            // 注意：从ViewModel传来的密码是原始密码（从PasswordBox获取），需要加密
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                exist.Password = MD5Helper.Encrypt(user.Password);
            }
            // 如果密码为空，保持原密码不变（不更新Password字段）

            exist.Role = user.Role;
            exist.RealName = user.RealName;
            exist.Email = user.Email;
            exist.Phone = user.Phone;
            exist.IsEnabled = user.IsEnabled;
            exist.LastLoginAt = user.LastLoginAt;
            exist.Remarks = user.Remarks;
            exist.UpdatedAt = DateTime.Now;

            _repo.Update(exist);
            await _db.SaveChangesAsync();
            return exist;
        }

        /// <summary>
        /// 软删除用户
        /// </summary>
        public async Task DeleteUserAsync(int id)
        {
            var exist = await _repo.GetAsync(u => u.Id == id && !u.IsDeleted);
            if (exist != null)
            {
                exist.IsDeleted = true;
                exist.DeletedAt = DateTime.Now;
                exist.UpdatedAt = DateTime.Now;

                _repo.Update(exist);
                await _db.SaveChangesAsync();
            }
        }
    }
}

