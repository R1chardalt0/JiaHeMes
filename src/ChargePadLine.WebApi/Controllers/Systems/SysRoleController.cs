using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems;
using ChargePadLine.Service.Systems.Dto;
using ChargePadLine.WebApi.util;
using Microsoft.Extensions.Logging;

namespace ChargePadLine.WebApi.Controllers.Systems
{
    public class SysRoleController : BaseController
    {

        private readonly IRoleService _roleService;
        private readonly ILogger<SysRoleController> _logger;

        public SysRoleController(IRoleService roleService, ILogger<SysRoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        [HttpGet("list")]
        public async Task<PagedResp<SysRole>> GetRoleList(int current, int pageSize, string? roleName, string? roleKey, DateTime? startTime, DateTime? endTime)
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
                var list = await _roleService.PaginationAsync(current, pageSize, roleName, roleKey, startTime, endTime);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<SysRole>();
            }
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRoleById(long roleId)
        {
            var data = await _roleService.GetRoleById(roleId);
            return Ok(new { code = 200, msg = "success", data });
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] SysRole role)
        {
            try
            {
                _logger.LogInformation("收到创建角色请求：RoleName={RoleName}, RoleKey={RoleKey}, MenuIds={MenuIds}", 
                    role?.RoleName, role?.RoleKey, 
                    role?.MenuIds != null ? string.Join(",", role.MenuIds) : "null");
                
                if (role == null)
                {
                    _logger.LogWarning("创建角色请求：role对象为null");
                    return BadRequest(new { code = 400, msg = "角色数据不能为空" });
                }
                
                var result = await _roleService.CreateRole(role);
                
                _logger.LogInformation("创建角色完成，返回结果：{Result}, RoleId={RoleId}", result, role.RoleId);
                
                return Ok(new { code = 200, msg = result > 0 ? "success" : "fail", data = new { roleId = role.RoleId, result } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建角色时发生异常：RoleName={RoleName}, RoleKey={RoleKey}", 
                    role?.RoleName, role?.RoleKey);
                return StatusCode(500, new { code = 500, msg = "创建角色失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] SysRole role)
        {
            var result = await _roleService.UpdateRole(role);
            return Ok(new { code = 200, msg = result > 0 ? "success" : "fail" });
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteRoleByIds([FromBody] long[] roleIds)
        {
            var result = await _roleService.DeleteRoleByIds(roleIds);
            return Ok(new { code = 200, msg = result > 0 ? "success" : "fail" });
        }

        /// <summary>
        /// 获取角色菜单权限
        /// </summary>
        // 获取指定角色的菜单ID集合
        [HttpGet("menu/{roleId}")]
        public async Task<IActionResult> GetRoleMenuIds(long roleId)
        {
            var menuIds = await _roleService.GetRoleMenuIds(roleId);
            return Ok(new { code = 200, msg = "success", data = menuIds });
        }

        /// <summary>
        /// 分配角色菜单权限（事务内先删后插 SysRoleMenu，并同步 SysRoles.MenuIds）
        /// </summary>
        // 分配角色菜单权限：遵循 BaseController 的路由模式 api/[controller]/[action]
        [HttpPost]
        public async Task<IActionResult> AllocateRoleMenus([FromBody] AllocateRoleMenusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _roleService.AllocateRoleMenus(dto.RoleId, dto.MenuIds);
            return Ok(new { code = 200, msg = "success", data = result });
        }

        /// <summary>
        /// 手动一次性同步所有角色菜单（将 SysRoles.MenuIds 灌入 SysRoleMenu）。
        /// 用于历史数据修复或异常恢复。执行后建议让用户重新登录。
        /// </summary>
        [HttpPost("syncAllRoleMenus")]
        public async Task<IActionResult> SyncAllRoleMenus()
        {
            var affected = await _roleService.SyncAllRoleMenus();
            return Ok(new { code = 200, msg = "success", data = affected });
        }
    }
}
