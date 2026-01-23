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
  /// BOM列表服务实现
  /// </summary>
  public class BomListService : IBomListService
  {
    private readonly IRepository<BomList> _bomListRepo;
    private readonly ILogger<BomListService> _logger;
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="bomListRepo">BOM列表仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public BomListService(
        IRepository<BomList> bomListRepo,
        ILogger<BomListService> logger,
        AppDbContext dbContext)
    {
      _bomListRepo = bomListRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 验证Status值
    /// </summary>
    /// <param name="status">状态值</param>
    /// <exception cref="InvalidOperationException">状态值无效时抛出异常</exception>
    private void ValidateStatus(int status)
    {
      if (status != 0 && status != 1)
      {
        throw new InvalidOperationException("状态值无效，只能是0（启用）或1（关闭）");
      }
    }

    /// <summary>
    /// 分页查询BOM列表
    /// </summary>
    public async Task<PaginatedList<BomListDto>> GetBomListsAsync(BomListQueryDto queryDto)
    {
      try
      {
        // 验证分页参数
        if (queryDto.PageIndex < 1) queryDto.PageIndex = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        IQueryable<BomList> query = _bomListRepo.GetQueryable();
        query = query.Include(b => b.BomItems); // 关联查询BomItems

        // BOM编码模糊匹配
        if (!string.IsNullOrEmpty(queryDto.BomCode))
        {
          query = query.Where(b => b.BomCode.Contains(queryDto.BomCode));
        }

        // BOM名称模糊匹配
        if (!string.IsNullOrEmpty(queryDto.BomName))
        {
          query = query.Where(b => b.BomName.Contains(queryDto.BomName));
        }

        // 状态精确匹配
        if (queryDto.Status.HasValue)
        {
          query = query.Where(b => b.Status == queryDto.Status.Value);
        }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var bomLists = await query
          .Skip((queryDto.PageIndex - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var bomListDtos = bomLists.Select(b => b.ToDto()).ToList();

        // 返回分页结果
        return new PaginatedList<BomListDto>(bomListDtos, totalCount, queryDto.PageIndex, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询BOM列表时发生错误");
        throw;
      }
    }

    /// <summary>
    /// 根据ID获取BOM详情
    /// </summary>
    public async Task<BomListDto?> GetBomListByIdAsync(Guid bomId)
    {
      var bomList = await _bomListRepo.GetQueryable().Include(b => b.BomItems) // 关联查询BomItems
          .FirstOrDefaultAsync(b => b.BomId == bomId);

      if (bomList == null)
        return null;

      return bomList.ToDto();
    }

    /// <summary>
    /// 创建BOM
    /// </summary>
    public async Task<BomListDto> CreateBomListAsync(BomListCreateDto dto)
    {
      try
      {
        // 验证Status值
        ValidateStatus(dto.Status);

        // 验证BOM编码唯一性
        var existingBom = await _bomListRepo.GetAsync(b => b.BomCode == dto.BomCode);
        if (existingBom != null)
        {
          _logger.LogError("BOM编码已存在: {BomCode}", dto.BomCode);
          throw new InvalidOperationException($"BOM编码已存在: {dto.BomCode}");
        }

        // 创建BOM实体
        var bomList = new BomList
        {
          BomId = Guid.NewGuid(),
          BomName = dto.BomName,
          BomCode = dto.BomCode,
          Status = dto.Status,
          Remark = dto.Remark,
          CreateTime = DateTimeOffset.Now
        };

        // 保存BOM
        var result = await _bomListRepo.InsertAsync(bomList);

        if (result != null)
        {
          _logger.LogInformation($"成功创建BOM，ID: {result.BomId}");
          return result.ToDto();
        }
        else
        {
          _logger.LogError("创建BOM失败");
          throw new InvalidOperationException("创建BOM失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建BOM时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新BOM
    /// </summary>
    public async Task<BomListDto> UpdateBomListAsync(BomListUpdateDto dto)
    {
      try
      {
        // 验证Status值
        ValidateStatus(dto.Status);

        // 查找现有BOM
        var existingBom = await _bomListRepo.GetAsync(b => b.BomId == dto.BomId);
        if (existingBom == null)
          throw new InvalidOperationException($"BOM不存在，ID: {dto.BomId}");

        // 检查BOM编码唯一性（排除当前BOM）
        if (dto.BomCode != existingBom.BomCode)
        {
          var existingBomWithSameCode = await _bomListRepo.GetAsync(b => b.BomCode == dto.BomCode && b.BomId != dto.BomId);
          if (existingBomWithSameCode != null)
          {
            _logger.LogError("BOM编码已存在: {BomCode}", dto.BomCode);
            throw new InvalidOperationException($"BOM编码已存在: {dto.BomCode}");
          }
        }

        // 更新BOM信息
        existingBom.BomName = dto.BomName;
        existingBom.BomCode = dto.BomCode;
        existingBom.Status = dto.Status;
        existingBom.Remark = dto.Remark;
        existingBom.UpdateTime = DateTimeOffset.Now;

        // 保存更新
        var result = await _bomListRepo.UpdateAsync(existingBom);

        if (result != null)
        {
          _logger.LogInformation($"成功更新BOM，ID: {result.BomId}");
          return result.ToDto();
        }
        else
        {
          _logger.LogError("更新BOM失败");
          throw new InvalidOperationException("更新BOM失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新BOM时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 删除BOM（支持单一和批量删除）
    /// </summary>
    public async Task<int> DeleteBomListsAsync(Guid[] bomIds)
    {
      if (bomIds == null || bomIds.Length == 0)
        return 0; // 没有要删除的BOM

      try
      {
        // 检查是否存在关联的BomItem记录
        var bomItems = await _dbContext.BomItem.Where(bi => bomIds.Contains(bi.BomId.Value)).ToListAsync();
        if (bomItems.Count > 0)
        {
          throw new InvalidOperationException("删除BOM失败：存在关联的BOM子项，不允许删除");
        }

        // 再删除BomList记录
        var boms = await _dbContext.BomList.Where(b => bomIds.Contains(b.BomId)).ToListAsync();
        if (boms.Count == 0)
        {
          return 0;
        }

        _dbContext.BomList.RemoveRange(boms);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"成功删除BOM，共删除 {boms.Count} 个");
        return boms.Count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除BOM时发生异常");
        // 如果是我们自定义的异常，直接抛出
        if (ex is InvalidOperationException && ex.Message.Contains("删除BOM失败"))
        {
          throw;
        }
        // 提供更详细的错误信息
        if (ex.Message.Contains("foreign key") || ex.Message.Contains("外键") || ex.Message.Contains("ForeignKey"))
        {
          throw new InvalidOperationException("删除BOM失败：存在关联的BOM子项或其他引用，无法删除", ex);
        }
        throw new InvalidOperationException($"删除BOM失败：{ex.Message}", ex);
      }
    }
  }
}
