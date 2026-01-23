using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.BOM;
using ChargePadLine.Service;
using ChargePadLine.Service.Trace;
using ChargePadLine.Service.Trace.Dto.BOM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Service.Trace.Impl
{
    /// <summary>
    /// BOM子项服务实现
    /// </summary>
    public class BomItemService : IBomItemService
    {
        private readonly IRepository<BomItem> _bomItemRepo;
        private readonly ILogger<BomItemService> _logger;
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bomItemRepo">BOM子项仓储</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="dbContext">数据库上下文</param>
        public BomItemService(
            IRepository<BomItem> bomItemRepo,
            ILogger<BomItemService> logger,
            AppDbContext dbContext)
        {
            _bomItemRepo = bomItemRepo;
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 分页查询BOM子项列表
        /// </summary>
        public async Task<PaginatedList<BomItemDto>> GetBomItemsAsync(BomItemQueryDto queryDto)
        {
            try
            {
                // 验证分页参数
                if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
                if (queryDto.PageSize < 1) queryDto.PageSize = 10;

                // 构建查询条件
                var query = _bomItemRepo.GetQueryable();

                // BOM子项ID精确匹配
                if (queryDto.BomItemId.HasValue)
                {
                    query = query.Where(b => b.BomItemId == queryDto.BomItemId.Value);
                }

                // BOM ID精确匹配
                if (queryDto.BomId.HasValue)
                {
                    query = query.Where(b => b.BomId == queryDto.BomId.Value);
                }

                // 工位编码模糊匹配
                if (!string.IsNullOrEmpty(queryDto.StationCode))
                {
                    query = query.Where(b => b.StationCode.Contains(queryDto.StationCode));
                }

                // 批次规则模糊匹配
                if (!string.IsNullOrEmpty(queryDto.BatchRule))
                {
                    query = query.Where(b => b.BatchRule.Contains(queryDto.BatchRule));
                }

                // 产品ID精确匹配
                if (queryDto.ProductId.HasValue)
                {
                    query = query.Where(b => b.ProductId == queryDto.ProductId.Value);
                }

                // 获取总数
                var totalCount = await query.CountAsync();

                // 分页
                var bomItems = await query
                  .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
                  .Take(queryDto.PageSize)
                  .ToListAsync();

                // 转换为DTO
                var bomItemDtos = bomItems.Select(b => b.ToDto()).ToList();

                // 返回分页结果
                return new PaginatedList<BomItemDto>(bomItemDtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询BOM子项列表时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 根据ID获取BOM子项详情
        /// </summary>
        public async Task<BomItemDto?> GetBomItemByIdAsync(Guid bomItemId)
        {
            var bomItem = await _bomItemRepo.GetQueryable()
                .FirstOrDefaultAsync(b => b.BomItemId == bomItemId);

            if (bomItem == null)
                return null;

            return bomItem.ToDto();
        }

        /// <summary>
        /// 根据BomId获取BOM子项列表
        /// </summary>
        public async Task<IEnumerable<BomItemDto>> GetBomItemsByBomIdAsync(Guid bomId)
        {
            try
            {
                var bomItems = await _bomItemRepo.GetQueryable()
                    .Where(b => b.BomId == bomId)
                    .ToListAsync();

                if(bomItems.Count > 0)
                {

                    //bomItems = bomItems.OrderBy(b => b.DateIndex).ThenBy(b => b.NumberIndex).ThenBy(b => b.ProductIndex).ToList();
                   bomItems =bomItems.OrderBy(b => int.Parse(b.StationCode.Replace("OP", "").ToString())).ThenBy(b => b.ProductId).ToList();


                }
                return bomItems.Select(b => b.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据BomId获取BOM子项列表时发生错误，BomId: {BomId}", bomId);
                throw;
            }
        }

        /// <summary>
        /// 创建BOM子项
        /// </summary>
        public async Task<BomItemDto> CreateBomItemAsync(BomItemCreateDto dto)
        {
            try
            {
                // 创建BOM子项实体
                var bomItem = new BomItem
                {
                    BomItemId = Guid.NewGuid(),
                    BomId = dto.BomId,
                    StationCode = dto.StationCode,
                    BatchRule = dto.BatchRule,
                    BatchQty = dto.BatchQty,
                    BatchSNQty = dto.BatchSNQty,
                    ProductId = dto.ProductId,
                    CreateTime = DateTimeOffset.Now,
                    ProductIndex = dto.ProductIndex,
                    DateIndex = dto.DateIndex,
                    NumberIndex = dto.NumberIndex,
                    ShelfLife = dto.ShelfLife,
                };

                // 保存BOM子项
                var result = await _bomItemRepo.InsertAsync(bomItem);

                if (result != null)
                {
                    _logger.LogInformation($"成功创建BOM子项，ID: {result.BomItemId}");
                    return result.ToDto();
                }
                else
                {
                    _logger.LogError("创建BOM子项失败");
                    throw new InvalidOperationException("创建BOM子项失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建BOM子项时发生异常");
                throw;
            }
        }

        /// <summary>
        /// 更新BOM子项
        /// </summary>
        public async Task<BomItemDto> UpdateBomItemAsync(BomItemUpdateDto dto)
        {
            try
            {
                // 查找现有BOM子项
                var existingBomItem = await _bomItemRepo.GetAsync(b => b.BomItemId == dto.BomItemId);
                if (existingBomItem == null)
                    throw new InvalidOperationException($"BOM子项不存在，ID: {dto.BomItemId}");

                // 更新BOM子项信息
                existingBomItem.BomId = dto.BomId;
                existingBomItem.StationCode = dto.StationCode;
                existingBomItem.BatchRule = dto.BatchRule;
                existingBomItem.BatchQty = dto.BatchQty;
                existingBomItem.BatchSNQty = dto.BatchSNQty;
                existingBomItem.ProductId = dto.ProductId;
                existingBomItem.UpdateTime = DateTimeOffset.Now;
                existingBomItem.ProductIndex = dto.ProductIndex;
                existingBomItem.DateIndex = dto.DateIndex;
                existingBomItem.NumberIndex = dto.NumberIndex;
                existingBomItem.ShelfLife = dto.ShelfLife;


                // 保存更新
                var result = await _bomItemRepo.UpdateAsync(existingBomItem);

                if (result != null)
                {
                    _logger.LogInformation($"成功更新BOM子项，ID: {result.BomItemId}");
                    return result.ToDto();
                }
                else
                {
                    _logger.LogError("更新BOM子项失败");
                    throw new InvalidOperationException("更新BOM子项失败");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新BOM子项时发生异常");
                throw;
            }
        }

        /// <summary>
        /// 删除BOM子项（支持单一和批量删除）
        /// </summary>
        public async Task<int> DeleteBomItemsAsync(Guid[] bomItemIds)
        {
            if (bomItemIds == null || bomItemIds.Length == 0)
                return 0; // 没有要删除的BOM子项

            try
            {
                var bomItems = await _dbContext.BomItem.Where(b => bomItemIds.Contains(b.BomItemId)).ToListAsync();
                if (bomItems.Count == 0)
                {
                    return 0;
                }

                _dbContext.BomItem.RemoveRange(bomItems);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"成功删除BOM子项，共删除 {bomItems.Count} 个");
                return bomItems.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除BOM子项时发生异常");
                throw;
            }
        }
    }
}