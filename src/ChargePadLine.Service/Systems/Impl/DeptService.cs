using ChargePadLine.DbContexts.Repository;
using ChargePadLine.DbContexts;
using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChargePadLine.Service.Systems.Impl
{
    /// <summary>
    /// 部门服务实现
    /// </summary>
    public class DeptService : IDeptService
    {
        private readonly IRepository<SysDept> _deptRepo;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="deptRepo">部门仓储</param>
        /// <param name="dbContext">数据库上下文</param>
        public DeptService(IRepository<SysDept> deptRepo, AppDbContext dbContext)
        {
            _deptRepo = deptRepo;
            _dbContext = dbContext;
        }


        /// <summary>
        /// 获取所有部门列表
        /// </summary>
        /// <returns>部门列表</returns>
        private async Task<List<SysDept>> SelectDeptList()
        {
            return await _deptRepo.GetListAsync();
        }

        /// <summary>
        /// 获取部门树列表
        /// </summary>
        /// <returns>部门树列表</returns>
        public async Task<List<SysDept>> GetDeptTreeAsync()
        {
            var deptList = await SelectDeptList();
            return BuildDeptTree(deptList, 0);
        }

        /// 构建部门树形结构
        /// </summary>
        /// <param name="deptList">部门列表</param>
        /// <param name="parentId">父部门ID</param>
        /// <returns>部门树</returns>
        private List<SysDept> BuildDeptTree(List<SysDept> deptList, long? parentId)
        {
            var tree = new List<SysDept>();
            var children = deptList
                .Where(m => (m.ParentId == parentId) || (parentId == 0 && m.ParentId == null))
                .OrderBy(m => m.OrderNum)
                .ToList();

            foreach (var child in children)
            {
                child.Children = BuildDeptTree(deptList, child.DeptId);
                tree.Add(child);
            }
            return tree;
        }

        /// <summary>
        /// 查询部门分页列表
        /// </summary>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="deptName"></param>
        /// <param name="orderNum"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<PaginatedList<SysDept>> PaginationAsync(int current, int pageSize, string? deptName, int? orderNum, string? status)
        {
            var query = this._dbContext.Set<SysDept>().OrderByDescending(s => s.CreateTime).AsQueryable();
            // 过滤部门名称
            if (!string.IsNullOrEmpty(deptName))
            {
                query = query.Where(r => r.DeptName.Contains(deptName));
            }

            // 过滤部门排序
            if (orderNum.HasValue)
            {
                query = query.Where(r => r.OrderNum == orderNum.Value);
            }

            // 过滤部门状态
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }
            // 分页查询
            var list = await query.RetrievePagedListAsync(current, pageSize);
            return list;
        }

        /// <summary>
        /// 根据部门ID查询部门信息
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        public async Task<List<SysDept>> SelectDeptById(long deptId)
        {
            return await _deptRepo.GetListAsync(m => m.DeptId == deptId);
        }

        /// <summary>
        /// 根据条件查询部门列表
        /// </summary>
        /// <param name="dept"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<SysDept>> SelectDeptList(SysDept dept, long id)
        {
            // 初始化空列表避免null引用异常
            List<SysDept> deptList = new List<SysDept>();
            var query = _deptRepo.GetQueryable();

            // 1. 应用部门查询参数过滤
            if (dept != null)
            {
                if (dept.DeptId > 0)
                    query = query.Where(m => m.DeptId == dept.DeptId);
                if (!string.IsNullOrEmpty(dept.DeptName))
                    query = query.Where(m => m.DeptName.Contains(dept.DeptName));
                if (dept.ParentId.HasValue && dept.ParentId >= 0)
                    query = query.Where(m => m.ParentId == dept.ParentId);
                if (!string.IsNullOrEmpty(dept.Status))
                    query = query.Where(m => m.Status == dept.Status);
            }

            // 2. 权限过滤（管理员/普通用户）
            if (id == 1) // 假设ID为1的是管理员用户
            {
                // 管理员：不过滤状态
                deptList = await query.ToListAsync();

            }
            else
            {
                // 普通用户：仅显示状态正常(0)的部门
                deptList = await query
                    .Where(m => m.Status == "0")
                    .ToListAsync();
            }

            // 3. 统一排序（按父ID升序，再按排序号升序）
            return deptList.OrderBy(m => m.ParentId).ThenBy(m => m.OrderNum).ToList();
        }

        /// <summary>
        /// 根据用户ID查询部门列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<SysDept>> SelectDeptList(long id)
        {
            if (id == 1) // 假设ID为1的是管理员用户
            {
                // 管理员返回所有部门
                return await _deptRepo.GetListAsync();
            }
            else
            {
                // 普通用户返回状态正常(0)的部门
                return await _deptRepo.GetListAsync(m => m.Status == "0");
            }
        }

        /// <summary>
        /// 创建部门
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public async Task<int> CreateDept(SysDept dept)
        {
            // 部门排序默认值处理
            if (dept.OrderNum == 0 || !dept.OrderNum.HasValue)
            {
                dept.OrderNum = 99;
            }  
            // 重置主键值，让数据库自动生成
            dept.DeptId = 0;
            // 设置创建时间
            dept.CreateTime = DateTime.Now;
            return await _deptRepo.InsertAsyncs(dept);
        }

        /// <summary>
        /// 更新部门
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public async Task<int> UpdateDept(SysDept dept)
        {
            // 设置更新时间
            dept.UpdateTime = DateTime.Now;
            return await _deptRepo.UpdateAsyncs(dept);
        }

        /// <summary>
        /// 删除部门及其子部门
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        public async Task<int> DeleteDeptById(long deptId)
        {
            // 使用数据库上下文获取事务
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 删除子部门
                    var children = await _deptRepo.GetListAsync(m => m.ParentId == deptId);
                    foreach (var child in children)
                    {
                        await DeleteDeptById(child.DeptId);
                    }

                    // 删除当前部门
                    var result = await _deptRepo.DeleteAsyncs(m => m.DeptId == deptId);

                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// 选择部门树 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<SysDept>> SelectDeptTree(long userId)
        {
            var deptList = await SelectDeptList(userId);
            return BuildDeptTree(deptList, 0);
        }
    }
}
