using DeviceManage.DBContext;
using DeviceManage.DBContext.Repository;
using DeviceManage.Helpers;
using DeviceManage.Models;
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
        private readonly ILogService _logService;
        private readonly ILogger<UserService> _logger;

        public UserService(IRepository<User> repo,
                           AppDbContext db,
                           ILogService logService,
                           ILogger<UserService> logger)
        {
            _repo = repo;
            _db = db;
            _logService = logService;
            _logger = logger;
        }

        #region 查询
        public async Task<PaginatedList<User>> GetAllUsersAsync(UserSearchDto dto)
        {
            var query = _db.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dto.Username))
                query = query.Where(u => u.Username != null && u.Username.Contains(dto.Username));
            if (!string.IsNullOrEmpty(dto.RealName))
                query = query.Where(u => u.RealName != null && u.RealName.Contains(dto.RealName));
            if (!string.IsNullOrEmpty(dto.Role))
                query = query.Where(u => u.RoleString != null && u.RoleString.Contains(dto.Role));
            if (dto.IsEnabled.HasValue)
                query = query.Where(u => u.IsEnabled == dto.IsEnabled.Value);

            return await query.RetrievePagedListAsync(dto.current, dto.pageSize);
        }

        public async Task<User?> GetUserByIdAsync(int id)
            => await _db.Users.Where(u => u.Id == id && !u.IsDeleted).FirstOrDefaultAsync();

        public async Task<User?> GetUserByUsernameAsync(string username)
            => await _repo.GetAsync(u => u.Username == username && !u.IsDeleted);
        #endregion

        #region 新增
        public async Task<User> AddUserAsync(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                var existing = await GetUserByUsernameAsync(user.Username);
                if (existing != null)
                    throw new InvalidOperationException($"用户名 '{user.Username}' 已存在。");
            }

            if (!string.IsNullOrWhiteSpace(user.Password))
                user.Password = MD5Helper.Encrypt(user.Password);

            user.IsDeleted = false;
            user.CreatedAt = DateTime.Now;

            await _repo.InsertAsync(user);
            await _db.SaveChangesAsync();

            await _logService.LogAsync(CurrentUserContext.UserId,
                                        CurrentUserContext.Username,
                                        OperationType.Create,
                                        "用户管理",
                                        $"新增用户：{user.Username} (ID:{user.Id})");
            return user;
        }
        #endregion

        #region 更新
        public async Task<User> UpdateUserAsync(User user)
        {
            var exist = await _repo.GetAsync(u => u.Id == user.Id && !u.IsDeleted);
            if (exist == null) return user;

            if (!string.IsNullOrWhiteSpace(user.Username) && exist.Username != user.Username)
            {
                var dup = await GetUserByUsernameAsync(user.Username);
                if (dup != null && dup.Id != user.Id)
                    throw new InvalidOperationException($"用户名 '{user.Username}' 已存在。");
            }

            exist.Username = user.Username;
            if (!string.IsNullOrWhiteSpace(user.Password))
                exist.Password = MD5Helper.Encrypt(user.Password);
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

            await _logService.LogAsync(CurrentUserContext.UserId,
                                        CurrentUserContext.Username,
                                        OperationType.Update,
                                        "用户管理",
                                        $"修改用户：{exist.Username} (ID:{exist.Id})");
            return exist;
        }
        #endregion

        #region 删除
        public async Task DeleteUserAsync(int id)
        {
            var exist = await _repo.GetAsync(u => u.Id == id && !u.IsDeleted);
            if (exist == null) return;

            exist.IsDeleted = true;
            exist.DeletedAt = DateTime.Now;
            exist.UpdatedAt = DateTime.Now;

            _repo.Update(exist);
            await _db.SaveChangesAsync();

            await _logService.LogAsync(CurrentUserContext.UserId,
                                        CurrentUserContext.Username,
                                        OperationType.Delete,
                                        "用户管理",
                                        $"删除用户：{exist.Username} (ID:{id})");
        }
        #endregion

        /// <summary>
        /// 仅更新最后登录时间，不记录操作日志
        /// </summary>
        public async Task UpdateLastLoginTimeAsync(int userId)
        {
            var user = await _repo.GetAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return;
            user.LastLoginAt = DateTime.Now;
            _repo.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
