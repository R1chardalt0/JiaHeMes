using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Production;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Service.Trace.Dto;
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
  /// 工单服务实现
  /// </summary>
  public class StationListService : IStationListService
  {
    private readonly IRepository<StationList> _stationListRepo;
    private readonly IRepository<BomRecipe> _bomRecipeRepo;
    private readonly IRepository<Material> _materialRepo;
    private readonly ILogger<StationListService> _logger;
    private readonly AppDbContext _dbContext;

    public StationListService(
        IRepository<StationList> stationListRepo,
        IRepository<BomRecipe> bomRecipeRepo,
        IRepository<Material> materialRepo,
        ILogger<StationListService> logger,
        AppDbContext dbContext)
    {
            _stationListRepo = stationListRepo;
      _bomRecipeRepo = bomRecipeRepo;
      _materialRepo = materialRepo;
      _logger = logger;
      _dbContext = dbContext;
    }

    /// <summary>
    /// 分页查询工单列表
    /// </summary>
    public async Task<PaginatedList<StationListDto>> PaginationAsync(StationListQueryDto queryDto)
        {
      try
      {
        // 验证分页参数
        if (queryDto.Current < 1) queryDto.Current = 1;
        if (queryDto.PageSize < 1) queryDto.PageSize = 10;

        // 构建查询条件
        var query = _stationListRepo.GetQueryable();

        //// 工单编码模糊匹配
        //if (!string.IsNullOrEmpty(queryDto.Code))
        //{
        //  query = query.Where(w => w.Code.Value.Contains(queryDto.Code));
        //}

        //// 产品编码模糊匹配
        //if (!string.IsNullOrEmpty(queryDto.ProductCode))
        //{
        //  query = query.Where(w => w.ProductCode.Value.Contains(queryDto.ProductCode));
        //}

        //// 工单状态筛选
        //if (queryDto.DocStatus.HasValue)
        //{
        //  query = query.Where(w => w.DocStatus == queryDto.DocStatus.Value);
        //}

        // 时间范围筛选
        // if (queryDto.StartTime.HasValue)
        // {
        //   query = query.Where(w => w.CreatedAt >= queryDto.StartTime.Value);
        // }

        // if (queryDto.EndTime.HasValue)
        // {
        //   query = query.Where(w => w.CreatedAt <= queryDto.EndTime.Value);
        // }

        // 获取总数
        var totalCount = await query.CountAsync();

        // 分页
        var stationLists = await query
          .Skip((queryDto.Current - 1) * queryDto.PageSize)
          .Take(queryDto.PageSize)
          .ToListAsync();

        // 转换为DTO
        var stationListDtos = stationLists.Select(w => w.ToDto()).ToList();

        // 返回分页结果
        return new PaginatedList<StationListDto>(stationListDtos, totalCount, queryDto.Current, queryDto.PageSize);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "查询工单列表时发生错误");
        throw;
      }
    }


    /// <summary>
    /// 根据ID获取工单详情
    /// </summary>
    public async Task<StationListDto?> GetStationInfoById(Guid id)
    {
      var stationList = await _stationListRepo.GetQueryable()

          .FirstOrDefaultAsync(w => w.StationId == id);

      if (stationList == null)
        return null;

      return stationList.ToDto();
    }

    /// <summary>
    /// 根据工单编码获取工单详情
    /// </summary>
    public async Task<StationListDto?> GetStationInfoByCode(string code)
    {
      var stationList = await _stationListRepo.GetQueryable()
          .FirstOrDefaultAsync(w => w.StationCode == code);

      if (stationList == null)
        return null;

      return stationList.ToDto();
    }
       
        /// <summary>
        /// 创建工单
        /// </summary>
        public async Task<StationListDto> CreateStationInfo(StationListDto dto)
    {
      try
      {
       
        // 验证工单编码唯一性

        var existingStationList = await _stationListRepo.GetAsync(w => w.StationCode == dto.StationCode);
        if (existingStationList != null)
        {
          _logger.LogError("工单编码已存在: {Code}", dto.StationCode);
          throw new InvalidOperationException($"工单编码已存在: {dto.StationCode}");
        }
        StationList stationList = new StationList();
                stationList.StationName = dto.StationName;
                stationList.StationCode = dto.StationCode;
                stationList.CreateTime = DateTime.Now;
        
        // 保存工单
        var result = await _stationListRepo.InsertAsync(stationList);

        if (result != null)
        {
          _logger.LogInformation($"成功创建工单，ID: {result.StationId}");

          // 返回创建的工单信息 - 使用扩展方法
          return result.ToDto();
        }
        else
        {
          _logger.LogError("创建工单失败");
          throw new InvalidOperationException("创建工单失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "创建工单时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    public async Task<StationListDto> UpdateStationInfo(StationListDto dto)
    {
      try
      {
        // 查找现有工单
        var existingStationList = await _stationListRepo.GetAsync(w => w.StationId == dto.StationId);
        if (existingStationList == null)
          throw new InvalidOperationException($"不存在，ID: {dto.StationId}");


        
        // 更新工单信息
        existingStationList.StationName = dto.StationName;
        existingStationList.StationCode = dto.StationCode;

 

        // 更新工单
        var result = await _stationListRepo.UpdateAsync(existingStationList);

        if (result != null)
        {
          _logger.LogInformation($"成功更新，ID: {result.StationId}");

          // 返回更新后的工单信息 - 使用扩展方法
          return result.ToDto();
        }
        else
        {
          _logger.LogError("更新失败");
          throw new InvalidOperationException("更新失败");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "更新时发生异常");
        throw;
      }
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    public async Task<bool> DeleteStationInfoById(Guid id)
    {
      try
      {
        // 查找工单
        var stationList = await _stationListRepo.GetAsync(w => w.StationId == id);
        if (stationList == null)
          return false; // 工单不存在

        // 开始事务
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
          // 真正删除工单记录
          await _stationListRepo.DeleteAsync(stationList);

          // 提交事务
          await transaction.CommitAsync();

          _logger.LogInformation($"成功删除工单，ID: {stationList.StationId}");
          return true; // 删除成功
        }
        catch
        {
          // 事务会在using块结束时自动回滚
          throw;
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "删除工单时发生异常");
        throw;
      }
    }

       
 
        Task<List<StationListDto>> IStationListService.GetAllStationInfos()
        {
            throw new NotImplementedException();
        }

       

         
    }
}