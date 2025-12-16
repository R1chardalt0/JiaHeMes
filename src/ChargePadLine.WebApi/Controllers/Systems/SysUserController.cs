using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems;
using ChargePadLine.Service.Systems.Dto;
using ChargePadLine.WebApi.util;

namespace ChargePadLine.WebApi.Controllers.Systems
{
   public class SysUserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IDeptService _deptService;

        public SysUserController(IUserService userService, IRoleService roleService, IDeptService deptService)
        {
            _userService = userService;
            _roleService = roleService;
            _deptService = deptService;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="userName">用户名</param>
        /// <param name="status">状态</param>
        /// <returns>用户列表</returns>
        [HttpGet("list")]
        public async Task<PagedResp<SysUser>> GetUserList(int current, int pageSize, string? userName, string? status)
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

                var query = new UserQueryDto
                {
                    Current = current,
                    PageSize = pageSize,
                    UserName = userName,
                    Status = status
                };

                var list = await _userService.GetUserListAsync(query);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<SysUser>();
            }
        }

        /// <summary>
        /// 根据用户编号获取详细信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户详情</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(long userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new { code = 404, msg = "用户不存在" });
                }
                return Ok(new { code = 200, msg = "success", data = user });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="dto">用户信息</param>
        /// <returns>新增结果</returns>
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserAddDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _userService.AddUserAsync(dto);
                return Ok(new { code = 200, msg = "新增用户成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">用户信息</param>
        /// <returns>修改结果</returns>
        [HttpPost("{userId}")]
        public async Task<IActionResult> UpdateUser(long userId, [FromBody] UserUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                dto.UserId = userId;
                await _userService.UpdateUserAsync(dto);
                return Ok(new { code = 200, msg = "修改用户成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userIds">用户ID数组</param>
        /// <returns>删除结果</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteUsers([FromBody] long[] userIds)
        {
            try
            {
                await _userService.DeleteUsersAsync(userIds);
                return Ok(new { code = 200, msg = "删除用户成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="dto">密码信息</param>
        /// <returns>重置结果</returns>
        [HttpPost("{userId}/resetPwd")]
        public async Task<IActionResult> ResetPassword(long userId, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _userService.ResetPasswordAsync(userId, dto);
                return Ok(new { code = 200, msg = "密码重置成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="status">状态(0正常 1停用)</param>
        /// <returns>修改结果</returns>
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(long userId,string status)
        {
            try
            {
                await _userService.ChangeStatusAsync(userId, status);
                return Ok(new { code = 200, msg = "状态修改成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 根据用户编号获取授权角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色列表</returns>
        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(long userId)
        {
            try
            {
                var roles = await _userService.GetUserRolesAsync(userId);
                return Ok(new { code = 200, msg = "success", data = roles });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 用户授权角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleIds">角色ID数组</param>
        /// <returns>授权结果</returns>
        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AuthRoles(long userId, [FromBody] long[] roleIds)
        {
            try
            {
                await _userService.AuthRolesAsync(userId, roleIds);
                return Ok(new { code = 200, msg = "角色授权成功" });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }

        /// <summary>
        /// 获取部门树列表
        /// </summary>
        /// <returns>部门树</returns>
        [HttpGet("deptTree")]
        public async Task<IActionResult> GetDeptTree()
        {
            try
            {
                var deptTree = await _deptService.GetDeptTreeAsync();
                return Ok(new { code = 200, msg = "success", data = deptTree });
            }
            catch (Exception ex)
            {
                return Ok(new { code = 500, msg = ex.Message });
            }
        }
    }
}
