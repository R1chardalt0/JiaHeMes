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

namespace ChargePadLine.Service.Trace.Impl
{
    /// <summary>
    /// 工单服务实现
    /// </summary>
    public class CommonInterfaseService : ICommonInterfaseService
    {
        private readonly IRepository<WorkOrder> _workOrderRepo;
        private readonly IRepository<BomRecipe> _bomRecipeRepo;
        private readonly IRepository<Material> _materialRepo;
        private readonly IRepository<MesSnListHistory> _mesSNListHistory;
        private readonly ILogger<OrderListService> _logger;
        private readonly AppDbContext _dbContext;

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
        /// 投料
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<FSharpResult<ValueTuple, (int, string)>> FeedMaterial(RequestFeedMaterialParams request)
        {
            var batchQty = 0;
            Guid ResourceId;
            var workOrder=new OrderList();
            if (request.Resource.IsNullOrEmpty())
            {
               return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
            }
            else
            {
                // 获取设备信息（异步查询）
                var DeviceInfosList =  await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
                if (DeviceInfosList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
                }
                if (DeviceInfosList.WorkOrderCode == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
                }

                // 检查工单是否存在且状态正确（异步查询）
                 workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderCode == DeviceInfosList.WorkOrderCode);
                if (workOrder == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{DeviceInfosList.WorkOrderCode}"));
                }
                request.WorkOrderCode = DeviceInfosList.WorkOrderCode;
                ResourceId = DeviceInfosList.ResourceId;
            }

            if (request.ProductCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"产品信息不能为空：{request.ProductCode}"));
            }

            // 获取产品信息（异步查询）
            var product = await _dbContext.ProductList.FirstOrDefaultAsync(x => x.ProductCode == request.ProductCode);

            if (product == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"产品不存在，产品编号：{request.ProductCode}"));
            }

            if (request.StationCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点信息不能为空：{request.StationCode}"));
            }
            // 检查上传站点是否存在（异步查询）
            var StationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
            if (StationList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
            }

            // 检查BOM是否存在（异步查询，使用已有的workOrder变量，避免重复查询）
            var BomItemList = await _dbContext.BomItem.FirstOrDefaultAsync(x => x.BomId == workOrder.BomId && x.StationCode == request.StationCode && x.ProductId == product.ProductListId);

            if (BomItemList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"BOM不存在，BOM编号：{workOrder.BomId}"));
            }

            if (request.BatchCode.IsNullOrEmpty() || request.BatchCode == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次信息不能为空：{request.BatchCode}"));
            }

            // 正则表达式（带分组）
            string pattern = BomItemList.BatchRule;

            // 优化正则表达式使用，只创建一个Regex对象
            Match match;
            if (pattern != "%")
            {
                Regex regex = new Regex(pattern);
                match = regex.Match(request.BatchCode);
            }
            else
            {
                match = Match.Empty; // 当pattern为%时，不需要匹配
            }

            if(pattern=="%" )
            {
                if (BomItemList.BatchQty == true)
                {
                    batchQty = Convert.ToInt32(BomItemList.BatchSNQty);
                }
                else if(request.BatchQty > 0)
                {
                    batchQty = request.BatchQty;
                }
                
            }
            else
            {
                if (match.Success)
                {
                    //Console.WriteLine("匹配成功！");
                    //Console.WriteLine($"完整匹配: {match.Value}");
                    //Console.WriteLine($"日期: {match.Groups[1].Value}");  // 第一组：日期
                    //Console.WriteLine($"数量: {match.Groups[2].Value}");  // 第二组：数量
                    //Console.WriteLine($"流水号: {match.Groups[3].Value}"); // 第三组：流水号



                    //if (product.ProductCode != match.Groups[1].Value)
                    //{
                    //    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次规则错误，批次物料{match.Groups[1].Value}，BOM物料{product.ProductCode}"));
                    //}

                    var BomItemQty = 0;
                    if (request.BatchQty > 0)
                    {
                        BomItemQty = Convert.ToInt32(request.BatchQty);
                    }
                    else
                    {
                        if (BomItemList.BatchQty == true)
                        {
                            BomItemQty = Convert.ToInt32(BomItemList.BatchSNQty);
                        }
                        else
                        {
                            var splitBatchSNQty = BomItemList.BatchSNQty.Split(",");
                            BomItemQty = Convert.ToInt32(request.BatchCode.Substring(Convert.ToInt32(splitBatchSNQty[0]), Convert.ToInt32(splitBatchSNQty[1])));
                        }
                    }
                    batchQty = BomItemQty;
                }
                else
                {

                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"批次规则错误"));
                }
            }

            // 合并批次检查逻辑为一个异步查询
            var checkOrderBomBatch = await _dbContext.MesOrderBomBatch.FirstOrDefaultAsync(x => 
                x.BatchCode == request.BatchCode || 
                (x.ResourceId == ResourceId && 
                 x.OrderBomBatchStatus == 1 && 
                 x.StationListId == StationList.StationId && 
                 x.ProductListId == product.ProductListId));
            
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
            
            // 创建新的上料批次
            MesOrderBomBatch mesOrderBomBatch = new MesOrderBomBatch();
            mesOrderBomBatch.OrderBomBatchId = Guid.NewGuid();
            mesOrderBomBatch.OrderListId = workOrder.OrderListId;
            mesOrderBomBatch.ProductListId = product.ProductListId;
            mesOrderBomBatch.BatchCode = request.BatchCode;
            mesOrderBomBatch.BatchQty = batchQty;
            mesOrderBomBatch.StationListId = StationList.StationId;
            mesOrderBomBatch.CreateTime = DateTimeOffset.Now;
            mesOrderBomBatch.UpdateTime = DateTimeOffset.Now;
            mesOrderBomBatch.OrderBomBatchStatus = 1;
            mesOrderBomBatch.CompletedQty = 0;
            mesOrderBomBatch.ResourceId = ResourceId;
            
            _dbContext.MesOrderBomBatch.Add(mesOrderBomBatch);
            await _dbContext.SaveChangesAsync(); // 使用异步保存

            



            return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"上料成功"));
        }

        /// <summary>
        /// 上传前校验：检查设备、工单、工艺路线、SN 状态等
        /// </summary>
        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadCheck(RequestUploadCheckParams request)
        {
            if (request.Resource.IsNullOrEmpty() && request.WorkOrderCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码和工单不能同时为空"));
            }
            var DeviceInfosList = new Deviceinfo();
            var workOrder=new OrderList();
            if (!request.Resource.IsNullOrEmpty())
            {
                // 1. 根据资源号获取设备信息（异步查询）
                DeviceInfosList = await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
                if (DeviceInfosList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
                }

                // 2. 检查设备是否已绑定工单
                if (DeviceInfosList.WorkOrderCode == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
                }
                request.WorkOrderCode = DeviceInfosList.WorkOrderCode;
            }
            //手工站根据SN 获取工单
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




            // 3. 检查工单是否存在且状态正确（异步查询）
            workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderCode == request.WorkOrderCode);
            if (workOrder == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单不存在，工单编号：{request.WorkOrderCode}"));
            }

            // 4. 检查上传站点是否存在（异步查询）
            var StationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
            if (StationList == null)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode},不存在"));
            }

            // 5. 取出该工单对应工艺路线下的所有站点明细（异步查询）
            var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                          join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                          join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                          where p.Id == workOrder.ProcessRouteId
                                          orderby iitm.RouteSeq
                                          select new { p.RouteCode, p.RouteName, iitm.FirstStation, iitm.MustPassStation, iitm.RouteSeq, iitm.StationCode, iitm.CheckAll, iitm.CheckStationList, s.StationId })
                                     .ToListAsync();

            // 6. 工艺路线为空则直接返回错误
            if (processRouteItems.Count == 0)
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线不存在，工单编号：{request.WorkOrderCode}"));
            }

            // 7. 找到当前上传站点在工艺路线中的记录
            var currentprocessRouteList = processRouteItems.First(x => x.StationCode == request.StationCode);

            // 8. 如果是首站，检查 SN 是否已存在
            if (currentprocessRouteList.FirstStation == true)
            {
                var snCurrent = await _dbContext.mesSnListCurrents
               .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent != null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"首站站点SN已存在{request.SN}"));
                }

            }
            else
            {
                // 9. 非首站：检查 SN 是否存在（异步查询）
                var SNList = await _dbContext.mesSnListCurrents.FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (SNList == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},不存在"));
                }

                // 10. 检查工单状态是否为生产中
                if (workOrder.OrderStatus != 3)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单状态不是生产中，工单状态：{workOrder.OrderStatus.ToString()}"));
                }

                // 11. 检查 SN 当前站点与上传站点是否一致
                if (SNList.CurrentStationListId != StationList.StationId && currentprocessRouteList.MustPassStation == true)
                {
                    var SNStationList = await _dbContext.StationList.FirstOrDefaultAsync(x => x.StationId == SNList.CurrentStationListId);
                    if (SNStationList == null)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不存在"));
                    }
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前站点不一致，SN站点{SNStationList.StationCode}，上传站点{request.StationCode}"));
                }

                // 12. 检查 SN 是否异常
                if (SNList.IsAbnormal == true)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},当前状态异常"));
                }

                // 13. 取出 SN 历史过站记录（异步查询）
                var SnListhistory = await (from snhis in _dbContext.mesSnListHistories
                                           join station in _dbContext.StationList on snhis.CurrentStationListId equals station.StationId
                                           where snhis.SnNumber == request.SN
                                           select new
                                           {
                                               SnHistory = snhis,      // 包含mesSnListHistories表的所有列
                                               Station = station       // 包含StationList表的所有列
                                           }).ToListAsync();

                // 14. 如果当前站点要求检查所有站点都PASS
                if (currentprocessRouteList.CheckAll == true)
                {
                    foreach (var processRouteItem in processRouteItems)
                    {
                        if (!SnListhistory.Any(x => x.SnHistory.CurrentStationListId == processRouteItem.StationId && x.SnHistory.StationStatus == 1))
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{processRouteItem.StationCode}没有PASS记录"));
                        }
                    }
                }

                // 15. 如果当前站点配置了必须检查的站点列表
                if (currentprocessRouteList.CheckStationList.IsNullOrEmpty() == false)
                {
                    var checkStationList = currentprocessRouteList.CheckStationList.Split(",");
                    foreach (var stationCode in checkStationList)
                    {
                        if (!SnListhistory.Any(x => x.Station.StationCode == stationCode && x.SnHistory.StationStatus == 1))
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},站点{stationCode}没有PASS记录"));
                        }
                    }
                }
                else
                {
                    // 16. 否则只检查上一必过站是否有PASS记录
                    var prevMustPassStation = processRouteItems
                        .Where(x => x.RouteSeq < currentprocessRouteList.RouteSeq && x.MustPassStation == true)
                        .OrderByDescending(x => x.RouteSeq)
                        .FirstOrDefault();
                    if (prevMustPassStation != null)
                    {
                        if (!SnListhistory.Any(x => x.SnHistory.CurrentStationListId == prevMustPassStation.StationId && x.SnHistory.StationStatus == 1))
                        {
                            return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN},上一必过站点{prevMustPassStation.StationCode}没有PASS记录"));
                        }
                    }
                }
            }

            var orderBom = await (from ord in _dbContext.OrderList
                                  join bomitem in _dbContext.BomItem on ord.BomId equals bomitem.BomId
                                  join sta in _dbContext.StationList on bomitem.StationCode equals sta.StationCode
                                  join dev in _dbContext.DeviceInfos.Where(x=>x.Resource==request.Resource) on ord.OrderCode equals dev.WorkOrderCode  
                                  where ord.OrderListId == workOrder.OrderListId && bomitem.StationCode == request.StationCode 
                                  select new
                                  {
                                      OrderBom = ord,
                                      BomItem = bomitem,
                                      Station = sta,
                                      DeviceInfo = dev
                                  }).ToListAsync();

            foreach (var orderBomItem in orderBom)
            {
                // 17. 检查MesOrderBomBatch上料情况
                var orderBomBatch = _dbContext.MesOrderBomBatch
                    .FirstOrDefault(x => x.OrderListId == workOrder.OrderListId && x.StationListId == orderBomItem.Station.StationId &&
                    x.ProductListId == orderBomItem.BomItem.ProductId && x.ResourceId== DeviceInfosList.ResourceId && x.BatchQty>x.CompletedQty);
                if (orderBomBatch == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}未进行上料操作"));
                }

                // 18. 检查上料状态是否正常
                if (orderBomBatch.OrderBomBatchStatus != 1)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料状态异常，状态：{orderBomBatch.OrderBomBatchStatus}"));
                }

                // 19. 检查上料数量是否足够（已使用数量 < 批次数量）
                if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料批次已使用完"));
                }
            }

            if (currentprocessRouteList.FirstStation == true)
            {
                // 首站不继续后续校验，直接返回
                return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"首站"));
            }
            // 20. 所有校验通过
            return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"检查通过"));
        }
        /// <summary>
        /// 数据上传主流程：
        /// 1. 复用 UploadCheck 做全部前置校验；
        /// 2. 首站：向 mesSnListCurrents 插入新记录，更新工单状态；
        /// 3. 非首站：将当前站点记录写入历史表，测试 PASS 时自动跳站，NG 则停留；
        /// 4. 最后一站：将 Current 表状态置为完工(4)。
        /// </summary>
        public async Task<FSharpResult<ValueTuple, (int, string)>> UploadData(RequestUploadCheckParams request)
        {
            // ---------- 1. 通用校验 ----------
            var checkResult = await UploadCheck(request);
            if (checkResult.ErrorValue.Item1 != 0)
            {
                return checkResult;   // 校验不通过直接返回
            }

            if (request.Resource.IsNullOrEmpty() && request.WorkOrderCode.IsNullOrEmpty())
            {
                return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码和工单不能同时为空"));
            }
            
            var deviceInfo = new Deviceinfo();
            var workOrder = new OrderList();
            
            if (!request.Resource.IsNullOrEmpty())
            {
                // 1. 根据资源号获取设备信息（异步查询）
                deviceInfo = await _dbContext.DeviceInfos.FirstOrDefaultAsync(x => x.Resource == request.Resource);
                if (deviceInfo == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备编码{request.Resource},不存在"));
                }

                // 2. 检查设备是否已绑定工单
                if (deviceInfo.WorkOrderCode == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"设备未绑定工单，设备资源号：{request.Resource}"));
                }
                
                request.WorkOrderCode = deviceInfo.WorkOrderCode;
            }
            //手工站根据SN 获取工单
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

            // 3. 检查工单是否存在且状态正确（异步查询）
            if (workOrder == null || workOrder.OrderCode != request.WorkOrderCode)
            {
                workOrder = await _dbContext.OrderList.FirstOrDefaultAsync(x => x.OrderCode == request.WorkOrderCode);
                if (workOrder == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工单{request.WorkOrderCode}不存在"));
                }
            }

            // 测试结果转换：PASS=1，其它=2
            var uploadStationStatus = request.TestResult == "PASS" ? 1 : 2;

            // ---------- 3. 首站逻辑 ----------
            if (checkResult.ErrorValue.Item2 == "首站")
            {
                // 再次防重
                var snCurrent = await _dbContext.mesSnListCurrents
                    .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent != null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, "首站站点SN已存在"));
                }

                // 取首站站点
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

                // 写入 Current 表
                var insertSnCurrent = new MesSnListCurrent
                {
                    SnNumber = request.SN,
                    OrderListId = workOrder.OrderListId,
                    CurrentStationListId = firstStation.StationId,
                    ResourceId = deviceInfo.ResourceId,
                    ProductListId = workOrder.ProductListId,
                    ProductionLineId = deviceInfo.ProductionLineId,
                    StationStatus = uploadStationStatus,
                    IsAbnormal = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };
                await _dbContext.mesSnListCurrents.AddAsync(insertSnCurrent);

                // 更新工单：状态=生产中(3)，累计完成数+1，写入实际开始时间
                workOrder.OrderStatus = 3;
                workOrder.CompletedQty += 1;
                workOrder.ActualStartTime = DateTime.Now;
                _dbContext.OrderList.Update(workOrder);

                await _dbContext.SaveChangesAsync();
                // 首站到此结束，不再走后续跳站逻辑
            }

            {
                // ---------- 4. 非首站：写历史 + 跳站 ----------
                var station = await _dbContext.StationList
                    .FirstOrDefaultAsync(x => x.StationCode == request.StationCode);
                if (station == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点编码{request.StationCode}不存在"));
                }

                var snCurrent = await _dbContext.mesSnListCurrents
                    .FirstOrDefaultAsync(x => x.SnNumber == request.SN);
                if (snCurrent == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"SN序列号{request.SN}在Current表中不存在"));
                }

                // 4.1 写入历史表
                var snHistory = new MesSnListHistory
                {
                    SNListHistoryId = Guid.NewGuid(),
                    SnNumber = snCurrent.SnNumber,
                    OrderListId = snCurrent.OrderListId,
                    CurrentStationListId = snCurrent.CurrentStationListId,
                    ProductionLineId = deviceInfo.ProductionLineId,
                    ProductListId = workOrder.ProductListId,
                    ResourceId = deviceInfo.ResourceId,
                    StationStatus = uploadStationStatus,
                    IsAbnormal = false,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    TestData = request.TestData
                };
                await _mesSNListHistory.InsertAsync(snHistory);   // 使用仓储插入


               List<MesSnTestData> mesSnTestData = new List<MesSnTestData>();
                // 将 request.TestData（JSON 字符串）反序列化并批量插入 MesSnTestData
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
                                    SNListHistoryId = snHistory.SNListHistoryId,
                                    ParametricKey = item.ParametricKey,
                                    TestValue = item.TestValue,
                                    Upperlimit = item.Upperlimit,
                                    Lowerlimit = item.Lowerlimit,
                                    Units = item.Units,
                                    TestResult = item.TestResult,
                                    Remark = item.Remark,
                                    CreateTime = DateTime.Now,
                                    UpdateTime = DateTime.Now
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



                // 4.2 测试 NG 直接返回，不再跳站
                if (uploadStationStatus != 1)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((0, $"测试结果为{request.TestResult}，数据上传成功{request.SN}"));
                }



                var orderBom = await (from ord in _dbContext.OrderList
                                      join bomitem in _dbContext.BomItem on ord.BomId equals bomitem.BomId
                                      join sta in _dbContext.StationList on bomitem.StationCode equals sta.StationCode
                                      join dev in _dbContext.DeviceInfos.Where(x => x.Resource == request.Resource) on ord.OrderCode equals dev.WorkOrderCode
                                      where ord.OrderListId == workOrder.OrderListId && bomitem.StationCode == request.StationCode
                                      select new
                                      {
                                          OrderBom = ord,
                                          BomItem = bomitem,
                                          Station = sta,
                                          DeviceInfo = dev
                                      }).ToListAsync();

                foreach (var orderBomItem in orderBom)
                {
                    // 17. 检查MesOrderBomBatch上料情况（异步查询）
                    var orderBomBatch = await _dbContext.MesOrderBomBatch
                        .FirstOrDefaultAsync(x => x.OrderListId == workOrder.OrderListId && x.StationListId == orderBomItem.Station.StationId &&
                        x.ProductListId == orderBomItem.BomItem.ProductId && x.ResourceId == orderBomItem.DeviceInfo.ResourceId && x.BatchQty > x.CompletedQty);
                    if (orderBomBatch == null)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}未进行上料操作"));
                    }

                    // 18. 检查上料状态是否正常
                    if (orderBomBatch.OrderBomBatchStatus != 1)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料状态异常，状态：{orderBomBatch.OrderBomBatchStatus}"));
                    }

                    // 19. 检查上料数量是否足够（已使用数量 < 批次数量）
                    if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
                    {
                        return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"站点{request.StationCode}上料批次已使用完"));
                    }
                    orderBomBatch.CompletedQty += 1;
                    if (orderBomBatch.CompletedQty >= orderBomBatch.BatchQty)
                    {
                        orderBomBatch.OrderBomBatchStatus = 2;
                    }

                    var orderBomBatchitem = new MesOrderBomBatchItem
                    {
                        OrderBomBatchId = orderBomBatch.OrderBomBatchId,
                        SnNumber = request.SN,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };

                    await _dbContext.MesOrderBomBatchItem.AddAsync(orderBomBatchitem);
                    _dbContext.MesOrderBomBatch.Update(orderBomBatch);
                    // 延迟保存，与后续操作一起批量提交
                }

                // 4.3 取工艺路线并计算下一站点
                var processRouteItems = await (from p in _dbContext.ProcessRoutes
                                               join iitm in _dbContext.ProcessRouteItems on p.Id equals iitm.HeadId
                                               join s in _dbContext.StationList on iitm.StationCode equals s.StationCode
                                               join ord in _dbContext.OrderList on p.Id equals ord.ProcessRouteId
                                               where ord.OrderListId == snCurrent.OrderListId //&& iitm.MustPassStation == true
                                               orderby iitm.RouteSeq
                                               select new { iitm.StationCode, s.StationId, iitm.RouteSeq, iitm.MustPassStation })
                                        .ToListAsync();

                var currentStep = processRouteItems.FirstOrDefault(x => x.StationCode == request.StationCode);
                if (currentStep == null)
                {
                    return FSharpResult<ValueTuple, (int, string)>.NewError((-1, $"工艺路线中找不到站点{request.StationCode}"));
                }

                if (currentStep.MustPassStation == false)
                {
                    var nextStep = processRouteItems
                    .Where(x => x.RouteSeq > currentStep.RouteSeq && x.MustPassStation == true)
                    .OrderBy(x => x.RouteSeq)
                    .FirstOrDefault();

                    if (nextStep == null)
                    {
                        // 最后一站：状态置为完工(4)
                        snCurrent.StationStatus = 4;
                    }
                    else
                    {
                        // 跳到下一站
                        snCurrent.CurrentStationListId = nextStep.StationId;
                        snCurrent.ResourceId = deviceInfo.ResourceId;
                        snCurrent.ProductionLineId = deviceInfo.ProductionLineId;
                    }

                    snCurrent.UpdateTime = DateTime.Now;
                    _dbContext.mesSnListCurrents.Update(snCurrent);
                }
                
                // 批量提交所有待处理的数据库操作
                await _dbContext.SaveChangesAsync();
                
            }

            // ---------- 5. 全部完成 ----------
            return FSharpResult<ValueTuple, (int, string)>.NewOk(default(ValueTuple));
        }

        
        /// <summary>
        /// 追溯单个SN的站点、上料及测试记录
        /// </summary>
        /// <param name="sn">SN号</param>
        /// <returns>追溯结果</returns>
        public async Task<FSharpResult<SnTraceDto, (int, string)>> TraceSN(string sn)
        {
            if (string.IsNullOrWhiteSpace(sn))
            {
                return FSharpResult<SnTraceDto, (int, string)>.NewError((-1, "SN不能为空"));
            }

            // 1. 基本信息：SN当前状态
            var snCurrent = await _dbContext.mesSnListCurrents
                .Include(c => c.OrderList)
                .Include(c => c.StationList)
                .FirstOrDefaultAsync(c => c.SnNumber == sn);

            if (snCurrent == null)
            {
                return FSharpResult<SnTraceDto, (int, string)>.NewError((-1, $"SN:{sn} 不存在"));
            }

            // 2. 历史过站记录
            var histories = await _dbContext.mesSnListHistories
                .Include(h => h.StationList)
                .Include(h => h.Resource)
                .Where(h => h.SnNumber == sn)
                .OrderBy(h => h.CreateTime)
                .ToListAsync();

            // 3. 测试数据
            var historyIds = histories.Select(h => h.SNListHistoryId).ToList();
            var testData = await _dbContext.mesSnTestDatas
                .Where(t => historyIds.Contains(t.SNListHistoryId))
                .ToListAsync();

            // 4. 上料批次及明细
            var batchItems = await _dbContext.MesOrderBomBatchItem
                .Include(b => b.OrderBomBatch)
                .ThenInclude(b => b.StationList)
                .Include(b => b.OrderBomBatch.ProductList)
                .Where(b => b.SnNumber == sn)
                .ToListAsync();

            // 组装DTO
            var dto = new SnTraceDto
            {
                SN = sn,
                OrderCode = snCurrent.OrderList?.OrderCode,
                ProductCode = snCurrent.OrderList?.ProductList?.ProductCode,
                CurrentStation = snCurrent.StationList?.StationCode,
                StationStatus = snCurrent.StationStatus,
                IsAbnormal = snCurrent.IsAbnormal,
                CreateTime = snCurrent.CreateTime.Value.DateTime,
                UpdateTime = snCurrent.UpdateTime.Value.DateTime,
                Stations = histories.Select(h => new StationTraceDto
                {
                    StationCode = h.StationList.StationCode,
                    StationName = h.StationList.StationName,
                    StationStatus = h.StationStatus,
                    ResourceCode = h.Resource?.Resource,
                    TestResult = h.StationStatus == 1 ? "PASS" : "NG",
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
            public int StationStatus { get; set; }
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
            public int StationStatus { get; set; }
            public string ResourceCode { get; set; }
            public string TestResult { get; set; }
            public DateTime TestTime { get; set; }
            public List<TestDataDto> TestData { get; set; } = new();
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