using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
    public class ProductionLineService: IProductionLineService
    {
        private readonly IRepository<ProductionLine> _lineRepo;
        private readonly AppDbContext _dbContext;

        public ProductionLineService(IRepository<ProductionLine> lineRepo, AppDbContext dbContext)
        {
            _lineRepo = lineRepo;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 分页查询生产线列表
        /// </summary>
        public async Task<PaginatedList<ProductionLine>> PaginationAsync(int current, int pageSize, string? productionLineName, string? productionLineCode, int? companyId, DateTime? startTime, DateTime? endTime)
        {
            var query = _dbContext.ProductionLines.OrderByDescending(s => s.CreatedAt).AsQueryable();

            // 过滤生产线名称
            if (!string.IsNullOrEmpty(productionLineName))
            {
                query = query.Where(r => r.ProductionLineName.Contains(productionLineName));
            }

            if (!string.IsNullOrEmpty(productionLineCode))
            {
                query = query.Where(r => r.ProductionLineCode.Contains(productionLineCode));
            }

            // 过滤公司ID
            if (companyId.HasValue)
            {
                query = query.Where(r => r.CompanyId == companyId.Value);
            }

            // 过滤创建时间范围
            if (startTime.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= startTime.Value);
            }

            if (endTime.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= endTime.Value);
            }

            // 分页查询
            var list = await query.RetrievePagedListAsync(current, pageSize);
            return list;
        }

        /// <summary>
        /// 获取生产线详情
        /// </summary>
        public async Task<ProductionLine> GetProductionLineById(Guid productionLineId)
        {
            return await _lineRepo.GetAsync(r => r.ProductionLineId == productionLineId);
        }

        /// <summary>
        /// 创建生产线
        /// </summary>
        public async Task<int> CreateProductionLine(ProductionLine productionLine)
        {
            // 验证生产线名称唯一性
            var exists = await _lineRepo.GetAsync(r => r.ProductionLineName == productionLine.ProductionLineName);
            if (exists != null)
                return -1;

            productionLine.CreatedAt = DateTime.Now;
            productionLine.UpdatedAt = DateTime.Now;
            return await _lineRepo.InsertAsyncs(productionLine);
        }

        /// <summary>
        /// 更新生产线
        /// </summary>
        public async Task<int> UpdateProductionLine(ProductionLine productionLine)
        {
            // 验证生产线名称唯一性（排除当前生产线）
            var exists = await _lineRepo.GetAsync(r => r.ProductionLineName == productionLine.ProductionLineName && r.ProductionLineId != productionLine.ProductionLineId);
            if (exists != null)
                return -1;

            productionLine.UpdatedAt = DateTime.Now;
            return await _lineRepo.UpdateAsyncs(productionLine);
        }

        /// <summary>
        /// 批量删除生产线
        /// </summary>
        public async Task<int> DeleteProductionLineByIds(Guid[] productionLineIds)
        {
            // 使用DbContext直接管理事务
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 删除生产线
                    var result = await _lineRepo.DeleteAsyncs(r => productionLineIds.Contains(r.ProductionLineId));

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
        /// 获取所有生产线列表
        /// </summary>
        public async Task<List<ProductionLine>> GetAllProductionLines()
        {
            return await _lineRepo.GetListAsync();
        }
    }
}
