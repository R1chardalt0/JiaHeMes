using ChargePadLine.DbContexts;
using ChargePadLine.DbContexts.Repository;
using ChargePadLine.Entitys.Trace;
using ChargePadLine.Entitys.Trace.BOM;
using ChargePadLine.Entitys.Trace.Order;
using ChargePadLine.Entitys.Trace.ProcessRouting;
using ChargePadLine.Entitys.Trace.Recipes.Entities;
using ChargePadLine.Entitys.Trace.TraceInformation;
using ChargePadLine.Entitys.Trace.WorkOrders;
using ChargePadLine.Service.Trace.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.FSharp.Core.ByRefKinds;
using System.Text.RegularExpressions;
using ChargePadLine.Service.Trace.Dto.BOM;
using System.Text.Json;
using ChargePadLine.Entitys.Trace.Packing;

namespace ChargePadLine.Service.Trace.Impl
{
  /// <summary>
  /// 通用接口服务实现类
  /// 负责处理MES系统中的核心业务逻辑：
  /// 1. 物料上料(FeedMaterial)
  /// 2. 数据上传前校验(UploadCheck)
  /// 3. 生产数据上传(UploadData)
  /// 4. SN追溯(TraceSN)
  /// </summary>
  public class CommonInterfaseService : ICommonInterfaseService
  {
    private readonly IRepository<WorkOrder> _workOrderRepo; // 工单仓储
    private readonly IRepository<BomRecipe> _bomRecipeRepo; // BOM配方仓储
    private readonly IRepository<Material> _materialRepo; // 物料仓储
    private readonly IRepository<MesSnListHistory> _mesSNListHistory; // SN历史记录仓储
    private readonly ILogger<OrderListService> _logger; // 日志记录器
    private readonly AppDbContext _dbContext; // 数据库上下文

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="workOrderRepo">工单仓储</param>
    /// <param name="bomRecipeRepo">BOM配方仓储</param>
    /// <param name="materialRepo">物料仓储</param>
    /// <param name="mesSNListHistory">SN历史记录仓储</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    public CommonInterfaseService(
        IRepository<WorkOrder> workOrderRepo,
        IRepository<BomRecipe> bomRecipeRepo,
        IRepository<Material> materialRepo,
        IRepository<MesSnListHistory> mesSNListHistory,
        ILogger<OrderListService> logger,
        AppDbContext dbContext)
    {
      _workOrderRepo = workOrderRepo;
      _bomRecipeRepo = bomRecipeRepo;
      _materialRepo = materialRepo;
      _logger = logger;
      _dbContext = dbContext;
      _mesSNListHistory = mesSNListHistory;
    }

    public Task<string> GetName()
    {
      return null;
    }

    public Task<List<string>> GetList()
    {
      throw new NotImplementedException();
    }
    /// <summary>
    /// 上传包装信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<FSharpResult<ValueTuple, (int, string)>> UploadPacking(PackingParams request)
    {
      var SNList = request.SNList.Split(",");
      var PackingList = new List<MesPacking>();
      var SncurrentList = new List<MesSnListCurrent>();
      var UploadCheckParamsList = new List<RequestUploadCheckParams>();
      foreach (var SN in SNList)
      {
        RequestUploadCheckParams UploadCheckParams = new RequestUploadCheckParams
        {
          SN = SN,
          Resource = request.Resource,
          StationCode = request.StationCode,
          WorkOrderCode = request.WorkOrderCode,
        };
        var CheckSN = await UploadCheck(UploadCheckParams);
        if (CheckSN.ErrorValue.Item1.ToString() != "0")
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, CheckSN.ErrorValue.Item2.ToString()));
        }


        PackingList.Add(new MesPacking
        {
          SN = SN,
          InnerBoxSN = request.InnerBox,
          OuterBoxSN = "",
          CreateTime = DateTime.Now,
        });
        UploadCheckParamsList.Add(UploadCheckParams);
      }

      foreach (var UploadCheckParams in UploadCheckParamsList)
      {
        await UploadData(UploadCheckParams);
      }

      await _dbContext.MesPacking.AddRangeAsync(PackingList);
      await _dbContext.SaveChangesAsync();
      return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"包装数据上传成功"));
    }
    /// <summary>
    /// 跳站
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<FSharpResult<ValueTuple, (int, string)>> JumpStation(JumpStationParams request)
    {
      var SNcurrentList = await _dbContext.mesSnListCurrents.FirstOrDefaultAsync(x => x.SnNumber == request.SN);

      if (SNcurrentList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
      }
      var CurrentStationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationId == SNcurrentList.CurrentStationListId);

      if (CurrentStationList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}站点不存在"));
      }
      var RequestStationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationCode == request.JumpStationCode);

      if (RequestStationList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"上传站点不存在"));
      }

      if (CurrentStationList.StationId == RequestStationList.StationId)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}当前站点一致，请勿重复返工"));
      }
      var workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderListId == SNcurrentList.OrderListId);
      if (workOrder == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在"));
      }
      var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                     join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                     join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                     where p.Id == workOrder.ProcessRouteId
                                     orderby iitm.RouteSeq
                                     select new
                                     {
                                       p.RouteCode,
                                       p.RouteName,
                                       iitm.FirstStation, // 是否为首站
                                       iitm.MustPassStation, // 是否为必过站
                                       iitm.RouteSeq, // 路线顺序
                                       iitm.StationCode, // 站点编码
                                       iitm.CheckAll, // 是否需要检查所有站点
                                       iitm.CheckStationList, // 需要检查的站点列表
                                       s.StationId // 站点ID
                                     })
                   .ToListAsync();

      var CurrentprocessRouteItems = processRouteItems.Where(x => x.StationId == SNcurrentList.CurrentStationListId).First();
      var RequestprocessRouteItems = processRouteItems.Where(x => x.StationCode == request.JumpStationCode).First();

      if (CurrentprocessRouteItems == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.JumpStationCode},工艺路线站点{CurrentStationList.StationCode},不存在"));
      }
      if (RequestprocessRouteItems == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.JumpStationCode},工艺路线站点{request.JumpStationCode},不存在"));
      }

      if (CurrentprocessRouteItems.RouteSeq > RequestprocessRouteItems.RouteSeq)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号,当前站点{CurrentStationList.StationCode},不能小于{RequestStationList.StationCode}站点，顺序不一致"));
      }
      var CurrentSNList = SNcurrentList;


      CurrentSNList.UpdateTime = DateTime.Now;
      CurrentSNList.CurrentStationListId = RequestStationList.StationId;
      CurrentSNList.StationStatus = StationStatusEnum.跳站;
      _dbContext.mesSnListCurrents.Update(CurrentSNList);

      var snHistory = new MesSnListHistory
      {
        SNListHistoryId = Guid.NewGuid(),
        SnNumber = SNcurrentList.SnNumber,
        OrderListId = SNcurrentList.OrderListId,
        CurrentStationListId = SNcurrentList.CurrentStationListId,
        ProductionLineId = SNcurrentList.ProductionLineId,
        ProductListId = SNcurrentList.ProductListId,
        ResourceId = SNcurrentList.ResourceId,
        StationStatus = StationStatusEnum.跳站,
        IsAbnormal = false,
        CreateTime = DateTime.Now,
        UpdateTime = DateTime.Now,
        Remark = $"跳站,{CurrentStationList.StationCode},{request.JumpStationCode}",

      };

      await _mesSNListHistory.InsertAsync(snHistory);

      await _dbContext.SaveChangesAsync();


      return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"跳站成功"));
    }
    /// <summary>
    /// 重工方法
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<FSharpResult<ValueTuple, (int, string)>> ReWork(ReWorkParams request)
    {
      var SNcurrentList = await _dbContext.mesSnListCurrents.FirstOrDefaultAsync(x => x.SnNumber == request.SN);

      if (SNcurrentList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
      }
      var CurrentStationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationId == SNcurrentList.CurrentStationListId);

      if (CurrentStationList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}站点不存在"));
      }
      var RequestStationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationCode == request.ReWorkStationCode);

      if (RequestStationList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"上传站点不存在"));
      }

      if (CurrentStationList.StationId == RequestStationList.StationId)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}当前站点一致，请勿重复返工"));
      }
      var workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderListId == SNcurrentList.OrderListId);
      if (workOrder == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在"));
      }
      var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                     join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                     join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                     where p.Id == workOrder.ProcessRouteId
                                     orderby iitm.RouteSeq
                                     select new
                                     {
                                       p.RouteCode,
                                       p.RouteName,
                                       iitm.FirstStation, // 是否为首站
                                       iitm.MustPassStation, // 是否为必过站
                                       iitm.RouteSeq, // 路线顺序
                                       iitm.StationCode, // 站点编码
                                       iitm.CheckAll, // 是否需要检查所有站点
                                       iitm.CheckStationList, // 需要检查的站点列表
                                       s.StationId // 站点ID
                                     })
                   .ToListAsync();

      var CurrentprocessRouteItems = processRouteItems.Where(x => x.StationId == SNcurrentList.CurrentStationListId).First();
      var RequestprocessRouteItems = processRouteItems.Where(x => x.StationCode == request.ReWorkStationCode).First();

      if (CurrentprocessRouteItems.RouteSeq <= RequestprocessRouteItems.RouteSeq)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号,当前站点{CurrentStationList.StationCode},不能小于{RequestStationList.StationCode}站点，顺序不一致"));
      }
      var CurrentSNList = SNcurrentList;

      CurrentSNList.ReworkStartStation = request.ReWorkStationCode;
      CurrentSNList.ReworkEndStation = CurrentStationList.StationCode;
      CurrentSNList.ReworkCount = SNcurrentList.ReworkCount ?? 0 + 1;
      CurrentSNList.ReworkTime = DateTime.Now;
      CurrentSNList.IsReworking = true;
      CurrentSNList.UpdateTime = DateTime.Now;
      CurrentSNList.CurrentStationListId = RequestStationList.StationId;
      _dbContext.mesSnListCurrents.Update(CurrentSNList);

      var snHistory = new MesSnListHistory
      {
        SNListHistoryId = Guid.NewGuid(),
        SnNumber = SNcurrentList.SnNumber,
        OrderListId = SNcurrentList.OrderListId,
        CurrentStationListId = SNcurrentList.CurrentStationListId,
        ProductionLineId = SNcurrentList.ProductionLineId,
        ProductListId = SNcurrentList.ProductListId,
        ResourceId = SNcurrentList.ResourceId,
        StationStatus = StationStatusEnum.返工,
        IsAbnormal = false,
        CreateTime = DateTime.Now,
        UpdateTime = DateTime.Now,
        Remark = $"返工,{CurrentStationList.StationCode},{request.ReWorkStationCode}",

      };

      // 处理物料解绑
      if (request.UnbindMaterialIds != null && request.UnbindMaterialIds.Count > 0)
      {
        // 根据ID获取需要解绑的物料批次明细
        var bomBatchItems = await _dbContext.MesOrderBomBatchItem
            .Where(item => request.UnbindMaterialIds.Contains(item.OrderBomBatchItemId))
            .ToListAsync();

        // 设置为已解绑
        foreach (var item in bomBatchItems)
        {
          item.IsUnbind = true;
          _dbContext.MesOrderBomBatchItem.Update(item);
        }
      }

      await _mesSNListHistory.InsertAsync(snHistory);

      await _dbContext.SaveChangesAsync();


      return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"返工成功"));
    }

    /// <summary>
    /// 物料上料方法
    /// 负责将物料批次上料至指定设备和站点，并进行相关校验
    /// </summary>
    /// <param name="request">上料请求参数，包含设备资源号、产品编码、站点编码、批次号等信息</param>
    /// <returns>上料结果，成功返回(0, "上料成功")，失败返回错误代码和错误信息</returns>
    public async Task<FSharpResult<ValueTuple, (int, string)>> FeedMaterial(RequestFeedMaterialParams request)
    {
      var batchQty = 0; // 批次数量
      Guid ResourceId; // 设备ID
      var workOrder = new OrderList(); // 工单信息
      DateTime? expirationTime = null;
      // 1. 验证设备资源号是否为空
      if (request.Resource.IsNullOrEmpty())
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
      }
      else
      {
        // 2. 根据资源号获取设备信息（异步查询）
        var DeviceInfosList = await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
        if (DeviceInfosList == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
        }

        // 3. 检查设备是否已绑定工单
        if (DeviceInfosList.WorkOrderCode == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
        }

        // 4. 检查工单是否存在且状态正确（异步查询）
        workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderCode == DeviceInfosList.WorkOrderCode);
        if (workOrder == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
        }

        // 5. 设置请求的工单编码和资源ID
        request.WorkOrderCode = DeviceInfosList.WorkOrderCode;
        ResourceId = DeviceInfosList.ResourceId;
      }

      // 6. 验证产品编码是否为空
      if (request.ProductCode.IsNullOrEmpty())
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"产品信息不能为空：{request.ProductCode}"));
      }

      // 7. 根据产品编码获取产品信息（异步查询）
      var product = await _dbContext.ProductList.FirstOrDefaultAsync(x => x.ProductCode == request.ProductCode);
      if (product == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"产品不存在，产品编号：{request.ProductCode}"));
      }

      // 8. 验证站点编码是否为空
      if (request.StationCode.IsNullOrEmpty())
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点信息不能为空：{request.StationCode}"));
      }

      // 9. 检查上传站点是否存在（异步查询）
      var StationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
      if (StationList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
      }

      // 10. 检查BOM是否存在（异步查询，使用已有的workOrder变量，避免重复查询）
      var BomItemList = await _dbContext.BomItem.FirstOrDefaultAsync(x =>
          x.BomId == workOrder.BomId &&
          x.StationCode == request.StationCode &&
          x.ProductId == product.ProductListId);

      if (BomItemList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"BOM不存在，BOM编号：{workOrder.BomId}"));
      }
      // 10.1. 检查批次有效期
      if (BomItemList.ShelfLife.HasValue && BomItemList.ShelfLife > 0)
      {
        expirationTime = DateTime.Now.AddDays(BomItemList.ShelfLife.Value);
      }

      // 11. 验证批次号是否为空
      if (request.BatchCode.IsNullOrEmpty() || request.BatchCode == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次信息不能为空：{request.BatchCode}"));
      }

      // 12. 获取BOM中定义的批次规则
      string pattern = BomItemList.BatchRule;

      // 13. 优化正则表达式使用，只创建一个Regex对象
      Match match;
      if (pattern != "%")
      {
        Regex regex = new Regex(pattern);
        match = regex.Match(request.BatchCode);
      }
      else
      {
        match = Match.Empty; // 当pattern为%时，不需要匹配批次规则
      }

      // 14. 根据批次规则计算批次数量
      if (pattern == "%")
      {
        if (request.BatchQty > 0)
        {
          // 使用请求中指定的批次数量
          batchQty = request.BatchQty;
        }
        else if (BomItemList.BatchQty == true)
        {
          // 使用BOM中定义的固定批次数量
          batchQty = Convert.ToInt32(BomItemList.BatchSNQty);
        }
      }
      else
      {
        if (match.Success)
        {
          // 批次规则匹配成功
          var BomItemQty = 0;
          if (request.BatchQty > 0)
          {
            // 使用请求中指定的批次数量
            BomItemQty = Convert.ToInt32(request.BatchQty);
          }
          else
          {
            if (BomItemList.BatchQty == true)
            {
              // 使用BOM中定义的固定批次数量
              BomItemQty = Convert.ToInt32(BomItemList.BatchSNQty);
            }
            else
            {
              // 从批次号中提取批次数量（根据BOM中定义的位置和长度）
              //var splitBatchSNQty = BomItemList.BatchSNQty.Split(",");
              //BomItemQty = Convert.ToInt32(request.BatchCode.Substring(
              //    Convert.ToInt32(splitBatchSNQty[0]),
              //    Convert.ToInt32(splitBatchSNQty[1])));

              var NumberIndex = (int)BomItemList.NumberIndex;
              BomItemQty = int.Parse(match.Groups[NumberIndex].ToString());
            }
          }
          batchQty = BomItemQty;
          // 检查批次物料是否一致
          var ProductIndex = (int)BomItemList.ProductIndex;
          if (request.ProductCode != match.Groups[ProductIndex].ToString())
          {
            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次规则错误,配置物料【{request.ProductCode}】，和批次物料【{match.Groups[ProductIndex].ToString()}】不一致"));
          }
        }
        else
        {
          // 批次规则匹配失败
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次规则错误"));
        }
      }

      // 15. 检查批次是否已存在或设备是否已有上料记录（合并查询优化性能） x.BatchQty>1 单批次物料可以上多个
      var checkOrderBomBatch = await _dbContext.MesOrderBomBatch.FirstOrDefaultAsync(x =>
          x.BatchCode == request.BatchCode ||
          (x.ResourceId == ResourceId &&
           x.OrderBomBatchStatus == 1 &&
           x.StationListId == StationList.StationId &&
           x.ProductListId == product.ProductListId && x.BatchQty > 1));

      if (checkOrderBomBatch != null)
      {
        string errorMessage;
        if (checkOrderBomBatch.BatchCode == request.BatchCode)
        {
          errorMessage = $"批次号已有上料记录[{checkOrderBomBatch.BatchCode}][{checkOrderBomBatch.CreateTime}]错误";
        }
        else
        {
          errorMessage = $"设备已有上料记录，批次编号：{checkOrderBomBatch.BatchCode}";
        }
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, errorMessage));
      }

      // 16. 创建新的上料批次记录
      MesOrderBomBatch mesOrderBomBatch = new MesOrderBomBatch();
      mesOrderBomBatch.OrderBomBatchId = Guid.NewGuid(); // 生成唯一批次ID
      mesOrderBomBatch.OrderListId = workOrder.OrderListId; // 关联工单
      mesOrderBomBatch.ProductListId = product.ProductListId; // 关联产品
      mesOrderBomBatch.BatchCode = request.BatchCode; // 批次号
      mesOrderBomBatch.BatchQty = batchQty; // 批次数量
      mesOrderBomBatch.StationListId = StationList.StationId; // 关联站点
      mesOrderBomBatch.CreateTime = DateTimeOffset.Now; // 创建时间
      mesOrderBomBatch.UpdateTime = DateTimeOffset.Now; // 更新时间
      mesOrderBomBatch.OrderBomBatchStatus = 1; // 上料状态：1-有效
      mesOrderBomBatch.CompletedQty = 0; // 已完成数量：初始为0
      mesOrderBomBatch.ResourceId = ResourceId; // 关联设备资源
                                                // 设置批次过期时间
      if (BomItemList.ShelfLife.HasValue && BomItemList.ShelfLife > 0)
      {
        mesOrderBomBatch.ExpirationTime = expirationTime;// 批次过期时间
      }

      // 17. 保存上料批次记录（异步保存）
      _dbContext.MesOrderBomBatch.Add(mesOrderBomBatch);
      await _dbContext.SaveChangesAsync();

      // 18. 返回上料成功结果
      return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"上料成功"));
    }

    /// <summary>
    /// 数据上传前校验方法
    /// 负责在数据上传前进行全面校验，包括设备、工单、工艺路线、SN状态等
    /// </summary>
    /// <param name="request">上传校验请求参数，包含设备资源号、工单编码、SN号、站点编码等信息</param>
    /// <returns>校验结果，成功返回(0, "检查通过")或(0, "首站")，失败返回错误代码和错误信息</returns>
    public async Task<FSharpResult<ValueTuple, (int, string)>> UploadCheck(RequestUploadCheckParams request)
    {
      // 1. 验证设备编码和工单不能同时为空
      if (request.Resource.IsNullOrEmpty() && request.WorkOrderCode.IsNullOrEmpty())
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码和工单不能同时为空"));
      }

      var DeviceInfosList = new Deviceinfo(); // 设备信息
      var workOrder = new OrderList(); // 工单信息


      // 2. 如果提供了设备资源号，根据资源号获取设备信息
      if (!request.Resource.IsNullOrEmpty())
      {
        // 2.1 根据资源号获取设备信息（异步查询）
        DeviceInfosList = await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
        if (DeviceInfosList == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
        }

        // 2.2 检查设备是否已绑定工单
        if (DeviceInfosList.WorkOrderCode == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
        }

        // 2.3 设置请求的工单编码
        request.WorkOrderCode = DeviceInfosList.WorkOrderCode;
      }
      // 3. 如果没有提供工单编码，根据SN获取工单（手工站场景）
      else if (request.WorkOrderCode.IsNullOrEmpty())
      {
        var firstSNList = await _dbContext.mesSnListCurrents.FirstAsync(x => x.SnNumber == request.SN);
        workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderListId == firstSNList.OrderListId);
        if (workOrder == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，SN：{request.SN}"));
        }
        request.WorkOrderCode = workOrder.OrderCode;
      }



      // 4. 检查工单是否存在且状态正确（异步查询）
      workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderCode == request.WorkOrderCode);
      if (workOrder == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{request.WorkOrderCode}"));
      }

      if (workOrder.PlanQty <= workOrder.CompletedQty)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单已全部完成，工单编号：{request.WorkOrderCode}"));
      }


      // 5. 检查上传站点是否存在（异步查询）
      var StationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
      if (StationList == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
      }

      // 6. 取出该工单对应工艺路线下的所有站点明细（异步查询，按路线顺序排序）
      var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                     join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                     join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                     where p.Id == workOrder.ProcessRouteId
                                     orderby iitm.RouteSeq
                                     select new
                                     {
                                       p.RouteCode,
                                       p.RouteName,
                                       iitm.FirstStation, // 是否为首站
                                       iitm.MustPassStation, // 是否为必过站
                                       iitm.RouteSeq, // 路线顺序
                                       iitm.StationCode, // 站点编码
                                       iitm.CheckAll, // 是否需要检查所有站点
                                       iitm.CheckStationList, // 需要检查的站点列表
                                       s.StationId // 站点ID
                                     })
                               .ToListAsync();

      // 7. 工艺路线为空则直接返回错误
      if (processRouteItems.Count == 0)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线不存在，工单编号：{request.WorkOrderCode}"));
      }

      // 8. 找到当前上传站点在工艺路线中的记录
      var currentprocessRouteList = processRouteItems.First(x => x.StationCode == request.StationCode);

      // 9. 如果是首站，检查 SN 是否已存在
      if (currentprocessRouteList.FirstStation == true)
      {
        var snCurrent = await _dbContext.mesSnListCurrents
       .FirstOrDefaultAsync(x => x.SnNumber == request.SN);

        if (snCurrent != null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"首站站点SN已存在{request.SN}"));
        }
      }
      // 10. 非首站：进行更详细的SN状态检查
      else
      {
        // 10.1 检查 SN 是否存在（异步查询）
        var SNList = await _dbContext.mesSnListCurrents.FirstOrDefaultAsync(x => x.SnNumber == request.SN);
        if (SNList == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},不存在"));
        }



        // 10.2 检查工单状态是否为生产中
        if (workOrder.OrderStatus != 3)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单状态不是生产中，工单状态：{workOrder.OrderStatus.ToString()}"));
        }

        // 10.3 检查 SN 当前站点与上传站点是否一致（仅针对必过站）
        if (SNList.CurrentStationListId != StationList.StationId && currentprocessRouteList.MustPassStation == true)
        {
          var SNStationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationId == SNList.CurrentStationListId);
          if (SNStationList == null)
          {
            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不存在"));
          }
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不一致，SN站点{SNStationList.StationCode}，上传站点{request.StationCode}"));
        }

        var CurrentStationList = processRouteItems.Where(x => x.StationId == SNList.CurrentStationListId).First();
        var RequestStationList = processRouteItems.Where(x => x.StationCode == request.StationCode).First();

        if (CurrentStationList.RouteSeq < RequestStationList.RouteSeq)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号,当前站点{CurrentStationList.StationCode},不能小于{RequestStationList.StationCode}站点，顺序不一致"));
        }

        // 10.4 检查 SN 是否异常
        if (SNList.IsAbnormal == true)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前状态异常"));
        }

        // 10.5 取出 SN 历史过站记录（异步查询，包含站点信息）
        var SnListhistory = await (from snhis in _dbContext.mesSnListHistories
                                   join station in _dbContext.StationList on snhis.CurrentStationListId equals station.StationId
                                   where snhis.SnNumber == request.SN
                                   select new
                                   {
                                     SnHistory = snhis,      // SN历史记录
                                     Station = station       // 站点信息
                                   }).ToListAsync();
        // 10.51 如果当前站点正在返工
        if (SNList.IsReworking == true)
        {
          // 获取返工后的站点数据
          SnListhistory = SnListhistory.Where(x => x.SnHistory.CreateTime > SNList.ReworkTime).ToList();
        }
        // 10.6 如果当前站点要求检查所有站点都PASS
        if (currentprocessRouteList.CheckAll == true)
        {
          foreach (var processRouteItem in processRouteItems)
          {
            if (!SnListhistory.Any(x =>
                x.SnHistory.CurrentStationListId == processRouteItem.StationId &&
                x.SnHistory.StationStatus == StationStatusEnum.合格))
            {
              return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{processRouteItem.StationCode}没有PASS记录"));
            }
          }
        }
        var snCurrent2 = await _dbContext.mesSnListCurrents
       .FirstOrDefaultAsync(x => x.SnNumber == request.SN);

        if (snCurrent2 == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},不存在"));
        }
        // 10.7 如果当前站点配置了必须检查的站点列表
        if (currentprocessRouteList.CheckStationList.IsNullOrEmpty() == false)
        {
          var checkStationList = currentprocessRouteList.CheckStationList.Split(",");
          foreach (var stationCode in checkStationList)
          {
            if (!SnListhistory.Any(x =>
                x.Station.StationCode == stationCode &&
                x.SnHistory.StationStatus == StationStatusEnum.合格))
            {
              return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{stationCode}没有PASS记录"));
            }
          }
        }
        // 10.8 否则只检查上一必过站是否有PASS记录
        else if (snCurrent2.StationStatus != StationStatusEnum.跳站 && snCurrent2.ReworkStartStation != request.StationCode)
        {
          var prevMustPassStation = processRouteItems
              .Where(x =>
                  x.RouteSeq < currentprocessRouteList.RouteSeq &&
                  x.MustPassStation == true)
              .OrderByDescending(x => x.RouteSeq)
              .FirstOrDefault();

          if (prevMustPassStation != null)
          {
            if (!SnListhistory.Any(x =>
                x.SnHistory.CurrentStationListId == prevMustPassStation.StationId &&
                x.SnHistory.StationStatus == StationStatusEnum.合格))
            {
              return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},上一必过站点{prevMustPassStation.StationCode}没有PASS记录"));
            }
          }
          if (SnListhistory.Where(x => x.Station.StationCode == request.StationCode && x.SnHistory.StationStatus == StationStatusEnum.合格).Any() == true)
          {
            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{request.StationCode}已有PASS记录"));
          }
        }
      }

      // 11. 查询工单BOM与设备的关联信息（异步查询）
      var orderBom = await (from ord in _dbContext.OrderList
                            join bomitem in _dbContext.BomItem on ord.BomId equals bomitem.BomId
                            join sta in _dbContext.StationList on bomitem.StationCode equals sta.StationCode
                            // join dev in _dbContext.DeviceInfos.Where(x => x.Resource == request.Resource) on ord.OrderCode equals dev.WorkOrderCode
                            where ord.OrderListId == workOrder.OrderListId && bomitem.StationCode == request.StationCode
                            select new
                            {
                              OrderBom = ord,      // 工单信息
                              BomItem = bomitem,    // BOM项信息
                              Station = sta,        // 站点信息
                                                    //  DeviceInfo = dev      // 设备信息
                            }).ToListAsync();

      // 12. 检查每个BOM项的上料情况
      foreach (var orderBomItem in orderBom)
      {
        // 12.1 检查MesOrderBomBatch上料情况
        var orderBomBatch = await _dbContext.MesOrderBomBatch.FirstOrDefaultAsync(x =>
            x.OrderListId == workOrder.OrderListId &&
            x.StationListId == orderBomItem.Station.StationId &&
            x.ProductListId == orderBomItem.BomItem.ProductId &&
            x.ResourceId == DeviceInfosList.ResourceId && x.OrderBomBatchStatus == 1 && (request.BatchNo.IsNullOrEmpty() || x.BatchCode == request.BatchNo) &&
            x.BatchQty > x.CompletedQty);

        if (orderBomBatch == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}未进行上料操作"));
        }

        // 12.2 检查上料状态是否正常
        if (orderBomBatch.OrderBomBatchStatus != 1)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料状态异常，状态：{orderBomBatch.OrderBomBatchStatus}"));
        }

        // 12.3 检查上料数量是否足够（已使用数量 < 批次数量）
        if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料批次已使用完"));
        }
        //12.4 检查批次是否过期
        if (!orderBomBatch.ExpirationTime.HasValue && orderBomBatch.ExpirationTime < DateTime.Now)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}批次{orderBomBatch.BatchCode}已过期"));
        }
      }

      // 13. 如果是首站，直接返回首站标识
      if (currentprocessRouteList.FirstStation == true)
      {
        // 首站不继续后续校验，直接返回
        return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"首站"));
      }
      var checkFirstSNList = await _dbContext.mesSnListCurrents.FirstAsync(x => x.SnNumber == request.SN);
      var checkWorkOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderListId == checkFirstSNList.OrderListId);

      if (checkWorkOrder == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，SN：{request.SN}"));
      }
      if (checkWorkOrder.OrderListId != workOrder.OrderListId)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不一致，SN工单编号：{checkWorkOrder.OrderCode},机台工单编号：{workOrder.OrderCode}"));
      }

      // 14. 所有校验通过
      return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"检查通过"));
    }
    /// <summary>
    /// 数据上传主流程方法
    /// 负责处理生产数据的上传，包含以下主要流程：
    /// 1. 复用 UploadCheck 做全部前置校验；
    /// 2. 首站：向 mesSnListCurrents 插入新记录，更新工单状态；
    /// 3. 非首站：将当前站点记录写入历史表，测试 PASS 时自动跳站，NG 则停留；
    /// 4. 最后一站：将 Current 表状态置为完工(4)。
    /// </summary>
    /// <param name="request">上传数据请求参数，包含设备资源号、工单编码、SN号、测试结果等信息</param>
    /// <returns>上传结果，成功返回OK，失败返回错误代码和错误信息</returns>
    public async Task<FSharpResult<ValueTuple, (int, string)>> UploadData(RequestUploadCheckParams request)
    {
      // ---------- 1. 通用校验：复用UploadCheck方法进行全面校验 ----------
      var checkResult = await UploadCheck(request);
      if (checkResult.ErrorValue.Item1 != 0)
      {
        return checkResult;   // 校验不通过直接返回
      }

      // 2. 再次验证设备编码和工单不能同时为空
      if (request.Resource.IsNullOrEmpty() && request.WorkOrderCode.IsNullOrEmpty())
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码和工单不能同时为空"));
      }

      var deviceInfo = new Deviceinfo(); // 设备信息
      var workOrder = new OrderList(); // 工单信息

      // 3. 如果提供了设备资源号，根据资源号获取设备信息
      if (!request.Resource.IsNullOrEmpty())
      {
        // 3.1 根据资源号获取设备信息（异步查询）
        deviceInfo = await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
        if (deviceInfo == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
        }

        // 3.2 检查设备是否已绑定工单
        if (deviceInfo.WorkOrderCode == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
        }

        // 3.3 设置请求的工单编码
        request.WorkOrderCode = deviceInfo.WorkOrderCode;
      }
      // 4. 如果没有提供工单编码，根据SN获取工单（手工站场景）
      else if (request.WorkOrderCode.IsNullOrEmpty())
      {
        var firstSNList = await _dbContext.mesSnListCurrents.FirstAsync(x => x.SnNumber == request.SN);
        workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderListId == firstSNList.OrderListId);
        if (workOrder == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，SN：{request.SN}"));
        }
        request.WorkOrderCode = workOrder.OrderCode;
      }

      // 5. 检查工单是否存在且状态正确（异步查询）
      if (workOrder == null || workOrder.OrderCode != request.WorkOrderCode)
      {
        workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderCode == request.WorkOrderCode);
        if (workOrder == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单{request.WorkOrderCode}不存在"));
        }
      }

      // 6. 测试结果转换：将字符串结果转换为数字状态码（PASS=1，其它=2）
      StationStatusEnum uploadStationStatus = request.TestResult == "PASS" ? StationStatusEnum.合格 : StationStatusEnum.不合格;

      // ---------- 7. 首站逻辑：处理首次进站的SN ----------
      if (checkResult.ErrorValue.Item2 == "首站")
      {
        // 7.1 再次防重，确保SN在首站未被处理过
        var snCurrent = await _dbContext.mesSnListCurrents
            .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
        if (snCurrent != null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "首站站点SN已存在"));
        }

        // 7.2 获取首站站点信息（异步查询）
        var firstStation = await (from p in _dbContext.ProcessRoutes
                                  join i in _dbContext.ProcessRouteItems on p.Id equals i.HeadId
                                  join s in _dbContext.StationList on i.StationCode equals s.StationCode
                                  where p.Id == workOrder.ProcessRouteId && i.FirstStation == true
                                  select new { s.StationId, s.StationCode })
                                 .FirstOrDefaultAsync();
        if (firstStation == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "工艺路线未配置首站"));
        }

        // 7.3 创建SN当前状态记录，写入mesSnListCurrents表
        var insertSnCurrent = new MesSnListCurrent
        {
          SnNumber = request.SN,                     // SN号
          OrderListId = workOrder.OrderListId,         // 关联工单
          CurrentStationListId = firstStation.StationId, // 当前站点ID
          ResourceId = deviceInfo.ResourceId,          // 设备资源ID
          ProductListId = workOrder.ProductListId,      // 产品ID
          ProductionLineId = deviceInfo.ProductionLineId, // 生产线ID
          StationStatus = uploadStationStatus,          // 站点状态
          IsAbnormal = false,                          // 是否异常
          CreateTime = DateTime.Now,                   // 创建时间
          UpdateTime = DateTime.Now                    // 更新时间
        };
        await _dbContext.mesSnListCurrents.AddAsync(insertSnCurrent);

        // 7.4 更新工单状态：设置为生产中(3)，累计完成数+1，写入实际开始时间
        workOrder.OrderStatus = 3;
        workOrder.CompletedQty += 1;
        workOrder.ActualStartTime = DateTime.Now;
        _dbContext.OrderList.Update(workOrder);

        // 7.5 保存首站处理的所有变更（异步保存）
        await _dbContext.SaveChangesAsync();
        // 首站处理到此结束，不再走后续跳站逻辑
      }

      // ---------- 8. 非首站逻辑：写历史记录 + 跳站处理 ----------
      {
        // 8.1 根据站点编码获取站点信息（异步查询）
        var station = await _dbContext.StationList
            .FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
        if (station == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode}不存在"));
        }

        // 8.2 根据SN获取当前状态记录（异步查询）
        var snCurrent = await _dbContext.mesSnListCurrents
            .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
        if (snCurrent == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
        }

        // 8.3 将当前站点记录写入历史表
        var snHistory = new MesSnListHistory
        {
          SNListHistoryId = Guid.NewGuid(),                  // 历史记录ID
          SnNumber = snCurrent.SnNumber,                     // SN号
          OrderListId = snCurrent.OrderListId,               // 关联工单
          CurrentStationListId = station.StationId, // 当前站点ID
          ProductionLineId = deviceInfo.ProductionLineId,    // 生产线ID
          ProductListId = workOrder.ProductListId,           // 产品ID
          ResourceId = deviceInfo.ResourceId,                // 设备资源ID
          StationStatus = uploadStationStatus,               // 站点状态
          IsAbnormal = false,                                // 是否异常
          CreateTime = DateTime.Now,                         // 创建时间
          UpdateTime = DateTime.Now,                         // 更新时间
          TestData = request.TestData                         // 测试数据
        };
        await _mesSNListHistory.InsertAsync(snHistory);   // 使用仓储插入历史记录

        // 8.4 处理测试数据：将JSON字符串反序列化并批量插入MesSnTestData表
        List<MesSnTestData> mesSnTestData = new List<MesSnTestData>();
        if (!string.IsNullOrWhiteSpace(request.TestData))
        {
          try
          {
            var testDataArray = JsonSerializer.Deserialize<List<MesSnTestData>>(request.TestData);
            if (testDataArray != null)
            {
              foreach (var item in testDataArray)
              {
                var testData = new MesSnTestData
                {
                  SNListHistoryId = snHistory.SNListHistoryId, // 关联历史记录ID
                  ParametricKey = item.ParametricKey,         // 参数名称
                  TestValue = item.TestValue,                 // 测试值
                  Upperlimit = item.Upperlimit,               // 上限
                  Lowerlimit = item.Lowerlimit,               // 下限
                  Units = item.Units,                         // 单位
                  TestResult = item.TestResult,               // 测试结果
                  Remark = item.Remark,                       // 备注
                  CreateTime = DateTime.Now,                  // 创建时间
                  UpdateTime = DateTime.Now                   // 更新时间
                };
                mesSnTestData.Add(testData);
              }
              if (mesSnTestData.Any())
              {
                await _dbContext.mesSnTestDatas.AddRangeAsync(mesSnTestData);
                // 延迟保存，与后续操作一起批量提交
              }
            }
          }
          catch (Exception ex)
          {
            // 记录日志或根据业务需要处理异常
            // 此处选择继续流程，仅忽略测试数据插入失败
          }
        }
        await _dbContext.SaveChangesAsync();
        // 8.5 测试结果处理：如果测试NG，直接返回，不再跳站
        if (uploadStationStatus != StationStatusEnum.合格)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"测试结果为{request.TestResult}，数据上传成功{request.SN}"));
        }



        // 8.6 查询工单BOM与设备的关联信息（异步查询）
        var orderBom = await (from ord in _dbContext.OrderList
                              join bomitem in _dbContext.BomItem on ord.BomId equals bomitem.BomId
                              join sta in _dbContext.StationList on bomitem.StationCode equals sta.StationCode
                              //  join dev in _dbContext.DeviceInfos.Where(x => x.Resource == request.Resource) on ord.OrderCode equals dev.WorkOrderCode
                              where ord.OrderListId == workOrder.OrderListId && bomitem.StationCode == request.StationCode
                              select new
                              {
                                OrderBom = ord,      // 工单信息
                                BomItem = bomitem,    // BOM项信息
                                Station = sta,        // 站点信息
                                                      //   DeviceInfo = dev      // 设备信息
                              }).ToListAsync();

        // 8.7 处理每个BOM项的上料批次信息
        foreach (var orderBomItem in orderBom)
        {
          // 8.7.1 检查MesOrderBomBatch上料情况（异步查询）
          var orderBomBatch = await _dbContext.MesOrderBomBatch
              .FirstOrDefaultAsync(x => x.OrderListId == workOrder.OrderListId &&
              x.StationListId == orderBomItem.Station.StationId &&
              x.ProductListId == orderBomItem.BomItem.ProductId &&
              x.ResourceId == deviceInfo.ResourceId &&
              x.OrderBomBatchStatus == 1 && (request.BatchNo.IsNullOrEmpty() || x.BatchCode == request.BatchNo) &&
              x.BatchQty > x.CompletedQty);
          if (orderBomBatch == null)
          {
            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}未进行上料操作"));
          }

          // 8.7.2 检查上料状态是否正常
          if (orderBomBatch.OrderBomBatchStatus != 1)
          {
            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料状态异常，状态：{orderBomBatch.OrderBomBatchStatus}"));
          }

          // 8.7.3 检查上料数量是否足够（已使用数量 < 批次数量）
          if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
          {
            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料批次已使用完"));
          }

          // 8.7.4 更新上料批次的已完成数量
          orderBomBatch.CompletedQty = (orderBomBatch.CompletedQty ?? 0) + 1;
          if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
          {
            // 如果已完成数量达到批次数量，将上料状态标记为已完成(2)
            orderBomBatch.OrderBomBatchStatus = 2;
          }

          // 8.7.5 创建上料批次项记录
          var orderBomBatchitem = new MesOrderBomBatchItem
          {
            OrderBomBatchId = orderBomBatch.OrderBomBatchId, // 关联上料批次
            SnNumber = request.SN,                           // 关联SN
            CreateTime = DateTime.Now,                       // 创建时间
            UpdateTime = DateTime.Now                        // 更新时间
          };

          // 8.7.6 添加上料批次项记录并更新上料批次
          await _dbContext.MesOrderBomBatchItem.AddAsync(orderBomBatchitem);
          _dbContext.MesOrderBomBatch.Update(orderBomBatch);
          // 延迟保存，与后续操作一起批量提交
        }

        // 8.8 获取工艺路线并计算下一站点（异步查询）
        var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                       join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                       join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                       join ord in _dbContext.OrderList on p.Id equals ord.ProcessRouteId
                                       where ord.OrderListId == snCurrent.OrderListId // 关联工单
                                       orderby iitm.RouteSeq                          // 按路线顺序排序
                                       select new
                                       {
                                         iitm.StationCode,
                                         s.StationId,
                                         iitm.RouteSeq,
                                         iitm.MustPassStation
                                       })
                                .ToListAsync();

        // 8.9 找到当前站点在工艺路线中的记录
        var currentStep = processRouteItems.FirstOrDefault(x => x.StationCode == request.StationCode);
        if (currentStep == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线中找不到站点{request.StationCode}"));
        }

        // 8.10 处理非必过站的跳站逻辑
        if (currentStep.MustPassStation == true)
        {
          // 8.10.1 查找下一个必过站
          var nextStep = processRouteItems
              .Where(x => x.RouteSeq > currentStep.RouteSeq && x.MustPassStation == true)
              .OrderBy(x => x.RouteSeq)
              .FirstOrDefault();

          if (nextStep == null)
          {
            // 8.10.2 如果是最后一站，将状态置为完工(4)
            snCurrent.StationStatus = StationStatusEnum.合格;
          }
          else
          {
            // 8.10.3 跳到下一个必过站
            snCurrent.CurrentStationListId = nextStep.StationId;
            snCurrent.ResourceId = deviceInfo.ResourceId;
            snCurrent.ProductionLineId = deviceInfo.ProductionLineId;
          }

          // 8.10.4 更新SN当前状态记录的更新时间
          snCurrent.UpdateTime = DateTime.Now;
          _dbContext.mesSnListCurrents.Update(snCurrent);
        }

        // 8.11 批量提交所有待处理的数据库操作
        await _dbContext.SaveChangesAsync();

      }

      // ---------- 5. 全部完成 ----------
      return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));
    }



    public async Task<FSharpResult<ValueTuple, (int, string)>> UploadMaster(RequestUploadCheckParams request)
    {

      // 2. 再次验证设备编码和工单不能同时为空
      if (request.Resource.IsNullOrEmpty() && request.WorkOrderCode.IsNullOrEmpty())
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码和工单不能同时为空"));
      }

      var deviceInfo = new Deviceinfo(); // 设备信息
      var workOrder = new OrderList(); // 工单信息

      // 3.1 根据资源号获取设备信息（异步查询）
      deviceInfo = await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
      if (deviceInfo == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
      }

      // 3.2 检查设备是否已绑定工单
      if (deviceInfo.WorkOrderCode == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
      }
      var firstSNList = await _dbContext.mesSnListCurrents.FirstAsync(x => x.SnNumber == request.SN);
      workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderListId == firstSNList.OrderListId);
      if (workOrder == null)
      {
        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，SN：{request.SN}"));
      }
      request.WorkOrderCode = workOrder.OrderCode;


      // 6. 测试结果转换：将字符串结果转换为数字状态码 6=点检
      var uploadStationStatus = StationStatusEnum.点检;


      // ---------- 8. 非首站逻辑：写历史记录 + 跳站处理 ----------
      {
        // 8.1 根据站点编码获取站点信息（异步查询）
        var station = await _dbContext.StationList
            .FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
        if (station == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode}不存在"));
        }

        // 8.2 根据SN获取当前状态记录（异步查询）
        var snCurrent = await _dbContext.mesSnListCurrents
            .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
        if (snCurrent == null)
        {
          return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
        }

        // 8.3 将当前站点记录写入历史表
        var snHistory = new MesSnListHistory
        {
          SNListHistoryId = Guid.NewGuid(),                  // 历史记录ID
          SnNumber = snCurrent.SnNumber,                     // SN号
          OrderListId = snCurrent.OrderListId,               // 关联工单
          CurrentStationListId = station.StationId, // 当前站点ID
          ProductionLineId = deviceInfo.ProductionLineId,    // 生产线ID
          ProductListId = workOrder.ProductListId,           // 产品ID
          ResourceId = deviceInfo.ResourceId,                // 设备资源ID
          StationStatus = uploadStationStatus,               // 站点状态
          IsAbnormal = false,                                // 是否异常
          CreateTime = DateTime.Now,                         // 创建时间
          UpdateTime = DateTime.Now,                         // 更新时间
          TestData = request.TestData,                         // 测试数据
          Remark = "点检"
        };
        await _mesSNListHistory.InsertAsync(snHistory);   // 使用仓储插入历史记录

        // 8.4 处理测试数据：将JSON字符串反序列化并批量插入MesSnTestData表
        List<MesSnTestData> mesSnTestData = new List<MesSnTestData>();
        if (!string.IsNullOrWhiteSpace(request.TestData))
        {
          try
          {
            var testDataArray = JsonSerializer.Deserialize<List<MesSnTestData>>(request.TestData);
            if (testDataArray != null)
            {
              foreach (var item in testDataArray)
              {
                var testData = new MesSnTestData
                {
                  SNListHistoryId = snHistory.SNListHistoryId, // 关联历史记录ID
                  ParametricKey = item.ParametricKey,         // 参数名称
                  TestValue = item.TestValue,                 // 测试值
                  Upperlimit = item.Upperlimit,               // 上限
                  Lowerlimit = item.Lowerlimit,               // 下限
                  Units = item.Units,                         // 单位
                  TestResult = item.TestResult,               // 测试结果
                  Remark = item.Remark,                       // 备注
                  CreateTime = DateTime.Now,                  // 创建时间
                  UpdateTime = DateTime.Now                   // 更新时间
                };
                mesSnTestData.Add(testData);
              }
              if (mesSnTestData.Any())
              {
                await _dbContext.mesSnTestDatas.AddRangeAsync(mesSnTestData);
                // 延迟保存，与后续操作一起批量提交
              }
            }
          }
          catch (Exception ex)
          {
            // 记录日志或根据业务需要处理异常
            // 此处选择继续流程，仅忽略测试数据插入失败
          }
        }

      }
      // 8.11 批量提交所有待处理的数据库操作
      await _dbContext.SaveChangesAsync();
      // ---------- 5. 全部完成 ----------
      return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));
    }

    /// <summary>
    /// SN追溯方法
    /// 负责查询单个SN的完整生产记录，包括站点过站记录、上料批次信息和测试数据
    /// </summary>
    /// <param name="sn">要追溯的SN号</param>
    /// <returns>追溯结果DTO，包含SN的当前状态、历史过站记录、测试数据和上料批次信息</returns>
    public async Task<FSharpResult<SnTraceDto, (int, string)>> TraceSN(string sn)
    {
      // 1. 验证SN号是否为空
      if (string.IsNullOrWhiteSpace(sn))
      {
        return FSharpResult<SnTraceDto, (int, string)>.NewError((-1, "SN不能为空"));
      }

      // 2. 查询SN的当前状态信息（异步查询，包含工单和当前站点信息）
      var snCurrent = await _dbContext.mesSnListCurrents
          .Include(c => c.OrderList)       // 包含工单信息
          .Include(c => c.StationList)     // 包含当前站点信息
          .Include(c => c.ProductList) // 包含产品信息
          .FirstOrDefaultAsync(c => c.SnNumber == sn);

      if (snCurrent == null)
      {
        return FSharpResult<SnTraceDto, (int, string)>.NewError((-1, $"SN:{sn} 不存在"));
      }

      // 3. 查询SN的历史过站记录（异步查询，包含站点和设备资源信息）
      var histories = await _dbContext.mesSnListHistories
          .Include(h => h.StationList)     // 包含站点信息
          .Include(h => h.Resource)        // 包含设备资源信息
          .Where(h => h.SnNumber == sn)
          .OrderBy(h => h.CreateTime)      // 按创建时间排序
          .ToListAsync();

      // 4. 查询SN的测试数据（异步查询，基于历史记录ID关联）
      var historyIds = histories.Select(h => h.SNListHistoryId).ToList();
      var testData = await _dbContext.mesSnTestDatas
          .Where(t => historyIds.Contains(t.SNListHistoryId))
          .ToListAsync();

      // 5. 查询SN的上料批次及明细信息（异步查询，包含批次和站点信息）
      var batchItems = await _dbContext.MesOrderBomBatchItem
          .Include(b => b.OrderBomBatch)           // 包含上料批次信息
          .ThenInclude(b => b.StationList)         // 包含站点信息
          .Include(b => b.OrderBomBatch.ProductList) // 包含产品信息
          .Where(b => b.SnNumber == sn)
          .ToListAsync();

      // 6. 组装SN追溯结果DTO
      var dto = new SnTraceDto
      {
        SN = sn,                                     // SN号
        OrderCode = snCurrent.OrderList?.OrderCode,   // 工单编码
        ProductCode = snCurrent?.ProductList?.ProductCode, // 产品编码
        CurrentStation = snCurrent.StationList?.StationCode, // 当前站点
        StationStatus = snCurrent.StationStatus,      // 站点状态
        IsAbnormal = snCurrent.IsAbnormal,            // 是否异常
        CreateTime = snCurrent.CreateTime.Value.DateTime, // 创建时间
        UpdateTime = snCurrent.UpdateTime.Value.DateTime, // 更新时间
        Stations = histories.Select(h => new StationTraceDto
        {
          StationCode = h.StationList.StationCode,
          StationName = h.StationList.StationName,
          StationStatus = h.StationStatus,
          ResourceCode = h.Resource?.Resource,
          TestResult = h.StationStatus.ToString(),
          Remark = h.Remark,
          TestTime = h.CreateTime.Value.DateTime,
          TestData = testData
                .Where(t => t.SNListHistoryId == h.SNListHistoryId)
                .Select(t => new TestDataDto
                {
                  ParametricKey = t.ParametricKey,
                  TestValue = t.TestValue,
                  Upperlimit = t.Upperlimit.ToString(),
                  Lowerlimit = t.Lowerlimit.ToString(),
                  Units = t.Units,
                  TestResult = t.TestResult,
                  Remark = t.Remark
                }).ToList()
        }).ToList(),
        FeedingBatches = batchItems.GroupBy(b => b.OrderBomBatch).Select(g => new FeedingBatchDto
        {
          BatchCode = g.Key.BatchCode,
          StationCode = g.Key.StationList.StationCode,
          ProductCode = g.Key.ProductList.ProductCode,
          BatchQty = (int)g.Key.BatchQty,
          CompletedQty = (int)g.Key.CompletedQty,
          Status = (int)g.Key.OrderBomBatchStatus,
          CreateTime = g.Key.CreateTime.Value.DateTime,
          Items = g.Select(x => new FeedingItemDto
          {
            SnNumber = x.SnNumber,
            CreateTime = x.CreateTime.Value.DateTime
          }).ToList()
        }).ToList()
      };

      return FSharpResult<SnTraceDto, (int, string)>.NewOk(dto);
    }

    /// <summary>
    /// SN追溯DTO
    /// </summary>
    public class SnTraceDto
    {
      public string SN { get; set; }
      public string OrderCode { get; set; }
      public string ProductCode { get; set; }
      public string CurrentStation { get; set; }
      public StationStatusEnum StationStatus { get; set; }
      public bool? IsAbnormal { get; set; }
      public DateTime CreateTime { get; set; }
      public DateTime UpdateTime { get; set; }
      public List<StationTraceDto> Stations { get; set; } = new();
      public List<FeedingBatchDto> FeedingBatches { get; set; } = new();
    }

    public class StationTraceDto
    {
      public string StationCode { get; set; }
      public string StationName { get; set; }
      public StationStatusEnum StationStatus { get; set; }
      public string ResourceCode { get; set; }
      public string TestResult { get; set; }
      public DateTime TestTime { get; set; }
      public List<TestDataDto> TestData { get; set; } = new();
      public string? Remark { get; internal set; }
    }

    public class TestDataDto
    {
      public string ParametricKey { get; set; }
      public string TestValue { get; set; }
      public string Upperlimit { get; set; }
      public string Lowerlimit { get; set; }
      public string Units { get; set; }
      public string TestResult { get; set; }
      public string Remark { get; set; }
    }

    public class FeedingBatchDto
    {
      public string BatchCode { get; set; }
      public string StationCode { get; set; }
      public string ProductCode { get; set; }
      public int BatchQty { get; set; }
      public int CompletedQty { get; set; }
      public int Status { get; set; }
      public DateTime CreateTime { get; set; }
      public List<FeedingItemDto> Items { get; set; } = new();
    }

    public class FeedingItemDto
    {
      public string SnNumber { get; set; }
      public DateTime CreateTime { get; set; }
    }

  }
}