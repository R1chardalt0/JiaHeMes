using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Systems;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems.Impl
{
    /// <summary>
    /// 岗位服务实现
    /// </summary>
    public class PostService : IPostService
    {
        private readonly IRepository<SysPost> _postRepo;
        private readonly AppDbContext _dbContext;

        public PostService(IRepository<SysPost> postRepo, AppDbContext dbContext)
        {
            _postRepo = postRepo;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取岗位列表（分页）
        /// </summary>
        public async Task<PaginatedList<SysPost>> PaginationAsync(int current, int pageSize, string? postName, string? postCode, string? status)
        {
            var query = _dbContext.SysPosts.OrderByDescending(s => s.CreateTime).AsQueryable();
            
            // 过滤岗位名称
            if (!string.IsNullOrEmpty(postName))
            {
                query = query.Where(p => p.PostName != null && p.PostName.Contains(postName));
            }
            
            // 过滤岗位编码
            if (!string.IsNullOrEmpty(postCode))
            {
                query = query.Where(p => p.PostCode != null && p.PostCode.Contains(postCode));
            }
            
            // 过滤状态
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }
            
            // 分页查询
            var list = await query.RetrievePagedListAsync(current, pageSize);
            return list;
        }

        /// <summary>
        /// 获取岗位详情
        /// </summary>
        public async Task<SysPost> GetPostById(long postId)
        {
            return await _postRepo.GetAsync(p => p.PostId == postId);
        }

        /// <summary>
        /// 创建岗位
        /// </summary>
        public async Task<int> CreatePost(SysPost post)
        {
            // 验证岗位编码唯一性
            if (!string.IsNullOrEmpty(post.PostCode))
            {
                var exists = await _postRepo.GetAsync(p => p.PostCode == post.PostCode);
                if (exists != null)
                    return -1;
            }

            // 验证岗位名称唯一性
            if (!string.IsNullOrEmpty(post.PostName))
            {
                var exists = await _postRepo.GetAsync(p => p.PostName == post.PostName);
                if (exists != null)
                    return -2;
            }

            // 确保状态值正确：必须是字符串 '0'（正常/启用）或 '1'（停用）
            if (string.IsNullOrEmpty(post.Status))
            {
                post.Status = "0"; // 默认启用
            }
            else
            {
                // 确保状态值是有效的字符串值
                post.Status = post.Status.Trim();
                if (post.Status != "0" && post.Status != "1")
                {
                    // 如果不是有效的状态值，尝试转换
                    if (post.Status.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                        post.Status.Equals("f", StringComparison.OrdinalIgnoreCase) ||
                        post.Status.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        post.Status = "1"; // 默认为停用
                    }
                    else
                    {
                        post.Status = "0"; // 默认为启用
                    }
                }
            }

            // 确保创建时间正确设置（使用 DateTimeOffset 以确保时区信息正确）
            if (!post.CreateTime.HasValue)
            {
                post.CreateTime = DateTimeOffset.Now;
            }
            post.UpdateTime = DateTimeOffset.Now;
            
            // 重置主键值，让数据库自动生成
            post.PostId = 0;
            
            return await _postRepo.InsertAsyncs(post);
        }

        /// <summary>
        /// 更新岗位
        /// </summary>
        public async Task<int> UpdatePost(SysPost post)
        {
            // 验证PostId是否有效
            if (post.PostId <= 0)
            {
                return 0; // PostId无效，返回0表示更新失败
            }

            // 先查询现有记录
            var existing = await _postRepo.GetAsync(p => p.PostId == post.PostId);
            if (existing == null)
            {
                return 0; // 记录不存在，返回0表示更新失败
            }

            // 验证岗位编码唯一性（排除当前岗位）
            if (!string.IsNullOrEmpty(post.PostCode))
            {
                var exists = await _postRepo.GetAsync(p => p.PostCode == post.PostCode && p.PostId != post.PostId);
                if (exists != null)
                    return -1;
            }

            // 验证岗位名称唯一性（排除当前岗位）
            if (!string.IsNullOrEmpty(post.PostName))
            {
                var exists = await _postRepo.GetAsync(p => p.PostName == post.PostName && p.PostId != post.PostId);
                if (exists != null)
                    return -2;
            }

            // 确保状态值正确：必须是字符串 '0'（正常/启用）或 '1'（停用）
            if (string.IsNullOrEmpty(post.Status))
            {
                post.Status = "0"; // 默认启用
            }
            else
            {
                // 确保状态值是有效的字符串值
                post.Status = post.Status.Trim();
                if (post.Status != "0" && post.Status != "1")
                {
                    // 如果不是有效的状态值，尝试转换
                    if (post.Status.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                        post.Status.Equals("f", StringComparison.OrdinalIgnoreCase) ||
                        post.Status.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        post.Status = "1"; // 默认为停用
                    }
                    else
                    {
                        post.Status = "0"; // 默认为启用
                    }
                }
            }

            // 更新时保留原有的创建时间和创建者
            post.CreateTime = existing.CreateTime;
            post.CreateBy = existing.CreateBy;
            
            // 更新更新时间
            post.UpdateTime = DateTimeOffset.Now;
            
            // 更新字段
            existing.PostCode = post.PostCode;
            existing.PostName = post.PostName;
            existing.PostSort = post.PostSort;
            existing.Status = post.Status;
            existing.Remark = post.Remark;
            existing.UpdateTime = post.UpdateTime;
            
            // 使用现有实体更新，避免跟踪问题
            return await _postRepo.UpdateAsyncs(existing);
        }

        /// <summary>
        /// 批量删除岗位
        /// </summary>
        public async Task<int> DeletePostByIds(long[] postIds)
        {
            // 检查是否有用户使用这些岗位
            var userPosts = await _dbContext.SysUserPost
                .Where(up => postIds.Contains(up.PostId))
                .ToListAsync();
            
            if (userPosts.Any())
            {
                // 返回 -1 表示有用户使用此岗位，不能删除
                return -1;
            }

            // 删除岗位
            var result = await _postRepo.DeleteAsyncs(p => postIds.Contains(p.PostId));
            return result;
        }
    }
}

