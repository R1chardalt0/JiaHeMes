using ChargePadLine.Entitys.Systems;

namespace ChargePadLine.Service.Systems
{
    /// <summary>
    /// 公司信息服务接口
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// 分页查询公司列表
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="companyName">公司名称</param>
        /// <returns>公司分页列表</returns>
        Task<PaginatedList<SysCompany>> PaginationAsync(int current, int pageSize, string? companyName);

        /// <summary>
        /// 根据主键获取公司信息
        /// </summary>
        /// <param name="companyId">公司主键</param>
        /// <returns>公司实体</returns>
        Task<SysCompany?> GetCompanyByIdAsync(int companyId);

        /// <summary>
        /// 新增公司
        /// </summary>
        /// <param name="company">公司实体</param>
        /// <returns>影响行数</returns>
        Task<int> CreateCompanyAsync(SysCompany company);

        /// <summary>
        /// 更新公司
        /// </summary>
        /// <param name="company">公司实体</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateCompanyAsync(SysCompany company);

        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="companyId">公司主键</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteCompanyAsync(int companyId);
    }
}

