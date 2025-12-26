using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Service.Systems;
using ChargePadLine.Entitys.Systems;
using System;
using ChargePadLine.WebApi.util;

namespace ChargePadLine.WebApi.Controllers.Systems
{
    public class SysDeptController : BaseController
    {
        private readonly IDeptService _deptService;
        public SysDeptController(IDeptService deptService)
        {
            _deptService = deptService ?? throw new ArgumentNullException(nameof(deptService));
        }

        /// <summary>
        /// 获取部门分页列表
        /// </summary>
        [HttpGet]
        public async Task<PagedResp<SysDept>> GetDeptPaginationAsync(string? deptName, int? orderNum, string? status, int current, int pageSize = 50)
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
                var list = await _deptService.PaginationAsync(current, pageSize, deptName, orderNum, status);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<SysDept>();
            }
        }

        /// <summary>
        /// 获取部门列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetDeptList([FromQuery] SysDept dept)
        {
            var userId = GetUserId();
            var list = await _deptService.SelectDeptList(dept, userId);
            return Ok(new { code = 200, msg = "success", data = list });
        }

        /// <summary>
        /// 获取部门详情
        /// </summary>
        [HttpGet("{deptId}")]
        public async Task<IActionResult> GetDeptById(long deptId)
        {
            var dept = await _deptService.SelectDeptById(deptId);
            return Ok(new { code = 200, msg = "success", data = dept });
        }

        /// <summary>
        /// 获取部门树形结构
        /// </summary>
        [HttpGet("tree")]
        public async Task<IActionResult> GetDeptTree()
        {
            var tree = await _deptService.GetDeptTreeAsync();
            return Ok(new { code = 200, msg = "success", data = tree });
        }

        /// <summary>
        /// 根据用户ID获取部门树
        /// </summary>
        [HttpGet("selectTree")]
        public async Task<IActionResult> GetSelectDeptTree()
        {
            var userId = GetUserId();
            var tree = await _deptService.SelectDeptTree(userId);
            return Ok(new { code = 200, msg = "success", data = tree });
        }

        /// <summary>
        /// 创建部门
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDept([FromBody] SysDept dept)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, msg = "参数验证失败", data = ModelState });
            }

            var result = await _deptService.CreateDept(dept);
            return Ok(new { code = 200, msg = result > 0 ? "创建成功" : "创建失败", data = result });
        }

        /// <summary>
        /// 更新部门
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateDept([FromBody] SysDept dept)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, msg = "参数验证失败", data = ModelState });
            }

            var result = await _deptService.UpdateDept(dept);
            return Ok(new { code = 200, msg = result > 0 ? "更新成功" : "更新失败", data = result });
        }

        /// <summary>
        /// 删除部门
        /// </summary>
        [HttpPost("{deptId}")]
        public async Task<IActionResult> DeleteDept(long deptId)
        {
            var result = await _deptService.DeleteDeptById(deptId);
            return Ok(new { code = 200, msg = result > 0 ? "删除成功" : "删除失败", data = result });
        }
    }
}
