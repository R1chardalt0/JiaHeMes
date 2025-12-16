using Microsoft.EntityFrameworkCore;
using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Systems;

namespace ChargePadLine.Service.Systems.Impl
{
    /// <summary>
    /// 公司信息服务实现
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<SysCompany> _companyRepo;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="companyRepo">公司仓储</param>
        /// <param name="dbContext">数据库上下文</param>
        public CompanyService(IRepository<SysCompany> companyRepo, AppDbContext dbContext)
        {
            _companyRepo = companyRepo;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 分页查询公司列表
        /// </summary>
        /// <param name="current">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="companyName">公司名称查询条件</param>
        /// <returns>公司分页列表</returns>
        public async Task<PaginatedList<SysCompany>> PaginationAsync(int current, int pageSize, string? companyName)
        {
            var query = _dbContext.SysCompanys.OrderByDescending(company => company.CreateTime).AsQueryable();

            if (!string.IsNullOrWhiteSpace(companyName))
            {
                query = query.Where(company => company.CompanyName != null && company.CompanyName.Contains(companyName));
            }

            return await query.RetrievePagedListAsync(current, pageSize);
        }

        /// <summary>
        /// 根据主键获取公司信息
        /// </summary>
        /// <param name="companyId">公司主键</param>
        /// <returns>公司实体</returns>
        public async Task<SysCompany?> GetCompanyByIdAsync(int companyId)
        {
            return await _companyRepo.GetAsync(company => company.CompanyId == companyId);
        }

        /// <summary>
        /// 新增公司
        /// </summary>
        /// <param name="company">公司实体</param>
        /// <returns>影响行数</returns>
        public async Task<int> CreateCompanyAsync(SysCompany company)
        {
			var normalizedName = company.CompanyName?.Trim();
			if (string.IsNullOrWhiteSpace(normalizedName))
			{
				throw new InvalidOperationException("公司名称不能为空");
			}

			var exists = await _dbContext.SysCompanys.AnyAsync(c => c.CompanyName == normalizedName);
			if (exists)
			{
				throw new InvalidOperationException("公司名称已存在");
			}

			company.CompanyId = 0;
			company.CompanyName = normalizedName;
			company.Remark = string.IsNullOrWhiteSpace(company.Remark) ? null : company.Remark.Trim();
			company.CompanyCode = Guid.NewGuid();
			company.CreateTime = DateTimeOffset.Now;

			return await _companyRepo.InsertAsyncs(company);
        }

        /// <summary>
        /// 更新公司
        /// </summary>
        /// <param name="company">公司实体</param>
        /// <returns>影响行数</returns>
		public async Task<int> UpdateCompanyAsync(SysCompany company)
		{
			var existingCompany = await _companyRepo.GetAsync(c => c.CompanyId == company.CompanyId);
			if (existingCompany == null)
			{
				throw new InvalidOperationException("公司不存在");
			}

			var normalizedName = company.CompanyName?.Trim();
			if (string.IsNullOrWhiteSpace(normalizedName))
			{
				throw new InvalidOperationException("公司名称不能为空");
			}

			var exists = await _dbContext.SysCompanys.AnyAsync(c => c.CompanyId != company.CompanyId && c.CompanyName == normalizedName);
			if (exists)
			{
				throw new InvalidOperationException("公司名称已存在");
			}

			existingCompany.CompanyName = normalizedName;
			existingCompany.Remark = string.IsNullOrWhiteSpace(company.Remark) ? null : company.Remark.Trim();
			existingCompany.UpdateTime = DateTimeOffset.Now;
			if (existingCompany.CompanyCode == Guid.Empty)
			{
				existingCompany.CompanyCode = Guid.NewGuid();
			}

			return await _companyRepo.UpdateAsyncs(existingCompany);
        }

        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="companyId">公司主键</param>
        /// <returns>影响行数</returns>
        public async Task<int> DeleteCompanyAsync(int companyId)
        {
            return await _companyRepo.DeleteAsyncs(company => company.CompanyId == companyId);
        }
    }
}

