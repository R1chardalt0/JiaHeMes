using System;
using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems;
using ChargePadLine.WebApi.util;

namespace ChargePadLine.WebApi.Controllers.Systems
{
    public class SysCompanyController : BaseController
    {
        private readonly ICompanyService _companyService;

        public SysCompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// 分页查询公司列表
        /// </summary>
        /// <param name="companyName">公司名称</param>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns>公司分页数据</returns>
        [HttpGet]
        public async Task<PagedResp<SysCompany>> GetCompanyPaginationAsync(string? companyName, int current, int pageSize = 50)
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

                var list = await _companyService.PaginationAsync(current, pageSize, companyName);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<SysCompany>();
            }
        }

        /// <summary>
        /// 根据主键获取公司详情
        /// </summary>
        /// <param name="companyId">公司主键</param>
        /// <returns>公司详情</returns>
        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetCompanyById(int companyId)
        {
            var company = await _companyService.GetCompanyByIdAsync(companyId);
            if (company == null)
            {
                return Ok(new { code = 404, msg = "公司不存在" });
            }

            return Ok(new { code = 200, msg = "success", data = company });
        }

        /// <summary>
        /// 新增公司
        /// </summary>
        /// <param name="company">公司实体</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] SysCompany company)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, msg = "参数验证失败", data = ModelState });
            }

			try
			{
				var result = await _companyService.CreateCompanyAsync(company);
				return Ok(new { code = 200, msg = result > 0 ? "创建成功" : "创建失败", data = result });
			}
			catch (InvalidOperationException ex)
			{
				return Ok(new { code = 400, msg = ex.Message });
			}
        }

        /// <summary>
        /// 更新公司
        /// </summary>
        /// <param name="company">公司实体</param>
        /// <returns>操作结果</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateCompany([FromBody] SysCompany company)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { code = 400, msg = "参数验证失败", data = ModelState });
            }

			try
			{
				var result = await _companyService.UpdateCompanyAsync(company);
				return Ok(new { code = 200, msg = result > 0 ? "更新成功" : "更新失败", data = result });
			}
			catch (InvalidOperationException ex)
			{
				return Ok(new { code = 400, msg = ex.Message });
			}
        }

        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="companyId">公司主键</param>
        /// <returns>操作结果</returns>
        [HttpPost("{companyId}")]
        public async Task<IActionResult> DeleteCompany(int companyId)
        {
            var result = await _companyService.DeleteCompanyAsync(companyId);
            return Ok(new { code = 200, msg = result > 0 ? "删除成功" : "删除失败", data = result });
        }
    }
}

