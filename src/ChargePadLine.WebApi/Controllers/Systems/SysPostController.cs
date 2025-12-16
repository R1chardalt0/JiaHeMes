using Microsoft.AspNetCore.Mvc;
using ChargePadLine.Entitys.Systems;
using ChargePadLine.Service.Systems;
using ChargePadLine.WebApi.util;

namespace ChargePadLine.WebApi.Controllers.Systems
{
    [Route("api/[controller]")]
    public class SysPostController : BaseController
    {
        private readonly IPostService _postService;

        public SysPostController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// 获取岗位列表（分页）
        /// </summary>
        [HttpGet("GetPostList/list")]
        public async Task<PagedResp<SysPost>> GetPostList(int current, int pageSize, string? postName, string? postCode, string? status)
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
                var list = await _postService.PaginationAsync(current, pageSize, postName, postCode, status);
                return RespExtensions.MakePagedSuccess(list);
            }
            catch
            {
                return RespExtensions.MakePagedEmpty<SysPost>();
            }
        }

        /// <summary>
        /// 获取岗位详情
        /// </summary>
        [HttpGet("GetPostById/{postId}")]
        public async Task<IActionResult> GetPostById(long postId)
        {
            var data = await _postService.GetPostById(postId);
            return Ok(new { code = 200, msg = "success", data });
        }

        /// <summary>
        /// 创建岗位
        /// </summary>
        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromBody] SysPost post)
        {
            var result = await _postService.CreatePost(post);
            if (result == -1)
            {
                return Ok(new { code = 400, msg = "岗位编码已存在" });
            }
            if (result == -2)
            {
                return Ok(new { code = 400, msg = "岗位名称已存在" });
            }
            return Ok(new { code = 200, msg = result > 0 ? "success" : "fail", data = result });
        }

        /// <summary>
        /// 更新岗位
        /// </summary>
        [HttpPost("UpdatePost")]
        public async Task<IActionResult> UpdatePost([FromBody] SysPost post)
        {
            var result = await _postService.UpdatePost(post);
            if (result == -1)
            {
                return Ok(new { code = 400, msg = "岗位编码已存在" });
            }
            if (result == -2)
            {
                return Ok(new { code = 400, msg = "岗位名称已存在" });
            }
            return Ok(new { code = 200, msg = result > 0 ? "success" : "fail", data = result });
        }

        /// <summary>
        /// 批量删除岗位
        /// </summary>
        [HttpPost("DeletePostByIds")]
        public async Task<IActionResult> DeletePostByIds([FromBody] long[] postIds)
        {
            var result = await _postService.DeletePostByIds(postIds);
            if (result == -1)
            {
                return Ok(new { code = 400, msg = "岗位正在使用中，无法删除" });
            }
            return Ok(new { code = 200, msg = result > 0 ? "success" : "fail", data = result });
        }
    }
}
