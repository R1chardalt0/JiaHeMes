using Microsoft.Extensions.Configuration;
using ChargePadLine.Common.Md5Module;
using ChargePadLine.Common.TokenModule.Models;
using ChargePadLine.Common.TokenModule;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChargePadLine.DbContexts;

namespace ChargePadLine.Service.Systems.Impl
{
    public class UserService : IUserService
    {
        public IRepository<SysUser> _userRepo { get; set; }
        private readonly IRepository<SysUserRole> _userRoleRepo;
        private readonly IRepository<SysUserPost> _userPostRepo;
        private readonly AppDbContext _dbContext;
        public UserService(
            IRepository<SysUser> userRepo,
            IRepository<SysUserRole> userRoleRepo,
            IRepository<SysUserPost> userPostRepo,
            AppDbContext dbContext)
        {
            this._userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
            _userPostRepo = userPostRepo;
            _dbContext = dbContext;
        }
        public async Task<SysUser> GetCustomerAsync(string userName)
        {
            // 判断输入的是手机号、邮箱还是用户名
            if (IsPhoneNumber(userName))
            {
                // 清理手机号格式（移除空格和连字符）
                var cleanedPhone = CleanPhoneNumber(userName);
                // 根据手机号查询（先尝试原始值，再尝试清理后的值）
                var user = await _userRepo.GetAsync(m => m.PhoneNumber == userName || m.PhoneNumber == cleanedPhone);
                return user;
            }
            else if (IsEmail(userName))
            {
                // 根据邮箱查询（去除首尾空格）
                var trimmedEmail = userName.Trim();
                return await _userRepo.GetAsync(m => m.Email == trimmedEmail);
            }
            else
            {
                // 默认根据用户名查询
                return await _userRepo.GetAsync(m => m.UserName == userName);
            }
        }

        /// <summary>
        /// 清理手机号格式（移除空格、连字符等）
        /// </summary>
        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;
            
            return phone.Trim().Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
        }

        /// <summary>
        /// 判断字符串是否为手机号（使用正则表达式验证：11位有效数字，以1开头，第二位为3-9）
        /// </summary>
        private bool IsPhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            // 移除可能的空格和特殊字符
            var cleaned = CleanPhoneNumber(input);
            
            // 使用正则表达式验证：^1[3-9]\d{9}$
            var phonePattern = @"^1[3-9]\d{9}$";
            return System.Text.RegularExpressions.Regex.IsMatch(cleaned, phonePattern);
        }

        /// <summary>
        /// 判断字符串是否为邮箱（使用正则表达式验证）
        /// </summary>
        private bool IsEmail(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            var trimmed = input.Trim();
            // 标准邮箱正则表达式：^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return System.Text.RegularExpressions.Regex.IsMatch(trimmed, emailPattern);
        }

        public async Task<bool> CheckPassword(UserLoginDto dto)
        {
            var res = await _userRepo.GetAsync(m =>
             m.UserName == dto.UserName && m.Password == dto.PassWord.ToMd5());
            if (res != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<SysUser> GetUserByIdAsync(long id)
        {
            var res = await _userRepo.GetAsync(m => m.UserId == id);
            return res;
        }


        public async Task<PaginatedList<SysUser>> GetUserListAsync(UserQueryDto dto)
        {
            var query = _dbContext.SysUsers.OrderByDescending(s => s.CreateTime).AsQueryable();

            // 过滤用户名
            if (!string.IsNullOrEmpty(dto.UserName))
            {
                query = query.Where(u => u.UserName.Contains(dto.UserName));
            }

            // 过滤状态
            if (!string.IsNullOrEmpty(dto.Status))
            {
                query = query.Where(u => u.Status == dto.Status);
            }

            // 分页查询
            return await query.RetrievePagedListAsync(dto.Current, dto.PageSize);
        }

        public async Task AddUserAsync(UserAddDto dto)
        {
            // 验证用户名唯一性
            var exists = await _userRepo.GetAsync(u => u.UserName == dto.UserName);
            if (exists != null)
            {
                throw new Exception("用户名已存在");
            }

            // 验证手机号唯一性（如果提供了手机号）
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                var phoneExists = await _userRepo.GetAsync(u => u.PhoneNumber == dto.PhoneNumber);
                if (phoneExists != null)
                {
                    throw new Exception("手机号已存在");
                }
            }

            // 验证邮箱唯一性（如果提供了邮箱）
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _userRepo.GetAsync(u => u.Email == dto.Email);
                if (emailExists != null)
                {
                    throw new Exception("邮箱已存在");
                }
            }

            var postIds = (dto.PostIds != null && dto.PostIds.Length > 0)
                ? dto.PostIds
                : (dto.PostId.HasValue ? new long[] { dto.PostId.Value } : Array.Empty<long>());

            var user = new SysUser
            {
                UserName = dto.UserName,
                Password = dto.Password.ToMd5(),
                DeptId = dto.DeptId,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                NickName=dto.NickName,
                Status = dto.Status,
                RoleIds = dto.RoleIds,
                PostIds = postIds.Length > 0 ? postIds : null,
                CreateTime = System.DateTime.Now,
                UpdateTime = System.DateTime.Now
            };

            await _userRepo.InsertAsyncs(user);

            // 处理角色关联
            if (dto.RoleIds != null && dto.RoleIds.Length > 0)
            {
                var userRoles = dto.RoleIds.Select(roleId => new SysUserRole
                {
                    UserId = user.UserId,
                    RoleId = roleId
                }).ToList();

                foreach (var userRole in userRoles)
                {
                    await _userRoleRepo.InsertAsyncs(userRole);
                }
            }

            // 处理岗位关联
            if (postIds.Length > 0)
            {
                foreach (var postId in postIds)
                {
                    var userPost = new SysUserPost
                    {
                        UserId = user.UserId,
                        PostId = postId
                    };
                    await _userPostRepo.InsertAsyncs(userPost);
                }
            }
        }

        public async Task UpdateUserAsync(UserUpdateDto dto)
        {
            var user = await _userRepo.GetAsync(u => u.UserId == dto.UserId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 验证用户名唯一性（排除当前用户）
            if (!string.IsNullOrEmpty(dto.UserName) && dto.UserName != user.UserName)
            {
                var exists = await _userRepo.GetAsync(u => u.UserName == dto.UserName);
                if (exists != null)
                {
                    throw new Exception("用户名已存在");
                }
                user.UserName = dto.UserName;
            }

            // 验证手机号唯一性（排除当前用户）
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
            {
                var phoneExists = await _userRepo.GetAsync(u => u.PhoneNumber == dto.PhoneNumber);
                if (phoneExists != null)
                {
                    throw new Exception("手机号已存在");
                }
            }

            // 验证邮箱唯一性（排除当前用户）
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var emailExists = await _userRepo.GetAsync(u => u.Email == dto.Email);
                if (emailExists != null)
                {
                    throw new Exception("邮箱已存在");
                }
            }

            var postIds = (dto.PostIds != null && dto.PostIds.Length > 0)
                ? dto.PostIds
                : (dto.PostId.HasValue ? new long[] { dto.PostId.Value } : Array.Empty<long>());

            // 更新其他字段
            user.DeptId = dto.DeptId;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Status = dto.Status;
            user.NickName = dto.NickName;
            user.RoleIds = dto.RoleIds;
            user.PostIds = postIds.Length > 0 ? postIds : null;
            user.UpdateTime = System.DateTime.Now;

            await _userRepo.UpdateAsyncs(user);

            // 更新角色关联
            if (dto.RoleIds != null)
            {
                // 删除原有角色关联
                await _userRoleRepo.DeleteAsyncs(ur => ur.UserId == dto.UserId);

                // 添加新的角色关联
                if (dto.RoleIds.Length > 0)
                {
                    var userRoles = dto.RoleIds.Select(roleId => new SysUserRole
                    {
                        UserId = dto.UserId,
                        RoleId = roleId
                    }).ToList();

                    foreach (var userRole in userRoles)
                    {
                        await _userRoleRepo.InsertAsyncs(userRole);
                    }
                }
            }

            // 更新岗位关联
            if (dto.PostIds != null || dto.PostId.HasValue)
            {
                await _userPostRepo.DeleteAsyncs(up => up.UserId == dto.UserId);

                if (postIds.Length > 0)
                {
                    foreach (var postId in postIds)
                    {
                        var userPost = new SysUserPost
                        {
                            UserId = dto.UserId,
                            PostId = postId
                        };
                        await _userPostRepo.InsertAsyncs(userPost);
                    }
                }
            }
        }

        public async Task DeleteUsersAsync(long[] userIds)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 删除用户角色关联
                    await _userRoleRepo.DeleteAsyncs(ur => userIds.Contains(ur.UserId));

                    // 删除用户
                    await _userRepo.DeleteAsyncs(u => userIds.Contains(u.UserId));

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task ResetPasswordAsync(long userId, ResetPasswordDto dto)
        {
            var user = await _userRepo.GetAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            user.Password = dto.NewPassword.ToMd5();
            user.UpdateTime = System.DateTime.Now;

            await _userRepo.UpdateAsyncs(user);
        }

        public async Task ChangeStatusAsync(long userId, string status)
        {
            var user = await _userRepo.GetAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            user.Status = status;
            user.UpdateTime = System.DateTimeOffset.Now;

            await _userRepo.UpdateAsyncs(user);
        }

        public async Task<List<long>> GetUserRolesAsync(long userId)
        {
            var userRoles = await _userRoleRepo.GetListAsync(ur => ur.UserId == userId);
            return userRoles.Select(ur => ur.RoleId).ToList();
        }

        public async Task AuthRolesAsync(long userId, long[] roleIds)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 删除原有角色关联
                    await _userRoleRepo.DeleteAsyncs(ur => ur.UserId == userId);

                    // 添加新的角色关联
                    if (roleIds != null && roleIds.Length > 0)
                    {
                        var userRoles = roleIds.Select(roleId => new SysUserRole
                        {
                            UserId = userId,
                            RoleId = roleId
                        }).ToList();

                        foreach (var userRole in userRoles)
                        {
                            await _userRoleRepo.InsertAsyncs(userRole);
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// 登录成功后更新最后登录时间与IP
        /// </summary>
        public async Task UpdateLastLoginAsync(long userId, string? ip)
        {
            var user = await _userRepo.GetAsync(u => u.UserId == userId);
            if (user == null) return;
            user.LoginDate = DateTimeOffset.Now;
            if (!string.IsNullOrWhiteSpace(ip))
            {
                user.LoginIp = ip.Length <= 64 ? ip : ip.Substring(0, 64);
            }
            await _userRepo.UpdateAsyncs(user);
        }
    }
}
