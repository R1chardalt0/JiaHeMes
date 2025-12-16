using ChargePadLine.Entitys.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Systems
{
    /// <summary>
    /// 部门服务接口
    /// </summary>
    public interface IDeptService
    {
        /// <summary>
        /// 获取部门树列表
        /// </summary>
        /// <returns>部门树列表</returns>
        Task<List<SysDept>> GetDeptTreeAsync();

        /// <summary>
        /// 根据条件分页获取部门列表
        /// </summary>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="deptName"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<PaginatedList<SysDept>> PaginationAsync(int current, int pageSize, string? deptName, string? status);

        /// <summary>
        /// 根据部门ID获取部门信息
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        Task<List<SysDept>> SelectDeptById(long deptId);

        /// <summary>
        /// 根据条件获取部门列表
        /// </summary>
        /// <param name="dept"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<SysDept>> SelectDeptList(SysDept dept, long id);

        /// <summary>
        /// 根据部门ID获取下级部门列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<SysDept>> SelectDeptList(long id);

        /// <summary>
        /// 创建部门
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        Task<int> CreateDept(SysDept dept);

        /// <summary>
        /// 更新部门
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        Task<int> UpdateDept(SysDept dept);

        /// <summary>
        /// 根据删除部门
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        Task<int> DeleteDeptById(long deptId);

        /// <summary>
        /// 选择部门树列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<SysDept>> SelectDeptTree(long userId);
    }
}
