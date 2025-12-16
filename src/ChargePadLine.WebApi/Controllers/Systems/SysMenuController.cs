using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems;
using ChargePadLine.Service.Systems.Impl;
using ChargePadLine.WebApi.util;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace ChargePadLine.WebApi.Controllers.Systems
{   
    public class SysMenuController : BaseController
    {
        public IMenuService _menuService { get; set; }
        public SysMenuController(IMenuService menuService)
        {
            this._menuService = menuService ?? throw new ArgumentNullException(nameof(menuService)); ;
        }

        /// <summary>
        /// 分页查询菜单列表
        /// </summary>
        /// <param name="menuName">菜单名称</param>
        /// <param name="status">菜单状态</param>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数，默认 50</param>
        /// <returns>菜单分页数据</returns>
        [HttpGet]
        public async Task<PagedResp<SysMenu>> GetMenuPaginationAsync(string? menuName, string? status,int current, int pageSize = 50)
        {
            try
            {
                if (current < 1)
                {
                    current = 1;
                }
                if (pageSize < 1)
                {
                    pageSize = 50;
                }
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                var list = await _menuService.PaginationAsync(current, pageSize, menuName, status);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<SysMenu>();
            }
        }

        /// <summary>
        /// 获取菜单列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetMenuList([FromQuery] SysMenu menu)
        {
            var userId = GetUserId(); 
            var list = await _menuService.SelectMenuList(menu, userId);
            return Ok(new { code = 200, msg = "success", data = list });
        }

        /// <summary>
        /// 获取菜单详情
        /// </summary>
        [HttpGet("{menuId}")]
        public async Task<IActionResult> GetMenuById(long menuId)
        {
            var menu = await _menuService.SelectMenuById(menuId);
            return Ok(new { code = 200, msg = "success", data = menu });
        }

        /// <summary>
        /// 获取菜单树形结构
        /// </summary>
        [HttpGet("tree")]
        public async Task<IActionResult> GetMenuTree()
        {
            var userId = GetUserId();
            var tree = await _menuService.SelectMenuTree(userId);
            return Ok(new { code = 200, msg = "success", data = tree });
        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMenu([FromBody] SysMenu menu)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, msg = "参数验证失败", data = ModelState });
            }

            var result = await _menuService.CreateMenu(menu);
            return Ok(new { code = 200, msg = result > 0 ? "创建成功" : "创建失败", data = result });
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateMenu([FromBody] SysMenu menu)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, msg = "参数验证失败", data = ModelState });
            }

            var result = await _menuService.UpdateMenu(menu);
            return Ok(new { code = 200, msg = result > 0 ? "更新成功" : "更新失败", data = result });
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        [HttpPost("{menuId}")]
        public async Task<IActionResult> DeleteMenu(long menuId)
        {
            var result = await _menuService.DeleteMenuById(menuId);
            return Ok(new { code = 200, msg = result > 0 ? "删除成功" : "删除失败", data = result });
        }

        /// <summary>
        /// 获取角色菜单权限
        /// </summary>
        [HttpGet("roleMenuIds/{roleId}")]
        public async Task<IActionResult> GetRoleMenuIds(long roleId)
        {
            var menuIds = await _menuService.SelectMenuListByRoleId(roleId);
            return Ok(new { code = 200, msg = "success", data = menuIds });
        }
    }
}