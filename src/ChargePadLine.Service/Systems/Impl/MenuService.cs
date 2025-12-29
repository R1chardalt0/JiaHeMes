using Microsoft.EntityFrameworkCore;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems.Impl
{
    public class MenuService : IMenuService
    {
        public IRepository<SysMenu> _menuRepo { get; set; }
        public IRepository<SysRoleMenu> _roleMenuRepo { get; set; }
        private readonly AppDbContext _dbContext; // 添加数据库上下文依赖
        public MenuService(IRepository<SysMenu> menuRepo, IRepository<SysRoleMenu> roleMenuRepo, AppDbContext dbContext)
        {
            this._menuRepo = menuRepo;
            this._roleMenuRepo = roleMenuRepo;
            _dbContext = dbContext;
        }
        // ... existing code ...
        public async Task<List<SysMenu>> SelectMenuList(SysMenu menu, long userId)
        {
            // 初始化空列表避免null引用异常
            List<SysMenu> menuList = new List<SysMenu>();
            var query = _menuRepo.GetQueryable();

            // 1. 应用菜单查询参数过滤
            if (menu != null)
            {
                if (menu.MenuId > 0)
                    query = query.Where(m => m.MenuId == menu.MenuId);
                if (!string.IsNullOrEmpty(menu.MenuName))
                    query = query.Where(m => m.MenuName.Contains(menu.MenuName));
                if (!string.IsNullOrEmpty(menu.ParentId.ToString()) && menu.ParentId >= 0)
                    query = query.Where(m => m.ParentId == menu.ParentId);
                if (!string.IsNullOrEmpty(menu.Path))
                    query = query.Where(m => m.Path.Contains(menu.Path));
            }

            // 2. 权限过滤（管理员/普通用户）
            if (SysMenu.IsAdmin(userId))
            {
                // 管理员：不过滤状态和可见性
                menuList = await query.ToListAsync();
            }
            else
            {
                // 普通用户：显示未停用且未隐藏的菜单（兼容空值当作正常/显示）
                menuList = await query
                    .Where(m => (m.Status == null || m.Status == "0") && (m.Visible == null || m.Visible == "0"))
                    .ToListAsync();
            }

            // 3. 统一排序（按父ID升序，再按排序号升序）
            return menuList.OrderBy(m => m.ParentId).ThenBy(m => m.OrderNum).ToList();
        }


        public async Task<List<SysMenu>> SelectMenuById(long menuId)
        {
            return await _menuRepo.GetListAsync(m => m.MenuId == menuId);
        }

        public async Task<List<long>> GetRoleMenuIds(long roleId)
        {
            var roleMenus = await _roleMenuRepo.GetListAsync(rm => rm.RoleId == roleId);
            return roleMenus.Select(rm => rm.MenuId).ToList();
        }

        /// <summary>
        /// 根据用户查询系统菜单列表
        /// 普通用户只能看到其角色拥有权限的菜单
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns></returns>
        public async Task<List<SysMenu>> SelectMenuList(long id)
        {
            if (id == 1) // 假设ID为1的是管理员用户
            {
                // 管理员返回所有菜单
                return await _menuRepo.GetListAsync();
            }
            else
            {
                // 普通用户：需要通过角色获取菜单权限
                // 1. 获取用户的所有角色
                var userRoles = await _dbContext.SysUserRole
                    .Where(ur => ur.UserId == id)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!userRoles.Any())
                {
                    // 用户没有分配角色，返回空列表
                    return new List<SysMenu>();
                }

                // 2. 获取这些角色拥有的菜单权限
                var roleMenuIds = await _dbContext.SysRoleMenu
                    .Where(rm => userRoles.Contains(rm.RoleId))
                    .Select(rm => rm.MenuId)
                    .Distinct()
                    .ToListAsync();

                if (!roleMenuIds.Any())
                {
                    return new List<SysMenu>();
                }

                // 3. 返回用户角色拥有权限的菜单（放宽对可见/状态的限制：null 或 空 视为正常/可见）
                var allMenus = await _menuRepo.GetListAsync();
                var allowed = allMenus
                    .Where(m => roleMenuIds.Contains(m.MenuId) && (string.IsNullOrEmpty(m.Status) || m.Status == "0"))
                    .ToList();

                // 4. 为了让树形结构完整，补齐这些菜单的所有父级目录（目录可不校验状态/可见）
                if (allowed.Any())
                {
                    var allById = allMenus.ToDictionary(x => x.MenuId, x => x);
                    var includeIds = new HashSet<long>(allowed.Select(x => x.MenuId));

                    foreach (var m in allowed)
                    {
                        var pid = m.ParentId;
                        while (pid.HasValue && pid.Value > 0)
                        {
                            if (!includeIds.Add(pid.Value)) break;
                            if (!allById.TryGetValue(pid.Value, out var parent)) break;
                            pid = parent.ParentId;
                        }
                    }

                    allowed = allMenus.Where(x => includeIds.Contains(x.MenuId)).ToList();
                }

                return allowed;
            }
        }

        public async Task<int> CreateMenu(SysMenu menu)
        {
            // 菜单排序默认值处理
            if (menu.OrderNum == 0)
            {
                menu.OrderNum = 99;
            }
            // 设置创建时间
            menu.CreateTime = DateTime.Now;
            return await _menuRepo.InsertAsyncs(menu);
        }

        public async Task<int> UpdateMenu(SysMenu menu)
        {
            // 设置更新时间
            menu.UpdateTime = DateTime.Now;
            return await _menuRepo.UpdateAsyncs(menu);
        }

        public async Task<int> DeleteMenuById(long menuId)
        {
            // 说明：原实现每次递归都会 BeginTransactionAsync()，同一个 DbContext 连接上会触发
            // "The connection is already in a transaction and cannot participate in another transaction."。
            // 修复思路：只开启一次事务；递归删除时复用同一事务/DbContext。

            // 已处于事务中则不再重复开启（例如上层调用已开启事务）
            if (_dbContext.Database.CurrentTransaction != null)
            {
                return await DeleteMenuByIdInternal(menuId);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await DeleteMenuByIdInternal(menuId);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 递归删除菜单（不负责开启/提交事务）
        /// </summary>
        private async Task<int> DeleteMenuByIdInternal(long menuId)
        {
            var affected = 0;

            // 删除子菜单（先删子再删父）
            var children = await _menuRepo.GetListAsync(m => m.ParentId == menuId);
            foreach (var child in children)
            {
                affected += await DeleteMenuByIdInternal(child.MenuId);
            }

            // 删除菜单角色关联
            affected += await _roleMenuRepo.DeleteAsyncs(rm => rm.MenuId == menuId);

            // 删除当前菜单
            affected += await _menuRepo.DeleteAsyncs(m => m.MenuId == menuId);

            return affected;
        }

        public async Task<List<SysMenu>> SelectMenuTree(long userId)
        {
            var menuList = await SelectMenuList(userId);
            return BuildMenuTree(menuList, 0);
        }

        // 构建菜单树形结构
        private List<SysMenu> BuildMenuTree(List<SysMenu> menuList, long parentId)
        {
            var tree = new List<SysMenu>();
            var children = menuList.Where(m => (m.ParentId ?? 0) == parentId).OrderBy(m => m.OrderNum).ToList();

            foreach (var child in children)
            {
                child.Children = BuildMenuTree(menuList, child.MenuId);
                tree.Add(child);
            }
            return tree;
        }

        public async Task<List<long>> SelectMenuListByRoleId(long roleId)
        {
            return await GetRoleMenuIds(roleId);
        }

        public async Task<PaginatedList<SysMenu>> PaginationAsync(int current, int pageSize, string? menuName, string? status)
        {
            var query = this._dbContext.SysMenus.OrderByDescending(s => s.CreateTime).AsQueryable();
            // 过滤菜单名称
            if (!string.IsNullOrEmpty(menuName))
            {
                query = query.Where(r => r.MenuName.Contains(menuName));
            }
            
            // 过滤菜单状态
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status.Contains(status));
            }
            // 分页查询
            var list = await query.RetrievePagedListAsync(current, pageSize);
            return list;
        }
    }
}