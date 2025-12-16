using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems
{
    public interface IPostService
    {
        /// <summary>
        /// 获取岗位列表（分页）
        /// </summary>
        Task<PaginatedList<SysPost>> PaginationAsync(int current, int pageSize, string? postName, string? postCode, string? status);

        /// <summary>
        /// 获取岗位详情
        /// </summary>
        Task<SysPost> GetPostById(long postId);

        /// <summary>
        /// 创建岗位
        /// </summary>
        Task<int> CreatePost(SysPost post);

        /// <summary>
        /// 更新岗位
        /// </summary>
        Task<int> UpdatePost(SysPost post);

        /// <summary>
        /// 批量删除岗位
        /// </summary>
        Task<int> DeletePostByIds(long[] postIds);
    }
}

