using ChargePadLine.Entitys.Systems;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems
{
    public interface IMenuService
    {
        Task<List<SysMenu>> SelectMenuById(long menuId);
        Task<List<SysMenu>> SelectMenuList(SysMenu menu, long id);
        Task<List<long>> GetRoleMenuIds(long roleId);
        Task<List<SysMenu>> SelectMenuList(long id);

        // 新增方法
        Task<int> CreateMenu(SysMenu menu);
        Task<int> UpdateMenu(SysMenu menu);
        Task<int> DeleteMenuById(long menuId);
        Task<List<SysMenu>> SelectMenuTree(long userId);
        Task<List<long>> SelectMenuListByRoleId(long roleId);
        Task<PaginatedList<SysMenu>> PaginationAsync(int current, int pageSize, string? menuName, string? status);
    }
}