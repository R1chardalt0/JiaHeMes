using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems
{
    public interface IRoleService
    {
        // 获取角色列表
        Task<PaginatedList<SysRole>> PaginationAsync(int current, int pageSize, string? roleName, string? RoleKey, string? status, DateTime? startTime, DateTime? endTime);

        // 获取角色详情
        Task<SysRole> GetRoleById(long roleId);

        // 创建角色
        Task<int> CreateRole(SysRole role);

        // 更新角色
        Task<int> UpdateRole(SysRole role);

        // 删除角色
        Task<int> DeleteRoleByIds(long[] roleIds);

        // 获取角色菜单权限
        Task<List<long>> GetRoleMenuIds(long roleId);

        // 分配角色菜单权限
        Task<int> AllocateRoleMenus(long roleId, long[] menuIds);

        // 一次性同步所有角色的菜单（将 SysRoles.MenuIds 灌入 SysRoleMenu）
        Task<int> SyncAllRoleMenus();
    }
}
