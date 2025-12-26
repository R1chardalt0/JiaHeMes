using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems.Impl
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<SysRole> _roleRepo;
        private readonly IRepository<SysRoleMenu> _roleMenuRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRepository<SysRole> roleRepo, IRepository<SysRoleMenu> roleMenuRepo, AppDbContext dbContext, ILogger<RoleService> logger)
        {
            _roleRepo = roleRepo;
            _roleMenuRepo = roleMenuRepo;
            this._dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        public async Task<PaginatedList<SysRole>> PaginationAsync(int current, int pageSize, string? roleName, string? RoleKey, string? status, DateTime? startTime, DateTime? endTime)
        {
            var query = _dbContext.SysRoles.OrderByDescending(s => s.CreateTime).AsQueryable();
            // 过滤角色名称
            if (!string.IsNullOrEmpty(roleName))
            {
                query = query.Where(r => r.RoleName.Contains(roleName));
            }
            // 过滤角色标识
            if (!string.IsNullOrEmpty(RoleKey))
            {
                query = query.Where(r => r.RoleKey.Contains(RoleKey));
            }
            // 过滤状态
            // 前端传的 status: "0"=禁用, "1"=启用
            // 后端实体 Status: "0"=正常(启用), "1"=停用(禁用)
            // 需要做映射：前端"0"(禁用) -> 后端"1"(停用)，前端"1"(启用) -> 后端"0"(正常)
            if (!string.IsNullOrWhiteSpace(status))
            {
                //string backendStatus;
                //if (status == "0")
                //{
                //    // 前端传"0"(禁用)，查询后端"1"(停用)
                //    backendStatus = "1";
                //}
                //else if (status == "1")
                //{
                //    // 前端传"1"(启用)，查询后端"0"(正常)
                //    backendStatus = "0";
                //}
                //else
                //{
                //    // 其他值直接使用
                //    backendStatus = status;
                //}
                //query = query.Where(r => r.Status != null && r.Status == backendStatus);
                query = query.Where(r => r.Status == status);
            }
            // 过滤创建时间范围
            if (startTime.HasValue)
            {
                query = query.Where(r => r.CreateTime >= startTime.Value);
            }
            if (endTime.HasValue)
            {
                query = query.Where(r => r.CreateTime <= endTime.Value);
            }
            // 分页查询
            var list = await query.RetrievePagedListAsync(current, pageSize);
            return list;
        }
        /// <summary>
        /// 获取角色详情
        /// </summary>
        public async Task<SysRole> GetRoleById(long roleId)
        {
            return await _roleRepo.GetAsync(r => r.RoleId == roleId);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        public async Task<int> CreateRole(SysRole role)
        {
            // 验证角色名称唯一性
            var exists = await _roleRepo.GetAsync(r => r.RoleName == role.RoleName);
            if (exists != null)
            {
                _logger.LogWarning("创建角色失败：角色名称已存在 - {RoleName}", role.RoleName);
                return -1;
            }

            // 保存菜单ID数组（在插入前保存，因为插入后可能需要使用）
            var menuIds = role.MenuIds ?? Array.Empty<long>();
            var menuIdsText = menuIds.Length > 0 ? string.Join(",", menuIds) : "null";
            // 使用控制台输出作为备用日志（确保即使日志被过滤也能看到）
            Console.WriteLine($"[RoleService] 开始创建角色：RoleName={role.RoleName}, RoleKey={role.RoleKey}, MenuIds数量={menuIds.Length}, MenuIds={string.Join(",", menuIds)}");
            
            _logger.LogInformation("开始创建角色：RoleName={RoleName}, RoleKey={RoleKey}, MenuIds数量={MenuCount}, MenuIds={MenuIds}", 
                role.RoleName, role.RoleKey, menuIds.Length, menuIds != null ? string.Join(",", menuIds) : "null");

            // 使用事务确保角色和菜单权限的一致性
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 确保 RoleId 为 0，让数据库自动生成主键
                    role.RoleId = 0;
                    role.CreateTime = DateTimeOffset.Now;
                    role.UpdateTime = DateTimeOffset.Now;
                    
                    // 直接使用 DbContext 添加角色，这样可以更好地控制事务
                    await _dbContext.SysRoles.AddAsync(role);
                    var result = await _dbContext.SaveChangesAsync();
                    
                    Console.WriteLine($"[RoleService] 角色插入完成，受影响行数：{result}, RoleId={role.RoleId}, MenuIds={menuIdsText}");
                    _logger.LogInformation("角色插入完成，受影响行数：{Result}, RoleId={RoleId}, MenuIds={MenuIds}", 
                        result, role.RoleId, menuIdsText);
                    
                    // 如果插入成功，则处理菜单权限关联
                    if (result > 0)
                    {
                        // 获取 RoleId - 使用多种方法确保在 PostgreSQL 中也能正确获取
                        long roleId = role.RoleId;
                        
                        // 方法1：检查 EF Core 是否已自动填充（最常见情况）
                        if (roleId <= 0)
                        {
                            _logger.LogWarning("RoleId未自动填充，尝试其他方法获取。RoleName={RoleName}, RoleKey={RoleKey}", 
                                role.RoleName, role.RoleKey);
                            
                            // 方法2：从实体跟踪中获取（适用于已跟踪的实体）
                            var entry = _dbContext.Entry(role);
                            if (entry != null)
                            {
                                try
                                {
                                    await entry.ReloadAsync();
                                    roleId = role.RoleId;
                                    if (roleId > 0)
                                    {
                                        _logger.LogInformation("通过刷新实体状态获取RoleId：{RoleId}", roleId);
                                    }
                                }
                                catch (Exception reloadEx)
                                {
                                    _logger.LogWarning(reloadEx, "刷新实体状态失败");
                                }
                            }
                            
                            // 方法3：如果还是0，则通过唯一标识查询（在事务内应该能查到）
                            if (roleId <= 0)
                            {
                                try
                                {
                                    var newRole = await _dbContext.SysRoles
                                        .Where(r => r.RoleName == role.RoleName && r.RoleKey == role.RoleKey)
                                        .OrderByDescending(r => r.RoleId)
                                        .FirstOrDefaultAsync();
                                    
                                    if (newRole != null)
                                    {
                                        roleId = newRole.RoleId;
                                        role.RoleId = roleId;
                                        _logger.LogInformation("通过查询获取到RoleId：{RoleId}", roleId);
                                    }
                                    else
                                    {
                                        _logger.LogError("无法通过查询获取新创建角色的RoleId！RoleName={RoleName}, RoleKey={RoleKey}", 
                                            role.RoleName, role.RoleKey);
                                    }
                                }
                                catch (Exception queryEx)
                                {
                                    _logger.LogError(queryEx, "查询RoleId时发生异常");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("RoleId已自动填充：{RoleId}", roleId);
                        }
                        
                        // 处理菜单权限关联
                        if (roleId > 0)
                        {
                            Console.WriteLine($"[RoleService] RoleId获取成功：{roleId}, MenuIds数量：{menuIds?.Length ?? 0}");
                            
                            if (menuIds.Length > 0)
                            {
                                var distinctIds = menuIds.Distinct().Where(id => id > 0).ToArray(); // 过滤掉无效的ID
                                var distinctIdsText = distinctIds.Length > 0 ? string.Join(",", distinctIds) : "null";
                                
                                Console.WriteLine($"[RoleService] 准备创建角色菜单关联，RoleId={roleId}, MenuIds数量={distinctIds.Length}, MenuIds={distinctIdsText}");
                                
                                _logger.LogInformation("准备创建角色菜单关联，RoleId={RoleId}, MenuIds数量={Count}, MenuIds={MenuIds}", 
                                    roleId, distinctIds.Length, distinctIdsText);
                                
                                if (distinctIds.Length > 0)
                                {
                                    try
                                    {
                                        // 批量添加角色菜单关联
                                        var roleMenus = distinctIds.Select(mid => new SysRoleMenu 
                                        { 
                                            RoleId = roleId, 
                                            MenuId = mid 
                                        }).ToList();
                                        
                                        Console.WriteLine($"[RoleService] 准备添加{roleMenus.Count}条角色菜单关联记录到数据库");
                                        _logger.LogInformation("准备添加{Count}条角色菜单关联记录", roleMenus.Count);
                                        
                                        await _dbContext.SysRoleMenu.AddRangeAsync(roleMenus);
                                        
                                        // 同步更新冗余字段 MenuIds
                                        role.MenuIds = distinctIds;
                                        
                                        // 保存菜单关联
                                        var menuResult = await _dbContext.SaveChangesAsync();
                                        
                                        Console.WriteLine($"[RoleService] 角色菜单关联保存完成，受影响行数：{menuResult}, 关联记录数：{roleMenus.Count}");
                                        _logger.LogInformation("角色菜单关联保存完成，受影响行数：{Result}, 关联记录数：{Count}", 
                                            menuResult, roleMenus.Count);
                                        
                                        if (menuResult <= 0)
                                        {
                                            Console.WriteLine($"[RoleService] 错误：角色菜单关联保存失败！RoleId={roleId}, MenuIds数量={distinctIds.Length}, 尝试保存的记录数={roleMenus.Count}");
                                            _logger.LogError("角色菜单关联保存失败！RoleId={RoleId}, MenuIds数量={Count}, 尝试保存的记录数={RecordCount}", 
                                                roleId, distinctIds.Length, roleMenus.Count);
                                        }
                                        else
                                        {
                                            // 验证数据是否真的保存成功
                                            var savedCount = await _dbContext.SysRoleMenu
                                                .CountAsync(rm => rm.RoleId == roleId);
                                            
                                            Console.WriteLine($"[RoleService] 验证：RoleId={roleId}的角色菜单关联记录数={savedCount}");
                                            _logger.LogInformation("验证：RoleId={RoleId}的角色菜单关联记录数={Count}", roleId, savedCount);
                                            
                                            if (savedCount == 0)
                                            {
                                                Console.WriteLine($"[RoleService] 严重错误：角色菜单关联保存后验证失败！RoleId={roleId}，数据库中未找到关联记录！");
                                                _logger.LogError("严重错误：角色菜单关联保存后验证失败！RoleId={RoleId}，数据库中未找到关联记录！", roleId);
                                            }
                                        }
                                    }
                                    catch (Exception menuEx)
                                    {
                                        Console.WriteLine($"[RoleService] 异常：创建角色菜单关联时发生异常！RoleId={roleId}, 错误={menuEx.Message}, 堆栈={menuEx.StackTrace}");
                                        _logger.LogError(menuEx, "创建角色菜单关联时发生异常！RoleId={RoleId}, MenuIds={MenuIds}", 
                                            roleId, distinctIdsText);
                                        throw; // 重新抛出异常，让事务回滚
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"[RoleService] 警告：过滤后的MenuIds为空（可能包含无效ID），跳过创建角色菜单关联");
                                    _logger.LogWarning("过滤后的MenuIds为空（可能包含无效ID），跳过创建角色菜单关联");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[RoleService] 信息：未提供菜单权限（menuIds为空），跳过创建角色菜单关联。MenuIds={menuIdsText}");
                                _logger.LogInformation("未提供菜单权限（menuIds为空），跳过创建角色菜单关联。MenuIds={MenuIds}", 
                                    menuIdsText);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[RoleService] 错误：无法获取有效的RoleId（RoleId={roleId}），无法创建角色菜单关联！");
                            _logger.LogError("无法获取有效的RoleId（RoleId={RoleId}），无法创建角色菜单关联！", roleId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("角色插入失败，受影响行数为0");
                    }
                    
                    await transaction.CommitAsync();
                    Console.WriteLine($"[RoleService] 事务提交成功，角色创建完成：RoleName={role.RoleName}");
                    _logger.LogInformation("事务提交成功，角色创建完成：RoleName={RoleName}", role.RoleName);
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RoleService] 异常：创建角色时发生异常！RoleName={role?.RoleName}, RoleKey={role?.RoleKey}, 错误={ex.Message}, 堆栈={ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[RoleService] 内部异常：{ex.InnerException.Message}, 堆栈={ex.InnerException.StackTrace}");
                    }
                    _logger.LogError(ex, "创建角色时发生异常：RoleName={RoleName}, RoleKey={RoleKey}", 
                        role?.RoleName, role?.RoleKey);
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// 更新角色（同时可更新角色菜单权限映射）
        /// </summary>
        public async Task<int> UpdateRole(SysRole role)
        {
            // 验证角色名称唯一性（排除当前角色）
            var exists = await _roleRepo.GetAsync(r => r.RoleName == role.RoleName && r.RoleId != role.RoleId);
            if (exists != null)
                return -1;

            // 在事务中同时更新角色基础信息与菜单权限映射
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    role.UpdateTime = DateTime.Now;

                    // 1) 更新角色自身字段
                    var result = await _roleRepo.UpdateAsyncs(role);

                    // 2) 如果前端提交了菜单ID，则同步更新 SysRoleMenu
                    // 说明：当前端不携带 MenuIds（例如某些兼容旧接口的调用），则不修改权限映射
                    if (role.MenuIds != null)
                    {
                        var menuIds = role.MenuIds ?? Array.Empty<long>();
                        var distinctIds = menuIds
                            .Where(id => id > 0)
                            .Distinct()
                            .ToArray();

                        // 删除旧的角色菜单映射
                        await _roleMenuRepo.DeleteAsyncs(rm => rm.RoleId == role.RoleId);

                        // 插入新的角色菜单映射
                        foreach (var mid in distinctIds)
                        {
                            await _roleMenuRepo.InsertAsyncs(new SysRoleMenu
                            {
                                RoleId = role.RoleId,
                                MenuId = mid
                            });
                        }

                        // 冗余字段保持一致
                        role.MenuIds = distinctIds;
                        await _roleRepo.UpdateAsyncs(role);
                    }

                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// 批量删除角色
        /// </summary>
        public async Task<int> DeleteRoleByIds(long[] roleIds)
        {
            // 使用DbContext直接管理事务，确保获取正确的事务类型
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 删除角色菜单关联
                    await _roleMenuRepo.DeleteAsyncs(rm => roleIds.Contains(rm.RoleId));

                    // 删除角色
                    var result = await _roleRepo.DeleteAsyncs(r => roleIds.Contains(r.RoleId));

                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// 分配角色菜单权限
        /// </summary>
        public async Task<int> AllocateRoleMenus(long roleId, long[] menuIds)
        {
            // 说明：分配角色菜单的权威写法。事务内先删后插 SysRoleMenu，并同步更新 SysRoles.MenuIds。
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1) 删除原有权限
                    await _roleMenuRepo.DeleteAsyncs(rm => rm.RoleId == roleId);

                    // 2) 批量插入新权限
                    var distinctIds = (menuIds ?? Array.Empty<long>()).Distinct().ToArray();
                    if (distinctIds.Length > 0)
                    {
                        foreach (var mid in distinctIds)
                        {
                            await _roleMenuRepo.InsertAsyncs(new SysRoleMenu { RoleId = roleId, MenuId = mid });
                        }
                    }

                    // 3) 同步冗余字段：更新 SysRoles.MenuIds，作为前端展示与运维排查的冗余
                    var role = await _roleRepo.GetAsync(r => r.RoleId == roleId);
                    if (role != null)
                    {
                        role.MenuIds = distinctIds;
                        await _roleRepo.UpdateAsyncs(role);
                    }

                    await transaction.CommitAsync();
                    return 1; // 返回成功标识
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// 一次性同步所有角色的菜单（将 SysRoles.MenuIds 灌入 SysRoleMenu）。
        /// 用于历史数据修复或异常恢复。
        /// </summary>
        public async Task<int> SyncAllRoleMenus()
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var roles = _dbContext.SysRoles.AsQueryable().ToList();
                int affected = 0;
                foreach (var r in roles)
                {
                    var ids = r.MenuIds ?? Array.Empty<long>();
                    // 删除旧有
                    await _roleMenuRepo.DeleteAsyncs(x => x.RoleId == r.RoleId);
                    // 插入新值
                    foreach (var mid in ids.Distinct())
                    {
                        await _roleMenuRepo.InsertAsyncs(new SysRoleMenu { RoleId = r.RoleId, MenuId = mid });
                        affected++;
                    }
                }
                await transaction.CommitAsync();
                return affected;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        /// <summary>
        /// 获取角色菜单权限
        /// </summary>
        public async Task<List<long>> GetRoleMenuIds(long roleId)
        {
            var roleMenus = await _roleMenuRepo.GetListAsync(rm => rm.RoleId == roleId);
            return roleMenus.Select(rm => rm.MenuId).ToList();
        }
    }
}
